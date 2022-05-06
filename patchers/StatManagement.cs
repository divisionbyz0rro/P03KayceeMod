using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using UnityEngine;
using System.Collections.Generic;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    internal static class StatManagement
    {
        internal static AscensionStat.Type ENERGY_SPENT = GuidManager.GetEnumValue<AscensionStat.Type>(P03Plugin.PluginGuid, "EnergySpent");
        internal static AscensionStat.Type HAMMER_USES = GuidManager.GetEnumValue<AscensionStat.Type>(P03Plugin.PluginGuid, "HammerUses");
        internal static AscensionStat.Type EXPERIMENTS_CREATED = GuidManager.GetEnumValue<AscensionStat.Type>(P03Plugin.PluginGuid, "ExperimentsCreated");

        private static readonly Sprite WIN_BACKGROUND = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascension_endscreen_victory.png", typeof(StatManagement).Assembly));
        private static readonly Sprite LOSE_BACKGROUND = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascension_endscreen_defeat.png", typeof(StatManagement).Assembly));

        private static readonly List<AscensionStat.Type> InvalidP03Stats = new ()
        {
            AscensionStat.Type.MantisGodsPicked,
            AscensionStat.Type.MostBones,
            AscensionStat.Type.PeltsTraded,
            AscensionStat.Type.SquirrelsKilled,
            AscensionStat.Type.TeethPulled,
            AscensionStat.Type.SquirrelsKilled
        };

        [HarmonyPatch(typeof(AscensionStat), nameof(AscensionStat.GetStringForType))]
        [HarmonyPostfix]
        private static void CustomStatLabels(AscensionStat __instance, ref string __result)
        {
            if (__instance.type == ENERGY_SPENT)
                __result = "Energy Expended";

            if (__instance.type == HAMMER_USES)
                __result = "Hammer Blows Dealt";

            if (__instance.type == EXPERIMENTS_CREATED)
                __result = "Abominations Created";
        }

        [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.SpendEnergy))]
        [HarmonyPrefix]
        private static void TrackSpendEnergy(int amount)
        {
            if (P03AscensionSaveData.IsP03Run)
                AscensionStatsData.TryIncreaseStat(ENERGY_SPENT, amount);
        }

        [HarmonyPatch(typeof(HammerItem), nameof(HammerItem.OnValidTargetSelected))]
        [HarmonyPostfix]
        private static void TrackUseHammer()
        {
            if (P03AscensionSaveData.IsP03Run)
                AscensionStatsData.TryIncrementStat(HAMMER_USES);
        }

        [HarmonyPatch(typeof(AscensionRunEndScreen), nameof(AscensionRunEndScreen.Initialize))]
        [HarmonyPostfix]
        private static void P03Initialize(bool victory, AscensionRunEndScreen __instance)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                __instance.backgroundSpriteRenderer.sprite = (victory ? WIN_BACKGROUND : LOSE_BACKGROUND);
            }
        }

        [HarmonyPatch(typeof(AscensionStatsScreen), nameof(AscensionStatsScreen.FillStatsText))]
        [HarmonyPrefix]
        private static void P03Stats(AscensionStatsScreen __instance)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                __instance.displayedStatTypes.RemoveAll(st => InvalidP03Stats.Contains(st));
                __instance.displayedStatTypes.Add(ENERGY_SPENT);
            }
        }
    }
}