using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

static class Program {
	delegate void Callback(string file);

	static void Descend(string path, Callback f) {
		foreach (var entry in new DirectoryInfo(path).EnumerateFileSystemInfos()) {
			if (entry is DirectoryInfo) {
				if (!entry.Name.StartsWith('.'))
					Descend(entry.FullName, f);
				continue;
			}
			f(entry.FullName);
		}
	}

	static void Exec(string program, string args) {
		Console.WriteLine($"{program} {args}");
		var process = new Process();
		process.StartInfo.FileName = program;
		process.StartInfo.Arguments = args;
		process.Start();
		process.WaitForExit();
		if (0 != process.ExitCode)
			Environment.Exit(process.ExitCode);
	}

	static void Help() {
		var name = typeof(Program).Assembly.GetName().Name;
		Console.WriteLine($"Usage: {name} [options]");
		Console.WriteLine("");
		Console.WriteLine("-h  Show help");
		Console.WriteLine("-V  Show version");
	}

	static void Main(string[] args) {
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
					Console.Error.WriteLine("{0}: unknown option", arg);
					Environment.Exit(1);
					break;
				}
				continue;
			}
			Help();
			return;
		}

		// Build
		Exec("dotnet", "clean /p:Configuration=Release /p:Platform=\"Any CPU\"");

		// References
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
			trees.Add(tree);
		});

		// Header
		Console.WriteLine("<!DOCTYPE html>");
		Console.WriteLine("<html lang=\"en\">");
		Console.WriteLine("<meta charset=\"utf-8\">");
		foreach (var entry in new DirectoryInfo(".").EnumerateFileSystemInfos())
			if (Path.GetExtension(entry.Name).Equals(".csproj", StringComparison.OrdinalIgnoreCase)) {
				Console.Write("<title>");
				Console.Write(Path.GetFileNameWithoutExtension(entry.Name));
				Console.WriteLine("</title>");
				break;
			}

		// Contents
		Console.WriteLine("<ul>");

		Console.Write("<li>");
		Etc.Link("classes");

		Console.WriteLine("</ul>");

		// Classes
		Etc.Header(1, "classes");
		foreach (var tree in trees) {
			var model = compilation.AddSyntaxTrees(tree).GetSemanticModel(tree);
			var root = tree.GetCompilationUnitRoot();
		}
	}

	static void Version() {
		var name = typeof(Program).Assembly.GetName().Name;
		var version = typeof(Program).Assembly.GetName()?.Version?.ToString(2);
		Console.WriteLine($"{name} {version}");
	}
}
