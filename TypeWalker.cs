using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class TypeWalker: CSharpSyntaxWalker {
	public static uint detail;

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
		case MethodDeclarationSyntax methodDeclaration:
			Console.Write(methodDeclaration.ReturnType);
			Console.Write(' ');
			break;
		}
		Console.WriteLine(Etc.Signature(method, model));
	}

	void Descend(int depth, BaseMethodDeclarationSyntax method) {
		if (!visited.Add(method))
			return;
		depth++;
		switch (detail) {
		case 1:
			foreach (var symbol in callees[method])
				if (methodsDictionary.TryGetValue(symbol, out var callee)) {
					if (TopLevel(callee))
						continue;
					Declare(depth, callee);
					Descend(depth, callee);
				}
			break;
		case 2:
			foreach (var symbol in callees[method])
				if (methodsDictionary.TryGetValue(symbol, out var callee)) {
					if (TopLevel(callee)) {
						Declare(depth, callee);
						continue;
					}
					Declare(depth, callee);
					Descend(depth, callee);
				}
			break;
		default:
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
			break;
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
			if (0 == detail) {
				Declare(1, method);
				continue;
			}
			if (TopLevel(method)) {
				visited.Clear();
				Declare(1, method);
				Descend(1, method);
			}
		}
		Console.WriteLine();
	}
}
