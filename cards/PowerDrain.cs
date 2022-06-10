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
    public class PowerDrain : AbilityBehaviour
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static PowerDrain()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Power Drain";
            info.rulebookDescription = "[creature] consumes all of its owner's available energy each turn.";
            info.canStack = false;
            info.powerLevel = -3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            PowerDrain.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(PowerDrain),
                TextureHelper.GetImageAsTexture("ability_powersink.png", typeof(PowerDrain).Assembly)
            ).Id;
        }

        public static bool ShouldStopEnergyGain => BoardManager.Instance.playerSlots.Any(s => s.Card != null && s.Card.HasAbility(AbilityID));

		[HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.AddEnergy))]
        [HarmonyPostfix]
        private static IEnumerator StopAddEnergy(IEnumerator sequence, ResourcesManager __instance)
        {
            yield return sequence;
            if (ShouldStopEnergyGain)
            {
                yield return __instance.SpendEnergy(__instance.PlayerEnergy);
            }
        }

        [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.RefreshEnergy))]
        [HarmonyPostfix]
        private static IEnumerator StopRefreshEnergy(IEnumerator sequence, ResourcesManager __instance)
        {
            yield return sequence;
            if (ShouldStopEnergyGain)
            {
                yield return __instance.SpendEnergy(__instance.PlayerEnergy);
            }
        }
    }
}
