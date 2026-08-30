using BaseLib.Abstracts;
using HelloSpire.HelloSpireCode.Extensions;
using Godot;

namespace HelloSpire.HelloSpireCode.Characters;

public class PaladinCardPool : CustomCardPoolModel
{
    public override string Title => Paladin.CharacterId; //Not a display name.

    public override string BigEnergyIconPath => "big_energy.png".CharacterUiPath(Paladin.AssetFolder);
    public override string TextEnergyIconPath => "text_energy.png".CharacterUiPath(Paladin.AssetFolder);

    // The card frame is a shader tint over the base (Ironclad-red) frame. ShaderColor drives it
    // straight from the character colour so frame, name and relic outlines all match.
    public override Color ShaderColor => Paladin.Color;

    public override Color DeckEntryCardColor => Paladin.Color;

    public override bool IsColorless => false;
}
