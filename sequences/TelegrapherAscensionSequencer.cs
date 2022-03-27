using System.Collections;
using DiskCardGame;
using System.Linq;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;
using System.Collections.Generic;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class TelegrapherAscensionSequencer : TelegrapherBattleSequencer
    {
        private static string[] APE_ADJECTIVES = new string[] { "Bored", "Annoyed", "Sexy", "Tech", "Broseph", "Overgrown", "Fancy", "Expensive", "Scandalous", "Medium", "Personal", "Non-Fungible", "Trimmed", "Golly", "Devious", "Grape"};

        private static Sprite[] APE_PORTRATS;
        private const int NUMBER_OF_APES = 5;
        private int apesCreated = 0;

        private bool introducedNFTs = false;

        public override bool RespondsToUpkeep(bool playerUpkeep)
        {
            if (!SaveFile.IsAscension)
                return base.RespondsToUpkeep(playerUpkeep);

            return !playerUpkeep && BoardManager.Instance.OpponentSlotsCopy.Any(s => s.Card != null && s.Card.Info.name == CustomCards.GOLLYCOIN);
        }

        private static CardInfo GenerateStupidAssApe(int statPoints)
        {
            CardInfo cardByName = CardLoader.GetCardByName(CustomCards.NFT);

            int seed = P03AscensionSaveData.RandomSeed + 100 * TurnManager.Instance.TurnNumber;
            
            List<AbilityInfo> validAbilities = ScriptableObjectLoader<AbilityInfo>.AllData.FindAll((AbilityInfo x) => x.metaCategories.Contains(AbilityMetaCategory.BountyHunter));
            CardModificationInfo cardModificationInfo = CardInfoGenerator.CreateRandomizedAbilitiesStatsMod(validAbilities, statPoints, 1, 1);
            cardModificationInfo.nameReplacement = Localization.Translate(APE_ADJECTIVES[SeededRandom.Range(0, APE_ADJECTIVES.Length, seed++)]) + " " + Localization.Translate("Ape");
            cardModificationInfo.energyCostAdjustment = statPoints / 2;
            cardByName.Mods.Add(cardModificationInfo);
            return cardByName;
        }

        private List<CardSlot> EmptyLanes()
        {
            return BoardManager.Instance.OpponentSlotsCopy.Where(s => (s.Card == null || s.Card.Info.name == CustomCards.GOLLYCOIN) && BoardManager.Instance.GetCardQueuedForSlot(s) == null).ToList();
        }

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            if (!SaveFile.IsAscension)
            {
                yield return base.OnUpkeep(playerUpkeep);
                yield break;
            }
            
            // Here, we spend a gollycoin on an NFT.
            

            // Doublecheck that we have a gollycoin
            if (playerUpkeep || !BoardManager.Instance.OpponentSlotsCopy.Any(s => s.Card != null && s.Card.Info.name == CustomCards.GOLLYCOIN))
                yield break;

            if (apesCreated < APE_CARDS_PLAN.Length)
            {

                // Describe what's happening
                if (!introducedNFTs)
                {
                    if (!StoryEventsData.EventCompleted(EventManagement.GOLLY_NFT))
                    {
                        ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                        yield return new WaitForSeconds(0.1f);
                    }

                    yield return TextDisplayer.Instance.PlayDialogueEvent("TelegrapherNFT", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                }

                List<CardSlot> slots = EmptyLanes();
                if (slots.Count == 0)
                    yield break;

                ViewManager.Instance.SwitchToView(View.Board, false, false);

                // Get rid of all gollycoin on the board
                foreach (CardSlot slot in BoardManager.Instance.OpponentSlotsCopy)
                    if (slot.Card != null && slot.Card.Info.name == CustomCards.GOLLYCOIN)
                        slot.Card.ExitBoard(0.4f, Vector3.zero);

                // Spawn a random ape
                int statPoints = Mathf.RoundToInt((float)Mathf.Min(6, (apesCreated + 3) * 2.5f));
                CardInfo ape = GenerateStupidAssApe(statPoints);
                apesCreated += 1;

                int seed = P03AscensionSaveData.RandomSeed + 1000 * apesCreated;
                CardSlot targetSlot = slots[SeededRandom.Range(0, slots.Count, seed)];
                yield return TurnManager.Instance.Opponent.QueueCard(ape, targetSlot);

                if (!introducedNFTs)
                    yield return TextDisplayer.Instance.PlayDialogueEvent("TelegrapherItsBeautiful", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

                introducedNFTs = true;
            }

        }

        // The rest of these override the behavior of the telegrapher normally

        public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            if (!SaveFile.IsAscension)
                yield return base.OnOtherCardDie(card, deathSlot, fromCombat, killer);

            yield break; // Do nothing here; I just don't want to tell the network manager anything anymore
        }

        public override IEnumerator OnOtherCardResolve(PlayableCard otherCard)
        {
            if (!SaveFile.IsAscension)
                yield return base.OnOtherCardResolve(otherCard);

            yield break; // Do nothing here; I just don't want to tell the network manager anything anymore
        }

        public override IEnumerator PlayerUpkeep()
        {
            if (!SaveFile.IsAscension)
                yield return base.PlayerUpkeep();

            yield break; // Again, I don't want to do anything network related
        }

        public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            if (!SaveFile.IsAscension)
                return base.RespondsToOtherCardDie(card, deathSlot, fromCombat, killer);

            return false;
        }

        public override bool RespondsToOtherCardResolve(PlayableCard otherCard)
        {
            if (!SaveFile.IsAscension)
                return base.RespondsToOtherCardResolve(otherCard);

            return false;
        }

        private readonly int[] APE_CARDS_PLAN = new int[]
        {
            2,
            0,
            1,
            1,
            1,
            1,
            1,
            1
        };
    }
}