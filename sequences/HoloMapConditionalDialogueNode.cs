using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class HoloMapConditionalDialogueNode : HoloMapDialogueNode
    {

        [HarmonyPatch(typeof(HoloFloatingLabel), nameof(HoloFloatingLabel.ManagedUpdate))]
        [HarmonyPrefix]
        private static bool DontIfLabelIsNull(HoloFloatingLabel __instance)
        {
            return __instance.line != null;
        }

        public override void OnCursorSelectEnd()
        {
            DetermineDialogue();
            this.SetHoveringEffectsShown(false);
            this.OnSelected();
            base.StartCoroutine(this.DialogueThenStorySequence());
        }

        public override void OnCursorEnter()
        {
            DetermineDialogue();
            label.gameObject.SetActive(true);
            this.label.SetText(Localization.Translate(this.floatingLabelText));
            base.OnCursorEnter();
        }

        public override void OnCursorExit()
        {
            label.gameObject.SetActive(false);
            base.OnCursorExit();
        }

        private IEnumerator DialogueThenStorySequence()
        {
            MapNodeManager.Instance.SetAllNodesInteractable(false);
            (GameFlowManager.Instance as Part3GameFlowManager).DisableTransitionToFirstPerson = true;
            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
            P03ModularNPCFace.Instance.SetNPCFace(this.npcCode);
            yield return HoloGameMap.Instance.FlickerHoloElements(false, 1);
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            yield return new WaitForSeconds(0.1f);
            P03AnimationController.Instance.SwitchToFace(this.face, true, true);
            yield return new WaitForSeconds(0.1f);
            yield return TextDisplayer.Instance.PlayDialogueEvent(this.dialogueId, TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return new WaitForSeconds(0.1f);

            if (this.completeStory != StoryEvent.NUM_EVENTS)
            {
                P03Plugin.Log.LogDebug($"Completing story event {this.completeStory}");
                StoryEventsData.SetEventCompleted(this.completeStory);
            }
            if (this.currencyReward != 0)
            {
                P03AnimationController.Face currentFace = P03AnimationController.Instance.CurrentFace;
                View currentView = ViewManager.Instance.CurrentView;
                yield return new WaitForSeconds(0.4f);
                yield return P03AnimationController.Instance.ShowChangeCurrency(this.currencyReward, true);
                Part3SaveData.Data.currency += this.currencyReward;
                yield return new WaitForSeconds(0.2f);
                P03AnimationController.Instance.SwitchToFace(currentFace);
                yield return new WaitForSeconds(0.1f);
                ViewManager.Instance.SwitchToView(currentView, false, false);
                yield return new WaitForSeconds(0.2f);
            }
            if (!string.IsNullOrEmpty(this.cardReward))
            {
                Part3SaveData.Data.deck.AddCard(CardLoader.GetCardByName(this.cardReward));
            }
            if (!string.IsNullOrEmpty(this.loseCardReward))
            {
                Part3SaveData.Data.deck.RemoveCardByName(this.loseCardReward);
            }
            if (!string.IsNullOrEmpty(this.loseItemReward))
            {
                if (Part3SaveData.Data.items.Contains(this.loseItemReward))
                {
                    ItemSlot slot = ItemsManager.Instance.Slots.First(s => s.Item != null && s.Item.name.Equals(this.loseItemReward));

                    View currentView = ViewManager.Instance.CurrentView;
                    yield return new WaitForEndOfFrame();
                    ViewManager.Instance.SwitchToView(View.ConsumablesOnly, false, true);
                    yield return new WaitForSeconds(0.8f);
                    slot.Item.PlayExitAnimation();
                    yield return new WaitForSeconds(1f);
                    ItemsManager.Instance.RemoveItemFromSaveData(this.loseItemReward);
                    slot.DestroyItem();
                    ViewManager.Instance.SwitchToView(currentView, false, false);
                    yield return new WaitForSeconds(0.2f);
                }
            }
            if (!string.IsNullOrEmpty(this.gainItemReward))
            {
                if (Part3SaveData.Data.items.Count < P03AscensionSaveData.MaxNumberOfItems)
                {
                    View currentView = ViewManager.Instance.CurrentView;
                    yield return new WaitForEndOfFrame();
                    ViewManager.Instance.SwitchToView(View.ConsumablesOnly, false, true);
                    yield return new WaitForSeconds(0.8f);
                    Part3SaveData.Data.items.Add(this.gainItemReward);
                    yield return new WaitForEndOfFrame();
                    ItemsManager.Instance.UpdateItems(false);
                    yield return new WaitForSeconds(1f);
                    ViewManager.Instance.SwitchToView(currentView, false, false);
                    yield return new WaitForSeconds(0.2f);
                }
            }
            
            EventManagement.GrantSpecialReward(this.specialReward);

            ViewManager.Instance.SwitchToView(View.MapDefault, false, false);
            yield return new WaitForSeconds(0.15f);
            HoloGameMap.Instance.StartCoroutine(HoloGameMap.Instance.FlickerHoloElements(true, 2));
            MapNodeManager.Instance.SetAllNodesInteractable(true);
            (GameFlowManager.Instance as Part3GameFlowManager).DisableTransitionToFirstPerson = false;
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;

            if (this.completeAfter)
            {
                this.SetCompleted();
                if (this.npc != null)
                    this.npc.SetActive(false);
            }
            else
                this.SetHidden(false, false);

            yield break;
        }

        protected virtual void DetermineDialogue()
        {
            this.repeatable = true;
            foreach(string rule in dialogueRules)
            {
                P03Plugin.Log.LogDebug($"Rule string: {rule}");

                string[] ruleSplit = rule.Split(',');
                StoryEvent preRequisite = (StoryEvent)int.Parse(ruleSplit[0]);
                StoryEvent antiPreRequisite = (StoryEvent)int.Parse(ruleSplit[1]);
                StoryEvent completeStory = (StoryEvent)int.Parse(ruleSplit[3]);
                string dialogueEvent = ruleSplit[2];
                int currency = int.Parse(ruleSplit[4]);
                string cardReward = ruleSplit[5];
                string loseCardReward = ruleSplit[6];
                string gainItem = ruleSplit[7];
                string loseItem = ruleSplit[8];
                bool completeAFter = bool.Parse(ruleSplit[9]);
                string npcCode = ruleSplit[10];
                EventManagement.SpecialReward reward = (EventManagement.SpecialReward)int.Parse(ruleSplit[11]);
                string floatingLabel = ruleSplit[12];

                if (((int)preRequisite == (int)StoryEvent.NUM_EVENTS || StoryEventsData.EventCompleted(preRequisite)) &&
                    ((int)antiPreRequisite == (int)StoryEvent.NUM_EVENTS || !StoryEventsData.EventCompleted(antiPreRequisite)))
                {
                    this.dialogueId = dialogueEvent;
                    this.completeStory = completeStory;
                    this.currencyReward = currency;
                    this.cardReward = cardReward;
                    this.loseCardReward = loseCardReward;
                    this.gainItemReward = gainItem;
                    this.loseItemReward = loseItem;
                    this.completeAfter = completeAFter;
                    this.npcCode = npcCode;
                    this.specialReward = reward;
                    this.floatingLabelText = floatingLabel;
                    return;
                }
            }
        }

        public void SetDialogueRule(string dialogueEvent, string floatingLabel, EventManagement.SpecialEvent specialEvent, StoryEvent preRequisite = StoryEvent.NUM_EVENTS, StoryEvent antiPreRequisite = StoryEvent.NUM_EVENTS, StoryEvent completeStory = StoryEvent.NUM_EVENTS, int completedCurrencyReward = 0, string completedCardReward = "", string loseCardReward = "", string gainItemReward = "", string loseItemReward = "", bool completeAfter = false, EventManagement.SpecialReward specialReward = EventManagement.SpecialReward.None)
        {
            string npcCode = EventManagement.GetDescriptorForNPC(specialEvent).faceCode;
            dialogueRules.Add($"{(int)preRequisite},{(int)antiPreRequisite},{dialogueEvent},{(int)completeStory},{completedCurrencyReward},{completedCardReward},{loseCardReward},{gainItemReward},{loseItemReward},{completeAfter},{npcCode},{(int)specialReward},{floatingLabel}");
        }

        [SerializeField]
        public List<string> dialogueRules = new();

        [SerializeField]
        public StoryEvent completeStory;

        [SerializeField]
        public int currencyReward;

        [SerializeField]
        public string cardReward;
        
        [SerializeField]
        public string gainItemReward;

        [SerializeField]
        public string loseItemReward;

        [SerializeField]
        public string loseCardReward;

        [SerializeField]
        public bool completeAfter;

        [SerializeField]
        public string npcCode;

        [SerializeField]
        public EventManagement.SpecialReward specialReward;

        [SerializeField]
        public string floatingLabelText;

        [SerializeField]
        public HoloFloatingLabel label;
    }
}