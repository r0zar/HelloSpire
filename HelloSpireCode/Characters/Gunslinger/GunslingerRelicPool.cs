using BaseLib.Abstracts;
using HelloSpire.HelloSpireCode.Extensions;
using Godot;

namespace HelloSpire.HelloSpireCode.Characters;

public class GunslingerRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => Gunslinger.Color;

    public override string BigEnergyIconPath => "big_energy.png".CharacterUiPath(Gunslinger.AssetFolder);
    public override string TextEnergyIconPath => "text_energy.png".CharacterUiPath(Gunslinger.AssetFolder);
}
