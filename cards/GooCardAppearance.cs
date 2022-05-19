using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class GooDiscCardAppearance : EmissiveDiscBorderBase
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        protected override Color EmissionColor { get { return GameColors.Instance.limeGreen; } }

        static GooDiscCardAppearance()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "GooDiscCardAppearance", typeof(GooDiscCardAppearance)).Id;
        }
    }
}