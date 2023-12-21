using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

static class Program {
	delegate void Callback(string file);

	static void Descend(string path, Callback f) {
		foreach (var entry in new DirectoryInfo(path).EnumerateFileSystemInfos()) {
			if (entry is DirectoryInfo) {
				switch (entry.Name) {
				case "bin":
				case "obj":
					continue;
				}
				if (entry.Name.StartsWith('.'))
					continue;
				Descend(entry.FullName, f);
				continue;
			}
			f(entry.FullName);
		}
	}

	static void Help() {
		var name = typeof(Program).Assembly.GetName().Name;
		Console.WriteLine($"{name} without args, expects C# project in current directory");
		Console.WriteLine("");
		Console.WriteLine("Options:");
		Console.WriteLine("-h  Show help");
		Console.WriteLine("-V  Show version");
	}

	static void Indent(int n) {
		while (0 != n--)
			Console.Write("  ");
	}

	static void Main(string[] args) {
		try {
			// Command line
			foreach (var arg in args) {
				var s = arg;
				if (s.StartsWith('-')) {
					while (s.StartsWith('-'))
						s = s[1..];
					switch (s) {
					case "?":
					case "h":
					case "help":
						Help();
						return;
					case "V":
					case "v":
					case "version":
						Version();
						return;
					default:
						throw new Error(arg + ": unknown option");
					}
				}
				Help();
				return;
			}

			// Reference DLLs
			var compilation =
				CSharpCompilation.Create(null).AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
			Descend(".", file => {
				if (!Path.GetExtension(file).Equals(".dll", StringComparison.OrdinalIgnoreCase))
					return;
				compilation = compilation.AddReferences(MetadataReference.CreateFromFile(file));
			});

			// Source files
			List<SyntaxTree> trees = new();
			Descend(".", file => {
				if (!Path.GetExtension(file).Equals(".cs", StringComparison.OrdinalIgnoreCase))
					return;
				var text = File.ReadAllText(file);
				var tree = CSharpSyntaxTree.ParseText(text, CSharpParseOptions.Default, file);
				if (tree.GetDiagnostics().Any()) {
					foreach (var diagnostic in tree.GetDiagnostics())
						Console.Error.WriteLine(diagnostic);
					Environment.Exit(1);
				}
				compilation = compilation.AddSyntaxTrees(tree);
				trees.Add(tree);
			});

			// Output
			foreach (var tree in trees) {
				var model = compilation.GetSemanticModel(tree);
				var root = tree.GetCompilationUnitRoot();
				foreach (var node in root.ChildNodes())
					switch (node) {
					case ClassDeclarationSyntax classDeclaration:
						if (classDeclaration.Modifiers.Any()) {
							Console.Write(string.Join(' ', classDeclaration.Modifiers));
							Console.Write(' ');
						}
						Console.Write("class ");
						TypeDeclaration(model, classDeclaration);
						break;
					}
			}
		} catch (Error e) {
			Console.Error.WriteLine(e.Message);
			Environment.Exit(1);
		}
	}

	static void TypeDeclaration(SemanticModel model, TypeDeclarationSyntax node) {
		Console.Write(node.Identifier);
		Console.WriteLine(node.BaseList);
		var methods = node.Members.OfType<BaseMethodDeclarationSyntax>();
		foreach (var method in methods) {
			Indent(1);
			Console.WriteLine(Etc.Signature(method));

			var walker = new CalleeWalker(model);
			walker.Visit(method);
			foreach (var callee in walker.Callees) {
				Indent(2);
				Console.WriteLine(callee);
			}
		}
	}

	static void Version() {
		var name = typeof(Program).Assembly.GetName().Name;
		var version = typeof(Program).Assembly.GetName()?.Version?.ToString(2);
		Console.WriteLine($"{name} {version}");
	}
}
