using System;
using System.Collections;
using System.Collections.Generic;
using DigitalRuby.LightningBolt;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Patchers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class MycologistAscensionBossOpponent : MycologistsBossOpponent
    {
        public override string PreIntroDialogueId => "P03AngryAboutMycoBoss";

        public override IEnumerator IntroSequence(EncounterData encounter)
        {
            yield return new WaitForSeconds(0.5f);
            yield return Part3BoardMushrooms.Instance.ShowSequence();
            yield return new WaitForSeconds(0.5f);

            // We have to manually show the lives drop here
            // The patch that normally does this won't fire because it's patching a method that we are overriding
            // and we don't call the base
            if (EventManagement.NumberOfLivesRemaining > 1)
            {
                int livesToDrop = EventManagement.NumberOfLivesRemaining - 1;
                yield return P03LivesFace.ShowChangeLives(-livesToDrop, true);
                EventManagement.NumberOfLivesRemaining = 1;
            }

            yield return TextDisplayer.Instance.PlayDialogueEvent(this.PreIntroDialogueId, TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            AudioController.Instance.SetLoopAndPlay("part3_boss", 0, true, true);
            AudioController.Instance.SetLoopVolumeImmediate(0.2f, 0);
            P03AnimationController.Instance.ShowInfected(true);
            ViewManager.Instance.SwitchToView(View.P03Face, false, true);
            yield return new WaitForSeconds(0.1f);
            this.SetSceneEffectsShown(true);
            P03AnimationController.Instance.FaceRenderer.SetTVEffectsEnabled(true);
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.MycologistAngry, false, true);
            yield return new WaitForSeconds(1f);
            yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistAscensionBoss", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.MycologistIdle, false, true);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
            base.SpawnScenery("GiantMushroomEffects");
            yield return new WaitForSeconds(1f);
            yield break;
        }

        private void Part2ChangeSpritesOnSlots()
        {
            Texture2D texture = ResourceBank.Get<Texture2D>("Art/Cards/card_slot_mycologist_combine");
            for (int i = 0; i < 5; i++)
            {
                CardSlot slot = BoardManager.Instance.GetSlots(false)[i];
                slot.SetTexture(texture);
            }
        }

        private void ResetConduitBorder()
        {
            for (int i = 0; i < 5; i++)
            {
                CardSlot slot = BoardManager.Instance.GetSlots(false)[i];
            }
        }

        public override IEnumerator StartNewPhaseSequence()
        {
            ViewManager.Instance.SwitchToView(View.Default, false, true);
            yield return base.ClearBoard();
            yield return base.ClearQueue();

            // We aren't going to use an encounter blueprint for this
            this.Blueprint = null;
            this.ReplaceAndAppendTurnPlan(new List<List<CardInfo>>()); // There are no cards in the plan!

            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.P03Face, false, true);
            yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistPartTwoIntro", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            Part2ChangeSpritesOnSlots();
            ViewManager.Instance.SwitchToView(View.Default, false, true);
            yield return new WaitForSeconds(0.25f);
            yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistPowerOnline", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            CardInfo leftConduit = CardLoader.GetCardByName(CustomCards.MYCO_HEALING_CONDUIT);
            yield return BoardManager.Instance.CreateCardInSlot(leftConduit, BoardManager.Instance.opponentSlots[0], resolveTriggers:false);
            yield return new WaitForSeconds(0.3f);
            CardInfo rightConduit = CardLoader.GetCardByName(CustomCards.MYCO_HEALING_CONDUIT);
            yield return BoardManager.Instance.CreateCardInSlot(rightConduit, BoardManager.Instance.opponentSlots[4], resolveTriggers:false);
            yield return new WaitForSeconds(0.9f);

            yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistBrainOnline", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            CardInfo brain = CardLoader.GetCardByName(CustomCards.BRAIN);
            yield return BoardManager.Instance.CreateCardInSlot(brain, BoardManager.Instance.opponentSlots[2], resolveTriggers:false);
            yield return new WaitForSeconds(0.9f);

            yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistTestSubjects", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            List<CardInfo> cards = EventManagement.MycologistTestSubjects;
            yield return BoardManager.Instance.CreateCardInSlot(cards[0], BoardManager.Instance.opponentSlots[1], resolveTriggers:false);
            yield return new WaitForSeconds(0.3f);
            yield return BoardManager.Instance.CreateCardInSlot(cards[1], BoardManager.Instance.opponentSlots[3], resolveTriggers:false);
            yield return new WaitForSeconds(1f);

            yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistsBossCombine", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            CardSlot leftSlot = BoardManager.Instance.opponentSlots[1];
            CardSlot centerSlot = BoardManager.Instance.opponentSlots[2];
            CardSlot rightSlot = BoardManager.Instance.opponentSlots[3];

            Tween.Position(centerSlot.Card.transform, centerSlot.transform.position + Vector3.up * 0.1f, 0.1f, 0f);
            Tween.Position(leftSlot.Card.transform, centerSlot.transform.position, 0.3f, 0f);
            Tween.Position(rightSlot.Card.transform, centerSlot.transform.position, 0.3f, 0f);
            yield return new WaitForSeconds(0.2f);

            AddStaticPortrait(centerSlot.Card);

            AudioController.Instance.PlaySound3D("teslacoil_overload", MixerGroup.TableObjectsSFX, centerSlot.gameObject.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);
            TableVisualEffectsManager.Instance.ThumpTable(0.3f);

            GameObject topLightning = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"), this.gameObject.transform.parent);
            topLightning.GetComponent<LightningBoltScript>().EndObject = centerSlot.Card.gameObject;

            yield return new WaitForSeconds(0.3f);

            GameObject.Destroy(topLightning);

            BoardManager.Instance.opponentSlots[1].Card.ExitBoard(0.01f, Vector3.down);
            BoardManager.Instance.opponentSlots[3].Card.ExitBoard(0.01f, Vector3.down);

            yield return new WaitForSeconds(1f);

            yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistSummonGoo", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            
            GameObject goobert = GameObject.Instantiate(Resources.Load<GameObject>(GoobertHuh.ItemData.prefabId), centerSlot.gameObject.transform);
            ConsumableItem itemcontroller = GoobertHuh.FixGameObject(goobert);
            GameObject.Destroy(itemcontroller);
            //GameObject.Destroy(goobert.GetComponentInChildren<GooWizardAnimationController>());
            //GameObject.Destroy(goobert.GetComponentInChildren<Animator>());
            Vector3 target = new Vector3(0f, .6f, 0f);
            goobert.transform.localPosition = target + (Vector3.up * 3f);
            goobert.transform.Find("GooWizardBottle").localEulerAngles = new (0f, 276f, 0f);
            Tween.LocalPosition(goobert.transform, target, 3f, 0f);
            
            yield return new WaitForSeconds(1f);

            yield return TextDisplayer.Instance.PlayDialogueEvent("GooScaredMycoBoss", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistHereWeGo", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            goobert.transform.Find("GooWizardBottle/GooWizard/Cork").gameObject.SetActive(false);

            GameObject gooLightning = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"), this.gameObject.transform.parent);
            gooLightning.GetComponent<LightningBoltScript>().StartObject = BoardManager.Instance.opponentSlots[0].gameObject;
            gooLightning.GetComponent<LightningBoltScript>().StartPosition = Vector3.up * 1.5f;
            gooLightning.GetComponent<LightningBoltScript>().EndObject = BoardManager.Instance.opponentSlots[4].gameObject;
            gooLightning.GetComponent<LightningBoltScript>().EndPosition = Vector3.up * 1.5f;

            base.StartCoroutine(ConstantStatic(centerSlot.gameObject, 4f));
            yield return new WaitForSeconds(1f);
            goobert.transform.Find("GooWizardBottle/GooWizard/Bottle").gameObject.SetActive(false);
            AudioController.Instance.PlaySound3D("bottle_break", MixerGroup.TableObjectsSFX, centerSlot.gameObject.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);

            base.StartCoroutine(this.GooSpeakBackground());

            Transform gooTransform = goobert.transform.Find("GooWizardBottle");
            Tween.LocalScale(gooTransform, new Vector3(0.3f, 3f, 0.3f), 3f, 0f);
            //Tween.LocalPosition(goobert.transform.Find("GooWizardBottle"), new Vector3(0f, -1.5f, 0f), 3f, 0f);

            AudioController.Instance.PlaySound3D("teslacoil_charge", MixerGroup.TableObjectsSFX, centerSlot.gameObject.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);
            yield return new WaitForSeconds(3.2f);
            AudioController.Instance.PlaySound3D("teslacoil_overload", MixerGroup.TableObjectsSFX, centerSlot.gameObject.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);



            Tween.LocalPosition(gooTransform, gooTransform.localPosition + Vector3.down * 5f, 0.3f, 0f);

            yield return new WaitForSeconds(0.3f);

            GameObject.Destroy(gooTransform.gameObject);
            GameObject.Destroy(gooLightning);

            BoardManager.Instance.opponentSlots[2].Card.ExitBoard(0.01f, Vector3.down);

            TableVisualEffectsManager.Instance.ThumpTable(0.3f);

            yield return new WaitForSeconds(0.4f);

            ViewManager.Instance.SwitchToView(View.Board, false, true);

            GameObject.Destroy(goobert);

            CardInfo bossCard = CardLoader.GetCardByName(CustomCards.MYCO_CONSTRUCT_BASE);
            yield return BoardManager.Instance.CreateCardInSlot(bossCard, BoardManager.Instance.opponentSlots[2], resolveTriggers:false);
            yield return BoardManager.Instance.opponentSlots[2].Card.TriggerHandler.OnTrigger(Trigger.ResolveOnBoard);
            yield return new WaitForSeconds(0.3f);

            ViewManager.Instance.SwitchToView(View.Default, false, false);
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
        }

        private IEnumerator ConstantStatic(GameObject obj, float seconds = 3f, float step = 0.05f)
        {
            int target = (int)Mathf.Round(seconds / step);
            for (int i = 0; i < target; i++)
            {
                AudioController.Instance.PlaySound3D("teslacoil_spark", MixerGroup.TableObjectsSFX, obj.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Large), null, new AudioParams.Randomization() { }, null, false);
                yield return new WaitForSeconds(step);
            }
            yield break;
        }

        private void AddStaticPortrait(Card card)
        {
	        card.StatsLayer.Material = ResourceBank.Get<Material>("Art/Materials/Static_Card");
	        AnimatingSprite animatingSprite = card.StatsLayer.Renderer.gameObject.AddComponent<AnimatingSprite>();
	        for (int i = 1; i <= 4; i++)
	        {
		        animatingSprite.textureFrames.Add(ResourceBank.Get<Texture2D>("Art/Cards/Special/Static_Card_" + i));
	        }
	        animatingSprite.StartAnimating();
            card.renderInfo.nameOverride = "Experiment";
            card.RenderCard();
        }

        private IEnumerator GooSpeakBackground()
        {
            yield return TextDisplayer.Instance.PlayDialogueEvent("GooOhMyGod", TextDisplayer.MessageAdvanceMode.Auto, TextDisplayer.EventIntersectMode.Wait, null, null);
        }

        public override IEnumerator PreDefeatedSequence()
        {
            AudioController.Instance.FadeOutLoop(2f, Array.Empty<int>());
            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.Default, false, true);
            yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistCompleteSuccess", TextDisplayer.MessageAdvanceMode.Auto, TextDisplayer.EventIntersectMode.Wait, null, null);
            Part3SaveData.Data.deck.AddCard(CardLoader.GetCardByName(CustomCards.MYCO_CONSTRUCT_BASE));
            StoryEventsData.SetEventCompleted(EventManagement.MYCO_DEFEATED);
            yield return this.UnInfectP03();
            ResetConduitBorder();
            yield break;
        }

        public override string PostDefeatedDialogueId => "P03WTFDidYouDo"; 
    }
}