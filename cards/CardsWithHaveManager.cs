using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class CardsWithAbilityHaveAbilityManager : Singleton<CardsWithAbilityHaveAbilityManager>
    {
        public const string RuleKey = "CardWith";

        public struct Rule
        {
            public Ability requiredAbility;
            public Ability gainedAbility;
            public string modId;

            public Rule(Ability required, Ability gained)
            {
                requiredAbility = required;
                gainedAbility = gained;
                modId = $"{RuleKey}{required}Gains{gained}";
            }
        }

        private List<Rule> PlayerRules = new();
        private List<Rule> OpponentRules = new();

        private void ApplyAbilities(List<Rule> rules, List<CardSlot> slots)
        {
            foreach (CardSlot slot in slots.Where(s => s.Card != null))
            {
                foreach (Rule rule in rules)
                {
                    if (slot.Card.HasAbility(rule.requiredAbility))
                    {
                        if (!slot.Card.TemporaryMods.Any(m => m.singletonId.Equals(rule.modId)))
                        {
                            CardModificationInfo info = new(rule.gainedAbility);
                            info.singletonId = rule.modId;
                            slot.Card.AddTemporaryMod(info);
                        }
                    }
                }

                // Remove all where there is no longer a rule for it
                // Remove all where the rule no longer applies to this card
                List<CardModificationInfo> modsToRemove = slot.Card.temporaryMods.Where(m => 
                    !string.IsNullOrEmpty(m.singletonId) &&
                    m.singletonId.StartsWith(RuleKey) &&
                    !rules.Any(r => r.modId.Equals(m.singletonId) && slot.Card.HasAbility(r.requiredAbility))
                ).ToList();
                foreach (CardModificationInfo mod in modsToRemove)
                    slot.Card.RemoveTemporaryMod(mod);
            }
        }

        private void DiscoverAbilities()
        {
            PlayerRules.Clear();
            OpponentRules.Clear();

            foreach (CardSlot slot in BoardManager.Instance.playerSlots.Concat(BoardManager.Instance.opponentSlots))
            {
                if (slot.Card != null)
                {
                    foreach (var abilityComb in slot.Card.GetComponents<CardsWithAbilityHaveAbility>())
                    {
                        if ((abilityComb.AppliesToFriendly && !slot.Card.OpponentCard) ||
                            (abilityComb.AppliesToOpposing && slot.Card.OpponentCard))
                            PlayerRules.Add(new(abilityComb.RequiredAbility, abilityComb.GainedAbility));

                        if ((abilityComb.AppliesToFriendly && slot.Card.OpponentCard) ||
                            (abilityComb.AppliesToOpposing && !slot.Card.OpponentCard))
                            OpponentRules.Add(new(abilityComb.RequiredAbility, abilityComb.GainedAbility));
                    }
                }
            }
        }

        public override void ManagedUpdate()
        {
            // Find all cards that have a 'cardswithhave' ability on them
            DiscoverAbilities();
            ApplyAbilities(PlayerRules, BoardManager.Instance.playerSlots);
            ApplyAbilities(OpponentRules, BoardManager.Instance.opponentSlots);
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPrefix]
        private static void CreateManager()
        {
            if (CardsWithAbilityHaveAbilityManager.Instance == null)
                TurnManager.Instance.gameObject.AddComponent<CardsWithAbilityHaveAbilityManager>();
        }
    }
}