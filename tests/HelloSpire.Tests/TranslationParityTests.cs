using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace HelloSpire.Tests;

/// <summary>
/// Keeps every translation in step with the English string tables in HelloSpire/localization/eng.
///
/// English is the source of truth. A translation that misses a key shows the raw key in game, and
/// one that drops or misspells a {Variable} shows a blank or a formatter error, so both fail here.
/// Formatter arguments may differ ("{Cards:plural:card|cards}" in English, "{Cards:diff()}" in
/// Korean, which has no plural), so only the variable names are compared.
/// </summary>
public partial class TranslationParityTests
{
    /// <summary>The game's folder codes: esp is Spain Spanish, spa is Latin American Spanish.</summary>
    public static TheoryData<string> Languages => new() { "kor", "zhs", "jpn", "fra", "esp", "spa" };

    private static string LocRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "HelloSpire.sln")))
            dir = dir.Parent;
        if (dir == null)
            throw new InvalidOperationException(
                $"Could not locate repo root (HelloSpire.sln) above {AppContext.BaseDirectory}");
        return Path.Combine(dir.FullName, "HelloSpire", "localization");
    }

    private static string[] TableNames(string lang) =>
        Directory.GetFiles(Path.Combine(LocRoot(), lang), "*.json")
            .Select(Path.GetFileName).OfType<string>().Order().ToArray();

    private static Dictionary<string, string> LoadTable(string lang, string file)
    {
        var json = File.ReadAllText(Path.Combine(LocRoot(), lang, file));
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
               ?? throw new InvalidOperationException($"{lang}/{file} is not a string table");
    }

    [GeneratedRegex(@"\{([A-Za-z_][A-Za-z0-9_]*)")]
    private static partial Regex Variable();

    [GeneratedRegex(@"\[(/?)([a-z]+)(?:=[^\]]*)?\]")]
    private static partial Regex BbTag();

    private static SortedSet<string> Variables(string text) =>
        new(Variable().Matches(text).Select(m => m.Groups[1].Value));

    [Theory]
    [MemberData(nameof(Languages))]
    public void Translation_HasEveryEnglishTable(string lang)
    {
        Assert.Equal(TableNames("eng"), TableNames(lang));
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void Translation_HasExactlyTheEnglishKeys(string lang)
    {
        foreach (var file in TableNames("eng"))
        {
            var eng = LoadTable("eng", file);
            var tr = LoadTable(lang, file);
            Assert.True(eng.Keys.Except(tr.Keys).ToList() is [], $"{lang}/{file} is missing: " +
                string.Join(", ", eng.Keys.Except(tr.Keys)));
            Assert.True(tr.Keys.Except(eng.Keys).ToList() is [], $"{lang}/{file} has keys English lacks: " +
                string.Join(", ", tr.Keys.Except(eng.Keys)));
        }
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void Translation_UsesTheSameVariablesAsEnglish(string lang)
    {
        var mismatches = new List<string>();
        foreach (var file in TableNames("eng"))
        {
            var tr = LoadTable(lang, file);
            foreach (var (key, english) in LoadTable("eng", file))
            {
                if (!tr.TryGetValue(key, out var text)) continue;
                var want = Variables(english);
                var got = Variables(text);
                if (!want.SetEquals(got))
                    mismatches.Add($"{file} {key}: expected {{{string.Join(",", want)}}}, got {{{string.Join(",", got)}}}");
            }
        }
        Assert.True(mismatches.Count == 0, $"{lang}:\n" + string.Join("\n", mismatches));
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void Translation_BalancesItsMarkupTags(string lang)
    {
        var broken = new List<string>();
        foreach (var file in TableNames(lang))
        {
            foreach (var (key, text) in LoadTable(lang, file))
            {
                var open = new Stack<string>();
                var ok = true;
                foreach (Match m in BbTag().Matches(text))
                {
                    var name = m.Groups[2].Value;
                    if (m.Groups[1].Value == "") open.Push(name);
                    else if (!open.TryPop(out var top) || top != name) { ok = false; break; }
                }
                if (!ok || open.Count > 0) broken.Add($"{file} {key}: {text}");
            }
        }
        Assert.True(broken.Count == 0, $"{lang}:\n" + string.Join("\n", broken));
    }

    /// <summary>
    /// [gold] marks a keyword the player can hover. A translation may phrase a line differently, but
    /// it should never silently lose one of those highlights.
    /// </summary>
    [Theory]
    [MemberData(nameof(Languages))]
    public void Translation_KeepsEveryKeywordHighlight(string lang)
    {
        var dropped = new List<string>();
        foreach (var file in TableNames("eng"))
        {
            var tr = LoadTable(lang, file);
            foreach (var (key, english) in LoadTable("eng", file))
            {
                if (!tr.TryGetValue(key, out var text)) continue;
                if (Count(text, "[gold]") < Count(english, "[gold]"))
                    dropped.Add($"{file} {key}: {text}");
            }
        }
        Assert.True(dropped.Count == 0, $"{lang}:\n" + string.Join("\n", dropped));
    }

    private static int Count(string text, string tag) => (text.Length - text.Replace(tag, "").Length) / tag.Length;

    [Fact]
    public void English_BalancesItsMarkupTags() => Translation_BalancesItsMarkupTags("eng");
}
