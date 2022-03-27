using DiskCardGame;
using InscryptionAPI.Card;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class ForceRevolverAppearance : CardAppearanceBehaviour
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        public override void ApplyAppearance()
        {
			if (base.Card.Anim is DiskCardAnimationController dac)
				dac.SetWeaponMesh(DiskCardWeapon.Revolver);
        }

        public static void Register()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "ForceRevolverAppearance", typeof(ForceRevolverAppearance)).Id;
        }
    }
}