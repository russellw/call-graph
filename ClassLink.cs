using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class ClassLink: CSharpSyntaxWalker {
	public override void VisitClassDeclaration(ClassDeclarationSyntax node) {
		Html.Write("<li>");
		Html.Link(node.Identifier.Text);

		base.VisitClassDeclaration(node);
	}

	public override void VisitStructDeclaration(StructDeclarationSyntax node) {
		Html.Write("<li>");
		Html.Link(node.Identifier.Text);

		base.VisitStructDeclaration(node);
	}
}
