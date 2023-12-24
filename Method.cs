using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class Method {
	public Method(BaseMethodDeclarationSyntax baseMethod) {
		this.baseMethod = baseMethod;
	}

	public void Print() {
		Etc.Modifiers(baseMethod);
	}

	readonly BaseMethodDeclarationSyntax baseMethod;
}
