using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class BrittleGainsUndying : CardsWithAbilityHaveAbility
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override Ability RequiredAbility => Ability.Brittle;

        public override Ability GainedAbility => Ability.DrawCopyOnDeath;

        static BrittleGainsUndying()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Brittle Lord";
            info.rulebookDescription = "As long as [creature] is alive, cards with Brittle have Unkillable.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            BrittleGainsUndying.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(BrittleGainsUndying),
                TextureHelper.GetImageAsTexture("ability_brittle_gains_deathless.png", typeof(BrittleGainsUndying).Assembly)
            ).Id;
        }
    }
}