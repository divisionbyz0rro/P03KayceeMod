using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class FullOfOil : AbilityBehaviour
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        internal static List<CardSlot> BuffedSlots = new();

        static FullOfOil()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Full of Oil";
            info.rulebookDescription = "When [creature] dies, it adds 2 health to the creature on either side and across from it.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            FullOfOil.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(FullOfOil),
                TextureHelper.GetImageAsTexture("ability_full_of_oil.png", typeof(FullOfOil).Assembly)
            ).Id;
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            List<CardSlot> targets = new();
            targets.AddRange(BoardManager.Instance.GetAdjacentSlots(this.Card.Slot));
            targets.Add(this.Card.Slot.opposingSlot);
            targets.RemoveAll(s => s == null || s.Card == null);

            if (targets.Count == 0)
                yield break;

            ViewManager.Instance.SwitchToView(View.Board, false, false);
            yield return new WaitForSeconds(0.15f);

            foreach (CardSlot slot in targets)
            {
                this.Card.Anim.StrongNegationEffect();
                slot.Card.Anim.StrongNegationEffect();
                yield return new WaitForSeconds(0.25f);
                slot.Card.TemporaryMods.Add(new(0, 2));
                yield return new WaitForSeconds(0.25f);
            }

            ViewManager.Instance.SwitchToView(View.Default, false, false);

            yield break;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        
    }
}
