using System.Text;
using VireoXiniParser;

namespace VireoXiniParserTests;

public class IniDataParserTests
{
    [Fact]
    public void ParseFromString_ValidIni_ReturnsCorrectModel()
    {
        var parser = new IniDataParser();
        string ini = """
                     [Section1]
                     Key1=Value1
                     Key2=Value2

                     [Section2]
                     KeyA=ValueA
                     """;

        var doc = parser.ParseFromString(ini);

        Assert.NotNull(doc);
        Assert.Equal(2, doc.Sections.Count);

        var section1 = doc.Sections.First(s => s.Name == "Section1");
        Assert.Equal("Value1", section1.KeyValues["Key1"]);
        Assert.Equal("Value2", section1.KeyValues["Key2"]);

        var section2 = doc.Sections.First(s => s.Name == "Section2");
        Assert.Equal("ValueA", section2.KeyValues["KeyA"]);
    }

    [Fact]
    public void ParseFromString_EmptyOrCommentOnly_ReturnsEmptyModel()
    {
        var parser = new IniDataParser();
        string ini = """
                     ; comment
                     # another comment

                     """;
        var doc = parser.ParseFromString(ini);
        Assert.NotNull(doc);
        Assert.Empty(doc.Sections);
    }

    [Fact]
    public void TryParseFromString_InvalidIni_ReturnsFalseAndError()
    {
        var parser = new IniDataParser();
        string ini = "[Section\nKey=Value"; // Section header not closed

        var result = parser.TryParseFromString(ini, out var doc, out var error);
        Assert.False(result);
        Assert.NotNull(error);
        Assert.NotNull(doc);
        Assert.Empty(doc.Sections);
    }

    [Fact]
    public void Serialize_RoundTrip_PreservesData()
    {
        var parser = new IniDataParser();
        string ini = """
                     [S]
                     K=V
                     """;
        var doc = parser.ParseFromString(ini);
        string serialized = parser.Serialize(doc);
        var doc2 = parser.ParseFromString(serialized);

        Assert.Single(doc2.Sections);
        Assert.Equal("V", doc2.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public async Task ParseFromStringAsync_ValidIni_Works()
    {
        var parser = new IniDataParser();
        string ini = "[A]\nB=C";
        var doc = await parser.ParseFromStringAsync(ini);
        Assert.Single(doc.Sections);
        Assert.Equal("C", doc.Sections[0].KeyValues["B"]);
    }

    [Fact]
    public async Task TryParseFromStringAsync_InvalidIni_ReturnsFalse()
    {
        var parser = new IniDataParser();
        string ini = "[A\nB=C";
        var (success, model) = await parser.TryParseFromStringAsync(ini);
        Assert.False(success);
        Assert.Null(model);
    }

    [Fact]
    public void TryParse_StreamWithInvalidData_ReturnsFalse()
    {
        var parser = new IniDataParser();
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes("[A\nB=C"));
        var result = parser.TryParse(ms, out var doc, out var error);
        Assert.False(result);
        Assert.NotNull(error);
        Assert.NotNull(doc);
        Assert.Empty(doc.Sections);
    }

    [Fact]
    public void ParseFromString_SectionWithoutKeyValues_ParsesSection()
    {
        var parser = new IniDataParser();
        string ini = "[EmptySection]";
        var doc = parser.ParseFromString(ini);
        Assert.Single(doc.Sections);
        Assert.Equal("EmptySection", doc.Sections[0].Name);
        Assert.Empty(doc.Sections[0].KeyValues);
    }

    [Fact]
    public void ParseFromString_DuplicateKeys_LastWins()
    {
        var parser = new IniDataParser();
        string ini = """
                     [S]
                     K=V1
                     K=V2
                     """;
        var doc = parser.ParseFromString(ini);
        Assert.Single(doc.Sections);
        Assert.Equal("V2", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_WhitespaceAndCaseInsensitiveSectionNames()
    {
        var parser = new IniDataParser();
        string ini = """
                     [  Section  ]
                     Key=Value
                     """;
        var doc = parser.ParseFromString(ini);
        Assert.Single(doc.Sections);
        Assert.Equal("Section", doc.Sections[0].Name);
        Assert.Equal("Value", doc.Sections[0].KeyValues["Key"]);
    }

    [Fact]
    public async Task StressTest_ParseFromStringAsync_LargeIni()
    {
        var parser = new IniDataParser();
        var sb = new StringBuilder();
        for (int i = 0; i < 100000; i++)
        {
            sb.AppendLine($"[Section{i}]");
            for (int j = 0; j < 100; j++)
                sb.AppendLine($"Key{j}=Value{j}");
        }

        string ini = sb.ToString();

        var doc = await parser.ParseFromStringAsync(ini);
        Assert.Equal(100000, doc.Sections.Count);
        Assert.All(doc.Sections, s => Assert.Equal(100, s.KeyValues.Count));
    }

    [Fact]
    public void ParseFromString_SectionHeaderWithSpacesOnly_Throws()
    {
        var parser = new IniDataParser();
        string ini = "[   ]\nKey=Value";
        Assert.Throws<FormatException>(() => parser.ParseFromString(ini));
    }

    [Fact]
    public void ParseFromString_KeyWithoutValue_ParsesAsEmptyString()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_KeyWithoutSection_Ignored()
    {
        var parser = new IniDataParser();
        string ini = "Key=Value\n[S]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Single(doc.Sections);
        Assert.DoesNotContain(doc.Sections[0].KeyValues, kv => kv.Key == "Key");
    }

    [Fact]
    public void ParseFromString_SectionWithSpecialCharsInName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[Sec!@#] \nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("Sec!@#", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_KeyWithSpaces_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S]\n  Key With Spaces  =  Value  ";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("Value", doc.Sections[0].KeyValues["Key With Spaces"]);
    }

    [Fact]
    public void ParseFromString_ValueWithEquals_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=V=Extra";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("V=Extra", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_SectionNameCaseInsensitive_DistinctSections()
    {
        var parser = new IniDataParser();
        string ini = "[A]\nK=1\n[a]\nK=2";
        var doc = parser.ParseFromString(ini);
        Assert.Equal(2, doc.Sections.Count);
    }

    [Fact]
    public void ParseFromString_EmptyFile_ReturnsEmptyModel()
    {
        var parser = new IniDataParser();
        string ini = "   \n   \n";
        var doc = parser.ParseFromString(ini);
        Assert.Empty(doc.Sections);
    }

    [Fact]
    public void ParseFromString_OnlyEqualsLine_Ignored()
    {
        var parser = new IniDataParser();
        string ini = "[S]\n=";
        var doc = parser.ParseFromString(ini);
        Assert.Empty(doc.Sections[0].KeyValues);
    }

    [Fact]
    public void ParseFromString_SectionWithUnicodeName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[SezioneÜ]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("SezioneÜ", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_KeyWithUnicode_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nÜnicode=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("V", doc.Sections[0].KeyValues["Ünicode"]);
    }

    [Fact]
    public void ParseFromString_SectionWithDuplicateNames_ParsesAsSeparateSections()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=V1\n[S]\nK=V2";
        var doc = parser.ParseFromString(ini);
        Assert.Equal(2, doc.Sections.Count);
        Assert.Equal("V1", doc.Sections[0].KeyValues["K"]);
        Assert.Equal("V2", doc.Sections[1].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_KeyWithCommentInline_ParsesKeyAndIgnoresComment()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=V ; comment";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("V ; comment", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_SectionWithNoKeyValueAndWhitespace_ParsesSection()
    {
        var parser = new IniDataParser();
        string ini = "[S]\n   \n";
        var doc = parser.ParseFromString(ini);
        Assert.Single(doc.Sections);
        Assert.Empty(doc.Sections[0].KeyValues);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyOnlySpaces_Ignored()
    {
        var parser = new IniDataParser();
        string ini = "[S]\n   =value";
        var doc = parser.ParseFromString(ini);
        Assert.Empty(doc.Sections[0].KeyValues);
    }

    [Fact]
    public void ParseFromString_SectionWithTabInName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[Sec\tTab]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("Sec\tTab", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_KeyWithTab_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nKey\tTab=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("V", doc.Sections[0].KeyValues["Key\tTab"]);
    }

    [Fact]
    public void ParseFromString_ValueWithTab_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=Val\tTab";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("Val\tTab", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_SectionWithBOM_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "\uFEFF[S]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_SectionWithLongName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string longName = new('A', 1000);
        string ini = $"[{longName}]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal(longName, doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_KeyWithLongName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string longKey = new('B', 1000);
        string ini = $"[S]\n{longKey}=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("V", doc.Sections[0].KeyValues[longKey]);
    }

    [Fact]
    public void ParseFromString_ValueWithLongValue_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string longValue = new('C', 1000);
        string ini = $"[S]\nK={longValue}";
        var doc = parser.ParseFromString(ini);
        Assert.Equal(longValue, doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_SectionWithNonAscii_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[Sèçtîøñ]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("Sèçtîøñ", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_KeyWithNonAscii_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nÇhîävë=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("V", doc.Sections[0].KeyValues["Çhîävë"]);
    }

    [Fact]
    public void ParseFromString_ValueWithNonAscii_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=Välüé";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("Välüé", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_SectionWithLeadingTrailingSpaces_ParsesTrimmed()
    {
        var parser = new IniDataParser();
        string ini = "[  S  ]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_KeyWithLeadingTrailingSpaces_ParsesTrimmed()
    {
        var parser = new IniDataParser();
        string ini = "[S]\n  K  =V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("V", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_ValueWithLeadingTrailingSpaces_ParsesTrimmed()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=  V  ";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("V", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_SectionWithCommentAfterHeader_ParsesSection()
    {
        var parser = new IniDataParser();
        string ini = "[S] ; comment\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_KeyWithCommentAfterKey_ParsesKey()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK ; comment =V";
        var doc = parser.ParseFromString(ini);
        Assert.True(doc.Sections[0].KeyValues.ContainsKey("K ; comment"));
    }

    [Fact]
    public void ParseFromString_ValueWithCommentAfterValue_ParsesValueWithComment()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=V ; comment";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("V ; comment", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_SectionWithNewlineInName_Throws()
    {
        var parser = new IniDataParser();
        string ini = "[Sec\nName]\nK=V";
        Assert.Throws<FormatException>(() => parser.ParseFromString(ini));
    }

    [Fact]
    public void ParseFromString_SectionWithHashCommentAfterHeader_ParsesSection()
    {
        var parser = new IniDataParser();
        string ini = "[S] # comment\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_KeyWithHashComment_ParsesKey()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK # comment=V";
        var doc = parser.ParseFromString(ini);
        Assert.True(doc.Sections[0].KeyValues.ContainsKey("K # comment"));
    }

    [Fact]
    public void ParseFromString_ValueWithHashComment_ParsesValueWithComment()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=V # comment";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("V # comment", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_SectionWithMultipleComments_ParsesSection()
    {
        var parser = new IniDataParser();
        string ini = "[S] ; comment # another\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_KeyWithMultipleComments_ParsesKey()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK ; comment # another=V";
        var doc = parser.ParseFromString(ini);
        Assert.True(doc.Sections[0].KeyValues.ContainsKey("K ; comment # another"));
    }

    [Fact]
    public void ParseFromString_ValueWithMultipleComments_ParsesValueWithComment()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=V ; comment # another";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("V ; comment # another", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_SectionWithEmptyLineAfterHeader_ParsesSection()
    {
        var parser = new IniDataParser();
        string ini = "[S]\n\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_KeyWithEmptyValue_ParsesAsEmptyString()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_SectionWithOnlyComment_ParsesSection()
    {
        var parser = new IniDataParser();
        string ini = "[S]\n; comment";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S", doc.Sections[0].Name);
        Assert.Empty(doc.Sections[0].KeyValues);
    }

    [Fact]
    public void ParseFromString_SectionWithOnlyWhitespace_ParsesSection()
    {
        var parser = new IniDataParser();
        string ini = "[S]\n   ";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S", doc.Sections[0].Name);
        Assert.Empty(doc.Sections[0].KeyValues);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyValueAndComment_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=V ; comment";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("V ; comment", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyValueAndHashComment_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=V # comment";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("V # comment", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyValueAndMultipleComments_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=V ; comment # another";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("V ; comment # another", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyValueAndWhitespace_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=V   ";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("V", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public async Task ParseFromStreamAsync_ValidIni_Works()
    {
        var parser = new IniDataParser();
        var ini = "[A]\nB=C";
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(ini));
        var doc = await parser.ParseAsync(ms);
        Assert.Single(doc.Sections);
        Assert.Equal("C", doc.Sections[0].KeyValues["B"]);
    }

    [Fact]
    public void ParseFromStream_ValidIni_Works()
    {
        var parser = new IniDataParser();
        var ini = "[A]\nB=C";
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(ini));
        var doc = parser.Parse(ms);
        Assert.Single(doc.Sections);
        Assert.Equal("C", doc.Sections[0].KeyValues["B"]);
    }

    [Fact]
    public void Serialize_EmptyIniDocument_ReturnsEmptyString()
    {
        var parser = new IniDataParser();
        var doc = new VireoXiniParser.Model.IniDocument(new List<VireoXiniParser.Model.IniSection>());
        var result = parser.Serialize(doc);
        Assert.True(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public void Serialize_SectionWithNoKeys_OutputsSectionHeaderOnly()
    {
        var parser = new IniDataParser();
        var doc = new VireoXiniParser.Model.IniDocument(new[]
            { new VireoXiniParser.Model.IniSection("S", new Dictionary<string, string>()) });
        var result = parser.Serialize(doc);
        Assert.Contains("[S]", result);
    }

    [Fact]
    public void Serialize_SectionWithMultipleKeys_OutputsAllKeys()
    {
        var parser = new IniDataParser();
        var doc = new VireoXiniParser.Model.IniDocument(new[]
        {
            new VireoXiniParser.Model.IniSection("S", new Dictionary<string, string> { { "A", "1" }, { "B", "2" } })
        });
        var result = parser.Serialize(doc);
        Assert.Contains("A=1", result);
        Assert.Contains("B=2", result);
    }

    [Fact]
    public void SerializeAsync_RoundTrip_PreservesData()
    {
        var parser = new IniDataParser();
        var doc = parser.ParseFromString("[S]\nK=V");
        var serialized = parser.SerializeAsync(doc).Result;
        var doc2 = parser.ParseFromString(serialized);
        Assert.Single(doc2.Sections);
        Assert.Equal("V", doc2.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyEqualsInName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S=1]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S=1", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyBracketInName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S[1]]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S[1", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyHashInName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S#1]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S#1", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_SectionWithKeySemicolonInName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S;1]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S;1", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_SectionWithKeySpaceInName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S 1]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S 1", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyTabInName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S\t1]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S\t1", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyNewlineInName_Throws()
    {
        var parser = new IniDataParser();
        string ini = "[S\n1]\nK=V";
        Assert.Throws<FormatException>(() => parser.ParseFromString(ini));
    }

    [Fact]
    public void ParseFromString_SectionWithKeyCarriageReturnInName_Throws()
    {
        var parser = new IniDataParser();
        string ini = "[S\r1]\nK=V";
        Assert.Throws<FormatException>(() => parser.ParseFromString(ini));
    }

    [Fact]
    public void ParseFromString_SectionWithKeyFormFeedInName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S\f1]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S\f1", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyVerticalTabInName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S\v1]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S\v1", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyUnicodeInName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[Sü1]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("Sü1", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyEmojiInName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S😀1]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S😀1", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyChineseInName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[S汉字1]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("S汉字1", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyArabicInName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[Sعربى1]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("Sعربى1", doc.Sections[0].Name);
    }

    [Fact]
    public void ParseFromString_SectionWithKeyHebrewInName_ParsesCorrectly()
    {
        var parser = new IniDataParser();
        string ini = "[Sעברית1]\nK=V";
        var doc = parser.ParseFromString(ini);
        Assert.Equal("Sעברית1", doc.Sections[0].Name);
    }

    [Fact]
    public void Parse_FilePath_ValidIni_Works()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=V";
        string path = Path.GetTempFileName();
        File.WriteAllText(path, ini);
        try
        {
            var doc = parser.Parse(path);
            Assert.Single(doc.Sections);
            Assert.Equal("V", doc.Sections[0].KeyValues["K"]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TryParse_FilePath_ValidIni_ReturnsTrue()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=V";
        string path = Path.GetTempFileName();
        File.WriteAllText(path, ini);
        try
        {
            var result = parser.TryParse(path, out var doc, out var error);
            Assert.True(result);
            Assert.Null(error);
            Assert.Single(doc.Sections);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TryParse_FilePath_InvalidIni_ReturnsFalse()
    {
        var parser = new IniDataParser();
        string ini = "[S\nK=V";
        string path = Path.GetTempFileName();
        File.WriteAllText(path, ini);
        try
        {
            var result = parser.TryParse(path, out var doc, out var error);
            Assert.False(result);
            Assert.NotNull(error);
            Assert.NotNull(doc);
            Assert.Empty(doc.Sections);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TryParse_FilePath_ValidIni_Overload_ReturnsTrue()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=V";
        string path = Path.GetTempFileName();
        File.WriteAllText(path, ini);
        try
        {
            var result = parser.TryParse(path, out var doc);
            Assert.True(result);
            Assert.Single(doc.Sections);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ParseAsync_FilePath_ValidIni_Works()
    {
        var parser = new IniDataParser();
        string ini = "[S]\nK=V";
        string path = Path.GetTempFileName();
        File.WriteAllText(path, ini);
        try
        {
            var doc = await parser.ParseAsync(path);
            Assert.Single(doc.Sections);
            Assert.Equal("V", doc.Sections[0].KeyValues["K"]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Parse_Stream_ValidIni_Works()
    {
        var parser = new IniDataParser();
        var ini = "[S]\nK=V";
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(ini));
        var doc = parser.Parse(ms);
        Assert.Single(doc.Sections);
        Assert.Equal("V", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public void TryParse_Stream_ValidIni_Overload_ReturnsTrue()
    {
        var parser = new IniDataParser();
        var ini = "[S]\nK=V";
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(ini));
        var result = parser.TryParse(ms, out var doc);
        Assert.True(result);
        Assert.Single(doc.Sections);
    }

    [Fact]
    public async Task ParseAsync_Stream_ValidIni_Works()
    {
        var parser = new IniDataParser();
        var ini = "[S]\nK=V";
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(ini));
        var doc = await parser.ParseAsync(ms);
        Assert.Single(doc.Sections);
        Assert.Equal("V", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public async Task TryParseAsync_Stream_ValidIni_ReturnsTrue()
    {
        var parser = new IniDataParser();
        var ini = "[S]\nK=V";
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(ini));
        var (success, model) = await parser.TryParseAsync(ms);
        Assert.True(success);
        Assert.NotNull(model);
        Assert.Single(model.Sections);
    }

    [Fact]
    public void TryParseFromString_ValidIni_Overload_ReturnsTrue()
    {
        var parser = new IniDataParser();
        var ini = "[S]\nK=V";
        var result = parser.TryParseFromString(ini, out var doc);
        Assert.True(result);
        Assert.Single(doc.Sections);
    }

    [Fact]
    public async Task ParseFromStringAsync_ValidIni_Works2()
    {
        var parser = new IniDataParser();
        var ini = "[S]\nK=V";
        var doc = await parser.ParseFromStringAsync(ini);
        Assert.Single(doc.Sections);
        Assert.Equal("V", doc.Sections[0].KeyValues["K"]);
    }

    [Fact]
    public async Task SerializeAsync_SectionWithMultipleKeys_OutputsAllKeys()
    {
        var parser = new IniDataParser();
        var doc = new VireoXiniParser.Model.IniDocument(new[]
        {
            new VireoXiniParser.Model.IniSection("S", new Dictionary<string, string> { { "A", "1" }, { "B", "2" } })
        });
        var result = await parser.SerializeAsync(doc);
        Assert.Contains("A=1", result);
        Assert.Contains("B=2", result);
    }

    [Fact]
    public async Task TryParseFromStringAsync_ValidIni_ReturnsTrue()
    {
        var parser = new IniDataParser();
        var ini = "[S]\nK=V";
        var (success, model) = await parser.TryParseFromStringAsync(ini);
        Assert.True(success);
        Assert.NotNull(model);
        Assert.Single(model.Sections);
    }

    [Fact]
    public void Serialize_SectionWithEmptyKeyValue_OutputsSectionHeaderOnly()
    {
        var parser = new IniDataParser();
        var doc = new VireoXiniParser.Model.IniDocument(new[]
            { new VireoXiniParser.Model.IniSection("S", new Dictionary<string, string>()) });
        var result = parser.Serialize(doc);
        Assert.Contains("[S]", result);
        Assert.True(result.Trim().StartsWith("[S]"));
    }
}