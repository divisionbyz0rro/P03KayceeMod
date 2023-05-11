using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class BuckWild : DrawCreatedCard
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static BuckWild()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Buck Wild";
            info.rulebookDescription = "When [creature] takes damage, the creature that dealt damage is pushed back to the queue.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            BuckWild.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(BuckWild),
                TextureHelper.GetImageAsTexture("ability_buckwild.png", typeof(BuckWild).Assembly)
            ).Id;
        }

        public override bool RespondsToTakeDamage(PlayableCard source) => source != null;

        private CardInfo LastCreatedCard;

        public override CardInfo CardToDraw => LastCreatedCard;

        public override IEnumerator OnTakeDamage(PlayableCard source)
        {
            this.Card.Anim.StrongNegationEffect();
            yield return new WaitForSeconds(0.2f);
            if (source.OpponentCard)
            {
                if (TurnManager.Instance.Opponent is not null)
                {
                    yield return TurnManager.Instance.Opponent.ReturnCardToQueue(source, 0.25f);
                }
                else
                {
                    BoardManager.Instance.ReturnCardToQueue(source);
                    yield return new WaitForSeconds(0.75f);
                }
            }
            else
            {
                LastCreatedCard = source.Info;
                source.ExitBoard(0.4f, Vector3.zero);
                yield return new WaitForSeconds(0.45f);
                yield return CreateDrawnCard();
            }
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSlot))]
        [HarmonyPostfix]
        private static IEnumerator StopAttackIfNull(IEnumerator sequence, CardSlot attackingSlot)
        {
            if (attackingSlot.Card == null)
                yield break;

            yield return sequence;
        }
    }
}
