class Program {
	static void Help() {
		var name = typeof(Program).Assembly.GetName().Name;
		Console.WriteLine($"Usage: {name} [options] path...");
		Console.WriteLine("");
		Console.WriteLine("-h  Show help");
		Console.WriteLine("-V  Show version");
		Console.WriteLine("-i  In-place edit");
		Console.WriteLine("-r  Recur into directories");
	}

	static void Main(string[] args) {
	}

	static void Version() {
		var name = typeof(Program).Assembly.GetName().Name;
		var version = typeof(Program).Assembly.GetName()?.Version?.ToString(2);
		Console.WriteLine($"{name} {version}");
	}
}
