using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class RandomBountyHunter : AbilityBehaviour
	{
		public static Ability AbilityID;
		public override Ability Ability => AbilityID;

        private static PlayableCard LastTriggeredCard;

		public override bool RespondsToDrawn() => true;

		public override IEnumerator OnDrawn()
		{
			(PlayerHand.Instance as PlayerHand3D).MoveCardAboveHand(base.Card);
			yield return base.Card.FlipInHand(new Action(this.AddMod));
			yield return base.LearnAbility(0.5f);
			yield break;
		}

		private void AddMod()
		{
            LastTriggeredCard = base.Card;
			base.Card.Status.hiddenAbilities.Add(this.Ability);
			CardModificationInfo mod = BountyHunterGenerator.GenerateMod(Math.Min(TurnManager.Instance.TurnNumber, 3), 10);
			base.Card.AddTemporaryMod(mod);
            base.Card.RenderCard();
		}

        static RandomBountyHunter()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Bounty Hunter";
            info.rulebookDescription = "When drawn, [creature] will turn into a random bounty hunter";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            RandomBountyHunter.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(RandomBountyHunter),
                TextureHelper.GetImageAsTexture("ability_bounty_hunter.png", typeof(RandomBountyHunter).Assembly)
            ).Id;
        }

        [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.DisplayedNameEnglish), MethodType.Getter)]
        [HarmonyPostfix]
        private static void UpdatedDisplayNameTheHardWay(ref CardInfo __instance, ref string __result)
        {
            if (__instance.HasAbility(RandomBountyHunter.AbilityID))
            {
                if (TurnManager.Instance != null && !TurnManager.Instance.GameEnded)
                {
                    // Look for this card on the board
                    if (BoardManager.Instance != null)
                    {
                        foreach (CardSlot slot in BoardManager.Instance.PlayerSlotsCopy)
                            if (slot.Card != null && slot.Card.Info == __instance)
                                foreach (var tMod in slot.Card.TemporaryMods)
                                    if (tMod.bountyHunterInfo != null)
                                    {
                                        __result = tMod.nameReplacement;
                                        return;
                                    }

                        foreach (PlayableCard pCard in PlayerHand.Instance.CardsInHand)
                            if (pCard.Info == __instance)
                                foreach (var tMod in pCard.TemporaryMods)
                                    if (tMod.bountyHunterInfo != null)
                                    {
                                        __result = tMod.nameReplacement;
                                        return;
                                    }

                        if (LastTriggeredCard != null && LastTriggeredCard.Info == __instance)
                            foreach (var tMod in LastTriggeredCard.TemporaryMods)
                                if (tMod.bountyHunterInfo != null)
                                {
                                    __result = tMod.nameReplacement;
                                    return;
                                }
                    }
                }
                P03Plugin.Log.LogDebug($"RBH Card Name is {__result}");
            }
        }
	}
}