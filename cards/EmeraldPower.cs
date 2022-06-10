using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class EmeraldPower : AbilityBehaviour, IPassiveHealthBuff
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static EmeraldPower()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Emerald Blessing";
            info.rulebookDescription = "[creature] provides +2 Health to all creatures you control.";
            info.canStack = false;
            info.powerLevel = 5;
            info.opponentUsable = true;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.GainGemGreen).Info.colorOverride;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            EmeraldPower.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(EmeraldPower),
                TextureHelper.GetImageAsTexture("ability_emerald_power.png", typeof(EmeraldPower).Assembly)
            ).Id;
        }

        public int GetPassiveHealthBuff(PlayableCard target)
        {
            return this.Card.OnBoard && target.OpponentCard == this.Card.OpponentCard ? 2 : 0;
        }
    }
}