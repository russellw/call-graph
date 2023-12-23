using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class Type {
	public Type(TypeDeclarationSyntax typeDeclaration) {
		this.typeDeclaration = typeDeclaration;
	}

	TypeDeclarationSyntax typeDeclaration;
}
