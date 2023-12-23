using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class Method {
	public Method(BaseMethodDeclarationSyntax baseMethod) {
		this.baseMethod = baseMethod;
	}

	readonly BaseMethodDeclarationSyntax baseMethod;
}
