namespace VireoXiniParser.Model;

/// <summary>
/// Represents an INI document consisting of a collection of sections.
/// </summary>
/// <param name="Sections">The list of sections contained in the INI document.</param>
public record IniDocument(IReadOnlyList<IniSection> Sections);

/// <summary>
/// Represents a section within an INI document, containing a name and a set of key-value pairs.
/// </summary>
/// <param name="Name">The name of the section.</param>
/// <param name="KeyValues">The key-value pairs within the section.</param>
public record IniSection(string Name, IReadOnlyDictionary<string, string> KeyValues);