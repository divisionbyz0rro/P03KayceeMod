using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class SapphirePower : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static SapphirePower()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Sapphire Blessing";
            info.rulebookDescription = "[creature] reduces the cost of all cards in your hand by 1.";
            info.canStack = false;
            info.powerLevel = 5;
            info.opponentUsable = true;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.GainGemBlue).Info.colorOverride;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            SapphirePower.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(SapphirePower),
                TextureHelper.GetImageAsTexture("ability_sapphire_power.png", typeof(SapphirePower).Assembly)
            ).Id;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.EnergyCost), MethodType.Getter)]
        [HarmonyPostfix]
        private static void AdjustCostForSapphirePower(PlayableCard __instance, ref int __result)
        {
            List<CardSlot> slots = BoardManager.Instance.GetSlots(!__instance.OpponentCard);
            __result -= slots.Where(s => s.Card != null && s.Card.HasAbility(AbilityID)).Count();
            if (__result < 0)
                __result = 0;
        }
    }
}