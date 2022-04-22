using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class RareDiscCardAppearance : EmissiveDiscBorderBase
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        protected override Color EmissionColor { get { return GameColors.Instance.darkGold; } }

        static RareDiscCardAppearance()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "RareDiscAppearance", typeof(RareDiscCardAppearance)).Id;
        }
    }
}