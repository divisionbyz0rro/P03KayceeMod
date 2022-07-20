using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
	public class NewPermaDeath : AbilityBehaviour
	{
        public static readonly Ability[] NOT_COPYABLE_ABILITIES = new Ability[] {
            Ability.QuadrupleBones,
            Ability.Evolve,
            Ability.IceCube,
            Ability.TailOnHit,
            Ability.PermaDeath
        };

        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static NewPermaDeath()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Skeleclocked";
            info.rulebookDescription = "When [creature] dies, it permanently becomes an Exeskeleton with the same abilities. If [creature] has Unkillable, it will be unaffected.";
            info.canStack = false;
            info.powerLevel = -1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            NewPermaDeath.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(NewPermaDeath),
                TextureHelper.GetImageAsTexture("ability_newpermadeath.png", typeof(NewPermaDeath).Assembly)
            ).Id;
        }

		public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
		{
			return true;
		}

		public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
		{
            if (this.Card.HasAbility(Ability.DrawCopy) || this.Card.HasAbility(Ability.DrawCopyOnDeath))
                yield break;

            if (this.Card.Slot != null && this.Card.HasAbility(CellUndying.AbilityID) && ConduitCircuitManager.Instance.SlotIsWithinCircuit(this.Card.Slot))
                yield break;

            // Create an exeskeleton
            DeckInfo deck = SaveManager.SaveFile.CurrentDeck;

            CardInfo card = deck.Cards.Find((CardInfo x) => x.HasAbility(NewPermaDeath.AbilityID) && x.name == this.Card.Info.name);

            // If there is no card with this name in your deck, it's probably because it's a transformer and it's
            // currently on its other side
            if (card == null && this.Card.HasAbility(Ability.Transformer) && this.Card.Info.evolveParams != null)
                card = deck.Cards.Find(x => x.HasAbility(NewPermaDeath.AbilityID) && x.name == this.Card.Info.evolveParams.evolution.name);

            // If the card is STILL null, then congratulations - you've managed to find some sort of weird edge case.
            // We will just let the game play out without erroring
            if (card == null)
                yield break;

            CardInfo replacement = CardLoader.GetCardByName("RoboSkeleton");
            CardModificationInfo mod = new ();
            mod.abilities = new (card.Abilities.Where(ab => ab != NewPermaDeath.AbilityID && !NOT_COPYABLE_ABILITIES.Contains(ab)).Take(3));
            replacement.mods.Add(mod);
            deck.AddCard(replacement);

			deck.RemoveCard(card);
			yield return base.LearnAbility(0.5f);
			yield break;
		}

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.HasAbility))]
        [HarmonyPrefix]
        public static bool PretendHasPermadeath(Ability ability, ref PlayableCard __instance, ref bool __result)
        {
            if (ability == Ability.PermaDeath && __instance.HasAbility(NewPermaDeath.AbilityID))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
