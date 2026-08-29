using BaseLib.Abstracts;
using HelloSpire.HelloSpireCode.Extensions;
using Godot;

namespace HelloSpire.HelloSpireCode.Character;

public class HelloSpireCardPool : CustomCardPoolModel
{
    public override string Title => TheGunslinger.CharacterId; //This is not a display name.
    
    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();


    /* The card back is a shader tint over a base frame. Driving it from the character's colour keeps
       cards, name and relic outlines matching; override H/S/V individually if the tint needs
       hand-tuning once the real frame art exists. */
    public override Color ShaderColor => TheGunslinger.Color;
    
    //Alternatively, leave these values at 1 and provide a custom frame image.
    /*public override Texture2D CustomFrame(CustomCardModel card)
    {
        //This will attempt to load HelloSpire/images/cards/frame.png
        return PreloadManager.Cache.GetTexture2D("cards/frame.png".ImagePath());
    }*/

    //Color of small card icons
    public override Color DeckEntryCardColor => TheGunslinger.Color;
    
    public override bool IsColorless => false;
}