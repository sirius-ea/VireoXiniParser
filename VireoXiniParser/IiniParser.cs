using VireoXiniParser.Model;

namespace VireoXiniParser;

/// <summary>
/// Interface for parsing and serializing INI files and content.
/// </summary>
public interface IIniParser
{
    /// <summary>
    /// Parses an INI document from the provided stream.
    /// </summary>
    /// <param name="stream">The stream containing INI data.</param>
    /// <returns>The parsed <see cref="IniDocument"/>.</returns>
    IniDocument Parse(Stream stream);

    /// <summary>
    /// Parses an INI document from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the INI file.</param>
    /// <returns>The parsed <see cref="IniDocument"/>.</returns>
    IniDocument Parse(string filePath);

    /// <summary>
    /// Asynchronously parses an INI document from the provided stream.
    /// </summary>
    /// <param name="stream">The stream containing INI data.</param>
    /// <returns>A task representing the asynchronous operation, with the parsed <see cref="IniDocument"/> as result.</returns>
    Task<IniDocument> ParseAsync(Stream stream);

    /// <summary>
    /// Asynchronously parses an INI document from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the INI file.</param>
    /// <returns>A task representing the asynchronous operation, with the parsed <see cref="IniDocument"/> as result.</returns>
    Task<IniDocument> ParseAsync(string filePath);

    /// <summary>
    /// Parses an INI document from a string containing INI content.
    /// </summary>
    /// <param name="iniContent">The string containing INI data.</param>
    /// <returns>The parsed <see cref="IniDocument"/>.</returns>
    IniDocument ParseFromString(string iniContent);

    /// <summary>
    /// Asynchronously parses an INI document from a string containing INI content.
    /// </summary>
    /// <param name="iniContent">The string containing INI data.</param>
    /// <returns>A task representing the asynchronous operation, with the parsed <see cref="IniDocument"/> as result.</returns>
    Task<IniDocument> ParseFromStringAsync(string iniContent);

    /// <summary>
    /// Serializes the specified <see cref="IniDocument"/> to a string.
    /// </summary>
    /// <param name="model">The <see cref="IniDocument"/> to serialize.</param>
    /// <returns>A string representation of the INI document.</returns>
    string Serialize(IniDocument model);

    /// <summary>
    /// Asynchronously serializes the specified <see cref="IniDocument"/> to a string.
    /// </summary>
    /// <param name="model">The <see cref="IniDocument"/> to serialize.</param>
    /// <returns>A task representing the asynchronous operation, with the serialized string as result.</returns>
    Task<string> SerializeAsync(IniDocument model);

    /// <summary>
    /// Tries to parse an INI document from the provided stream.
    /// </summary>
    /// <param name="stream">The stream containing INI data.</param>
    /// <param name="iniModel">When this method returns, contains the parsed <see cref="IniDocument"/>, if parsing succeeded; otherwise, null.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    bool TryParse(Stream stream, out IniDocument iniModel);

    /// <summary>
    /// Tries to parse an INI document from the provided stream, returning an error message if parsing fails.
    /// </summary>
    /// <param name="stream">The stream containing INI data.</param>
    /// <param name="iniModel">When this method returns, contains the parsed <see cref="IniDocument"/>, if parsing succeeded; otherwise, null.</param>
    /// <param name="errorMessage">When this method returns, contains the error message if parsing failed; otherwise, null.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    bool TryParse(Stream stream, out IniDocument iniModel, out string? errorMessage);

    /// <summary>
    /// Tries to parse an INI document from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the INI file.</param>
    /// <param name="iniModel">When this method returns, contains the parsed <see cref="IniDocument"/>, if parsing succeeded; otherwise, null.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    bool TryParse(string filePath, out IniDocument iniModel);

    /// <summary>
    /// Tries to parse an INI document from the specified file path, returning an error message if parsing fails.
    /// </summary>
    /// <param name="filePath">The path to the INI file.</param>
    /// <param name="iniModel">When this method returns, contains the parsed <see cref="IniDocument"/>, if parsing succeeded; otherwise, null.</param>
    /// <param name="errorMessage">When this method returns, contains the error message if parsing failed; otherwise, null.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    bool TryParse(string filePath, out IniDocument iniModel, out string? errorMessage);

    /// <summary>
    /// Asynchronously tries to parse an INI document from the provided stream.
    /// </summary>
    /// <param name="stream">The stream containing INI data.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with a tuple containing a boolean indicating success and the parsed <see cref="IniDocument"/> if successful.
    /// </returns>
    Task<(bool Success, IniDocument? Model)> TryParseAsync(Stream stream);

    /// <summary>
    /// Tries to parse an INI document from a string containing INI content.
    /// </summary>
    /// <param name="iniContent">The string containing INI data.</param>
    /// <param name="iniModel">When this method returns, contains the parsed <see cref="IniDocument"/>, if parsing succeeded; otherwise, null.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    bool TryParseFromString(string iniContent, out IniDocument iniModel);

    /// <summary>
    /// Tries to parse an INI document from a string containing INI content, returning an error message if parsing fails.
    /// </summary>
    /// <param name="iniContent">The string containing INI data.</param>
    /// <param name="iniModel">When this method returns, contains the parsed <see cref="IniDocument"/>, if parsing succeeded; otherwise, null.</param>
    /// <param name="errorMessage">When this method returns, contains the error message if parsing failed; otherwise, null.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    bool TryParseFromString(string iniContent, out IniDocument iniModel, out string? errorMessage);

    /// <summary>
    /// Asynchronously tries to parse an INI document from a string containing INI content.
    /// </summary>
    /// <param name="iniContent">The string containing INI data.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with a tuple containing a boolean indicating success and the parsed <see cref="IniDocument"/> if successful.
    /// </returns>
    Task<(bool Success, IniDocument? Model)> TryParseFromStringAsync(string iniContent);
}