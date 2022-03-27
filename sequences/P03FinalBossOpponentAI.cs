using System.Collections.Generic;
using DiskCardGame;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class P03FinalBossOpponentAI : AI
    {
        public override List<CardSlot> SelectSlotsForCards(List<CardInfo> cardInfos, CardSlot[] slots)
        {
            // Here's the deal. When there are seven slots to pick from, the AI takes FOREVER
            // to pick, and the game breaks in half.

            // Let's try this hack. If there are more than six available slots, we'll
            // try three different subsets and pick the best one
            if (slots.Length <= 5)
                return base.SelectSlotsForCards(cardInfos, slots);

            List<CardSlot> subset1 = new List<CardSlot>();
            List<CardSlot> subset2 = new List<CardSlot>();
            List<CardSlot> subset3 = new List<CardSlot>();
            for (int i = 0; i < slots.Length; i++)
            {
                if (i != 0 && i != slots.Length - 1) subset1.Add(slots[i]);
                if (i != 1 && i != slots.Length - 2) subset2.Add(slots[i]);
                if (i != 2 && i != slots.Length - 3) subset3.Add(slots[i]);
            }

            List<CardSlot> set1Best = base.SelectSlotsForCards(cardInfos, subset1.ToArray());
            List<CardSlot> set2Best = base.SelectSlotsForCards(cardInfos, subset2.ToArray());
            List<CardSlot> set3Best = base.SelectSlotsForCards(cardInfos, subset3.ToArray());

            int set1Score = base.EvaluateCardPlacements(cardInfos, set1Best.ToArray());
            int set2Score = base.EvaluateCardPlacements(cardInfos, set2Best.ToArray());
            int set3Score = base.EvaluateCardPlacements(cardInfos, set3Best.ToArray());

            if (set1Score >= set2Score && set1Score >= set3Score)
                return set1Best;

            if (set2Score >= set1Score && set2Score >= set3Score)
                return set2Best;

            return set3Best;
        }
    }
}