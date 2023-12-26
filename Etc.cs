using System.Collections.Immutable;
using System.Runtime.CompilerServices;

static class Etc {
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
}
