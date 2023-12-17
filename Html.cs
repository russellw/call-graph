static class Html {
	public static void Header(int level, string id) {
		Header(level, id, Capitalize(id));
	}

	public static void Header(int level, string id, string text) {
		Console.Write($"<h{level} id=\"{id}\">");
		Console.Write(text);
		Console.WriteLine($"</h{level}>");
	}

	public static void Link(string href) {
		Link(href, Capitalize(href));
	}

	public static void Link(string href, string text) {
		Console.Write($"<a href=\"{href}\">");
		Console.Write(text);
		Console.WriteLine("</a>");
	}

	public static void Open(string file) {
		writer = new(file);
		writer.NewLine = "\n";
	}

	static StreamWriter writer = null!;

	static string Capitalize(string s) {
		return char.ToUpperInvariant(s[0]) + s[1..];
	}
}
