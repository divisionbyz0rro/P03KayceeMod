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
        public virtual bool AppliesToOpposing => false;
        public virtual bool AppliesToFriendly => true;

        // internal string ModId
        // {
        //     get { return $"CardWith{this.RequiredAbility}Gains{this.GainedAbility}"; }
        // }

        // private void RemoveAllExistingTempMods()
        // {
        //     RemoveFromOpponent();

        //     List<CardSlot> slots = this.Card.OpponentCard ? BoardManager.Instance.OpponentSlotsCopy : BoardManager.Instance.PlayerSlotsCopy;

        //     // Now we need to remove any temp mods for cards on the this side of board that might have it
        //     // UNLESS THERE'S ANOTHER THING THAT GRANTS IT
        //     bool hasAnother = slots
        //                       .Where(s => s.Card != null && s.Card != this.Card)
        //                       .SelectMany(s => s.Card.gameObject.GetComponents<CardsWithAbilityHaveAbility>())
        //                       .Any(c => c.RequiredAbility == this.RequiredAbility && c.GainedAbility == this.GainedAbility);

        //     if (!hasAnother)
        //     {
        //         foreach (CardSlot slot in slots)
        //         {
        //             if (slot.Card != null)
        //             {
        //                 CardModificationInfo mod = slot.Card.TemporaryMods.FirstOrDefault(m => m.singletonId.Equals(this.ModId));
        //                 if (mod != null)
        //                 {
        //                     slot.Card.RemoveTemporaryMod(mod, true);
        //                 }
        //             }
        //         }
        //     }
        // }

        // private void RemoveFromOpponent()
        // {
        //     List<CardSlot> otherSlots = !this.Card.OpponentCard ? BoardManager.Instance.OpponentSlotsCopy : BoardManager.Instance.PlayerSlotsCopy;
        //     // Now we need to remove any temp mods for cards on the other side of board that might have it
        //     // Due to, say, conveyor or rotation of the board for any other reason
        //     bool opponentHas = otherSlots
        //                        .Where(s => s.Card != null)
        //                        .SelectMany(s => s.Card.gameObject.GetComponents<CardsWithAbilityHaveAbility>())
        //                        .Any(c => c.RequiredAbility == this.RequiredAbility && c.GainedAbility == this.GainedAbility);

        //     if (!opponentHas)
        //     {
        //         foreach (CardSlot slot in otherSlots)
        //         {
        //             if (slot.Card != null)
        //             {
        //                 CardModificationInfo mod = slot.Card.TemporaryMods.FirstOrDefault(m => m.singletonId.Equals(this.ModId));
        //                 if (mod != null)
        //                 {
        //                     slot.Card.RemoveTemporaryMod(mod, true);
        //                 }
        //             }
        //         }
        //     }
        // }

        // private void SetAppropriateTempMods()
        // {
        //     List<CardSlot> slots = this.Card.OpponentCard ? BoardManager.Instance.OpponentSlotsCopy : BoardManager.Instance.PlayerSlotsCopy;

        //     foreach (CardSlot slot in slots)
        //     {
        //         if (slot.Card != null && slot.Card.HasAbility(RequiredAbility))
        //         {
        //             CardModificationInfo mod = new (this.GainedAbility);
        //             mod.singletonId = this.ModId;
        //             slot.Card.AddTemporaryMod(mod);
        //         }
        //     }
            
        //     RemoveFromOpponent();
        // }

        // public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard) => true;
        
        // public override IEnumerator OnOtherCardAssignedToSlot(PlayableCard otherCard)
        // {
        //     SetAppropriateTempMods();
        //     yield break;
        // }

        // public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        // public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        // {
        //     RemoveAllExistingTempMods();
        //     yield break;
        // }
    }
}