using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.CompilerServices;

static class Etc {
	public static void Print(object a, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0) {
		Console.Error.WriteLine($"{file}:{line}: {a}");
	}

	public static void Print<T>(List<T> a, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0) {
		Console.Error.WriteLine("{0}:{1}: [{2}]", file, line, string.Join(", ", a));
	}

	public static string Signature(BaseMethodDeclarationSyntax baseMethod) {
		string s;
		switch (baseMethod) {
		case ConstructorDeclarationSyntax constructor: {
			var name = constructor.Identifier;
			var parameters = baseMethod.ParameterList;
			s = $"{name}{parameters}";
			break;
		}
		case MethodDeclarationSyntax method: {
			var returnType = method.ReturnType;
			var name = method.Identifier;
			var parameters = baseMethod.ParameterList;
			s = $"{returnType} {name}{parameters}";
			break;
		}
		default:
			throw new NotImplementedException(baseMethod.ToString());
		}
		if (baseMethod.Modifiers.Any()) {
			var modifiers = string.Join(" ", baseMethod.Modifiers.Select(m => m.Text));
			return $"{modifiers} {s}";
		}
		return s;
	}
}
