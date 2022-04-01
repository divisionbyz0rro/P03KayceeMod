using HarmonyLib;
using DiskCardGame;
using System.Collections;
using UnityEngine;
using Infiniscryption.P03KayceeRun.Sequences;
using Infiniscryption.P03KayceeRun.Faces;
using InscryptionAPI.Encounters;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class BossManagement
    {
        public static readonly string P03FinalBossAI = AIManager.Add(P03Plugin.PluginGuid, "P03FinalBossAI", typeof(P03FinalBossOpponentAI)).Id;
        public static Opponent.Type P03FinalBossOpponent { get; private set; }

        [HarmonyPatch(typeof(Part3BossOpponent), nameof(Part3BossOpponent.IntroSequence))]
        [HarmonyPostfix]
        public static IEnumerator ReduceLivesOnBossNode(IEnumerator sequence)
        {
            if (!SaveFile.IsAscension)
            {
                yield return sequence;
                yield break;
            }

            bool hasShownLivesDrop = false;
            while (sequence.MoveNext())
            {
                if (sequence.Current is WaitForSeconds)
                {
                    yield return sequence.Current;
                    sequence.MoveNext();

                    if (EventManagement.NumberOfLivesRemaining > 1 && !hasShownLivesDrop)
                    {
                        int livesToDrop = EventManagement.NumberOfLivesRemaining - 1;
                        yield return P03LivesFace.ShowChangeLives(-livesToDrop, true);
                        EventManagement.NumberOfLivesRemaining = 1;

                        yield return EventManagement.SayDialogueOnce("P03OnlyOneBossLife", EventManagement.ONLY_ONE_BOSS_LIFE);
                    }
                    hasShownLivesDrop = true;
                }
                yield return sequence.Current;
            }
            yield break;
        }

        [HarmonyPatch(typeof(CanvasBossOpponent), nameof(CanvasBossOpponent.IntroSequence))]
        [HarmonyPostfix]
        public static IEnumerator CanvasResetLives(IEnumerator sequence)
        {
            yield return ReduceLivesOnBossNode(sequence);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
        }

        [HarmonyPatch(typeof(Part3BossOpponent), nameof(Part3BossOpponent.BossDefeatedSequence))]
        [HarmonyPostfix]
        public static IEnumerator AscensionP03ResetLives(IEnumerator sequence)
        {
            if (SaveFile.IsAscension)
            {
                // Reset lives to maximum
                if (EventManagement.NumberOfLivesRemaining < AscensionSaveData.Data.currentRun.maxPlayerLives)
                {
                    int livesToAdd = AscensionSaveData.Data.currentRun.maxPlayerLives - EventManagement.NumberOfLivesRemaining;
                    yield return P03LivesFace.ShowChangeLives(livesToAdd, true);
                    yield return new WaitForSeconds(0.5f);
                    EventManagement.NumberOfLivesRemaining = AscensionSaveData.Data.currentRun.maxPlayerLives;
                }
            }

            yield return sequence;
            yield break;
        }

        [HarmonyPatch(typeof(HoloGameMap), "BossDefeatedSequence")]
        [HarmonyPostfix]
        public static IEnumerator AscensionP03BossDefeatedSequence(IEnumerator sequence, StoryEvent bossDefeatedStoryEvent)
        {
            if (!SaveFile.IsAscension)
            {
                yield return sequence;
                yield break;
            }

            EventManagement.AddCompletedZone(bossDefeatedStoryEvent);

            if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.NoBossRares))
            {
                Part3SaveData.Data.deck.AddCard(CardLoader.GetCardByName(CustomCards.DRAFT_TOKEN));
                ChallengeActivationUI.TryShowActivation(AscensionChallenge.NoBossRares);
                yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionBossDraftToken", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            }
            else
            {
                Part3SaveData.Data.deck.AddCard(CardLoader.GetCardByName(CustomCards.RARE_DRAFT_TOKEN));
                yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionBossRareToken", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            }

            yield return FastTravelManagement.ReturnToHomeBase();

            yield break;
        }

        public static void RegisterBosses()
        {
            OpponentManager.BaseGameOpponents.OpponentById(Opponent.Type.CanvasBoss)
                .SetNewSequencer(P03Plugin.PluginGuid, "AscensionCanvasSequencer", typeof(CanvasAscensionSequencer));

            OpponentManager.BaseGameOpponents.OpponentById(Opponent.Type.ArchivistBoss).Opponent = typeof(ArchivistAscensionOpponent);

            OpponentManager.BaseGameOpponents.OpponentById(Opponent.Type.TelegrapherBoss)
                .SetOpponent(typeof(TelegrapherAscensionOpponent))
                .SetNewSequencer(P03Plugin.PluginGuid, "AscensionTelgrapherSequencer", typeof(TelegrapherAscensionSequencer));

            P03FinalBossOpponent = OpponentManager.Add(P03Plugin.PluginGuid, "P03AscensionFinalBoss", string.Empty, typeof(P03AscensionOpponent))
                .SetNewSequencer(P03Plugin.PluginGuid, "P03FinalBossSequencer", typeof(P03FinalBossSequencer))
                .Id;
        }
    }
}