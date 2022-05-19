using HarmonyLib;
using DiskCardGame;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    internal class DamageRacePatches
    {
        [HarmonyPatch(typeof(DamageRaceBattleSequencer), nameof(DamageRaceBattleSequencer.BuildCustomEncounter))]
        [HarmonyPostfix]
        private static void UseBlueprintData(ref EncounterData __result, CardBattleNodeData nodeData)
        {
            if (SaveFile.IsAscension)
            {
                __result.Blueprint = nodeData.blueprint;
                __result.opponentTurnPlan = EncounterBuilder.BuildOpponentTurnPlan(nodeData.blueprint, nodeData.difficulty, false);
            }
        }
    }
}