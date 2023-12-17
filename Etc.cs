using Microsoft.CodeAnalysis.CSharp.Syntax;

static class Etc {
	public static string Name(TypeDeclarationSyntax node) {
		var s = node.Identifier.Text;
		while (node.Parent is TypeDeclarationSyntax parent) {
			node = parent;
			s = $"{node.Identifier.Text}.{s}";
		}
		return s;
	}
}
