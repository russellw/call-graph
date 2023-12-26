using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class CalleeWalker: CSharpSyntaxWalker {
	public readonly OrderedSet<IMethodSymbol> Callees = new();

	public CalleeWalker(SemanticModel model, Dictionary<IMethodSymbol, BaseMethodDeclarationSyntax> methodsDictionary) {
		this.model = model;
		this.methodsDictionary = methodsDictionary;
	}

	public override void VisitInvocationExpression(InvocationExpressionSyntax node) {
		base.VisitInvocationExpression(node);
		Add(node);
	}

	public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node) {
		base.VisitObjectCreationExpression(node);
		Add(node);
	}

	readonly Dictionary<IMethodSymbol, BaseMethodDeclarationSyntax> methodsDictionary;
	readonly SemanticModel model;

	void Add(SyntaxNode node) {
		var info = model.GetSymbolInfo(node);
		if (info.Symbol != null)
			Callees.Add((IMethodSymbol)info.Symbol);
		foreach (var symbol in info.CandidateSymbols)
			Callees.Add((IMethodSymbol)symbol);
	}
}
