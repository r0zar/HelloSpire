using BaseLib.Abstracts;
using HelloSpire.HelloSpireCode.Extensions;
using Godot;

namespace HelloSpire.HelloSpireCode.Characters;

public class PaladinPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Paladin.Color;

    public override string BigEnergyIconPath => "big_energy.png".CharacterUiPath(Paladin.AssetFolder);
    public override string TextEnergyIconPath => "text_energy.png".CharacterUiPath(Paladin.AssetFolder);
}
