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
    public class FullyLoaded : AbilityBehaviour
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        internal static List<CardSlot> BuffedSlots = new();

        static FullyLoaded()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Fully Loaded";
            info.rulebookDescription = "When [creature] dies, it leaves a permanent +1 attack bonus in the lane it occupied.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            FullyLoaded.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(FullyLoaded),
                TextureHelper.GetImageAsTexture("ability_fully_loaded.png", typeof(FullyLoaded).Assembly)
            ).Id;
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            PowerUpSlot(this.Card.Slot);
            yield break;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveAttackBuffs))]
        [HarmonyPostfix]
        private static void AddBuffForCardSlot(PlayableCard __instance, ref int __result)
        {
            if (BuffedSlots.Contains(__instance.Slot))
                __result += 1;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPostfix]
        private static void ResetBuffedSlots()
        {
            BuffedSlots.Clear();
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPostfix]
        private static void CleanUpBuffedSlots()
        {
            foreach (CardSlot slot in BuffedSlots)
                DepowerSlot(slot);
            BuffedSlots.Clear();
        }

        private static void PowerUpSlot(CardSlot slot)
        {
            BuffedSlots.Add(slot);
            slot.SetTexture(TextureHelper.GetImageAsTexture("cardslot_fully_loaded.png", typeof(FullyLoaded).Assembly));
        }

        private static void DepowerSlot(CardSlot slot)
        {
            if (SaveManager.SaveFile.IsPart1)
                slot.SetTexture(ResourceBank.Get<Texture>("Art/Cards/card_slot"));
            if (SaveManager.SaveFile.IsPart3)
                slot.SetTexture(ResourceBank.Get<Texture>("Art/Cards/card_slot_tech"));
            if (SaveManager.SaveFile.IsGrimora)
                slot.SetTexture(ResourceBank.Get<Texture>("Art/Cards/card_slot_undead"));
            if (SaveManager.SaveFile.IsMagnificus)
                slot.SetTexture(ResourceBank.Get<Texture>("Art/Cards/card_slot_wizard"));
        }
    }
}
