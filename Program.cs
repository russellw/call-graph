using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

static class Program {
	delegate void Callback(string file);

	static void Descend(string path, Callback f) {
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
				}
				Descend(entry.FullName, f);
			}
			return;
		}
		f(path);
	}

	static void Help() {
		var name = typeof(Program).Assembly.GetName().Name;
		Console.WriteLine($"{name} [options] path...");
		Console.WriteLine("");
		Console.WriteLine("-h   Show help");
		Console.WriteLine("-V   Show version");
		Console.WriteLine("");
		Console.WriteLine("-lN  Level of detail");
		Console.WriteLine("  0  Method declarations only");
		Console.WriteLine("  1  Trees of exclusive callers");
		Console.WriteLine("  2  Call graph within class");
		Console.WriteLine("  3  All method calls");
	}

	static void Main(string[] args) {
		try {
			// Command line
			var options = true;
			var paths = new List<string>();
			var stdin = false;
			for (var i = 0; i < args.Length; i++) {
				var arg = args[i];
				if (!arg.StartsWith('-') || !options) {
					paths.Add(arg);
					continue;
				}
				var j = 0;
				do
					j++;
				while (j < arg.Length && '-' == arg[j]);
				if (arg.Length == j) {
					switch (j) {
					case 1:
						stdin = true;
						continue;
					case 2:
						options = false;
						continue;
					}
					throw new IOException(arg + ": unknown option");
				}
				switch (arg[j]) {
				case '?':
				case 'h':
					Help();
					return;
				case 'V':
				case 'v':
					Version();
					return;
				default:
					throw new IOException(arg + ": unknown option");
				}
			}
			if (!paths.Any())
				paths.Add(".");

			// Source files
			var compilation = CSharpCompilation.Create(null);
			var trees = new List<SyntaxTree>();
			foreach (var path in paths)
				Descend(path, file => {
					if (!Path.GetExtension(file).Equals(".cs", StringComparison.OrdinalIgnoreCase))
						return;
					var text = File.ReadAllText(file);
					var tree = CSharpSyntaxTree.ParseText(text, CSharpParseOptions.Default, file);
					foreach (var diagnostic in tree.GetDiagnostics())
						Console.Error.WriteLine(diagnostic);
					compilation = compilation.AddSyntaxTrees(tree);
					trees.Add(tree);
				});

			// Output
			foreach (var tree in trees) {
				var model = compilation.GetSemanticModel(tree);
				var root = tree.GetCompilationUnitRoot();
				new TypeWalker(model).Visit(root);
			}
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
