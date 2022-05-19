using System;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
	public class CellDeEvolve : Evolve
	{
		public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static CellDeEvolve()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Transforms When Unpowered";
            info.rulebookDescription = "If [creature] is NOT within a circuit at the beginning of the turn, it will transform back its original form.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.conduitCell = true;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.CellDrawRandomCardOnDeath).Info.colorOverride;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            CellDeEvolve.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(CellDeEvolve),
                TextureHelper.GetImageAsTexture("ability_celldevolve.png", typeof(CellDeEvolve).Assembly)
            ).Id;
        }

		public override bool RespondsToUpkeep(bool playerUpkeep)
        {
            return base.RespondsToUpkeep(playerUpkeep) && !ConduitCircuitManager.Instance.SlotIsWithinCircuit(this.Card.Slot);
        }
	}
}
