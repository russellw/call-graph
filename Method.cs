using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class Method {
	public Method(BaseMethodDeclarationSyntax baseMethod) {
		this.baseMethod = baseMethod;
	}

	public void Print(int level) {
		Indent(level);
		Etc.Modifiers(baseMethod);
	}

	readonly BaseMethodDeclarationSyntax baseMethod;
	readonly string key;

	static void Indent(int n) {
		while (0 != n--)
			Console.Write("    ");
	}
}
