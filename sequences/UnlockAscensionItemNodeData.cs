using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Guid;
using System.Linq;
using Infiniscryption.P03KayceeRun.Patchers;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class UnlockAscensionItemNodeData : SpecialNodeData
    {
        public static readonly string[] PART1_ITEMS_TO_PART3 = new string[] { "PocketWatch" };

        public static readonly HoloMapNode.NodeDataType UnlockItemsAscension = GuidManager.GetEnumValue<HoloMapNode.NodeDataType>(P03Plugin.PluginGuid, "UnlockAscensionItemNodeData");

        [HarmonyPatch(typeof(HoloMapNode), "AssignNodeData")]
        [HarmonyPrefix]
        public static bool PatchTradeSequenceNodeData(ref HoloMapNode __instance)
        {
            if (__instance.NodeType == UnlockItemsAscension)
            {
                __instance.Data = new UnlockAscensionItemNodeData();
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
        [HarmonyPrefix]
        private static void FixItemRulebooksForAscension()
        {
            if (SaveFile.IsAscension)
            {
                foreach (ConsumableItemData itemData in ItemsUtil.AllConsumables)
                {
                    if (PART1_ITEMS_TO_PART3.Any(n => itemData.name.Equals(n)))
                    {
                        P03Plugin.Log.LogInfo($"Updating item {itemData.name} for temple {ScreenManagement.ScreenState.ToString()} and isP03 {P03AscensionSaveData.IsP03Run}");
                        if (P03AscensionSaveData.IsP03Run)
                            itemData.rulebookCategory = AbilityMetaCategory.Part3Rulebook;
                        else
                            itemData.rulebookCategory = AbilityMetaCategory.Part1Rulebook;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MenuController), nameof(MenuController.TransitionToGame))]
        [HarmonyPrefix]
        private static void SyncCardsAndAbilitiesWhenTransitioningToGame()
        {
            if (!SaveFile.IsAscension)
            {
                foreach (ConsumableItemData itemData in ItemsUtil.AllConsumables)
                {
                    if (PART1_ITEMS_TO_PART3.Any(n => itemData.name.Equals(n)))
                    {
                        itemData.rulebookCategory = AbilityMetaCategory.Part1Rulebook;
                    }
                }
            }
        }
    }
}