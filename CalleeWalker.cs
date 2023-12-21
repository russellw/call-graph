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

		var method = Callee(node);
		if (method == null)
			return;
		Callees.Add(method.ToString()!);
	}

	readonly SemanticModel model;

	ISymbol? Callee(InvocationExpressionSyntax node) {
		var info = model.GetSymbolInfo(node);
		if (info.Symbol != null)
			return info.Symbol;
		Etc.Print(node);
		Etc.Print(info.CandidateSymbols);
		Etc.Print(info.CandidateReason);
		Console.Error.WriteLine();
		return null;
	}
}
