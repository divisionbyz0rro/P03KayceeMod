using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class AscensionTransformerCardNodeData : SpecialNodeData
    {
        public static readonly HoloMapNode.NodeDataType AscensionTransformCard = GuidManager.GetEnumValue<HoloMapNode.NodeDataType>(P03Plugin.PluginGuid, "AscensionTransformCard");

        [HarmonyPatch(typeof(HoloMapNode), "AssignNodeData")]
        [HarmonyPrefix]
        public static bool PatchTradeSequenceNodeData(ref HoloMapNode __instance)
        {
            if (__instance.NodeType == AscensionTransformCard)
            {
                __instance.Data = new AscensionTransformerCardNodeData();
                return false;
            }
            return true;
        }
    }
}