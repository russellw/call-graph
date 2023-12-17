using System.Runtime.CompilerServices;

static class Etc {
	public static void Print(object a, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0) {
		Console.Error.WriteLine($"{file}:{line}: {a}");
	}

	public static void Print<T>(List<T> a, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0) {
		Console.Error.WriteLine("{0}:{1}: [{2}]", file, line, string.Join(", ", a));
	}
}
