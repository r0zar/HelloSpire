using BaseLib.Abstracts;
using HelloSpire.HelloSpireCode.Extensions;
using Godot;

namespace HelloSpire.HelloSpireCode.Characters;

public class GunslingerCardPool : CustomCardPoolModel
{
    public override string Title => Gunslinger.CharacterId; //Not a display name.

    public override string BigEnergyIconPath => "big_energy.png".CharacterUiPath(Gunslinger.AssetFolder);
    public override string TextEnergyIconPath => "text_energy.png".CharacterUiPath(Gunslinger.AssetFolder);

    // TODO(Phase 1): tune the card-back tint for this character.
    public override float H => 1f;
    public override float S => 1f;
    public override float V => 1f;

    public override Color DeckEntryCardColor => Gunslinger.Color;

    public override bool IsColorless => false;
}
