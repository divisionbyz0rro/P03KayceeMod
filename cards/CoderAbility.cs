using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class Programmer : AbilityBehaviour
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public static readonly Trait CodeTrait = GuidManager.GetEnumValue<Trait>(P03Plugin.PluginGuid, "IsBlockOfCode");

        public static void Register()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Programmer";
            info.rulebookDescription = "public override IEnumerator OnUpkeep(){\n  if (this.codeCard == null) this.codeCard = BoardManager.CreateCard('CODE');\n  else this.codeCard.AddTemporaryMod();\n}";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            Programmer.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(Programmer),
                TextureHelper.GetImageAsTexture("ability_coder.png", typeof(Programmer).Assembly)
            ).Id;
        }

        private IEnumerator CreateCodeCard(List<CardSlot> slots)
        {
            // Look for an empty slot                
            List<CardSlot> possibleSlots = slots.Where(s => s.Card == null).ToList();
            if (possibleSlots.Count == 0)
                yield break;

            CardSlot targetSlot = null;
            int randomSeed = P03AscensionSaveData.RandomSeed + TurnManager.Instance.TurnNumber * 100 + slots.IndexOf(this.Card.slot);
            foreach (int idx in preferredSlots)
            {
                if (slots[idx].Card == null)
                {
                    targetSlot = slots[idx];
                    break;
                }
            }

            bool isBug = SeededRandom.Value(randomSeed++) < 0.1f;
            CardInfo codeInfo = CardLoader.GetCardByName(isBug ? CustomCards.CODE_BUG : CustomCards.CODE_BLOCK);

            ViewManager.Instance.SwitchToView(View.Board);
            yield return new WaitForSeconds(0.25f);
            yield return BoardManager.Instance.CreateCardInSlot(codeInfo, targetSlot);
            yield return new WaitForSeconds(1.2f);
            ViewManager.Instance.SwitchToView(View.Default);
            yield break;
        }

        public override bool RespondsToResolveOnBoard()
        {
            return true;
        }

        public override IEnumerator OnResolveOnBoard()
        {
            List<CardSlot> slots = !this.Card.OpponentCard ? BoardManager.Instance.PlayerSlotsCopy : BoardManager.Instance.OpponentSlotsCopy;

            PlayableCard codeCard = slots.Where(s => s.Card != null).Select(s => s.Card).FirstOrDefault(c => c.Info.HasTrait(CodeTrait));
            if (codeCard == null)
                yield return CreateCodeCard(slots);
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
                yield return CreateCodeCard(slots);
                yield break;
            }

            // The code already exists. Now we modify it
            ViewManager.Instance.SwitchToView(View.BoardCentered);
            
            // Programmers only modify stats not abilities
            // Artists modify abilities
            this.Card.Anim.SetShaking(true);
            codeCard.Anim.SetShaking(true);
            yield return new WaitForSeconds(1.2f);

            codeCard.Anim.SetOverclocked(true);
            bool attack = SeededRandom.Bool(randomSeed++);
            codeCard.AddTemporaryMod(new (attack ? 1 : 0, attack ? 0 : 1));
            yield return new WaitForSeconds(0.6f);
            ViewManager.Instance.SwitchToView(View.Default);
        }
    }
}
