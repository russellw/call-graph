using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class Method {
	public Method(BaseMethodDeclarationSyntax baseMethod, SemanticModel model) {
		this.baseMethod = baseMethod;
		symbol = model.GetDeclaredSymbol(baseMethod)!;
	}

	public void Print(int level) {
		Indent(level);
		Etc.Modifiers(baseMethod);
	}

	readonly BaseMethodDeclarationSyntax baseMethod;
	readonly ISymbol symbol;

	static void Indent(int n) {
		while (0 != n--)
			Console.Write("    ");
	}
}
