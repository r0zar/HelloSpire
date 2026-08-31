using BaseLib.Abstracts;
using HelloSpire.HelloSpireCode.Extensions;
using Godot;

namespace HelloSpire.HelloSpireCode.Characters;

public class AlchemistCardPool : CustomCardPoolModel
{
    public override Godot.Color ShaderColor => Alchemist.Color;
    public override string Title => Alchemist.CharacterId; //Not a display name.

    public override string BigEnergyIconPath => "big_energy.png".CharacterUiPath(Alchemist.AssetFolder);
    public override string TextEnergyIconPath => "text_energy.png".CharacterUiPath(Alchemist.AssetFolder);

    public override Color DeckEntryCardColor => Alchemist.Color;

    public override bool IsColorless => false;
}
