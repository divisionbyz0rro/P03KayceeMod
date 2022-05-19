using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class Necromancer : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static Necromancer()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Second Chance";
            info.rulebookDescription = "Whenever a friendly non-Brittle creature dies, if [creature] is on the battlefield, a 1/1 skeleton will be created in its place.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            Necromancer.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(Necromancer),
                TextureHelper.GetImageAsTexture("ability_necromancer.png", typeof(Necromancer).Assembly)
            ).Id;
        }

        public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            return this.Card.OnBoard && card.OpponentCard == this.Card.OpponentCard && !card.HasAbility(Ability.Brittle) && !card.HasAbility(Ability.IceCube);
        }

        public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            yield return base.PreSuccessfulTriggerSequence();
            yield return new WaitForSeconds(0.3f);

            CardInfo info = null;
            if (card.Info.iceCubeParams != null)
            {
                info = CardLoader.GetCardByName(Card.Info.iceCubeParams.creatureWithin.name);
            }
            else
            {
                info = CardLoader.GetCardByName("RoboSkeleton");
                CardModificationInfo mod = new();
                mod.healthAdjustment = -1;
                mod.attackAdjustment = -1;
                mod.energyCostAdjustment = -2;
                info.Mods.Add(mod);
            }
            
            yield return BoardManager.Instance.CreateCardInSlot(info, deathSlot, 0.15f, true);
            yield return base.LearnAbility(0.5f);
            yield break;
        }
    }
}