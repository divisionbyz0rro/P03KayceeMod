using HarmonyLib;
using DiskCardGame;
using System.Collections.Generic;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class CardChoiceGenerator
    {
        private static Dictionary<int, CardMetaCategory> selectionCategories = new()
        {
            { RunBasedHoloMap.NEUTRAL, CustomCards.NeutralRegion },
            { RunBasedHoloMap.MAGIC, CustomCards.WizardRegion },
            { RunBasedHoloMap.UNDEAD, CustomCards.UndeadRegion },
            { RunBasedHoloMap.NATURE, CustomCards.NatureRegion },
            { RunBasedHoloMap.TECH, CustomCards.TechRegion }
        };

        [HarmonyPatch(typeof(Part3CardChoiceGenerator), "GenerateChoices")]
        [HarmonyPrefix]
        public static bool AscensionChoiceGeneration(CardChoicesNodeData data, int randomSeed, ref List<CardChoice> __result)
        {
            if (SaveFile.IsAscension && HoloMapAreaManager.Instance != null)
            {
                int region = RunBasedHoloMap.GetRegionCodeFromWorldID(HoloMapAreaManager.Instance.CurrentWorld.name);

                // We need one card specific to the region and two cards belonging to the neutral or specific region

                // Don't allow rares
                List<CardInfo> regionCards = ScriptableObjectLoader<CardInfo>.AllData.FindAll(x => x.metaCategories.Contains(selectionCategories[region]) && !x.metaCategories.Contains(CardMetaCategory.Rare));
                List<CardInfo> regionAndNeutralCards = ScriptableObjectLoader<CardInfo>.AllData.FindAll(x => (x.metaCategories.Contains(selectionCategories[region]) || x.metaCategories.Contains(CustomCards.NeutralRegion)) && !x.metaCategories.Contains(CardMetaCategory.Rare));

                __result = new();

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