using BaseLib.Abstracts;
using HelloSpire.HelloSpireCode.Extensions;
using Godot;

namespace HelloSpire.HelloSpireCode.Character;

public class HelloSpirePotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => TheGunslinger.Color;
    

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}