using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class TripleCardStrike : AbilityBehaviour, IGetOpposingSlots
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static TripleCardStrike()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Big Strike";
            info.rulebookDescription = "[creature] attacks all cards in all lanes opposing it, or attacks just the center opposing lane if there are no cards opposite it.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.flipYIfOpponent = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            TripleCardStrike.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(TripleCardStrike),
                TextureHelper.GetImageAsTexture("ability_tricard_strike.png", typeof(TripleCardStrike).Assembly)
            ).Id;
        }

        public List<CardSlot> GetOpposingSlots(List<CardSlot> originalSlots, List<CardSlot> otherAddedSlots)
        {
            List<CardSlot> retval = new();
            
            int slot = this.Card.Slot.Index;
            List<CardSlot> opposingSlots = BoardManager.Instance.GetSlots(this.Card.OpponentCard);
            if (slot > 0 && opposingSlots[slot - 1].Card != null)
                retval.Add(opposingSlots[slot - 1]);
            if (opposingSlots[slot].Card != null)
                retval.Add(opposingSlots[slot]);
            if (slot + 1 < opposingSlots.Count && opposingSlots[slot + 1].Card != null)
                retval.Add(opposingSlots[slot + 1]);

            return retval;
        }

        public bool RemoveDefaultAttackSlot()
        {
            int slot = this.Card.Slot.Index;
            List<CardSlot> opposingSlots = BoardManager.Instance.GetSlots(this.Card.OpponentCard);
            if (slot > 0 && opposingSlots[slot - 1].Card != null)
                return true;
            if (slot + 1 < opposingSlots.Count && opposingSlots[slot + 1].Card != null)
                return true;
            if (opposingSlots[slot].Card != null)
                return true;
            return false;
        }

        public bool RespondsToGetOpposingSlots() => true;
    }
}