using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class ClassLink: CSharpSyntaxWalker {
	public override void VisitClassDeclaration(ClassDeclarationSyntax node) {
		base.VisitClassDeclaration(node);

		Html.Write("<li>");
		Html.Link(node.Identifier.Text);
	}
}
