using HarmonyLib;
using DiskCardGame;
using System.Collections;
using UnityEngine;
using Infiniscryption.P03KayceeRun.Faces;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class LifeManagement
    {
        [HarmonyPatch(typeof(Part3SaveData), "OnRespawn")]
        [HarmonyPrefix]
        public static void ManagePlayerLivesLeft()
        {
            if (SaveFile.IsAscension)
            {
                // Reduce the number of lives
                EventManagement.NumberOfLivesRemaining = EventManagement.NumberOfLivesRemaining - 1;
                EventManagement.NumberOfZoneEnemiesKilled = 0;
            }
        }

        [HarmonyPatch(typeof(Part3GameFlowManager), "PlayerRespawnSequence")]
        [HarmonyPostfix]
        public static IEnumerator ShowLivesAtStartOfRespawn(IEnumerator sequence)
        {
            bool hasShownLivesLost = false;
            while (sequence.MoveNext())
            {
                if (sequence.Current is WaitForSeconds && !hasShownLivesLost)
                {
                    yield return sequence.Current;

                    // Now we show the lives sequence
                    yield return P03LivesFace.ShowChangeLives(-1, true);
                    yield return new WaitForSeconds(0.1f);
                    hasShownLivesLost = true;

                    // And if we have no more lives, we stop this sequence entirely and move to the end of game sequence:
                    if (EventManagement.NumberOfLivesRemaining == 1) // It hasn't been decremented yet
                    {
                        yield return LostAscensionRunSequence();
                        yield break;
                    }

                    bool diedToBoss = Traverse.Create(Part3GameFlowManager.Instance).Field("diedToBoss").GetValue<bool>();
                    bool createBloodStain = !diedToBoss && Part3SaveData.Data.currency > 0;
                    if (!createBloodStain)
                    {
                        ViewManager.Instance.SwitchToView(View.MapDefault, false, false);
		                yield return new WaitForSeconds(0.1f);
                    }

                    continue;
                }
                yield return sequence.Current;
            }
            yield break;
        }

        private static IEnumerator LostAscensionRunSequence()
        {
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
			yield return new WaitForSeconds(0.1f);
            yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionDeath", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
		    yield return new WaitForSeconds(0.1f);
            EventManagement.FinishAscension(false);
            yield break;
        }

        [HarmonyPatch(typeof(Part3SaveData), "ResetBattlesInCurrentZone")]
        [HarmonyPrefix]
        public static bool ResetBattlesForAscension(ref Part3SaveData __instance)
        {
            if (SaveFile.IsAscension)
            {
                foreach (Part3SaveData.MapAreaStateData area in __instance.areaData)
                {
                    area.clearedBattles.ForEach(delegate(LookDirection x)
                    {
                        area.clearedDirections.Remove(x);
                    });
                    area.clearedBattles.Clear();
                }
                return false;
            }
            return true;
        }
    }
}