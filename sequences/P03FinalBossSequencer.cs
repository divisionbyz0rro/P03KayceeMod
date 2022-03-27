using System.Collections;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class P03FinalBossSequencer : BossBattleSequencer
    {
        public override Opponent.Type BossType => BossManagement.P03FinalBossOpponent;

        public override StoryEvent DefeatedStoryEvent => EventManagement.DEFEATED_P03;

        public static readonly string[] MODS = new string[] { "Special Hammer Mod", "Incredible Drafting Mod", "The Community API", "Super-Duper Unity Editor" };

        public P03AscensionOpponent P03AscensionOpponent
        {
            get
            {
                return TurnManager.Instance.opponent as P03AscensionOpponent;
            }
        }

        public override EncounterData BuildCustomEncounter(CardBattleNodeData nodeData)
        {
            EncounterData data = base.BuildCustomEncounter(nodeData);
            data.aiId = BossManagement.P03FinalBossAI;
            return data;
        }

        private int upkeepCounter = -1;

        public override IEnumerator OpponentUpkeep()
        {
            upkeepCounter += 1;
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Default);

            if (TurnManager.Instance.opponent.NumLives == 1)
                yield break;

            int sequenceNumber = upkeepCounter <= 8 ? upkeepCounter : ((upkeepCounter - 1) % 6) + 1;

            switch (upkeepCounter)
            {
                case 1:
                    yield return P03AscensionOpponent.ShopForModSequence(MODS[0], upkeepCounter == 1, upkeepCounter <= 8);
                    yield break;

                case 2:
                    yield return P03AscensionOpponent.HammerSequence();
                    yield break;

                case 3:
                    yield return P03AscensionOpponent.ShopForModSequence(MODS[1], false, upkeepCounter <= 8);
                    yield break;

                case 4:
                    yield return P03AscensionOpponent.ExchangeTokensSequence();
                    yield break;

                case 5:
                    yield return P03AscensionOpponent.ShopForModSequence(MODS[2], false, upkeepCounter <= 8);
                    yield break;

                case 6:
                    yield return P03AscensionOpponent.APISequence();
                    yield break;

                case 7:
                    yield return P03AscensionOpponent.ShopForModSequence(MODS[3], false, upkeepCounter <= 8);
                    yield break;

                case 8:
                    yield return P03AscensionOpponent.UnityEngineSequence();
                    yield break;
            }
        }        

        public override IEnumerator GameEnd(bool playerWon)
        {
            OpponentAnimationController.Instance.ClearLookTarget();

            if (playerWon)
            {
                ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03BeatFinalBoss", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                ViewManager.Instance.SwitchToView(View.Default, false, false);
                P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Happy, true, true);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03NothingMatters", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                ViewManager.Instance.SwitchToView(View.DefaultUpwards, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03ThreeMovesAhead", TextDisplayer.MessageAdvanceMode.Auto, TextDisplayer.EventIntersectMode.Wait, null, null);
                FactoryScrybes scrybes = FactoryManager.Instance.Scrybes;
                scrybes.Show();
                yield return new WaitForSeconds(0.2f);
                P03AnimationController.Instance.SetHeadTrigger("neck_snap");
                CustomCoroutine.WaitOnConditionThenExecute(() => P03AnimationController.Instance.CurrentFace == P03AnimationController.Face.Choking, delegate
                {
                    AudioController.Instance.PlaySound3D("p03_head_off", MixerGroup.TableObjectsSFX, P03AnimationController.Instance.transform.position, 1f, 0f, null, null, null, null, false);
                });
                yield return new WaitForSeconds(12f);
                P03AnimationController.Instance.gameObject.SetActive(false);
                scrybes.leshy.SetEyesAnimated(true);
                yield return TextDisplayer.Instance.PlayDialogueEvent("LeshyFinalBossDialogue", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield return new WaitForSeconds(0.5f);
                StoryEventsData.SetEventCompleted(EventManagement.HAS_DEFEATED_P03);
                EventManagement.FinishAscension(true);
            }
            else
            {
                ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03LostFinalBoss", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield return new WaitForSeconds(1.5f);
                EventManagement.FinishAscension(false);
            }
        }
    }
}