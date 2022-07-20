using System;
using System.Collections;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class MycologistWell : HoloMapNode
    {
        private void ShowHandleDown(bool down, bool immediate = false)
        {
            this.anim.Play(down ? "down" : "up", 0, immediate ? 1f : 0f);
        }

        private void Start()
        {
            this.ShowHandleDown(this.handleDown, true);
        }

        public override void OnSetActive(bool active)
        {
            base.OnSetActive(active);
            this.ShowHandleDown(this.handleDown, true);
            HoloMapGenericInteractable clicker = this.gameObject.GetComponentInChildren<HoloMapGenericInteractable>();
            clicker.selectedEvent = new ();
            clicker.selectedEvent.AddListener(this.OnCursorSelectEnd);
        }

        public override void OnCursorSelectEnd()
        {
            this.handleDown = !this.handleDown;
	        this.ShowHandleDown(this.handleDown, false);
        	AudioController.Instance.PlaySound2D("holomap_node_selected", MixerGroup.TableObjectsSFX, 0.5f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.VerySmall), null, new AudioParams.Randomization(true), null, false);

            if (this.handleDown)
            {
                this.SetHoveringEffectsShown(false);
                this.OnSelected();

                if (!StoryEventsData.EventCompleted(EventManagement.MYCO_ENTRY_APPROVED) && !StoryEventsData.EventCompleted(EventManagement.MYCO_ENTRY_DENIED))
                    base.StartCoroutine(this.MycologistSequence());
                else if (StoryEventsData.EventCompleted(EventManagement.MYCO_ENTRY_APPROVED) && !StoryEventsData.EventCompleted(EventManagement.MYCO_DEFEATED))
                    base.OnCursorSelectEnd();
            }
        }

        public override IEnumerator OnArriveAtNode()
        {
            EventManagement.MycologistReturnPosition = Part3SaveData.Data.playerPos;
            HoloMapAreaManager.Instance.MoveToAreaDirectly(RunBasedHoloMap.MYCOLOGIST_HOME_POSITION);
            yield break;
        }

        private IEnumerator MycologistSequence()
        {
            bool allowedIntoMycologist = true;

            // Set up the basic mycologist set up
            MapNodeManager.Instance.SetAllNodesInteractable(false);
            (GameFlowManager.Instance as Part3GameFlowManager).DisableTransitionToFirstPerson = true;
            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
            yield return HoloGameMap.Instance.FlickerHoloElements(false, 1);
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Thinking);
            ViewManager.Instance.SwitchToView(View.P03Face, false, true);
            yield return new WaitForSeconds(0.1f);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03MycologistWut", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return new WaitForSeconds(0.4f);
            P03AnimationController.Instance.ShowInfected(true);
            P03AnimationController.Instance.FaceRenderer.SetTVEffectsEnabled(true);
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.MycologistIdle);
            yield return new WaitForSeconds(0.4f);

            // Talk to the mycologist
            yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistWellIntro", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            // Check to see if the player has the organic material
            if (Part3SaveData.Data.items.Contains(GoobertHuh.ItemData.name, StringComparer.OrdinalIgnoreCase))
            {
                ViewManager.Instance.SwitchToView(View.ConsumablesOnly, false, true);
                yield return new WaitForSeconds(0.5f);
                yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistOrganicSuccess", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistOrganicFear", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                ItemSlot slot = ItemsManager.Instance.Slots.First(s => s.Item != null && s.Item.name.Equals(GoobertHuh.ItemData.name));
                slot.Item.PlayExitAnimation();
                yield return new WaitForSeconds(1f);
                ItemsManager.Instance.RemoveItemFromSaveData(GoobertHuh.ItemData.name);
                slot.DestroyItem();
                ViewManager.Instance.SwitchToView(View.P03Face, false, true);
                yield return new WaitForSeconds(0.2f);
                StoryEventsData.SetEventCompleted(EventManagement.FLUSHED_GOOBERT);
            }
            else
            {
                allowedIntoMycologist = false;
                yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistOrganicFailure", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            }

            // Check to see if the player has the bounty hunter brain
            if (Part3SaveData.Data.deck.Cards.Any(ci => ci.name.Equals(CustomCards.BRAIN)))
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistBrainSuccess", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                Part3SaveData.Data.deck.RemoveCardByName(CustomCards.BRAIN);
            } else if (Part3SaveData.Data.deck.Cards.Any(ci => ci.name.Equals(CustomCards.BOUNTY_HUNTER_SPAWNER)))
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistBrainModified", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                allowedIntoMycologist = false;
            } else
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistBrainMissing", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                allowedIntoMycologist = false;
            }
            
            // Check to see if the player fixed the generator
            if (StoryEventsData.EventCompleted(EventManagement.GENERATOR_SUCCESS))
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistPowerSuccess", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            }
            else
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistPowerFailed", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                allowedIntoMycologist = false;
            }

            if (!allowedIntoMycologist)
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistEntryDenied", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                StoryEventsData.SetEventCompleted(EventManagement.MYCO_ENTRY_DENIED);
                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistEntryAllowed", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                StoryEventsData.SetEventCompleted(EventManagement.MYCO_ENTRY_APPROVED);
                yield return new WaitForSeconds(0.2f);
            }

            yield return UnInfectP03();
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Thinking);
            yield return new WaitForSeconds(0.7f);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03AngryMycoFailure", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Default);

            ViewManager.Instance.SwitchToView(View.MapDefault, false, false);
            yield return new WaitForSeconds(0.15f);
            HoloGameMap.Instance.StartCoroutine(HoloGameMap.Instance.FlickerHoloElements(true, 2));
            MapNodeManager.Instance.SetAllNodesInteractable(true);
            (GameFlowManager.Instance as Part3GameFlowManager).DisableTransitionToFirstPerson = false;
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
        }

        private IEnumerator UnInfectP03()
        {
            P03AnimationController.Instance.SetHeadTrigger("reset");
            P03AnimationController.Instance.SetHeadTrigger("twitch_right");
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Angry, true, true);
            yield return new WaitForSeconds(1f);
            P03AnimationController.Instance.SetHeadTrigger("twitch_left");
            P03AnimationController.Instance.ShowInfected(false);
            P03AnimationController.Instance.FaceRenderer.SetTVEffectsEnabled(false);
            yield break;
        }

        [SerializeField]
        public Animator anim;

        [SerializeField]
        public bool handleDown;
    }
}