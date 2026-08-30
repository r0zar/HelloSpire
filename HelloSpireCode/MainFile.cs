using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace HelloSpire.HelloSpireCode;

//You're recommended but not required to keep all your code in this package and all your assets in the HelloSpire folder.
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "HelloSpire"; //Used for resource filepath
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        //If you want to use scripts defined in your mod for Godot scenes, uncomment the following line.
        //Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(assembly);
     
        // The Alchemist's bench talks to the game through this bridge; without it the potion,
        // Gold, Max HP and choice systems all no-op (see LabBridge).
        HelloSpireCode.Alchemist.LabBridge.Current = new HelloSpireCode.Alchemist.WiredLabBridge();

        Harmony harmony = new(ModId);

        harmony.PatchAll(assembly);
    }
}
