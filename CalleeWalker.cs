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
		var symbol = Callee(node);
		if (null != symbol)
			if (methodsDictionary.TryGetValue(symbol, out BaseMethodDeclarationSyntax method))
				Callees.Add(method);
	}

	readonly Dictionary<ISymbol, BaseMethodDeclarationSyntax> methodsDictionary;
	readonly SemanticModel model;

	ISymbol? Callee(InvocationExpressionSyntax node) {
		var info = model.GetSymbolInfo(node);
		if (info.Symbol != null)
			return info.Symbol;
		return info.CandidateSymbols.FirstOrDefault();
	}
}
