using System.Text;
using VireoXiniParser.Model;

namespace VireoXiniParser;

public class IniDataParser : IIniParser
{
    public IniDocument Parse(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        return Parse(stream);
    }

    public bool TryParse(string filePath, out IniDocument iniModel, out string? errorMessage)
    {
        try
        {
            iniModel = Parse(filePath);
            errorMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            iniModel = new IniDocument([]);
            errorMessage = $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            return false;
        }
    }

    public bool TryParse(string filePath, out IniDocument iniModel)
    {
        return TryParse(filePath, out iniModel, out _);
    }

    public async Task<IniDocument> ParseAsync(string filePath)
    {
        await using FileStream stream = File.OpenRead(filePath);
        return await ParseAsync(stream);
    }

    public IniDocument Parse(Stream stream)
    {
        using StreamReader reader = new(stream, leaveOpen: false);
        string content = reader.ReadToEnd();
        return ParseFromString(content);
    }

    public bool TryParse(Stream stream, out IniDocument iniModel, out string? errorMessage)
    {
        try
        {
            iniModel = Parse(stream);
            errorMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            iniModel = new IniDocument([]);
            errorMessage = $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            return false;
        }
    }

    public bool TryParse(Stream stream, out IniDocument iniModel)
    {
        return TryParse(stream, out iniModel, out _);
    }

    public async Task<IniDocument> ParseAsync(Stream stream)
    {
        using StreamReader reader = new(stream, leaveOpen: false);
        string content = await reader.ReadToEndAsync();
        return await ParseFromStringAsync(content);
    }

    public async Task<(bool Success, IniDocument? Model)> TryParseAsync(Stream stream)
    {
        try
        {
            IniDocument model = await ParseAsync(stream);
            return (true, model);
        }
        catch
        {
            return (false, null);
        }
    }

    public IniDocument ParseFromString(string iniContent)
    {
        // Rimuovi BOM se presente
        if (!string.IsNullOrEmpty(iniContent) && iniContent[0] == '\uFEFF')
            iniContent = iniContent[1..];
        List<IniSection> sections = [];
        Dictionary<string, string>? currentKeyValues = null;
        string? currentSection = null;
        string[] separator = ["\r\n", "\n", "\r"];

        string[] lines = iniContent.Split(separator, StringSplitOptions.None);
        int lineNumber = 0;
        foreach (string rawLine in lines)
        {
            lineNumber++;
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';') || line.StartsWith('#'))
                continue;
            if (line.StartsWith('['))
            {
                int closeIdx = line.IndexOf(']');
                if (closeIdx == -1)
                    throw new FormatException($"Section header not closed at line {lineNumber}: '{rawLine}'");
                string sectionName = line[1..closeIdx].Trim();
                if (string.IsNullOrWhiteSpace(sectionName))
                    throw new FormatException($"Section name is empty or whitespace at line {lineNumber}: '{rawLine}'");
                if (currentSection != null && currentKeyValues != null)
                    sections.Add(new IniSection(currentSection, new Dictionary<string, string>(currentKeyValues)));
                currentSection = sectionName;
                currentKeyValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            else if (currentKeyValues != null)
            {
                int idx = line.IndexOf('=');
                if (idx < 0)
                    continue;
                string key = line[..idx].Trim();
                string value = line[(idx + 1)..].Trim();
                if (key.Contains('\n') || key.Contains('\r'))
                    throw new FormatException($"Key contains newline at line {lineNumber}: '{rawLine}'");
                if (value.Contains('\n') || value.Contains('\r'))
                    throw new FormatException($"Value contains newline at line {lineNumber}: '{rawLine}'");
                if (key.Length == 0)
                    continue; // ignore lines with only = and no key
                currentKeyValues[key] = value;
            }
        }

        if (currentSection != null && currentKeyValues != null)
            sections.Add(new IniSection(currentSection, new Dictionary<string, string>(currentKeyValues)));
        return new IniDocument(sections);
    }

    public bool TryParseFromString(string iniContent, out IniDocument iniModel, out string? errorMessage)
    {
        try
        {
            iniModel = ParseFromString(iniContent);
            errorMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            iniModel = new IniDocument([]);
            errorMessage = $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            return false;
        }
    }

    public bool TryParseFromString(string iniContent, out IniDocument iniModel)
    {
        return TryParseFromString(iniContent, out iniModel, out _);
    }

    public async Task<IniDocument> ParseFromStringAsync(string iniContent)
    {
        return await Task.Run(() => ParseFromString(iniContent));
    }

    public async Task<(bool Success, IniDocument? Model)> TryParseFromStringAsync(string iniContent)
    {
        try
        {
            IniDocument model = await ParseFromStringAsync(iniContent);
            return (true, model);
        }
        catch
        {
            return (false, null);
        }
    }

    public string Serialize(IniDocument model)
    {
        StringBuilder sb = new();
        foreach (IniSection section in model.Sections)
        {
            sb.AppendLine($"[{section.Name}]");
            foreach (var kv in section.KeyValues)
                sb.AppendLine($"{kv.Key}={kv.Value}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public async Task<string> SerializeAsync(IniDocument model)
    {
        return await Task.Run(() => Serialize(model));
    }
}