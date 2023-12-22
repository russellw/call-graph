using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

static class Etc {
	public static string Name(ITypeSymbol type) {
		if (type is INamedTypeSymbol namedType && namedType.ContainingType != null)
			return $"{namedType.ContainingType.Name}.{namedType.Name}";
		return type.ToString()!;
	}

	public static string Name(TypeSyntax type, SemanticModel model) {
		var symbol = model.GetSymbolInfo(type).Symbol;
		if (null == symbol)
			return type.ToString();
		return Name((ITypeSymbol)symbol);
	}

	public static void Print(object a, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0) {
		Console.Error.WriteLine($"{file}:{line}: {a}");
	}

	public static void Print<T>(ImmutableArray<T> a, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0) {
		var s = string.Join(", ", a);
		Console.Error.WriteLine($"{file}:{line}: [{s}]");
	}

	public static void Print<T>(List<T> a, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0) {
		var s = string.Join(", ", a);
		Console.Error.WriteLine($"{file}:{line}: [{s}]");
	}

	public static string Signature(BaseMethodDeclarationSyntax baseMethod, SemanticModel model) {
		var parameters =
			string.Join(", ", baseMethod.ParameterList.Parameters.Select(p => $"{Name(p.Type!, model)} {p.Identifier}"));
		switch (baseMethod) {
		case ConstructorDeclarationSyntax constructor: {
			var name = constructor.Identifier;
			return $"{name}({parameters})";
		}
		case MethodDeclarationSyntax method: {
			var name = method.Identifier;
			return $"{name}({parameters})";
		}
		default:
			throw new NotImplementedException(baseMethod.ToString());
		}
	}

	public static string Signature(IMethodSymbol method) {
		var containingType = method.ContainingType;
		var name = method.Name;
		var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
		return $"{containingType}.{name}({parameters})";
	}
}
