using HarmonyLib;
using DiskCardGame;
using System.Collections.Generic;
using Infiniscryption.P03KayceeRun.Sequences;
using System.Linq;
using System;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class CardChoiceGenerator
    {

        public class Part3RareCardChoicesNodeData : CardChoicesNodeData {} 

        private static Dictionary<RunBasedHoloMap.Zone, CardMetaCategory> selectionCategories = new()
        {
            { RunBasedHoloMap.Zone.Neutral, CustomCards.NeutralRegion },
            { RunBasedHoloMap.Zone.Magic, CustomCards.WizardRegion },
            { RunBasedHoloMap.Zone.Undead, CustomCards.UndeadRegion },
            { RunBasedHoloMap.Zone.Nature, CustomCards.NatureRegion },
            { RunBasedHoloMap.Zone.Tech, CustomCards.TechRegion }
        };

        private static string GetNameContribution(CardInfo card)
        {
            string[] nameSplit = card.displayedName.Split(' ', '-');
            if (nameSplit.Length > 1)
            {
                if (nameSplit[0].ToLowerInvariant().Contains("gem"))
                    return nameSplit[1];
                else
                    return nameSplit[0];
            }

            if (card.displayedName.Contains("bot"))
                return card.displayedName.Replace("bot", "");

            return card.displayedName;
        }

        private static CardInfo GenerateMycoCard(int randomSeed)
        {
            List<CardInfo> allCards = ScriptableObjectLoader<CardInfo>.AllData.FindAll(x => TradeChipsSequencer.IsValidDraftCard(x) && !x.metaCategories.Contains(CardMetaCategory.Rare)).ToList();
            CardInfo left = allCards[SeededRandom.Range(0, allCards.Count, randomSeed++)];
            CardInfo right = allCards[SeededRandom.Range(0, allCards.Count, randomSeed++)];

            string name = GetNameContribution(left) + "-" + GetNameContribution(right);

            int health = Math.Max(left.Health, right.Health);
            int attack = Math.Max(left.Attack, right.Attack);
            List<Ability> abilities = new ();
            abilities.AddRange(left.Abilities);
            abilities.AddRange(right.Abilities);
            int energyCost = Math.Max(left.energyCost, right.energyCost);

            CardModificationInfo mod = new();
            mod.nonCopyable = true;
            mod.abilities = abilities;
            mod.healthAdjustment = health;
            mod.attackAdjustment = attack;
            mod.energyCostAdjustment = energyCost;
            mod.nameReplacement = name;
            mod.gemify = left.Gemified || right.Gemified;
            if (mod.abilities.Contains(Ability.Transformer))
            {
                if (left.evolveParams != null)
                    mod.transformerBeastCardId = left.evolveParams.evolution.name;
                else if (right.evolveParams != null)
                    mod.transformerBeastCardId = right.evolveParams.evolution.name;
            }

            CardInfo retval = CardLoader.GetCardByName(CustomCards.FAILED_EXPERIMENT_BASE);
            (retval.mods ??= new()).Add(mod);
            return retval;
        }

        [HarmonyPatch(typeof(Part3CardChoiceGenerator), "GenerateChoices")]
        [HarmonyPrefix]
        public static bool AscensionChoiceGeneration(CardChoicesNodeData data, int randomSeed, ref List<CardChoice> __result)
        {
            if (SaveFile.IsAscension && HoloMapAreaManager.Instance != null)
            {
                RunBasedHoloMap.Zone region = RunBasedHoloMap.GetRegionCodeFromWorldID(HoloMapAreaManager.Instance.CurrentWorld.name);

                __result = new();

                if (region == RunBasedHoloMap.Zone.Mycologist)
                {
                    int newRandomSeed = P03AscensionSaveData.RandomSeed;
                    for (int i = 0; i < 3; i++)
                        __result.Add(new () { CardInfo = GenerateMycoCard(newRandomSeed + 100 * i)});

                    return false;
                }

                // We need one card specific to the region and two cards belonging to the neutral or specific region
                Predicate<IEnumerable<CardMetaCategory>> rareMatcher = data is Part3RareCardChoicesNodeData ? x => x.Any(m => m == CardMetaCategory.Rare) : x => !x.Any(m => m == CardMetaCategory.Rare);

                // Don't allow rares
                List<CardInfo> regionCards = ScriptableObjectLoader<CardInfo>.AllData.FindAll(x => x.metaCategories.Contains(selectionCategories[region]) && rareMatcher(x.metaCategories));
                List<CardInfo> regionAndNeutralCards = ScriptableObjectLoader<CardInfo>.AllData.FindAll(x => (x.metaCategories.Contains(selectionCategories[region]) || x.metaCategories.Contains(CustomCards.NeutralRegion)) && rareMatcher(x.metaCategories));

                if (regionCards.Count > 0)
                {
                    CardInfo newCard = regionCards[SeededRandom.Range(0, regionCards.Count, randomSeed++)];
                    //__result.Add(new CardChoice() { CardInfo = CustomCards.ModifyCardForAscension(newCard) });
                    __result.Add(new CardChoice() { CardInfo = CardLoader.Clone(newCard) });
                    regionCards.Remove(newCard);
                    regionAndNeutralCards.Remove(newCard);
                }

                // 50% chance that the second card also comes from the region
                if (SeededRandom.Bool(randomSeed++))
                {
                    if (regionCards.Count > 0)
                    {
                        CardInfo newCard = regionCards[SeededRandom.Range(0, regionCards.Count, randomSeed++)];
                        //__result.Add(new CardChoice() { CardInfo = CustomCards.ModifyCardForAscension(newCard) });
                        __result.Add(new CardChoice() { CardInfo = CardLoader.Clone(newCard) });
                        regionAndNeutralCards.Remove(newCard);
                    }
                }

                while (__result.Count < 3)
                {
                    CardInfo newCard = regionAndNeutralCards[SeededRandom.Range(0, regionAndNeutralCards.Count, randomSeed++)];
                    //__result.Add(new CardChoice() { CardInfo = CustomCards.ModifyCardForAscension(newCard) });
                    __result.Add(new CardChoice() { CardInfo = CardLoader.Clone(newCard) });
                    regionAndNeutralCards.Remove(newCard);
                }

                //                InfiniscryptionP03Plugin.Log.LogInfo($"I selected the following cards for region {region}: {string.Join(",", __result.Select(c => c.CardInfo.name))}");

                return false;
            }
            return true;
        }
    }
}