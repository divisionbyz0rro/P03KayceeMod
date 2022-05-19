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
    public class SummonFamiliar : AbilityBehaviour
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        internal static List<CardSlot> BuffedSlots = new();

        static SummonFamiliar()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Summon Familiar";
            info.rulebookDescription = "When [creature] enters play, it summons a familiar bot in the slot adjacent to the right. If that slot is full, it will summon it in the slot to the left. If both are full, nothing will be summoned.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            SummonFamiliar.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(SummonFamiliar),
                TextureHelper.GetImageAsTexture("ability_familiar.png", typeof(SummonFamiliar).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            ViewManager.Instance.SwitchToView(View.Board, false, false);
            CardSlot target = BoardManager.Instance.GetAdjacent(this.Card.Slot, false);
            if (target == null || target.Card != null)
                target = BoardManager.Instance.GetAdjacent(this.Card.Slot, true);

            if (target == null || target.Card != null)
            {
                this.Card.Anim.StrongNegationEffect();
                yield return new WaitForSeconds(0.2f);
                yield break;
            }

            // Figure out the card we're getting
            int randomSeed = P03AscensionSaveData.RandomSeed + TurnManager.Instance.TurnNumber * 25 + this.Card.Slot.Index;
            float randomDraw = SeededRandom.Value(randomSeed);

            string familiarName = ExpansionPackCards_1.EXP_1_PREFIX + "_Salmon";
            if (randomDraw < 0.3f)
                familiarName = "CXformerAdder";
            else if (randomDraw < 0.6f)
                familiarName = "CXformerRaven";
            else if (randomDraw < 0.9f)
                familiarName = "CXformerWolf";

            CardInfo familiar = CardLoader.GetCardByName(familiarName);
            if (familiar.HasAbility(Ability.Transformer))
                familiar.mods.Add(new() { negateAbilities = new() { Ability.Transformer }});

            yield return BoardManager.Instance.CreateCardInSlot(familiar, target);
            yield return new WaitForSeconds(0.25f);

        }

        
    }
}
