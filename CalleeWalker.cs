using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class CalleeWalker: CSharpSyntaxWalker {
	public readonly OrderedSet<string> Callees = new();

	public CalleeWalker(SemanticModel model) {
		this.model = model;
	}

	public override void VisitInvocationExpression(InvocationExpressionSyntax node) {
		base.VisitInvocationExpression(node);
		if (Callee(node) is IMethodSymbol method)
			Callees.Add(Etc.Signature(method));
	}

	readonly SemanticModel model;

	ISymbol? Callee(InvocationExpressionSyntax node) {
		var info = model.GetSymbolInfo(node);
		if (info.Symbol != null)
			return info.Symbol;
		return info.CandidateSymbols.FirstOrDefault();
	}
}
