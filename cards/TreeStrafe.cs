using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class TreeStrafe : Strafe
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static TreeStrafe()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Seed Strafe";
            info.rulebookDescription = "At the end of its controller's turn, [creature] moves one space in the direction indicated (if it can) and leaves behind a tree.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            TreeStrafe.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(TreeStrafe),
                TextureHelper.GetImageAsTexture("ability_treestrafe.png", typeof(TreeStrafe).Assembly)
            ).Id;
        }

        public override IEnumerator PostSuccessfulMoveSequence(CardSlot cardSlot)
		{
			if (cardSlot.Card == null)
			{
				yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName("Tree_Hologram"), cardSlot, 0.1f, true);
			}
			yield break;
		}
    }
}