using System.Diagnostics;

static class Program {
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
	}

	static void Version() {
		var name = typeof(Program).Assembly.GetName().Name;
		var version = typeof(Program).Assembly.GetName()?.Version?.ToString(2);
		Console.WriteLine($"{name} {version}");
	}
}
