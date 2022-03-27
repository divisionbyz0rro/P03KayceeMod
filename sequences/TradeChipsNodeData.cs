using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Guid;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class TradeChipsNodeData : SpecialNodeData
    {
        public static readonly HoloMapNode.NodeDataType TradeChipsForCards = GuidManager.GetEnumValue<HoloMapNode.NodeDataType>(P03Plugin.PluginGuid, "TradeChipsForCards");

        [HarmonyPatch(typeof(HoloMapNode), "AssignNodeData")]
        [HarmonyPrefix]
        public static bool PatchTradeSequenceNodeData(ref HoloMapNode __instance)
        {
            if (__instance.NodeType == TradeChipsForCards)
            {
                __instance.Data = new TradeChipsNodeData();
                return false;
            }
            return true;
        }
    }
}