using BaseLib.Abstracts;
using HelloSpire.HelloSpireCode.Extensions;
using Godot;

namespace HelloSpire.HelloSpireCode.Characters;

public class AlchemistPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Alchemist.Color;

    public override string BigEnergyIconPath => "big_energy.png".CharacterUiPath(Alchemist.AssetFolder);
    public override string TextEnergyIconPath => "text_energy.png".CharacterUiPath(Alchemist.AssetFolder);
}
