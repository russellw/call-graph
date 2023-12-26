using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class CalleeWalker: CSharpSyntaxWalker {
	public readonly OrderedSet<IMethodSymbol> Callees = new();

	public CalleeWalker(SemanticModel model) {
		this.model = model;
	}

	public override void VisitInvocationExpression(InvocationExpressionSyntax node) {
		base.VisitInvocationExpression(node);

		var info = model.GetSymbolInfo(node);
		if (info.Symbol != null)
			Callees.Add((IMethodSymbol)info.Symbol);
		foreach (var symbol in info.CandidateSymbols)
			Callees.Add((IMethodSymbol)symbol);
	}

	readonly SemanticModel model;
}
