using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class Class {
	public static readonly List<Class> Classes = new();

	public readonly SemanticModel Model;
	public readonly TypeDeclarationSyntax Node;

	public Class(TypeDeclarationSyntax node, SemanticModel model) {
		Node = node;
		Model = model;
	}

	public override string ToString() {
		var node = Node;
		var s = node.Identifier.Text;
		while (node.Parent is TypeDeclarationSyntax parent) {
			node = parent;
			s = $"{node.Identifier.Text}.{s}";
		}
		return s;
	}

	public string WithBases() {
		var s = ToString();
		if (Node.BaseList != null)
			s += Node.BaseList;
		return s;
	}
}
