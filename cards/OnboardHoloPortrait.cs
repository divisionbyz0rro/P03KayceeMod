using DiskCardGame;
using InscryptionAPI.Card;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class OnboardHoloPortrait : CardAppearanceBehaviour
    {
        private bool portraitSpawned = false;

        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        public override void ApplyAppearance()
		{
			if (base.Card.Anim is DiskCardAnimationController dcac && this.Card is PlayableCard pCard && pCard.OnBoard && !portraitSpawned)
			{
				dcac.SpawnHoloPortrait(this.Card.Info.holoPortraitPrefab);
                this.Card.renderInfo.hidePortrait = true;
                portraitSpawned = true;
			}
		}

        public override void OnPreRenderCard()
        {
            this.ApplyAppearance();
        }

        static OnboardHoloPortrait()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "OnboardHoloPortrait", typeof(OnboardHoloPortrait)).Id;
        }
    }
}