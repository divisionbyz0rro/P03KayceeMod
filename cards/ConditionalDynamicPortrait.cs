using DiskCardGame;
using InscryptionAPI.Card;
using System.Linq;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
	public class ConditionalDynamicPortrait : CardAppearanceBehaviour
	{
        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        static ConditionalDynamicPortrait()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "ConditionalDynamicPortrait", typeof(ConditionalDynamicPortrait)).Id;
        }

        private GameObject _animatedPortrait;
        private GameObject AnimatedPortrait
        {
            get
            {
                if (_animatedPortrait != null)
                    return _animatedPortrait;

                _animatedPortrait = CardLoader.GetCardByName("!BOUNTYHUNTER_BASE").animatedPortrait;
                return _animatedPortrait;
            }
        }

		public override void ApplyAppearance()
		{
            base.Card.RenderInfo.prefabPortrait = null;
			base.Card.RenderInfo.hidePortrait = false;
            //base.Card.RenderInfo.portraitColor = GameColors.Instance.gold;
            if (base.Card is PlayableCard pCard)
            {
                if (pCard.temporaryMods.Any(mod => mod.bountyHunterInfo != null))
                {
                    base.Card.RenderInfo.prefabPortrait = AnimatedPortrait;
                    base.Card.RenderInfo.hidePortrait = true;
                    base.Card.RenderInfo.nameOverride = pCard.temporaryMods.First(mod => mod.bountyHunterInfo != null).nameReplacement;
                    P03Plugin.Log.LogDebug($"Bounty hunter name override {base.Card.renderInfo.nameOverride}");
                }
            }
		}

		public override void ResetAppearance()
		{
			base.Card.RenderInfo.prefabPortrait = null;
			base.Card.RenderInfo.hidePortrait = false;
		}

        public override void OnPreRenderCard()
        {
            ApplyAppearance();
        }
	}
}