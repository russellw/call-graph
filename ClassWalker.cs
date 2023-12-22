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

	readonly SemanticModel model;

	static void Indent(int n) {
		while (0 != n--)
			Console.Write("  ");
	}

	static void Modifiers(MemberDeclarationSyntax node) {
		foreach (var modifier in node.Modifiers) {
			Console.Write(modifier);
			Console.Write(' ');
		}
	}

	static void TypeDeclaration(TypeDeclarationSyntax node, SemanticModel model) {
		Console.Write(node.Identifier);
		Console.WriteLine(node.BaseList);
		var methods = node.Members.OfType<BaseMethodDeclarationSyntax>();
		foreach (var baseMethod in methods) {
			Indent(1);
			Modifiers(baseMethod);
			switch (baseMethod) {
			case MethodDeclarationSyntax method:
				Console.Write(method.ReturnType);
				Console.Write(' ');
				break;
			}
			Console.WriteLine(Etc.Signature(baseMethod, model));

			var walker = new CalleeWalker(model);
			walker.Visit(baseMethod);
			foreach (var callee in walker.Callees) {
				Indent(2);
				Console.WriteLine(callee);
			}
		}
	}
}
