using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Guid;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class AscensionRecycleCardNodeData : SpecialNodeData
    {
        public static readonly HoloMapNode.NodeDataType AscensionRecycleCard = GuidManager.GetEnumValue<HoloMapNode.NodeDataType>(P03Plugin.PluginGuid, "AscensionRecycleCard");

        [HarmonyPatch(typeof(HoloMapNode), "AssignNodeData")]
        [HarmonyPrefix]
        public static bool PatchTradeSequenceNodeData(ref HoloMapNode __instance)
        {
            if (__instance.NodeType == AscensionRecycleCard)
            {
                __instance.Data = new AscensionRecycleCardNodeData();
                return false;
            }
            return true;
        }
    }
}