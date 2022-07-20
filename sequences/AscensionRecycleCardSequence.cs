using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class AscensionRecycleCardSequence: RecycleCardSequencer
    {
        public static AscensionRecycleCardSequence Instance { get; private set; } 

        public static bool ShouldOverrideCardDisplayer { get; private set; }

        public AscensionRecycleCardSequence()
        {
            this.cardArray = SpecialNodeHandler.Instance.recycleCardSequencer.cardArray;
            this.deckPile = SpecialNodeHandler.Instance.recycleCardSequencer.deckPile;
            this.recycleMachine = SpecialNodeHandler.Instance.recycleCardSequencer.recycleMachine;
        }

        private GameObject _selectableCardPrefab;
        private GameObject SelectableCardPrefab
        {
            get
            {
                if (_selectableCardPrefab == null)
                    _selectableCardPrefab = SpecialNodeHandler.Instance.buildACardSequencer.selectableCardPrefab;

                return _selectableCardPrefab;
            }
        }

        private Transform _cardExamineMarker;
        private Transform CardExamineMarker
        {
            get
            {
                if (_cardExamineMarker == null)
                    _cardExamineMarker = SpecialNodeHandler.Instance.buildACardSequencer.cardExamineMarker;

                return _cardExamineMarker;
            }
        }

        [HarmonyPatch(typeof(SpecialNodeHandler), "StartSpecialNodeSequence")]
        [HarmonyPrefix]
        public static bool HandleAscensionItems(ref SpecialNodeHandler __instance, SpecialNodeData nodeData)
        {
            if (nodeData is AscensionRecycleCardNodeData)
            {
                if (AscensionRecycleCardSequence.Instance == null)
                    AscensionRecycleCardSequence.Instance = __instance.gameObject.AddComponent<AscensionRecycleCardSequence>();
                
                SpecialNodeHandler.Instance.StartCoroutine(AscensionRecycleCardSequence.Instance.RecycleCardForDraftTokenSequence());
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(RecycleMachine), nameof(RecycleMachine.DisplayCard))]
        [HarmonyPostfix]
        public static void DisplayTokenString(ref RecycleMachine __instance, CardInfo card, int currencyValue, string format)
        {
            if (ShouldOverrideCardDisplayer)
            {
                if (card.metaCategories.Contains(CardMetaCategory.Rare))
                {
                    if (card.ModAbilities.Count > 0)
                        __instance.currencyText.text = "RARE++";
                    else
                        __instance.currencyText.text = "RARE";
                }
                else if (card.Gemified || card.mods.Count > 0)
                    __instance.currencyText.text = "TKN++";
                else
                    __instance.currencyText.text = "TKN";
            }
        }

        [HarmonyPatch(typeof(RecycleCardSequencer), nameof(RecycleCardSequencer.GetCardStatPointsValue))]
        [HarmonyPrefix]
        public static bool GetSPForAscension(CardInfo info, ref int __result)
        {
            if (ShouldOverrideCardDisplayer)
            {
                if (info.metaCategories.Contains(CardMetaCategory.Rare))
                {
                    if (info.ModAbilities.Count > 0)
                        __result = 4;
                    else
                        __result = 3;
                }
                else if (info.Gemified || info.mods.Count > 0)
                    __result = 2;
                else
                    __result = 1;
                return false;
            }
            return true;
        }

        private CardInfo GetCardInfo()
        {
            if (this.selectedCardValue >= 3)
            {
                CardInfo rareCard = CardLoader.GetCardByName(CustomCards.RARE_DRAFT_TOKEN);
                if (this.selectedCardInfo.ModAbilities.Count > 0)
                {
                    CardModificationInfo cardMod = new();
                    cardMod.abilities = new List<Ability>(this.selectedCardInfo.ModAbilities);
                    rareCard.mods.Add(cardMod);
                }
                return rareCard;
            }

            CardInfo baseCard = CardLoader.GetCardByName(CustomCards.DRAFT_TOKEN);
            
            List<Ability> abilities = this.selectedCardInfo.Abilities;
            if (abilities.Count > 0)
            {
                CardModificationInfo cardMod = new();
                cardMod.abilities = new List<Ability>(abilities);
                baseCard.mods.Add(cardMod);
            }

            return baseCard;
        }

        public IEnumerator RecycleCardForDraftTokenSequence()
        {
            ShouldOverrideCardDisplayer = true;
            this.cardValueMode = RecycleCardSequencer.CardValueMode.StatPoints;

            yield return TextDisplayer.Instance.PlayDialogueEvent("P03AscensionRecycle", TextDisplayer.MessageAdvanceMode.Input);
            yield return this.InitializeRecycleMachine();
            yield return new WaitForSeconds(1.25f);
            yield return this.SelectAndRecycleCard();
            this.recycleMachine.OpenHatch();
            yield return new WaitForSeconds(0.5f);

            GameObject cardGO = UnityEngine.Object.Instantiate<GameObject>(this.SelectableCardPrefab);
            SelectableCard card = cardGO.GetComponent<SelectableCard>();
            
            CardInfo cardInfo = GetCardInfo();

            card.Initialize(cardInfo);
            card.SetEnabled(false);
            card.SetInteractionEnabled(false);
            card.transform.position = new Vector3(-1f, 10f, 0.0f);
            Tween.Position(card.transform, this.CardExamineMarker.position, 0.25f, 0.0f, Tween.EaseOut);
            Tween.Rotation(card.transform, this.CardExamineMarker.rotation, 0.25f, 0.0f, Tween.EaseOut);

            yield return TextDisplayer.Instance.PlayDialogueEvent($"P03AscensionToken{this.selectedCardValue}", TextDisplayer.MessageAdvanceMode.Input);
            card.ExitBoard(0.25f, Vector3.zero);

            Part3SaveData.Data.deck.AddCard(cardInfo);

            yield return new WaitForSeconds(0.25f);
            this.recycleMachine.CloseHatch();
            yield return new WaitForSeconds(0.4f);
            yield return this.CleanupRecycleMachine();

            yield return new WaitForSeconds(0.6f);

            ShouldOverrideCardDisplayer = false;

            if (GameFlowManager.Instance != null)
                GameFlowManager.Instance.TransitionToGameState(GameState.Map);
        }
    }
}