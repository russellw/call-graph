using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class ClassLink: CSharpSyntaxWalker {
	public override void VisitClassDeclaration(ClassDeclarationSyntax node) {
		Do(node);
		base.VisitClassDeclaration(node);
	}

	public override void VisitStructDeclaration(StructDeclarationSyntax node) {
		Do(node);
		base.VisitStructDeclaration(node);
	}

	static void Do(TypeDeclarationSyntax node) {
		Html.Write("<li>");
		Html.Link(node.Identifier.Text);
	}
}
