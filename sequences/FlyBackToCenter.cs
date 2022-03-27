using System.Collections;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class FlyBackToCenterIfBossDefeated : HoloAreaSpecialSequencer
	{
        private bool isFlying = false;

        public override void OnAreaEntered()
        {
            HoloMapBossNode bossNode = this.gameObject.GetComponentInChildren<HoloMapBossNode>();
            if (bossNode != null && bossNode.Completed && !isFlying)
            {
                isFlying = true;
                EventManagement.AddCompletedZone(Traverse.Create(bossNode).Field("bossDefeatedStoryEvent").GetValue<StoryEvent>());
                CustomCoroutine.Instance.StartCoroutine(FlyBackToCenter());
            }
        }

        private IEnumerator FlyBackToCenter()
        {
            yield return FastTravelManagement.ReturnToHomeBase();
            isFlying = false;
            yield return new WaitForSeconds(0.1f);
            yield break;
        }
	}
}
