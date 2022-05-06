using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using System;
using System.Linq;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Faces;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    internal static class CreditsManagement
    {
        [HarmonyPatch(typeof(CreditsDisplayer), nameof(CreditsDisplayer.Start))]
        [HarmonyPrefix]
        public static void ModifyCredits(CreditsDisplayer __instance)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                string database = DataHelper.GetResourceString("credits_database", "csv");
                string[] lines = database.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                EventManagement.SawCredits = true;

                __instance.creditsData.credits.Clear();

                foreach(string line in lines.Skip(1))
                {
                    List<string> cols = DialogueManagement.SplitColumn(line);

                    P03Plugin.Log.LogDebug($"Line {line} has {cols.Count} columns");
                    
                    CreditEntry credit = new (cols[0], cols[1], cols[2]);
                    credit.fullScreen = cols[3].ToLowerInvariant().Contains("y");
                    credit.empty = cols[4].ToLowerInvariant().Contains("y");
                    credit.isTitle = cols[5].ToLowerInvariant().Contains("y");
                    credit.startCondensed = cols[6].ToLowerInvariant().Contains("y");
                    credit.endCondensed = cols[7].ToLowerInvariant().Contains("y");
                    __instance.creditsData.credits.Add(credit);
                }
            }
        }
    }
}