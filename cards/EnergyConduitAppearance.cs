using InscryptionAPI.Card;
using DiskCardGame;
using UnityEngine;
using InscryptionAPI.Helpers;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class EnergyConduitAppearnace : CardAppearanceBehaviour
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        private static Sprite[] PORTRAITS = new Sprite[] {
            TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("portrait_conduitenergy.png", typeof(EnergyConduitAppearnace).Assembly), TextureHelper.SpriteType.CardPortrait),
            TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("portrait_conduitenergy_1.png", typeof(EnergyConduitAppearnace).Assembly), TextureHelper.SpriteType.CardPortrait),
            TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("portrait_conduitenergy_2.png", typeof(EnergyConduitAppearnace).Assembly), TextureHelper.SpriteType.CardPortrait),
            TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("portrait_conduitenergy_3.png", typeof(EnergyConduitAppearnace).Assembly), TextureHelper.SpriteType.CardPortrait)
        };

        public override void ApplyAppearance()
        {
            if (this.Card is PlayableCard pCard)
            {
                if (pCard.slot == null)
                    return;

                NewConduitEnergy behaviour = this.Card.GetComponent<NewConduitEnergy>();
                if (behaviour == null)
                    return;

                if (!behaviour.CompletesCircuit())
                    pCard.renderInfo.portraitOverride = PORTRAITS[0];
                else
                    pCard.renderInfo.portraitOverride = PORTRAITS[behaviour.RemainingEnergy];
            }
        }

        public override void OnPreRenderCard()
        {
            this.ApplyAppearance();
        }

        internal static void Register()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "EnergyConduitAppearance", typeof(EnergyConduitAppearnace)).Id;
        }
    }
}