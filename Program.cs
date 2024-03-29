using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

static class Program {
	static CSharpCompilation compilation = CSharpCompilation.Create(null);
	static bool options = true;
	static readonly List<SyntaxTree> trees = new();

	static void Descend(string path) {
		if (Directory.Exists(path)) {
			foreach (var entry in new DirectoryInfo(path).EnumerateFileSystemInfos()) {
				if (entry is DirectoryInfo) {
					switch (entry.Name) {
					case "bin":
					case "obj":
						continue;
					}
					if (entry.Name.StartsWith('.'))
						continue;
				} else if (!entry.Extension.Equals(".cs", StringComparison.OrdinalIgnoreCase))
					continue;
				Descend(entry.FullName);
			}
			return;
		}
		Do(path, File.ReadAllText(path));
	}

	static void Do(string file, string text) {
		var tree = CSharpSyntaxTree.ParseText(text, CSharpParseOptions.Default, file);
		foreach (var diagnostic in tree.GetDiagnostics())
			Console.Error.WriteLine(diagnostic);
		compilation = compilation.AddSyntaxTrees(tree);
		trees.Add(tree);
	}

	static void Help() {
		var name = typeof(Program).Assembly.GetName().Name;
		Console.WriteLine($"{name} [options] path...");
		Console.WriteLine("");
		Console.WriteLine("-   Read stdin");
		Console.WriteLine("-h  Show help");
		Console.WriteLine("-v  Show version");
		Console.WriteLine("");
		Console.WriteLine("-l  List methods only, don't show calls");
	}

	static bool IsOption(string arg) {
		if (!options)
			return false;
		if (0 == arg.Length)
			return false;
		switch (arg[0]) {
		case '-':
			return true;
		case '/':
			return '\\' == Path.DirectorySeparatorChar;
		}
		return false;
	}

	static void Main(string[] args) {
		try {
			// Command line
			var paths = new List<string>();
			var stdin = false;
			for (var i = 0; i < args.Length; i++) {
				var arg = args[i];

				// Path
				if (!IsOption(arg)) {
					paths.Add(arg);
					continue;
				}

				// Option
				var j = 0;
				do
					j++;
				while (j < arg.Length && '-' == arg[j]);
				if (arg.Length == j)
					switch (arg) {
					case "-":
						stdin = true;
						continue;
					case "--":
						options = false;
						continue;
					}
				else
					switch (arg[j]) {
					case '?':
					case 'h':
						Help();
						return;
					case 'V':
					case 'v':
						Version();
						return;
					case 'l':
						TypeWalker.listOnly = true;
						continue;
					}
				throw new IOException(arg + ": unknown option");
			}
			if (!paths.Any() && !stdin)
				paths.Add(".");

			// Source files
			foreach (var path in paths)
				Descend(path);
			if (stdin)
				Do("stdin", Console.In.ReadToEnd());

			// Output
			foreach (var tree in trees) {
				var model = compilation.GetSemanticModel(tree);
				var root = tree.GetCompilationUnitRoot();
				new TypeWalker(model).Visit(root);
			}
			Console.ResetColor();
		} catch (IOException e) {
			Console.Error.WriteLine(e.Message);
			Environment.Exit(1);
		}
	}

	static void Version() {
		var name = typeof(Program).Assembly.GetName().Name;
		var version = typeof(Program).Assembly.GetName()?.Version?.ToString(2);
		Console.WriteLine($"{name} {version}");
	}
}
