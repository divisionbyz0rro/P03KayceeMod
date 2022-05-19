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

            // Create an exeskeleton
            DeckInfo deck = SaveManager.SaveFile.CurrentDeck;

            CardInfo replacement = CardLoader.GetCardByName("RoboSkeleton");
            CardModificationInfo mod = new ();
            mod.abilities = new (this.Card.Info.Abilities.Where(ab => ab != NewPermaDeath.AbilityID && !NOT_COPYABLE_ABILITIES.Contains(ab)).Take(3));
            replacement.mods.Add(mod);
            deck.AddCard(replacement);

			CardInfo card = deck.Cards.Find((CardInfo x) => x.HasAbility(NewPermaDeath.AbilityID) && x.name == base.Card.Info.name);
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
