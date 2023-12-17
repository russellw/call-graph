using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class ClassWalker: CSharpSyntaxWalker {
	public static readonly List<TypeDeclarationSyntax> Classes = new();

	public override void VisitClassDeclaration(ClassDeclarationSyntax node) {
		Classes.Add(node);
		base.VisitClassDeclaration(node);
	}

	public override void VisitStructDeclaration(StructDeclarationSyntax node) {
		Classes.Add(node);
		base.VisitStructDeclaration(node);
	}
}
