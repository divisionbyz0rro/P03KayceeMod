using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class LatchDeathLatch : Latch
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override Ability LatchAbility => AbilityID;

        static LatchDeathLatch()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Death Latch";
            info.rulebookDescription = "[creature] will die at the end of the turn. When [creature] perishes, its owner chooses a creature to gain this sigil. This will repeat.";
            info.canStack = false;
            info.powerLevel = -1;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            LatchDeathLatch.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(LatchDeathLatch),
                TextureHelper.GetImageAsTexture("ability_latch_death.png", typeof(LatchDeathLatch).Assembly)
            ).Id;
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd)
        {
            return this.Card.OpponentCard != playerTurnEnd;
        }

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            yield return this.Card.Die(false, null, true);
        }
    }
}