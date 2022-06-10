using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GBC;

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
            AscensionStat.Type.SacrificesMade
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
            AscensionStatsData.TryIncreaseStat(ENERGY_SPENT, amount);
        }

        [HarmonyPatch(typeof(HammerItem), nameof(HammerItem.OnValidTargetSelected))]
        [HarmonyPostfix]
        private static void TrackUseHammer()
        {
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
            if (__instance.gameObject.GetComponent<AscensionRunEndScreen>() != null)
            {
                __instance.displayedStatTypes.RemoveAll(st => InvalidP03Stats.Contains(st));
                __instance.displayedStatTypes.Add(ENERGY_SPENT);
            }
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.Start))]
        [HarmonyPostfix]
        private static void AddAdditionalStats()
        {
            AscensionStatsScreen statScreen = AscensionMenuScreens.Instance.statsScreen.GetComponentInChildren<AscensionStatsScreen>();
            float yGap = statScreen.statsText[1].gameObject.transform.parent.position.y - statScreen.statsText[0].gameObject.transform.parent.position.y;

            GameObject template = statScreen.statsText[0].gameObject.transform.parent.gameObject;
            for (int i = 0; i < 3; i++)
            {
                GameObject newItem = GameObject.Instantiate(template, template.transform.parent);
                float newY = statScreen.statsText.Last().gameObject.transform.parent.localPosition.y + yGap;
                newItem.transform.localPosition = new(newItem.transform.localPosition.x, newY, newItem.transform.localPosition.z);
                statScreen.statsText.Add(newItem.GetComponentInChildren<PixelText>());
            }

            if (!statScreen.displayedStatTypes.Contains(ENERGY_SPENT))
                statScreen.displayedStatTypes.Add(ENERGY_SPENT);

            if (!statScreen.displayedStatTypes.Contains(HAMMER_USES))
                statScreen.displayedStatTypes.Add(HAMMER_USES);

            if (!statScreen.displayedStatTypes.Contains(EXPERIMENTS_CREATED))
                statScreen.displayedStatTypes.Add(EXPERIMENTS_CREATED);

        }

        [HarmonyPatch(typeof(AscensionStatsScreen), nameof(AscensionStatsScreen.OnEnable))]
        [HarmonyPostfix]
        private static void HideUnusedStats(AscensionStatsScreen __instance)
        {
            foreach(PixelText obj in __instance.statsText)
            {
                P03Plugin.Log.LogInfo(obj.Text);
                if (obj.Text.ToLowerInvariant().StartsWith("statistic"))
                {
                    P03Plugin.Log.LogInfo("Setting ^ inactive");
                    obj.gameObject.transform.parent.gameObject.SetActive(false);
                }
            }
        }
    }
}