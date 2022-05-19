using HarmonyLib;
using DiskCardGame;
using System.Collections;
using Infiniscryption.P03KayceeRun.Patchers;
using Pixelplacement;
using System.Collections.Generic;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    internal static class MycologistsTradePatches
    {
        [HarmonyPatch(typeof(TradeCardsSequencer), nameof(TradeCardsSequencer.TradeCardsSequence))]
        [HarmonyPostfix]
        private static IEnumerator NewDialogueOne(IEnumerator sequence, TradeCardsSequencer __instance, TradeCardsNodeData tradeCardsData)
        {
            if (!SaveFile.IsAscension || EventManagement.CurrentZone != RunBasedHoloMap.Zone.Mycologist)
            {
                yield return sequence;
                yield break;
            }

            yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistTakeOne", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return __instance.ChooseCards(tradeCardsData);
        }

        [HarmonyPatch(typeof(TradeCardsSequencer), nameof(TradeCardsSequencer.ChooseCardToGiveAndCompleteTrade))]
        [HarmonyPostfix]
        private static IEnumerator UpdateTradeData(IEnumerator sequence, TradeCardsSequencer __instance, SelectableCard cardToTake)
        {
            if (!SaveFile.IsAscension || EventManagement.CurrentZone != RunBasedHoloMap.Zone.Mycologist)
            {
                yield return sequence;
                yield break;
            }

            RuleBookController.Instance.SetShown(false, true);

            // CHANGE FROM COPIED CODE:
            // Say something different
            yield return TextDisplayer.Instance.PlayDialogueEvent("MycologistGiveBack", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            
            ViewManager.Instance.SwitchToView(View.Choices, false, true);
            Tween.Position(cardToTake.transform, __instance.selectedCardPosMarker.position, 0.2f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            Tween.Rotation(cardToTake.transform, new Vector3(85f, 14.5f, 0f), 0.2f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            SelectableCard selectedCard = null;
            List<CardInfo> cards = new List<CardInfo>(Part3SaveData.Data.deck.Cards);

            // CHANGE FROM COPIED CODE:
            // Cannot select a previously selected experiment
            cards.RemoveAll(ci => ci.name.Equals(CustomCards.FAILED_EXPERIMENT_BASE));

            yield return __instance.cardArray.SelectCardFrom(cards, __instance.deckPile, delegate(SelectableCard c)
            {
                selectedCard = c;
            }, null, true);
            Tween.Rotation(cardToTake.transform, new Vector3(90f, 0f, 0f), 0.1f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            CardInfo info = selectedCard.Info;

            // CHANGE FROM COPIED CODE:
            // Record the selected traded card
            EventManagement.AddMycologistsTestSubject(info);

            selectedCard.SetInteractionEnabled(false);
            Tween.Position(selectedCard.transform, selectedCard.transform.position + Vector3.forward * 8f, 0.25f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, delegate()
            {
                Object.Destroy(selectedCard.gameObject);
            }, true);
            Part3SaveData.Data.deck.RemoveCard(info);
            yield return new WaitForSeconds(0.25f);
            yield break;
        }
    }
}