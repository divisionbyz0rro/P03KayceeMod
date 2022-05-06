using System;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
	public class CellEvolve : Evolve
	{
		public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static CellEvolve()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Transforms When Powered";
            info.rulebookDescription = "If [creature] is within a circuit at the beginning of the turn, it will transform into a stronger form.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.conduitCell = true;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.CellDrawRandomCardOnDeath).Info.colorOverride;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            CellEvolve.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(CellEvolve),
                TextureHelper.GetImageAsTexture("ability_cellevolve.png", typeof(CellEvolve).Assembly)
            ).Id;
        }

		public override bool RespondsToUpkeep(bool playerUpkeep)
        {
            return base.RespondsToUpkeep(playerUpkeep) && ConduitCircuitManager.Instance.SlotIsWithinCircuit(this.Card.Slot);
        }
	}
}
