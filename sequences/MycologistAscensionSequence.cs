using System.Collections;
using DiskCardGame;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class MycologistAscensionSequence : MycologistsBattleSequencer
    {
        public override IEnumerator PlayerUpkeep()
        {
            if (TurnManager.Instance.Opponent is MycologistAscensionBossOpponent mabo)
            {
                if (mabo.NumLives > 1)
                {
                    if (!DialogueEventsData.EventIsPlayed("MycologistsBossCombine"))
                        yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistsBossCombine", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

                    // Player cards will never be merged in this new boss
                    yield return this.Mycologists.PushCardsToCenterAndCombine(false);
                    yield break;
                }
            }
            yield break;
        }
    }
}