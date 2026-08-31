using Godot;

namespace HelloSpire.HelloSpireCode.Extensions;

//Mostly utilities to get asset paths.
public static class StringExtensions
{
    public static string ImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", path);
    }

    public static string CardImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "card_portraits", path);
        if (ResourceLoader.Exists(path)) return path;
        
        MainFile.Logger.Info("Could not find card image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "card_portraits", "card.png");
    }

    public static string BigCardImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "card_portraits", "big", path);
        if (ResourceLoader.Exists(path)) return path;
        
        MainFile.Logger.Info("Could not find big card image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "card_portraits", "big", "card.png");
    }

    public static string PowerImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "powers", path);
        if (ResourceLoader.Exists(path)) return path;
        
        MainFile.Logger.Info("Could not find power image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "powers", "power.png");
    }

    public static string BigPowerImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "powers", "big", path);
        if (ResourceLoader.Exists(path)) return path;
        
        MainFile.Logger.Info("Could not find big power image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "powers", "big", "power.png");
    }

    public static string RelicImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "relics", path);
        if (ResourceLoader.Exists(path)) return path;
        
        MainFile.Logger.Info("Could not find relic image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "relics", "relic.png");
    }

    public static string BigRelicImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "relics", "big", path);
        if (ResourceLoader.Exists(path)) return path;
        
        MainFile.Logger.Info("Could not find big relic image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "relics", "big", "relic.png");
    }

    public static string PotionImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "potions", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find potion image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "potions", "potion.png");
    }

    public static string PotionOutlineImagePath(this string path)
    {
        // Outlines live in their own subfolder. This used to look in images/potions/, which is
        // where the filled art is -- so a potion with art got its own fill handed back as its
        // silhouette, and one without fell through to the default outline for the wrong reason.
        path = Path.Join(MainFile.ResPath, "images", "potions", "outline", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find potion outline image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "potions", "outline", "potion.png");
    }

    /// <summary>
    /// Per-character UI art: images/charui/&lt;character&gt;/&lt;path&gt;.
    /// Character UI is the one asset class that genuinely cannot be shared between
    /// characters, so it is the only tree namespaced by character. Cards, relics and
    /// potions resolve by class name, which is already unique mod-wide.
    /// </summary>
    public static string CharacterUiPath(this string path, string character)
    {
        return Path.Join(MainFile.ResPath, "images", "charui", character, path);
    }
}