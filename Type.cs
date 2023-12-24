using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class Type {
	public static readonly List<Type> types = new();

	public static void Add(TypeDeclarationSyntax typeDeclaration) {
		switch (typeDeclaration) {
		case ClassDeclarationSyntax:
		case StructDeclarationSyntax:
			return;
		}
		var type = new Type(typeDeclaration);
		typeDictionary.Add(type.key, type);
		types.Add(type);
	}

	public void AddMethods() {
		foreach (var baseMethod in typeDeclaration.Members.OfType<BaseMethodDeclarationSyntax>())
			methods.Add(new Method(baseMethod));
	}

	public void Print() {
		Etc.Modifiers(typeDeclaration);
		switch (typeDeclaration) {
		case ClassDeclarationSyntax:
			Console.Write("class ");
			break;
		case StructDeclarationSyntax:
			Console.Write("struct ");
			break;
		default:
			throw new NotImplementedException(key);
		}
		Console.WriteLine(key);
	}

	static readonly Dictionary<string, Type> typeDictionary = new();

	readonly string key;
	readonly List<Method> methods = new();
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
