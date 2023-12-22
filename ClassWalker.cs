using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class ClassWalker: CSharpSyntaxWalker {
	public ClassWalker(SemanticModel model) {
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

	readonly Dictionary<BaseMethodDeclarationSyntax, OrderedSet<string>> callees = new();
	readonly SemanticModel model;
	readonly Dictionary<string, BaseMethodDeclarationSyntax> signatureMethods = new();

	int Callers(BaseMethodDeclarationSyntax callee) {
		var s = Etc.Signature(callee, model);
		var n = 0;
		foreach (var entry in callees)
			if (entry.Value.Contains(s))
				n++;
		return n;
	}

	static void Indent(int n) {
		while (0 != n--)
			Console.Write("  ");
	}

	void Method(int level, BaseMethodDeclarationSyntax baseMethod) {
		// Signature
		Indent(level);
		Modifiers(baseMethod);
		switch (baseMethod) {
		case MethodDeclarationSyntax method:
			Console.Write(method.ReturnType);
			Console.Write(' ');
			break;
		}
		Console.WriteLine(Etc.Signature(baseMethod, model));

		// Callees
		foreach (var s in callees[baseMethod]) {
			Method(level + 1, callee);
		}
	}

	static void Modifiers(MemberDeclarationSyntax member) {
		foreach (var modifier in member.Modifiers) {
			Console.Write(modifier);
			Console.Write(' ');
		}
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
		if (!Private(baseMethod))
			return true;
		return 1 < Callers(baseMethod);
	}

	void TypeDeclaration(TypeDeclarationSyntax node, SemanticModel model) {
		Console.Write(node.Identifier);
		Console.WriteLine(node.BaseList);
		var methods = node.Members.OfType<BaseMethodDeclarationSyntax>();

		// Cache lookups
		callees.Clear();
		signatureMethods.Clear();
		foreach (var baseMethod in methods) {
			var walker = new CalleeWalker(model);
			walker.Visit(baseMethod);
			callees.Add(baseMethod, walker.Callees);
			signatureMethods.Add(Etc.Signature(baseMethod, model), baseMethod);
		}

		// Output
		foreach (var baseMethod in methods)
			if (TopLevel(baseMethod))
				Method(1, baseMethod);
	}
}
