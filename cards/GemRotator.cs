using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;
using System.Collections.Generic;
using InscryptionAPI.Triggers;
using InscryptionAPI.Helpers;
using System.Collections;
using System;
using System.Linq;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class GemRotator : AbilityBehaviour, IOnUpkeepInHand
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        private const string SINGLETON_ID = "GemRotationTemporaryMod";
        private static readonly List<Ability> GemAbilities = new() { Ability.GainGemGreen, Ability.GainGemBlue, Ability.GainGemOrange };

        static GemRotator()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Rotating Gem";
            info.rulebookDescription = "When [creature] is drawn, this becomes a random gem that changes at the start of every turn.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            GemRotator.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(GemRotator),
                TextureHelper.GetImageAsTexture("ability_gemcycler.png", typeof(GemRotator).Assembly)
            ).Id;
        }

        public override bool RespondsToDrawn()
		{
			return true;
		}

		public override IEnumerator OnDrawn()
		{
            if (this.Card.InHand)
            {
			    yield return base.Card.FlipInHand(new Action(this.AddMod));
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                base.Card.SetFaceDown(true, false);
                yield return new WaitForSeconds(0.3f);
                this.AddMod();
                base.Card.SetFaceDown(false, false);
                yield return new WaitForSeconds(0.3f);
            }
            base.Card.Anim.LightNegationEffect();
			yield return base.LearnAbility(0.5f);
			yield break;
		}

        private static bool IsTargetMod(CardModificationInfo mod)
        {
            return String.Equals(mod.singletonId, SINGLETON_ID);
        }

		private void AddMod()
		{
			this.Card.Status.hiddenAbilities.Add(this.Ability);
            CardModificationInfo mod = this.Card.TemporaryMods.FirstOrDefault(IsTargetMod);
			if (mod == null)
            {
                mod = new();
                int randomSeed = SaveManager.SaveFile.GetCurrentRandomSeed() + TurnManager.Instance.TurnNumber;
                mod.abilities.Add(GemAbilities[SeededRandom.Range(0, GemAbilities.Count, randomSeed)]);
                mod.singletonId = SINGLETON_ID;
            } else {
                base.Card.RemoveTemporaryMod(mod);
                int index = GemAbilities.IndexOf(mod.abilities[0]) + 1;
                if (index == GemAbilities.Count)
                    index = 0;
                
                mod = new (GemAbilities[index]);
                mod.singletonId = SINGLETON_ID;
            }
            
            base.Card.AddTemporaryMod(mod);

            // Make sure the gems actually update in the resources manager.
            ResourcesManager manager = ResourcesManager.Instance;
            if (manager != null)
                manager.ForceGemsUpdate();
		}

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep;
        public bool RespondsToUpkeepInHand(bool playerUpkeep) => playerUpkeep;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            yield return OnUpkeepInHand(playerUpkeep);
        }

        public IEnumerator OnUpkeepInHand(bool playerUpkeep)
        {
            yield return OnDrawn();
        }
    }
}