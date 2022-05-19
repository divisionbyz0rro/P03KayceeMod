using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public abstract class CardsWithAbilityHaveAbility : AbilityBehaviour
    {
        public abstract Ability RequiredAbility { get; }
        public abstract Ability GainedAbility { get; }

        private void RemoveAllExistingTempMods()
        {
            foreach (CardSlot slot in BoardManager.Instance.PlayerSlotsCopy.Concat(BoardManager.Instance.OpponentSlotsCopy))
            {
                if (slot.Card != null)
                {
                    CardModificationInfo mod = slot.Card.TemporaryMods.FirstOrDefault(m => m.HasAbility(GainedAbility));
                    if (mod != null)
                    {
                        slot.Card.temporaryMods.Remove(mod);
                        slot.Card.SetInfo(slot.Card.Info);
                    }
                }
            }
        }

        private void SetAppropriateTempMods()
        {
            List<CardSlot> slots = this.Card.OpponentCard ? BoardManager.Instance.OpponentSlotsCopy : BoardManager.Instance.PlayerSlotsCopy;
            foreach (CardSlot slot in slots)
                if (slot.Card != null && slot.Card.HasAbility(RequiredAbility))
                {
                    (slot.Card.temporaryMods ??= new()).Add(new (GainedAbility));
                    slot.Card.SetInfo(slot.Card.Info);
                }
        }

        public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard) => true;
        
        public override IEnumerator OnOtherCardAssignedToSlot(PlayableCard otherCard)
        {
            RemoveAllExistingTempMods();
            SetAppropriateTempMods();
            yield break;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            RemoveAllExistingTempMods();
            yield break;
        }
    }
}