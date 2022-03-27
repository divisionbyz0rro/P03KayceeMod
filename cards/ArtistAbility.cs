using System;
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
	public class Artist : AbilityBehaviour
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public override int Priority => -10;

        public static readonly Trait CodeTrait = GuidManager.GetEnumValue<Trait>(P03Plugin.PluginGuid, "IsBlockOfCode");

        public static void Register()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Artist";
            info.rulebookDescription = "Adds a random ability to a Code Snippet every turn.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            Artist.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(Artist),
                TextureHelper.GetImageAsTexture("ability_artist.png", typeof(Artist).Assembly)
            ).Id;
        }

		public override bool RespondsToUpkeep(bool playerUpkeep)
        {
            return this.Card.OpponentCard == !playerUpkeep;
        }

        private static int[] preferredSlots = new int[] { 2, 3, 1, 4, 0 };
		public override IEnumerator OnUpkeep(bool playerUpkeep)
		{
            // Look for code on this side of the board

            List<CardSlot> slots = playerUpkeep ? BoardManager.Instance.PlayerSlotsCopy : BoardManager.Instance.OpponentSlotsCopy;

            PlayableCard codeCard = slots.Where(s => s.Card != null).Select(s => s.Card).FirstOrDefault(c => c.Info.HasTrait(CodeTrait));

            int randomSeed = P03AscensionSaveData.RandomSeed + TurnManager.Instance.TurnNumber * 100 + slots.IndexOf(this.Card.slot);
            if (codeCard == null)
            {
                yield break;
            }

            // The code already exists. Now we modify it
            ViewManager.Instance.SwitchToView(View.Board);
            
            // Programmers only modify stats not abilities
            // Artists modify abilities
            this.Card.Anim.SetShaking(true);
            codeCard.Anim.SetShaking(true);
            yield return new WaitForSeconds(1.2f);

            codeCard.Anim.SetOverclocked(true);
            CardModificationInfo newAbilityMod = new (AbilitiesUtil.GetRandomLearnedAbility(randomSeed, true, categoryCriteria: AbilityMetaCategory.BountyHunter));

            // Remove an ability if it has three on it already
            List<CardModificationInfo> abilityMods = codeCard.temporaryMods.Where(m => m.abilities != null && m.abilities.Count > 0).ToList();
            if (abilityMods.Count == 3)
            {
                CardModificationInfo abilityToRemove = abilityMods[SeededRandom.Range(0, abilityMods.Count, randomSeed++)];
                codeCard.temporaryMods.Remove(abilityToRemove);
            }

            codeCard.AddTemporaryMod(newAbilityMod);
            yield return new WaitForSeconds(0.6f);
            ViewManager.Instance.SwitchToView(View.Default);
        }
    }
}
