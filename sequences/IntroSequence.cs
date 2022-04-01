using System.Collections;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public static class IntroSequence
    {
        [HarmonyPatch(typeof(Part3GameFlowManager), "SceneSpecificInitialization")]
        [HarmonyPrefix]
        public static bool ForceAscensionToStart()
        {
            if (SaveFile.IsAscension)
            {
                ItemsManager.Instance.SetSlotsAtEdge(true, true);
                Part3GameFlowManager.Instance.StartCoroutine(ReplaceIntroSequenceForAscension());
                return false;
            }
            return true;
        }

        public static IEnumerator ReplaceIntroSequenceForAscension()
        {
            PauseMenu.pausingDisabled = false;
            InteractionCursor.Instance.SetHidden(true);

            if (!StoryEventsData.EventCompleted(EventManagement.SAW_P03_INTRODUCTION))
            {
	            ViewManager.Instance.SwitchToView(View.Default, true, false);
                P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Thinking, false, false);
            }

            HoloGameMap.Instance.HideMapImmediate();
            UIManager.Instance.Effects.GetEffect<ScreenColorEffect>().SetColor(GameColors.Instance.nearWhite);
            UIManager.Instance.Effects.GetEffect<ScreenColorEffect>().SetAlpha(1f);
            UIManager.Instance.Effects.GetEffect<ScreenColorEffect>().SetIntensity(0f, 0.4f);

            if (!StoryEventsData.EventCompleted(EventManagement.SAW_P03_INTRODUCTION))
            {

                AudioSource audio = AudioController.Instance.PlaySound2D("part3_intro", MixerGroup.None, 1f, 0f, null, null, null, null, false);

                yield return new WaitForSeconds(4.5f);
                P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Default, true, true);

                ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionIntro", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                audio.Stop();
                yield return new WaitForSeconds(0.5f);
                yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionIntroConfused", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield return new WaitForSeconds(2f);
                ViewManager.Instance.SwitchToView(View.P03FaceClose, false, false);
                yield return new WaitForSeconds(0.5f);
                yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionIntroAnger", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionIntroOkay", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

                ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
                ViewManager.Instance.SwitchToView(View.Default, false, false);
            }

            ViewManager.Instance.SwitchToView(View.MapDeckReview, false, false);

            if (StoryEventsData.EventCompleted(EventManagement.SAW_P03_INTRODUCTION))
                yield return new WaitForSeconds(1f);

            InteractionCursor.Instance.SetHidden(false);

            if (!StoryEventsData.EventCompleted(EventManagement.SAW_P03_INTRODUCTION))
            {
                DeckReviewSequencer.Instance.SetDeckReviewShown(true);
            }

            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
            yield return new WaitUntil(() => ViewManager.Instance.CurrentView != View.MapDeckReview);
            ViewManager.Instance.SwitchToView(View.MapDefault, false, true);
            Part3GameFlowManager.Instance.StartGameStateDirect(GameState.Map);
            yield return new WaitUntil(() => !HoloGameMap.Instance.FullyUnrolled);
	        yield return new WaitUntil(() => HoloGameMap.Instance.FullyUnrolled);

            if (!StoryEventsData.EventCompleted(EventManagement.SAW_P03_INTRODUCTION))
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("Part3DraftIntro", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            }

            StoryEventsData.SetEventCompleted(EventManagement.SAW_P03_INTRODUCTION);

            if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.BOUNTY_HUNTER))
                ChallengeActivationUI.Instance.ShowActivation(AscensionChallengeManagement.BOUNTY_HUNTER );
        }
    }
}