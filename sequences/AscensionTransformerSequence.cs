using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Encounters;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class AscensionTransformerSequence : MonoBehaviour, ICustomNodeSequence
    {
        public static AscensionTransformerSequence Instance { get; private set; }

        private Animator leftTable;
        private Animator rightTable;

        private SelectCardFromDeckSlot leftSlot;
        private SelectCardFromDeckSlot rightSlot;
        private GameObject computerScreen;

        private P03FaceCardDisplayer rightCard;
        private P03FaceCardDisplayer leftCard;

        private GameObject confirmButton;

        private GameObject parentObject;

        private bool leftSelecting = false;
        private bool rightSelecting = false;
        private bool selectingCards { get { return leftSelecting || rightSelecting; }}

        private bool selectionConfirmed = false;

        public AscensionTransformerSequence()
        {
            this.parentObject = new("AscensionTransformerSequence");
            this.parentObject.transform.SetParent(this.gameObject.transform);
            
            this.leftTable = GameObject.Instantiate(SpecialNodeHandler.Instance.overclockCardSequencer.cardTableAnim.transform.parent.gameObject, this.parentObject.transform).GetComponentInChildren<Animator>();
            this.rightTable = GameObject.Instantiate(SpecialNodeHandler.Instance.overclockCardSequencer.cardTableAnim.transform.parent.gameObject, this.parentObject.transform).GetComponentInChildren<Animator>();
            
            this.computerScreen = GameObject.Instantiate(SpecialNodeHandler.Instance.buildACardSequencer.screen.gameObject, this.parentObject.transform);
            
            GameObject.Destroy(this.computerScreen.transform.Find("Anim/ScreenInteractables").gameObject);
            
            foreach (string key in new string[] { "Statpoints", "STAGE_Empty", "STAGE_Cost", "STAGE_Stats", "STAGE_Abilities", "STAGE_Portrait", "STAGE_Name", "STAGE_Confirm"} )
            {
                
                GameObject.Destroy(this.computerScreen.transform.Find($"RenderCamera/Content/BuildACardInterface/{key}").gameObject);
            }
            
            
            this.leftCard = this.computerScreen.transform.Find("RenderCamera/Content/BuildACardInterface/Card").gameObject.GetComponent<P03FaceCardDisplayer>();
            
            GameObject rightScreen = GameObject.Instantiate(this.computerScreen.transform.Find($"RenderCamera/Content/BuildACardInterface/Card").gameObject, this.computerScreen.transform.Find("RenderCamera/Content/BuildACardInterface"));
            
            rightScreen.transform.localPosition = new Vector3(4.5f, 0f, 0f);
            
            this.rightCard = rightScreen.GetComponent<P03FaceCardDisplayer>();

            this.leftCard.gameObject.transform.Find("BuildACardPortrait").gameObject.SetActive(false);
            this.rightCard.gameObject.transform.Find("BuildACardPortrait").gameObject.SetActive(false);

            this.leftCard.gameObject.transform.Find("Portrait").gameObject.SetActive(true);
            this.rightCard.gameObject.transform.Find("Portrait").gameObject.SetActive(true);

            // Get the confirmation button
            this.confirmButton = new GameObject("confirmButton");
            confirmButton.transform.SetParent(this.computerScreen.transform);
            confirmButton.transform.localPosition = new Vector3(-.05f, 0f, 0f);
            confirmButton.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            confirmButton.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            SpriteRenderer renderer = confirmButton.AddComponent<SpriteRenderer>();
            Texture2D button = Resources.Load<Texture2D>("art/ui/factorypuzzles/captchapuzzle_correct");
            renderer.sprite = Sprite.Create(button, new Rect(0f, 0f, button.width, button.height), new Vector2(0.5f, 0.5f));
            renderer.SetMaterial(Resources.Load<Material>("art/materials/sprite_coloroverlay"));
            renderer.material.SetColor("_Color", GameColors.Instance.darkLimeGreen);
            confirmButton.AddComponent<BoxCollider>().size = renderer.size;
            MainInputInteractable mii = confirmButton.AddComponent<MainInputInteractable>();
            mii.CursorSelectStarted = x => selectionConfirmed = true;
            this.confirmButton.SetActive(false);
        }

        private IEnumerator EnterComponents()
        {
            this.leftTable.gameObject.transform.Find("Base/CardSlot/Quad").GetComponent<MeshRenderer>().material.mainTexture = TextureHelper.GetImageAsTexture("card_slot_transformer.png", typeof(AscensionTransformerSequence).Assembly);
            this.rightTable.gameObject.transform.Find("Base/CardSlot/Quad").GetComponent<MeshRenderer>().material.mainTexture = Resources.Load<Texture2D>("art/cards/card_slot_left");

            // Computer screen
            this.confirmButton.SetActive(false);
            this.leftCard.gameObject.SetActive(false);
            this.rightCard.gameObject.SetActive(false);
            this.computerScreen.SetActive(true);
			AudioController.Instance.PlaySound3D("slot_platform_lower", MixerGroup.TableObjectsSFX, base.transform.position, 1f, 0f, null, null, null, null, false);            
            this.leftTable.transform.parent.localPosition = new Vector3(0f, 5f, -1f);
            this.rightTable.transform.parent.localPosition = new Vector3(2.2f, 5f, -1f);
            yield return new WaitForSeconds(0.2f);
            P03Plugin.Log.LogDebug("Did lower screen");
            AudioController.Instance.PlaySound3D("slot_platform_raise", MixerGroup.TableObjectsSFX, this.leftTable.transform.position, 1f, 0f, null, null, null, null, false);
            this.leftTable.Play("enter", 0, 0f);
            yield return new WaitForSeconds(0.5f);
            P03Plugin.Log.LogDebug("Did raise left table");
            AudioController.Instance.PlaySound3D("slot_platform_raise", MixerGroup.TableObjectsSFX, this.rightTable.transform.position, 1f, 0f, null, null, null, null, false);
            this.rightTable.Play("enter", 0, 0f);
            yield return new WaitForSeconds(0.5f);
            P03Plugin.Log.LogDebug("Did raise right table");

            this.leftSlot = this.leftTable.gameObject.GetComponentInChildren<SelectCardFromDeckSlot>();
            this.rightSlot = this.rightTable.gameObject.GetComponentInChildren<SelectCardFromDeckSlot>();

            // left/right card slots
            this.leftSlot.RevealAndEnable();
	        this.leftSlot.ClearDelegates();
            P03Plugin.Log.LogDebug("Did prep left slot");
            this.rightSlot.RevealAndEnable();
	        this.rightSlot.ClearDelegates();
            P03Plugin.Log.LogDebug("Did prep right slot");
        }

        public IEnumerator ExecuteCustomSequence(CustomNodeData nodeData)
        {
            selectionConfirmed = false;
            yield return this.EnterComponents();

            yield return EventManagement.SayDialogueOnce("P03AscensionTransformer", EventManagement.TRANSFORMER_CHANGES);

	        this.leftSlot.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(this.leftSlot.CursorSelectStarted, new Action<MainInputInteractable>(this.OnSlotSelected));
            this.rightSlot.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(this.rightSlot.CursorSelectStarted, new Action<MainInputInteractable>(this.OnSlotSelected));

            yield return new WaitUntil(() => selectionConfirmed );

            this.confirmButton.SetActive(false);

            CardModificationInfo mod = GetFinalCardMod();
            Part3SaveData.Data.deck.ModifyCard(this.leftSlot.Card.Info, mod);
            Part3SaveData.Data.deck.RemoveCard(this.rightSlot.Card.Info);

            // Send away the computer screen
            yield return new WaitForSeconds(0.1f);
            Animator anim = this.computerScreen.GetComponentInChildren<Animator>();
            anim.Play("shake", 0, 0f);
            yield return new WaitForSeconds(0.25f);
            anim.Play("exit", 0, 0f);
			AudioController.Instance.PlaySound3D("slot_platform_lower", MixerGroup.TableObjectsSFX, base.transform.position, 1f, 0.25f, null, null, null, null, false);
			CustomCoroutine.WaitThenExecute(0.5f, () => this.computerScreen.SetActive(false), false);

            ViewManager.Instance.SwitchToView(View.CardMergeSlots, false, true);
            yield return new WaitForSeconds(0.25f);

            // "Permadie" the right card
            this.rightSlot.Card.Anim.PlayPermaDeathAnimation(true);
            yield return new WaitForSeconds(1.5f);
            this.rightSlot.SetShown(false);
            yield return this.rightSlot.pile.DestroyCards();
            this.rightSlot.DestroyCard();

            this.rightTable.Play("exit", 0, 0f);
	        AudioController.Instance.PlaySound3D("slot_platform_lower", MixerGroup.TableObjectsSFX, this.rightTable.transform.position, 1f, 0f, null, null, null, null, false);

            yield return new WaitForSeconds(0.5f);

            // "Flip" the left card
            this.leftSlot.Card.Anim.SetFaceDown(true, false);
            yield return new WaitForSeconds(0.2f);
            this.leftSlot.Card.Anim.SetShaking(true);
            this.leftSlot.Card.SetInfo(this.leftSlot.Card.Info);
            yield return new WaitForSeconds(0.85f);
            this.leftSlot.Card.Anim.SetShaking(false);
            this.leftSlot.Card.Anim.SetFaceDown(false, false);
            yield return new WaitForSeconds(1.5f);

            // Leave the left card
            ViewManager.Instance.SwitchToView(View.Default, false, true);
            yield return new WaitForSeconds(0.2f);
            this.leftSlot.FlyOffCard();
            yield return new WaitForSeconds(0.5f);
            this.leftSlot.SetShown(false);
            yield return this.leftSlot.pile.DestroyCards();
            this.leftSlot.DestroyCard();

            this.leftTable.Play("exit", 0, 0f);
	        AudioController.Instance.PlaySound3D("slot_platform_lower", MixerGroup.TableObjectsSFX, this.leftTable.transform.position, 1f, 0f, null, null, null, null, false);

            yield return new WaitForSeconds(0.2f);

            yield return new WaitForSeconds(0.1f);
            if (GameFlowManager.Instance != null)
                GameFlowManager.Instance.TransitionToGameState(GameState.Map, null);

            yield break;
        }

        private List<CardInfo> GetValidCards()
        {
            List<CardInfo> list = new List<CardInfo>(Part3SaveData.Data.deck.Cards);
            list.RemoveAll((CardInfo x) => x.HasAbility(Ability.Transformer) || x.Abilities.Count >= 4);
            if (this.leftSlot.Card != null && this.rightSelecting)
                list.Remove(this.leftSlot.Card.Info);
            if (this.rightSlot.Card != null && this.leftSelecting)
                list.Remove(this.rightSlot.Card.Info);
            return list;
        }

        private CardModificationInfo GetTempCardMod(bool left = true)
        {
            CardModificationInfo mod = new(Ability.Transformer);

            if (left)
            {
                if (this.leftSlot.Card != null && this.rightSlot.Card != null)
                {
                    int targetCost = (leftSlot.Card.Info.EnergyCost + rightSlot.Card.Info.EnergyCost) / 2;
                    if (targetCost > this.leftSlot.Card.Info.EnergyCost)
                        mod.energyCostAdjustment = targetCost - this.leftSlot.Card.Info.EnergyCost;
                }
            }
            else
            {
                mod.energyCostAdjustment = -this.rightSlot.Card.Info.EnergyCost;

                if (this.leftSlot.Card != null && this.rightSlot.Card != null)
                    mod.healthAdjustment = this.leftSlot.Card.Info.Health - this.rightSlot.Card.Info.Health;
            }

            return mod;
        }

        private CardModificationInfo GetFinalCardMod()
        {
            CardModificationInfo mod = new(Ability.Transformer);
            mod.nonCopyable = true;

            CardInfo card = this.rightSlot.Card.Info;
            mod.transformerBeastCardId = "@" + card.name + (card.Gemified ? "+Gemified" : "") + string.Join("", card.ModAbilities.Select(a => $"+{a.ToString()}"));

            int targetCost = (leftSlot.Card.Info.EnergyCost + rightSlot.Card.Info.EnergyCost) / 2;
            if (targetCost > this.leftSlot.Card.Info.EnergyCost)
                mod.energyCostAdjustment = targetCost - this.leftSlot.Card.Info.EnergyCost;

            return mod;
        }

        private void DisplayCards()
        {
            if (this.leftSlot.Card != null)
            {
                P03Plugin.Log.LogDebug($"Displaying left card {this.leftSlot.Card.Info.name}");
                this.leftCard.DisplayCard(this.leftSlot.Card.Info, GetTempCardMod(true), false);
                this.leftCard.gameObject.SetActive(true);
            }
            else
            {
                this.leftCard.gameObject.SetActive(false);
            }
            
            if (this.rightSlot.Card != null)
            {
                P03Plugin.Log.LogDebug($"Displaying left card {this.rightSlot.Card.Info.name}");
                this.rightCard.DisplayCard(this.rightSlot.Card.Info, GetTempCardMod(false), false);
                this.rightCard.gameObject.SetActive(true);
            }
            else
            {
                this.rightCard.gameObject.SetActive(false);
            }
        }

        private void OnSlotSelected(MainInputInteractable slot)
        {
            this.confirmButton.SetActive(false);
            leftSelecting = slot == this.leftSlot;
            rightSelecting = slot == this.rightSlot;

            this.leftSlot.SetEnabled(false);
            this.leftSlot.ShowState(HighlightedInteractable.State.NonInteractable, false, 0.15f);
            this.rightSlot.SetEnabled(false);
            this.rightSlot.ShowState(HighlightedInteractable.State.NonInteractable, false, 0.15f);

            (slot as SelectCardFromDeckSlot).SelectFromCards(this.GetValidCards(), delegate
            {
                base.StartCoroutine(this.OnSelectionEnded());
            }, false);
        }

        private IEnumerator OnSelectionEnded()
        {
            leftSelecting = rightSelecting = false;

            this.leftSlot.SetShown(true, false);
            this.leftSlot.ShowState(HighlightedInteractable.State.Interactable, false, 0.15f);
            this.rightSlot.SetShown(true, false);
            this.rightSlot.ShowState(HighlightedInteractable.State.Interactable, false, 0.15f);

            ViewManager.Instance.SwitchToView(View.Default, false, true);
            yield return new WaitForSeconds(0.2f);
            this.DisplayCards();

            this.confirmButton.SetActive(this.leftSlot.Card != null && this.rightSlot.Card != null);

            yield break;
        }

        [HarmonyPatch(typeof(SpecialNodeHandler), "StartSpecialNodeSequence")]
        [HarmonyPrefix]
        public static bool HandleAscensionItems(ref SpecialNodeHandler __instance, SpecialNodeData nodeData)
        {
            if (nodeData is AscensionTransformerCardNodeData)
            {
                if (AscensionTransformerSequence.Instance == null)
                    AscensionTransformerSequence.Instance = __instance.gameObject.AddComponent<AscensionTransformerSequence>();
                
                SpecialNodeHandler.Instance.StartCoroutine(AscensionTransformerSequence.Instance.ExecuteCustomSequence(null));
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Transformer), nameof(Transformer.GetTransformCardInfo))]
        [HarmonyPrefix]
        public static bool GetAndBuildSpecialTransformerCardInfo(ref Transformer __instance, ref CardInfo __result)
        {
            CardModificationInfo cardModificationInfo = __instance.Card.Info.Mods.Find(m => !string.IsNullOrEmpty(m.transformerBeastCardId));
            if (cardModificationInfo != null && cardModificationInfo.transformerBeastCardId[0] == '@')
            {
                string[] split = cardModificationInfo.transformerBeastCardId.Split('+');
                __result = CardLoader.GetCardByName(split[0].Replace("@", ""));
                __result.mods = new ();

                // Beast mod mod
                CardModificationInfo bMod = Transformer.GetBeastModeStatsMod(__result, __instance.Card.Info);
                bMod.nameReplacement = __instance.Card.Info.DisplayedNameLocalized;
                bMod.nonCopyable = true;
                __result.mods.Add(bMod);

                for (int i = 1; i < split.Length; i++)
                {
                    if (split[i].ToLowerInvariant() == "gemified")
                        __result.mods.Add(new() { gemify = true, nonCopyable = true });
                    else
                        __result.mods.Add(new((Ability)Enum.Parse(typeof(Ability), split[i])) { nonCopyable = true });
                }

                // Transformer info
                __result.mods.Add(new(Ability.Transformer) { nonCopyable = true });

                // Set Evolve
                __result.evolveParams = new ();
                __result.evolveParams.evolution = __instance.Card.Info;
                __result.evolveParams.turnsToEvolve = 1;

                return false;
            }
            return true;
        }
    }
}