using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class Type
    {
    TypeDeclarationSyntax node;

    public Type(TypeDeclarationSyntax node)
    {
        this.node = node;
    }
}
