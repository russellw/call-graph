static class Html {
	public static void Close() {
		writer.Dispose();
	}

	public static void Header(int level, string id) {
		Header(level, id, Capitalize(id));
	}

	public static void Header(int level, string id, string text) {
		Write($"<h{level} id=\"{id}\">");
		Write(text);
		WriteLine($"</h{level}>");
	}

	public static void Link(string href) {
		Link(href, Capitalize(href));
	}

	public static void Link(string href, string text) {
		Write($"<a href=\"{href}\">");
		Write(text);
		WriteLine("</a>");
	}

	public static void Open(string file) {
		writer = new(file);
		writer.NewLine = "\n";
		Console.WriteLine(file);
	}

	public static void Write(string s) {
		writer.Write(s);
	}

	public static void WriteLine(string s) {
		writer.WriteLine(s);
	}

	static StreamWriter writer = null!;

	static string Capitalize(string s) {
		return char.ToUpperInvariant(s[0]) + s[1..];
	}
}
