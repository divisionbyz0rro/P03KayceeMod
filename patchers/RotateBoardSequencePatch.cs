using HarmonyLib;
using DiskCardGame;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    internal static class RotateBoardSequencePatch
    {
        private static CardSlot GetClockwiseSlot(CardSlot slot)
        {
            if (slot.IsPlayerSlot)
            {
                if (slot.Index == 0)
                    return BoardManager.Instance.opponentSlots[0];
                else
                    return BoardManager.Instance.playerSlots[slot.Index - 1];
            }
            if (slot.Index + 1 >= BoardManager.Instance.opponentSlots.Count)
                return BoardManager.Instance.playerSlots[BoardManager.Instance.playerSlots.Count - 1];
            else
                return BoardManager.Instance.opponentSlots[slot.Index + 1];
        }

        private static CardSlot GetDestinationForSlot(CardSlot slot, List<CardSlot> playerSlots, List<CardSlot> opponentSlots)
        {
            int index = slot.Index;
            CardSlot currentInvestigatedSlot = GetClockwiseSlot(slot);
            List<CardSlot> currentSlots = playerSlots.Contains(slot) ? playerSlots : opponentSlots;
            while (currentInvestigatedSlot != slot)
            {
                if (currentInvestigatedSlot.Card == null || !currentInvestigatedSlot.Card.Info.HasTrait(CustomCards.Unrotateable))
                    return currentInvestigatedSlot;

                currentInvestigatedSlot = GetClockwiseSlot(currentInvestigatedSlot);
            }
            return slot;
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.MoveAllCardsClockwise))]
        [HarmonyPostfix]
        private static IEnumerator MoveAllCardsClockwiseAccountForUnrotateable(IEnumerator sequence)
        {
            if (BoardManager.Instance.AllSlots.Any((CardSlot x) => x.Card != null && x.Card.Info.HasTrait(CustomCards.Unrotateable)))
            {
                Dictionary<PlayableCard, CardSlot> cardDestinations = new Dictionary<PlayableCard, CardSlot>();
                List<CardSlot> playerSlots = BoardManager.Instance.GetSlots(true);
                List<CardSlot> opponentSlots = BoardManager.Instance.GetSlots(false);
                foreach (CardSlot slot in BoardManager.Instance.AllSlots)
                    if (slot.Card != null && !slot.Card.Info.HasTrait(CustomCards.Unrotateable))
                        cardDestinations.Add(slot.Card, GetDestinationForSlot(slot, playerSlots, opponentSlots));

                foreach (CardSlot cardSlot in BoardManager.Instance.AllSlots)
                {
                    if (cardSlot.Card != null && !cardSlot.Card.Info.HasTrait(CustomCards.Unrotateable))
                    {
                        cardSlot.Card.Slot = null;
                        cardSlot.Card = null;
                    }
                }
                foreach (KeyValuePair<PlayableCard, CardSlot> assignment in cardDestinations)
                {
                    PlayableCard card = assignment.Key;
                    assignment.Key.SetIsOpponentCard(!assignment.Value.IsPlayerSlot);
                    yield return BoardManager.Instance.AssignCardToSlot(card, assignment.Value, 0.1f, null, true);
                    if (card.FaceDown)
                    {
                        bool flag = assignment.Value.Index == 0 && !assignment.Value.IsPlayerSlot;
                        bool flag2 = assignment.Value.Index == BoardManager.Instance.GetSlots(false).Count - 1 && assignment.Value.IsPlayerSlot;
                        if (flag || flag2)
                        {
                            card.SetFaceDown(false, false);
                            card.UpdateFaceUpOnBoardEffects();
                        }
                    }
                }
                Singleton<ResourcesManager>.Instance.ForceGemsUpdate();
            }
            else
            {
                yield return sequence;
            }
            yield break;
        }
    }
}