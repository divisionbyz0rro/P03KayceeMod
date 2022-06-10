using HarmonyLib;
using DiskCardGame;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public abstract class ConduitGainAbility : Conduit
    {
        protected abstract Ability AbilityToGive { get; }

        internal static List<ConduitGainAbility> ActiveAbilities = new();

        public override bool RespondsToResolveOnBoard() => true;

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        public override IEnumerator OnResolveOnBoard()
        {
            ActiveAbilities.Add(this);
            yield return base.OnResolveOnBoard();
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            ActiveAbilities.Remove(this);
            yield return base.OnResolveOnBoard();
        }

        private static Dictionary<PlayableCard, CardModificationInfo> ConduitAbilityMods = new();

        private static CardModificationInfo GetConduitAbilityMod(PlayableCard card)
        {
            CardModificationInfo mod = null;
            ConduitAbilityMods.TryGetValue(card, out mod);

            if (mod == null)
            {
                mod = new ();
                mod.singletonId = "ConduitGainAbilityMod";
                card.AddTemporaryMod(mod);
                ConduitAbilityMods.Add(card, mod);
            }

            return mod;
        }

        private static void ClearConduitAbilityMods(PlayableCard card)
        {
            if (ConduitAbilityMods.ContainsKey(card))
            {
                CardModificationInfo info = ConduitAbilityMods[card];
                card.RemoveTemporaryMod(info);
                ConduitAbilityMods.Remove(card);
                card.UpdateFaceUpOnBoardEffects();
            }
        }

        private static List<Ability> GetConduitAbilitiesForSlot(CardSlot slot)
        {
            List<Ability> retval = new();
            List<PlayableCard> conduits = ConduitCircuitManager.Instance.GetConduitsForSlot(slot);
            foreach (ConduitGainAbility ability in ActiveAbilities.Where(ab => ab != null))
                foreach (PlayableCard card in conduits)
                    if (ability.Card == card)
                        retval.Add(ability.AbilityToGive);

            return retval;
        }

        private static bool Match(List<Ability> a, List<Ability> b)
        {
            var anotb = a.Except(b).ToList();
            var bnota = b.Except(a).ToList();
            return !anotb.Any() && !bnota.Any();
        }

        private static void ResolveForSlots(List<CardSlot> slots)
        {
            foreach (CardSlot slot in slots.Where(s => s.Card != null))
            {
                List<Ability> conduitAbilities = GetConduitAbilitiesForSlot(slot);
                CardModificationInfo info = GetConduitAbilityMod(slot.Card);

                if (!Match(conduitAbilities, info.abilities))
                {
                    info.abilities.Clear();
                    info.abilities.AddRange(conduitAbilities);
                    slot.Card.AddTemporaryMod(info);
                    slot.Card.UpdateFaceUpOnBoardEffects();
                }
            }
        }

        private static void FixCardsNotOnBoard()
        {
            foreach (PlayableCard card in PlayerHand.Instance.CardsInHand)
                ClearConduitAbilityMods(card);

            foreach (PlayableCard card in TurnManager.Instance.Opponent.queuedCards)
                ClearConduitAbilityMods(card);
        }

        private static void ClearAllCards()
        {
            FixCardsNotOnBoard();
            foreach (PlayableCard card in BoardManager.Instance.playerSlots.Where(s => s.Card != null).Select(s => s.Card))
                ClearConduitAbilityMods(card);

            foreach (PlayableCard card in BoardManager.Instance.opponentSlots.Where(s => s.Card != null).Select(s => s.Card))
                ClearConduitAbilityMods(card);
        }

        private static void CleanList()
        {
            ActiveAbilities.RemoveAll(ab => ab == null);
        }

        [HarmonyPatch(typeof(ConduitCircuitManager), nameof(ConduitCircuitManager.ManagedUpdate))]
        [HarmonyPostfix]
        private static void ManageAllActiveAbilityMods()
        {
            if (!GameFlowManager.Instance || GameFlowManager.Instance.CurrentGameState != GameState.CardBattle)
                return;

            CleanList();

            if (ActiveAbilities.Count == 0)
                ClearAllCards();

            else
            {
                ResolveForSlots(BoardManager.Instance.opponentSlots);
                ResolveForSlots(BoardManager.Instance.playerSlots);
                FixCardsNotOnBoard();
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPostfix]
        private static void CleanupActiveAbilities()
        {
            ActiveAbilities.Clear();
        }
    }
}