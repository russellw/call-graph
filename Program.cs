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
			if (string.Equals(Path.GetExtension(file), ".dll", StringComparison.OrdinalIgnoreCase))
				compilation = compilation.AddReferences(MetadataReference.CreateFromFile(file));
		});
	}

	static void Version() {
		var name = typeof(Program).Assembly.GetName().Name;
		var version = typeof(Program).Assembly.GetName()?.Version?.ToString(2);
		Console.WriteLine($"{name} {version}");
	}
}
