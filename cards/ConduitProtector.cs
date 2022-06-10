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
    public class ConduitProtector : AbilityBehaviour
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static ConduitProtector()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Conduit Protector";
            info.rulebookDescription = "When [creature] is within a completed circuit, damage that would be dealt to the conduits is dealt to this creature instead.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.conduitCell = true;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.CellDrawRandomCardOnDeath).Info.colorOverride;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            ConduitProtector.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(ConduitProtector),
                TextureHelper.GetImageAsTexture("ability_conduitprotector.png", typeof(ConduitProtector).Assembly)
            ).Id;
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSlot))]
        [HarmonyPostfix]
        private static IEnumerator RedirectAttackSlotAttackSlot(IEnumerator sequence, CombatPhaseManager __instance, CardSlot attackingSlot, CardSlot opposingSlot, float waitAfter = 0f)
        {
            if (attackingSlot.Card == null)
                yield break;

            if (ConduitCircuitManager.Instance == null)
            {
                yield return sequence;
                yield break;
            }

            List<CardSlot> possibles = BoardManager.Instance.opponentSlots;
            if (!possibles.Contains(opposingSlot))
                possibles = BoardManager.Instance.playerSlots;

            foreach (CardSlot slot in possibles.GetRange(1, possibles.Count - 2))
            {
                if (slot.Card == null || !slot.Card.HasAbility(ConduitProtector.AbilityID))
                    continue;

                List<PlayableCard> conduits = ConduitCircuitManager.Instance.GetConduitsForSlot(slot);
                
                if (conduits.Count > 0 && conduits.Any(pc => pc.Slot == opposingSlot))
                {
                    yield return __instance.SlotAttackSlot(attackingSlot, slot, waitAfter);
                    yield break;
                }
            }

            yield return sequence;
        }
    }
}
