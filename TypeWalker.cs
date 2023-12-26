using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class TypeWalker: CSharpSyntaxWalker {
	public static bool listOnly;

	public TypeWalker(SemanticModel model) {
		this.model = model;
	}

	public override void VisitClassDeclaration(ClassDeclarationSyntax node) {
		Modifiers(node);
		Console.Write("class ");
		TypeDeclaration(node, model);

		base.VisitClassDeclaration(node);
	}

	public override void VisitStructDeclaration(StructDeclarationSyntax node) {
		Modifiers(node);
		Console.Write("struct ");
		TypeDeclaration(node, model);

		base.VisitStructDeclaration(node);
	}

	readonly Dictionary<BaseMethodDeclarationSyntax, OrderedSet<IMethodSymbol>> callees = new();
	readonly Dictionary<IMethodSymbol, BaseMethodDeclarationSyntax> methodsDictionary = new(SymbolEqualityComparer.Default);
	readonly SemanticModel model;
	readonly HashSet<BaseMethodDeclarationSyntax> visited = new();

	int Callers(BaseMethodDeclarationSyntax callee) {
		var symbol = model.GetDeclaredSymbol(callee)!;
		var n = 0;
		foreach (var entry in callees)
			if (entry.Value.Contains(symbol))
				n++;
		return n;
	}

	void Declare(int depth, BaseMethodDeclarationSyntax method) {
		Indent(depth);
		Modifiers(method);
		switch (method) {
		case ConstructorDeclarationSyntax constructorDeclaration:
			Console.Write(constructorDeclaration.Identifier);
			break;
		case ConversionOperatorDeclarationSyntax conversionOperatorDeclaration:
			Console.Write(conversionOperatorDeclaration.Type);
			break;
		case MethodDeclarationSyntax methodDeclaration:
			Console.Write(methodDeclaration.ReturnType);
			Console.Write(' ');
			Console.Write(methodDeclaration.Identifier);
			break;
		case OperatorDeclarationSyntax operatorDeclaration:
			Console.Write("operator ");
			Console.Write(operatorDeclaration.OperatorToken);
			break;
		default:
			throw new NotImplementedException(method.Kind().ToString());
		}
		Console.WriteLine(method.ParameterList);
	}

	void Descend(int depth, BaseMethodDeclarationSyntax method) {
		if (!visited.Add(method))
			return;
		depth++;
		if (listOnly) {
			foreach (var symbol in callees[method])
				if (methodsDictionary.TryGetValue(symbol, out var callee)) {
					if (TopLevel(callee))
						continue;
					Declare(depth, callee);
					Descend(depth, callee);
				}
			return;
		}
		foreach (var symbol in callees[method]) {
			if (methodsDictionary.TryGetValue(symbol, out var callee)) {
				if (TopLevel(callee)) {
					Declare(depth, callee);
					continue;
				}
				Declare(depth, callee);
				Descend(depth, callee);
				continue;
			}
			Indent(depth);
			Console.WriteLine(symbol);
		}
	}

	static void Indent(int n) {
		while (0 != n--)
			Console.Write("    ");
	}

	static void Modifiers(MemberDeclarationSyntax member) {
		foreach (var modifier in member.Modifiers) {
			Console.Write(modifier);
			Console.Write(' ');
		}
	}

	static string Name(ITypeSymbol type) {
		if (type is INamedTypeSymbol namedType && namedType.ContainingType != null)
			return $"{namedType.ContainingType.Name}.{namedType.Name}";
		return type.ToString()!;
	}

	static string Name(TypeSyntax type, SemanticModel model) {
		var symbol = model.GetSymbolInfo(type).Symbol;
		if (null == symbol)
			return type.ToString();
		return Name((ITypeSymbol)symbol);
	}

	static void ParentDot(SyntaxNode? node) {
		switch (node) {
		case BaseNamespaceDeclarationSyntax namespaceDeclaration:
			ParentDot(node.Parent);
			Console.Write(namespaceDeclaration.Name);
			break;
		case TypeDeclarationSyntax typeDeclaration:
			ParentDot(node.Parent);
			Console.Write(typeDeclaration.Identifier);
			break;
		default:
			return;
		}
		Console.Write('.');
	}

	static bool Private(MemberDeclarationSyntax member) {
		foreach (var modifier in member.Modifiers)
			switch (modifier.Kind()) {
			case SyntaxKind.InternalKeyword:
			case SyntaxKind.ProtectedKeyword:
			case SyntaxKind.PublicKeyword:
				return false;
			}
		return true;
	}

	static string Signature(BaseMethodDeclarationSyntax method, SemanticModel model) {
		var parameters = string.Join(", ", method.ParameterList.Parameters.Select(p => $"{Name(p.Type!, model)} {p.Identifier}"));
		switch (method) {
		case ConstructorDeclarationSyntax constructorDeclaration:
			return $"{constructorDeclaration.Identifier}({parameters})";
		case ConversionOperatorDeclarationSyntax conversionOperatorDeclaration:
			return $"operator {conversionOperatorDeclaration.Type}({parameters})";
		case MethodDeclarationSyntax methodDeclaration:
			return $"{methodDeclaration.Identifier}({parameters})";
		case OperatorDeclarationSyntax operatorDeclaration:
			return $"operator {operatorDeclaration.OperatorToken}({parameters})";
		}
		throw new NotImplementedException(method.Kind().ToString());
	}

	static string Signature(IMethodSymbol method) {
		var containingType = method.ContainingType;
		var name = method.Name;
		var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
		return $"{containingType}.{name}({parameters})";
	}

	bool TopLevel(BaseMethodDeclarationSyntax baseMethod) {
		// A method is treated as top level, if it could have callers outside the class
		if (!Private(baseMethod))
			return true;
		if (baseMethod is MethodDeclarationSyntax method && "Main" == method.Identifier.Text)
			return true;

		// Or has multiple callers inside
		return 1 < Callers(baseMethod);
	}

	void TypeDeclaration(TypeDeclarationSyntax node, SemanticModel model) {
		ParentDot(node.Parent);
		Console.Write(node.Identifier);
		Console.WriteLine(node.BaseList);
		var methods = node.Members.OfType<BaseMethodDeclarationSyntax>();

		// Mostly we want to refer to methods as BaseMethodDeclarationSyntax
		// but (InvocationExpressionSyntax, model) gives IMethodSymbol
		// so need to be able to convert back
		// though only for methods in the current class
		// being the only ones we need to do something more with
		// than just report without further comment
		methodsDictionary.Clear();
		foreach (var method in methods)
			methodsDictionary.Add(model.GetDeclaredSymbol(method)!, method);

		// Callees
		callees.Clear();
		foreach (var method in methods) {
			var walker = new CalleeWalker(model);
			walker.Visit(method);
			callees.Add(method, walker.Callees);
		}

		// Output
		foreach (var method in methods) {
			if (TopLevel(method)) {
				visited.Clear();
				Declare(1, method);
				Descend(1, method);
			}
		}
		Console.WriteLine();
	}
}
