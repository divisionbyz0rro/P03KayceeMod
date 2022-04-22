using DiskCardGame;
using InscryptionAPI.Card;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class GoldPortrait : CardAppearanceBehaviour
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        public override void ApplyAppearance()
        {
            P03Plugin.Log.LogDebug("Gold appearance");
			this.Card.RenderInfo.portraitColor = GameColors.Instance.gold;

            if (this.Card.StatsLayer is DiskRenderStatsLayer drsl)
                drsl.lightColor = GameColors.Instance.gold;
        }

        public override void OnPreRenderCard()
        {
            base.OnPreRenderCard();
            ApplyAppearance();
        }

        static GoldPortrait()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "GoldPortrait", typeof(GoldPortrait)).Id;
        }
    }
}