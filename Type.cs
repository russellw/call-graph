using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class Type {
	public Type(TypeDeclarationSyntax node) {
		this.node = node;
	}

	TypeDeclarationSyntax node;
}
