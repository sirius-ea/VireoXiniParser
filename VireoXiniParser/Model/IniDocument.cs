namespace VireoXiniParser.Model;

public record IniDocument(IReadOnlyList<IniSection> Sections);

public record IniSection(string Name, IReadOnlyDictionary<string, string> KeyValues);