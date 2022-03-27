using System.Collections;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class AscensionFinaleSequencer : HoloAreaSpecialSequencer
	{		
		public override IEnumerator PreEnteredSequence()
		{
            (GameFlowManager.Instance as Part3GameFlowManager).DisableTransitionToFirstPerson = true;
            ViewManager.Instance.SwitchToView(View.P03Face, false, true);
            yield return new WaitForSeconds(0.5f);
            yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionFinale", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return new WaitForSeconds(0.25f);
            ViewManager.Instance.SwitchToView(View.Default, false, true);
            EventManagement.FinishAscension(true);
			yield break;
		}
	}
}
