using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class Type {
	public static void Add(TypeDeclarationSyntax typeDeclaration) {
		var type = new Type(typeDeclaration);
		types.Add(type.key, type);
	}

	static readonly Dictionary<string, Type> types = new();

	readonly string key;
	readonly TypeDeclarationSyntax typeDeclaration;

	Type(TypeDeclarationSyntax typeDeclaration) {
		this.typeDeclaration = typeDeclaration;
		key = DottedName();
	}

	string DottedName() {
		SyntaxNode? a = typeDeclaration;
		var parts = new List<string>();
		for (;;) {
			string name;
			switch (a) {
			case NamespaceDeclarationSyntax namespaceDeclaration:
				name = namespaceDeclaration.Name.ToString();
				break;
			case TypeDeclarationSyntax typeDeclaration:
				name = typeDeclaration.Identifier.ToString();
				break;
			default:
				parts.Reverse();
				return string.Join('.', parts);
			}
			a = a.Parent;
			parts.Add(name);
		}
	}
}
