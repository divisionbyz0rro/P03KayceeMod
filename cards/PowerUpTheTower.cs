using System.Collections;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class PowerUpTheTower : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility AbilityID { get; private set; }

        private bool Active
        {
            get
            {
                return this.PlayableCard.OnBoard &&
                       ConduitCircuitManager.Instance != null &&
                       ConduitCircuitManager.Instance.SlotIsWithinCircuit(this.PlayableCard.Slot);
            }
        }

        private IEnumerator SayDialogue(string dialogueCode)
        {
            string faceCode = EventManagement.GetDescriptorForNPC(EventManagement.SpecialEvent.PowerUpTheTower).faceCode;
            P03ModularNPCFace.Instance.SetNPCFace(faceCode);
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            yield return new WaitForSeconds(0.1f);
            P03AnimationController.Instance.SwitchToFace(P03ModularNPCFace.ModularNPCFace, true, true);
            yield return new WaitForSeconds(0.1f);
            yield return TextDisplayer.Instance.PlayDialogueEvent(dialogueCode, TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return new WaitForSeconds(0.1f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
            yield return new WaitForSeconds(0.15f);
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            if (!playerUpkeep)
                yield break;

            if (EventManagement.PowerUpkeepCount >= EventManagement.POWER_TURNS)
                yield break;

            if (Active)
            {
                EventManagement.PowerUpkeepCount += 1;
                yield return SayDialogue($"P03PowerTower{EventManagement.PowerUpkeepCount}");
            }
            else
            {
                yield return SayDialogue($"P03PowerTowerNeedsCircuit");
            }
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            if (EventManagement.PowerUpkeepCount >= EventManagement.POWER_TURNS)
                yield break;

            yield return SayDialogue("P03PowerTowerDied");

            if (EventManagement.PowerUpkeepCount > 0)
                EventManagement.PowerUpkeepCount -= 1;
        }

        static PowerUpTheTower()
        {
            AbilityID = SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "PowerUpTheTower", typeof(PowerUpTheTower)).Id;
        }
    }
}