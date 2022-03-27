using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    public class EncounterBlueprintHelper
    {
        public string name;
        public List<string> dominantTribes;
        public int maxDifficulty;
        public int minDifficulty;
        public int oldPreviewDifficulty;
        public List<string> redundantAbilities;
        public bool regionSpecific;
        public List<string> unlockedCardPrerequisites;
        public List<string> randomReplacementCards;
        public List<List<int>> overclockBlueprint;
        public int powerLevel;
        public string powerLevelString;
        public List<List<string>> turnPlan;

        public override string ToString()
        {
            string retval = $"Name: {name}"; //0
            retval += "\n" + $"maxDifficulty: {maxDifficulty}"; //1
            retval += "\n" + $"minDifficulty: {minDifficulty}"; //2
            retval += "\n" + $"powerLevel: {powerLevel}"; //3
            retval += "\n" + $"powerLevelString: {powerLevelString}"; //4
            retval += "\n" + $"oldPreviewDifficulty: {oldPreviewDifficulty}"; //5
            retval += "\n" + $"regionSpecific: {regionSpecific}"; //6
            retval += "\n" + $"dominantTribes: [{string.Join(",", dominantTribes)}]"; //7
            retval += "\n" + $"redundantAbilities: [{string.Join(",", redundantAbilities)}]"; //8
            retval += "\n" + $"unlockedCardPrerequisites: [{string.Join(",", unlockedCardPrerequisites)}]"; //9
            retval += "\n" + $"randomReplacementCards: [{string.Join(",", randomReplacementCards)}]"; //10

            string overclock = string.Join(",", overclockBlueprint.Select(p => $"D:{p[0]}/T:{p[1]}"));
            retval += "\n" + $"overclockBlueprint: [{overclock}]";

            foreach (List<string> turn in turnPlan)
                retval += "\n" + string.Join(",", turn);
            
            return retval;            
        }

        public EncounterBlueprintHelper(string stringVal)
        {
            string[] bpArray = stringVal.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            P03Plugin.Log.LogDebug($"Split length {bpArray.Length}");
            this.name = bpArray[0].Split(':')[1].Trim(); 
            this.maxDifficulty = int.Parse(bpArray[1].Split(':')[1].Trim()); 
            this.minDifficulty = int.Parse(bpArray[2].Split(':')[1].Trim()); 
            this.powerLevel = int.Parse(bpArray[3].Split(':')[1].Trim()); 
            this.powerLevelString = bpArray[4].Split(':')[1].Trim(); 
            this.oldPreviewDifficulty = int.Parse(bpArray[5].Split(':')[1].Trim()); 
            this.regionSpecific = bool.Parse(bpArray[6].Split(':')[1].Trim()); 
            this.dominantTribes = AsSplitArray(bpArray[7].Split(':')[1].Trim()); 
            this.redundantAbilities = AsSplitArray(bpArray[8].Split(':')[1].Trim()); 
            this.unlockedCardPrerequisites = AsSplitArray(bpArray[9].Split(':')[1].Trim()); 
            this.randomReplacementCards = AsSplitArray(bpArray[10].Split(':')[1].Trim()); 
            this.overclockBlueprint = AsOverclock(bpArray[11].Replace("overclockBlueprint: ", "")); 
            this.turnPlan = bpArray.Skip(12).Select(s => String.IsNullOrEmpty(s) ? new List<string>() : s.Split(',').ToList()).ToList(); 
        }

        private static List<List<int>> AsOverclock(string ocString)
        {
            return ocString.Replace("[", "").Replace("]", "").Split(',').Select(s => s.Split(new char[] { 'D', 'T', ':', '/'}, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToList()).ToList();
        }

        private static List<string> AsSplitArray(string csvString)
        {
            if (csvString == "[]")
                return new List<string>();
            return csvString.Replace("[", "").Replace("]", "").Split(',').Select(s => s.Trim()).ToList();
        }

        private static string BlueprintToString(EncounterBlueprintData.CardBlueprint bp)
        {
            if (bp == default(EncounterBlueprintData.CardBlueprint))
                return "EMPTY TURN";

            string retval = bp.card == null ? "NONE" : $"{bp.card.name}";

            if (bp.difficultyReplace)
                retval += $" -> {bp.replacement.name} {bp.difficultyReq}";
            else if (bp.randomReplaceChance > 0)
                retval += $" -> RANDOM {bp.randomReplaceChance}%";

            return retval;
        }

        private static EncounterBlueprintData.CardBlueprint StringToBlueprint(string bpString, int maxDifficulty)
        {
            try
            {
                if (bpString.Equals("EMPTY TURN"))
                    return default(EncounterBlueprintData.CardBlueprint);

                string[] splitString = bpString.Trim().Split(' ');
                
                EncounterBlueprintData.CardBlueprint retval = new EncounterBlueprintData.CardBlueprint();
                retval.card = splitString[0] == "NONE" ? null : CardLoader.GetCardByName(splitString[0]);

                retval.minDifficulty = 0;
                retval.maxDifficulty = maxDifficulty == 0 ? 6 : maxDifficulty;

                if (splitString.Length > 1)
                {
                    if (splitString[3].EndsWith("%"))
                    {
                        retval.randomReplaceChance = int.Parse(splitString[3].Replace("%", ""));
                        P03Plugin.Log.LogInfo($"Card {retval.card} randomly replaced with chance {retval.randomReplaceChance}");
                    }
                    else
                    {
                        retval.replacement = CardLoader.GetCardByName(splitString[2]);
                        retval.difficultyReq = int.Parse(splitString[3]);
                        retval.difficultyReplace = true;
                        P03Plugin.Log.LogInfo($"Card {retval.card} replaced by {retval.replacement} at difficulty {retval.difficultyReq}");
                    }
                }
                else
                {
                    P03Plugin.Log.LogInfo($"Card {retval.card}");
                }

                return retval;
            } catch {
                P03Plugin.Log.LogError($"ERROR PROCESSING CARD BLUEPRINT: {bpString}");
                throw;
            }
        }

        public EncounterBlueprintHelper(EncounterBlueprintData data)
        {
            name = data.name;
            dominantTribes = data.dominantTribes.Select(t => t.ToString()).ToList();
            maxDifficulty = data.maxDifficulty;
            minDifficulty = data.minDifficulty;
            oldPreviewDifficulty = data.oldPreviewDifficulty;
            redundantAbilities = data.redundantAbilities.Select(t => t.ToString()).ToList();
            regionSpecific = data.regionSpecific;
            unlockedCardPrerequisites = data.unlockedCardPrerequisites.Select(t => t.name).ToList();
            randomReplacementCards = data.randomReplacementCards.Select(t => t.name).ToList();
            overclockBlueprint = data.turnMods.Where(bp => bp.overlockCards).Select(bp => new List<int> { bp.applyAtDifficulty, bp.turn }).ToList();
            powerLevel = data.PowerLevel;
            powerLevelString = data.PowerLevelString;

            turnPlan = data.turns.Select(turn => turn.Select(BlueprintToString).ToList()).ToList();
        }

        public EncounterBlueprintData AsBlueprint()
        {
            EncounterBlueprintData data = ScriptableObject.CreateInstance<EncounterBlueprintData>();
            data.name = this.name;
            data.dominantTribes = this.dominantTribes.Select(s => (Tribe)Enum.Parse(typeof(Tribe), s)).ToList();
            data.maxDifficulty = this.maxDifficulty;
            data.minDifficulty = this.minDifficulty;
            data.oldPreviewDifficulty = this.oldPreviewDifficulty;
            data.redundantAbilities = this.redundantAbilities.Select(s => (Ability)Enum.Parse(typeof(Ability), s)).ToList();
            data.regionSpecific = this.regionSpecific;
            data.unlockedCardPrerequisites = this.unlockedCardPrerequisites.Select(n => CardLoader.GetCardByName(n)).ToList();
            data.randomReplacementCards = this.randomReplacementCards.Select(n => CardLoader.GetCardByName(n)).ToList();
            data.turnMods = this.overclockBlueprint.Where(ar => ar != null && ar.Count == 2).Select(ar => new EncounterBlueprintData.TurnModBlueprint() { overlockCards = true, applyAtDifficulty = ar[0], turn = ar[1]}).ToList();
            data.PowerLevel = this.powerLevel;
            data.PowerLevelString = this.powerLevelString;

            data.turns = turnPlan.Select(turn => turn.Select(s => StringToBlueprint(s, data.maxDifficulty)).Where(bp => bp != null).ToList()).ToList();


            return data;
        }
    }
}