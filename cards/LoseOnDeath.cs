using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class LoseOnDeath : AbilityBehaviour
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static LoseOnDeath()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Synchronized Fate";
            info.rulebookDescription = "When [creature] dies, you lose the game.";
            info.canStack = false;
            info.powerLevel = -5;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            LoseOnDeath.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(LoseOnDeath),
                TextureHelper.GetImageAsTexture("ability_loseondeath.png", typeof(LoseOnDeath).Assembly)
            ).Id;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
        {
            return true;
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            yield return LifeManager.Instance.ShowDamageSequence(20, 1, true, 0f, ResourceBank.Get<GameObject>("Prefabs/Environment/ScaleWeights/Weight_DataFile_Skull"), 0f);
            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.Default);
            yield break;
        }
	}
}
