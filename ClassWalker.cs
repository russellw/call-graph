using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class ClassWalker: CSharpSyntaxWalker {
	public ClassWalker(SemanticModel model) {
		this.model = model;
	}

	public override void VisitClassDeclaration(ClassDeclarationSyntax node) {
		Class.Classes.Add(new Class(node, model));
		base.VisitClassDeclaration(node);
	}

	public override void VisitStructDeclaration(StructDeclarationSyntax node) {
		Class.Classes.Add(new Class(node, model));
		base.VisitStructDeclaration(node);
	}

	readonly SemanticModel model;
}
