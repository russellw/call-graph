using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class CalleeWalker: CSharpSyntaxWalker {
	public readonly OrderedSet<BaseMethodDeclarationSyntax> Callees = new();

	public CalleeWalker(SemanticModel model, Dictionary<ISymbol, BaseMethodDeclarationSyntax> methodsDictionary) {
		this.model = model;
		this.methodsDictionary = methodsDictionary;
	}

	public override void VisitInvocationExpression(InvocationExpressionSyntax node) {
		base.VisitInvocationExpression(node);
		foreach (var symbol in Symbols(node))
			if (methodsDictionary.TryGetValue(symbol, out BaseMethodDeclarationSyntax? method))
				Callees.Add(method);
	}

	readonly Dictionary<ISymbol, BaseMethodDeclarationSyntax> methodsDictionary;
	readonly SemanticModel model;

	IEnumerable<ISymbol> Symbols(InvocationExpressionSyntax node) {
		var info = model.GetSymbolInfo(node);
		if (info.Symbol != null)
			yield return info.Symbol;
		foreach (var symbol in info.CandidateSymbols)
			yield return symbol;
	}
}
