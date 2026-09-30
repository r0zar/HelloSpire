using System.Text.Json;
using Xunit;

namespace HelloSpire.Tests;

/// <summary>
/// Reads localization JSON directly off disk rather than referencing HelloSpire.csproj -- that
/// project requires the game's own assemblies (0Harmony.dll, sts2.dll) via Sts2PathDiscovery.props,
/// which aren't available outside a machine with Slay the Spire 2 installed. Testing the shipped
/// data files directly keeps this project buildable and runnable anywhere.
/// </summary>
public class LocalizationTests
{
    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "HelloSpire.sln")))
            dir = dir.Parent;
        if (dir == null)
            throw new InvalidOperationException(
                $"Could not locate repo root (HelloSpire.sln) above {AppContext.BaseDirectory}");
        return dir.FullName;
    }

    private static JsonElement LoadHoverTips()
    {
        var path = Path.Combine(RepoRoot(), "HelloSpire", "localization", "eng", "static_hover_tips.json");
        var json = File.ReadAllText(path);
        return JsonDocument.Parse(json).RootElement;
    }

    [Fact]
    public void StaticHoverTips_IsValidJson()
    {
        var root = LoadHoverTips();
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
    }

    /// <summary>
    /// Regression test for issue #13: the Gadget tooltip previously said only that Gadgets are
    /// "worth the same" with the Cylinder empty or full, without ever stating that Gadget is its
    /// own track alongside the Cylinder rather than a Cylinder mechanic. Players reported this
    /// reading as if Gadgets were somehow tied to ammo state.
    /// </summary>
    [Fact]
    public void Gadget_Tooltip_ClarifiesItIsASeparateTrackFromTheCylinder()
    {
        var root = LoadHoverTips();
        var description = root.GetProperty("HELLOSPIRE-GADGET.description").GetString();

        Assert.NotNull(description);
        Assert.Contains("own track", description);
        Assert.Contains("[gold]Loads[/gold]", description);
        Assert.Contains("[gold]Fires[/gold]", description);
        Assert.Contains("[gold]Cycles[/gold]", description);
        Assert.Contains("[gold]Spins[/gold]", description);
    }
}
