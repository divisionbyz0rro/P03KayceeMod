using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using System.Text;
using System.IO;
using System.IO.Compression;
using System;
using System.Collections.Generic;
using System.Linq;
using Infiniscryption.P03KayceeRun.Items;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class P03AscensionSaveData
    {
        public const string ASCENSION_SAVE_KEY = "CopyOfPart3AscensionSave";
        public const string REGULAR_SAVE_KEY = "CopyOfPart3Save";

        public static int RandomSeed
        {
            get
            {
                return AscensionSaveData.Data.currentRunSeed
                       + 10 * EventManagement.CompletedZones.Count
                       + 100 * EventManagement.VisitedZones.Count
                       + 1000 * EventManagement.NumberOfZoneEnemiesKilled
                       + 10000 * EventManagement.NumberOfGeneratedNPCs
                       + Part3SaveData.Data.playerPos.gridX
                       + 100000 * Part3SaveData.Data.playerPos.gridY;
            }
        }

        public static int MaxNumberOfItems
        {
            get
            {
                return 3 - AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.LessConsumables);
            }
        }

        private static string SaveKey
        {
            get
            {
                if (SceneLoader.ActiveSceneName == "Ascension_Configure")
                    return ASCENSION_SAVE_KEY;

                if (SceneLoader.ActiveSceneName == SceneLoader.StartSceneName)
                    return REGULAR_SAVE_KEY;

                if (SaveFile.IsAscension)
                    return ASCENSION_SAVE_KEY;

                return REGULAR_SAVE_KEY;
            }
        }

        public static bool IsP03Run
        {
            get 
            {
                if (SceneLoader.ActiveSceneName.ToLowerInvariant().Contains("part3"))
                    return true;

                if (ScreenManagement.ScreenState == CardTemple.Tech)
                    return true;

                if (SceneLoader.ActiveSceneName.ToLowerInvariant().Contains("part1"))
                    return false;

                if (AscensionSaveData.Data != null && AscensionSaveData.Data.currentRun != null && AscensionSaveData.Data.currentRun.playerLives > 0)
                    return ModdedSaveManager.SaveData.GetValueAsBoolean(P03Plugin.PluginGuid, "IsP03Run");

                return false;
            }
            set
            {
                ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, "IsP03Run", value);
            }
        }

        private static string ToCompressedJSON(object data)
        {
            if (data == null)
                return default(string);

            string value = SaveManager.ToJSON(data);
            //InfiniscryptionP03Plugin.Log.LogInfo($"JSON SAVE: {value}");
            var bytes = Encoding.Unicode.GetBytes(value);
            using (MemoryStream input = new MemoryStream(bytes))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    using (GZipStream stream = new GZipStream(output, CompressionLevel.Optimal))
                    {
                        input.CopyTo(stream);
                        //stream.Flush();
                    }
                    string result = Convert.ToBase64String(output.ToArray());
                    //InfiniscryptionP03Plugin.Log.LogInfo($"B64 SAVE: {result}");
                    return result;
                }
            }
        }

        private static T FromCompressedJSON<T>(string data)
        {
            if (string.IsNullOrEmpty(data))
                return default(T);

            var bytes = Convert.FromBase64String(data);
            using(MemoryStream input = new MemoryStream(bytes))
            {
                using(MemoryStream output = new MemoryStream())
                {
                    using(GZipStream stream = new GZipStream(input, CompressionMode.Decompress))
                    {
                        stream.CopyTo(output);
                        //output.Flush();            
                    }
                    string json = Encoding.Unicode.GetString(output.ToArray());
                    //P03Plugin.Log.LogInfo($"SAVE JSON for {SaveKey}: {json}");
                    return SaveManager.FromJSON<T>(json);
                }
            }
        }

        public static void EnsureRegularSave()
        {
            // The only way there is not a copy of the regular save is because you went straight to a p03 ascension run
            // after installing the mod. This means that the current part3savedata is your actual act 3 save data
            // We don't want to lose that.
            if (ModdedSaveManager.SaveData.GetValue(P03Plugin.PluginGuid, REGULAR_SAVE_KEY) == default(string))
                ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, REGULAR_SAVE_KEY, ToCompressedJSON(Part3SaveData.Data));
        }

        [HarmonyPatch(typeof(Part3SaveData), "Initialize")]
        [HarmonyPrefix]
        private static void ClearSaveData(ref Part3SaveData __instance)
        {
            ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, SaveKey, default(string));
        }

        [HarmonyPatch(typeof(SaveManager), "SaveToFile")]
        public static class Part3SaveDataFixImprovement
        {
            // Okay, I recognize that this is all kind of crazy.

            // Here's the problem: the game wants your Botopia save data to be in a specific place
            // I don't want you to lose your original Part 3 save. Plus, **I** really don't want to lose that save either!
            // Why? Because I want to be able to leave Kaycee's Mod and go explore original Botopia so I can
            // check out how it behaves, etc.

            // So what we do is we actually keep two Part3Save copies alive in the ModdedSaveFile, and we swap in whichever
            // one is necessary based on context (see the patch for LoadFromFile)
            
            // But whenever the file is saved, only the original part 3 save data gets saved in the normal spot
            // This fixes issues that arise when people unload the P03 KCM mod.

            [HarmonyPrefix]
            public static void Prefix(ref Part3SaveData __state)
            {
                // What this does is save a copy of the current part 3 save data somewhere else
                // The idea is that when you play part 3, every time you save we keep a copy of that data
                // And whenever you play ascension part 3, same thing.
                //
                // That way, if you switch over to the other type of part 3, we can load the last time this happened.
                // And whenever creating a new ascension part 3 run, we check to see if there is a copy of part 3 save yet
                // If not, we will end up creating one

                P03Plugin.Log.LogInfo($"Saving {SaveKey}");
                ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, SaveKey, ToCompressedJSON(SaveManager.SaveFile.part3Data));

                // Then, right before we actually save the data, we swap back in the original part3 data
                __state = SaveManager.SaveFile.part3Data;

                EnsurePart3Saved();
                string originalPart3String = ModdedSaveManager.SaveData.GetValue(P03Plugin.PluginGuid, REGULAR_SAVE_KEY);
                Part3SaveData originalPart3Data = FromCompressedJSON<Part3SaveData>(originalPart3String);
                SaveManager.SaveFile.part3Data = originalPart3Data;

                // SEE BELOW FOR WHAT HAPPENS NEXT: \/ \/ \/ 
            }

            [HarmonyPostfix]
            public static void Postfix(Part3SaveData __state)
            {
                // Now that we've saved the file, we swap back whatever we had before
                SaveManager.SaveFile.part3Data = __state;
            }
        }

        

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.TestSaveFileCorrupted))]
        [HarmonyPrefix]
        public static void RepairMissingPart3Data(SaveFile file)
        {
            if (file.part3Data == null)
            {
                file.part3Data = new Part3SaveData();
                file.part3Data.Initialize();
            }
        }

        [HarmonyPatch(typeof(SaveManager), "LoadFromFile")]
        [HarmonyPostfix]
        [HarmonyAfter(new string[] { "cyantist.inscryption.api" })]
        public static void LoadPart3AscensionSaveData()
        {
            string part3Data = ModdedSaveManager.SaveData.GetValue(P03Plugin.PluginGuid, SaveKey);
            Part3SaveData data = FromCompressedJSON<Part3SaveData>(part3Data);

            if (data == default(Part3SaveData))
            {
                data = new Part3SaveData();
                data.Initialize();
            }

            SaveManager.SaveFile.part3Data = data;
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
        [HarmonyPrefix]
        private static void FixStarters(bool newRun)
        {
            bool cannotChooseDecks = AscensionUnlockSchedule.NumStarterDecksUnlocked(AscensionSaveData.Data.challengeLevel) <= 1;
            if (newRun && cannotChooseDecks)
                AscensionSaveData.Data.currentStarterDeck = IsP03Run ? StarterDecks.DEFAULT_STARTER_DECK : "Vanilla";
        }

        [HarmonyPatch(typeof(AscensionSaveData), nameof(AscensionSaveData.NewRun))]
        [HarmonyPostfix]
        public static void InitializePart3Save()
        {
            if (IsP03Run)
            {
                Part3SaveData data = new Part3SaveData();
                data.Initialize();
                SaveManager.SaveFile.part3Data = data;
            }
        }

        [HarmonyPatch(typeof(Part3SaveData), "Initialize")]
        [HarmonyPrefix]
        public static void EnsurePart3Saved()
        {
            if (SaveFile.IsAscension)
            {
                // Check to see if there is a part 3 save data yet
                EnsureRegularSave();
            }
        }

        [HarmonyPatch(typeof(AscensionSaveData), nameof(AscensionSaveData.EndRun))]
        [HarmonyPrefix]
        public static void ClearP03SaveOnEndRun()
        {
            SaveManager.SaveFile.part3Data = null;
            ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, ASCENSION_SAVE_KEY, default(string));
        }

        [HarmonyPatch(typeof(RunState), nameof(RunState.Run), MethodType.Getter)]
        [HarmonyPostfix]
        public static void RunIsNullForP03(ref RunState __result)
        {
            if (IsP03Run)
            {
                __result = __result ?? new ();
                __result.regionTier = EventManagement.CompletedZones.Count;
            }
        }

        [HarmonyPatch(typeof(Part3SaveData), "Initialize")]
        [HarmonyPostfix]
        private static void RewritePart3IntroSequence(ref Part3SaveData __instance)
        {
            if (!P03Plugin.Initialized)
                return;

            if (SaveFile.IsAscension && AscensionSaveData.Data.currentRun != null)
            {
                string worldId = RunBasedHoloMap.GetAscensionWorldID(RunBasedHoloMap.Zone.Neutral);
                Tuple<int, int> pos = RunBasedHoloMap.GetStartingSpace(RunBasedHoloMap.Zone.Neutral);
                Part3SaveData.WorldPosition worldPosition = new(worldId, pos.Item1, pos.Item2);

                __instance.playerPos = worldPosition;
                __instance.checkpointPos = new Part3SaveData.WorldPosition(__instance.playerPos);
                __instance.reachedCheckpoints = new List<string>() { __instance.playerPos.worldId };

                EventManagement.NumberOfZoneEnemiesKilled = 0;

                __instance.deck = new DeckInfo();
                __instance.deck.Cards.Clear();

                StarterDeckInfo deckInfo = StarterDecksUtil.GetInfo(AscensionSaveData.Data.currentStarterDeck);

                List<CardInfo> starterDeckCards = deckInfo.cards.Select(i => CardLoader.GetCardByName(i.name)).ToList();           

                foreach(CardInfo info in starterDeckCards)
                    //__instance.deck.AddCard(CustomCards.ModifyCardForAscension(info));
                    __instance.deck.AddCard(info);

                if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.WeakStarterDeck))
                {
                    foreach(CardInfo info in __instance.deck.Cards)
                    {
                        if (info.mods == null)
                            info.mods = new();
                        info.mods.Add(new(Ability.BuffEnemy));
                    }
                }
                
                __instance.deck.AddCard(CardLoader.GetCardByName(CustomCards.DRAFT_TOKEN));
                __instance.deck.AddCard(CardLoader.GetCardByName(CustomCards.DRAFT_TOKEN));



                __instance.deck.AddCard(CardLoader.GetCardByName("EnergyRoller"));
                __instance.deck.AddCard(CardLoader.GetCardByName("PlasmaGunner"));

                
                // __instance.deck.AddCard(CardLoader.GetCardByName(CustomCards.UNC_TOKEN));
                // __instance.deck.AddCard(CardLoader.GetCardByName(CustomCards.UNC_TOKEN));
                // __instance.deck.AddCard(CardLoader.GetCardByName(CustomCards.UNC_TOKEN));

                
                // __instance.deck.AddCard(CardLoader.GetCardByName(CustomCards.RARE_DRAFT_TOKEN));

                /*
                __instance.deck.AddCard(CardLoader.GetCardByName(CustomCards.RARE_DRAFT_TOKEN));
                __instance.deck.AddCard(CardLoader.GetCardByName(CustomCards.RARE_DRAFT_TOKEN));
                */

                __instance.sideDeckAbilities.Add(Ability.ConduitNull);

                if (__instance.items == null)
                    __instance.items = new List<string>();

                if (MaxNumberOfItems >= 1)
                    __instance.items.Add(ShockerItem.ItemData.name);

                if (MaxNumberOfItems >= 2)
                    __instance.items.Add("PocketWatch");

                if (MaxNumberOfItems >= 3 && !AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.NoHook))
                    __instance.items.Add("BombRemote");

                __instance.reachedCheckpoints.Add("NorthNeutralPath"); // This makes bounty hunters work properly
                                                                       // Without this, your bounty can never reach tier 1

                // TEMPORARY: Force the mycologists active at the start
                // __instance.deck.AddCard(CardLoader.GetCardByName(CustomCards.BRAIN));
                // __instance.items[0] = GoobertHuh.ItemData.name;
                // StoryEventsData.SetEventCompleted(EventManagement.GENERATOR_SUCCESS);

                if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.BOUNTY_HUNTER))
                    __instance.bounty = 45 * AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallengeManagement.BOUNTY_HUNTER); // Good fucking luck
            }
        }

        // This keeps the oil painting puzzle from breaking the game
        [HarmonyPatch(typeof(OilPaintingPuzzle), nameof(OilPaintingPuzzle.GenerateSolution))]
        [HarmonyPrefix]
        public static bool ReplaceGenerateForP03(ref List<string> __result)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                __result = new List<string>() { null, null, CustomCards.VIRUS_SCANNER, CustomCards.VIRUS_SCANNER };
                return false;
            }
            return true;
        }
    }
}