using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using System.Linq;
using System;
using System.Collections.Generic;
using InscryptionAPI.Guid;
using Infiniscryption.P03KayceeRun.Sequences;
using System.Collections;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class EventManagement
    {
        public static readonly StoryEvent ALL_ZONE_ENEMIES_KILLED = (StoryEvent)GuidManager.GetEnumValue<StoryEvent>(P03Plugin.PluginGuid, "AllZoneEnemiesKilled");
        public static readonly StoryEvent ALL_BOSSES_KILLED = (StoryEvent)GuidManager.GetEnumValue<StoryEvent>(P03Plugin.PluginGuid, "AllBossesKilled");
        public static readonly StoryEvent HAS_DRAFT_TOKEN = (StoryEvent)GuidManager.GetEnumValue<StoryEvent>(P03Plugin.PluginGuid, "HasDraftToken");
        public static readonly StoryEvent SAW_P03_INTRODUCTION = (StoryEvent)GuidManager.GetEnumValue<StoryEvent>(P03Plugin.PluginGuid, "SawP03Introduction");
        public static readonly StoryEvent GOLLY_NFT = (StoryEvent)GuidManager.GetEnumValue<StoryEvent>(P03Plugin.PluginGuid, "GollyNFTIntro");
        public static readonly StoryEvent DEFEATED_P03 = (StoryEvent)GuidManager.GetEnumValue<StoryEvent>(P03Plugin.PluginGuid, "DefeatedP03");    
        public static readonly StoryEvent ONLY_ONE_BOSS_LIFE = (StoryEvent)GuidManager.GetEnumValue<StoryEvent>(P03Plugin.PluginGuid, "P03AscensionOneBossLife");    
        public static readonly StoryEvent OVERCLOCK_CHANGES = (StoryEvent)GuidManager.GetEnumValue<StoryEvent>(P03Plugin.PluginGuid, "P03AscensionOverclock");   
        public static readonly StoryEvent TRANSFORMER_CHANGES = (StoryEvent)GuidManager.GetEnumValue<StoryEvent>(P03Plugin.PluginGuid, "P03AscensionTransformer");   
        public static readonly StoryEvent HAS_DEFEATED_P03 = (StoryEvent)GuidManager.GetEnumValue<StoryEvent>(P03Plugin.PluginGuid, "HasDefeatedP03");   

        public const string GAME_OVER = "GameOverZone";

        public static readonly StoryEvent[] P03AscensionSaveEvents = new StoryEvent[]
        {
            SAW_P03_INTRODUCTION,
            GOLLY_NFT,
            ONLY_ONE_BOSS_LIFE,
            OVERCLOCK_CHANGES,
            TRANSFORMER_CHANGES,
            HAS_DEFEATED_P03
        };

        public static readonly MechanicsConcept[] P03_MECHANICS = new MechanicsConcept[]
        {
            MechanicsConcept.BossMultipleLives,
            MechanicsConcept.GainCurrency,
            MechanicsConcept.HoloMapCheckpoint,
            MechanicsConcept.HoloMapFastTravel,
            MechanicsConcept.OnlineFriendCards,
            MechanicsConcept.Part3AttachGem,
            MechanicsConcept.Part3Bloodstain,
            MechanicsConcept.Part3Bounty,
            MechanicsConcept.Part3BountyTiers,
            MechanicsConcept.Part3BuildACard,
            MechanicsConcept.Part3Consumables,
            MechanicsConcept.Part3CreateTransformer,
            MechanicsConcept.Part3ModifySideDeck,
            MechanicsConcept.Part3OverclockCard,
            MechanicsConcept.Part3RecycleCard,
            MechanicsConcept.Part3Respawn,
            MechanicsConcept.Part3TradeCards,
            MechanicsConcept.PhotographerRestoreSnapshot,
            MechanicsConcept.PhotographerTakeSnapshot
        };

        private static readonly Dictionary<HoloMapNode.NodeDataType, float> CostAdjustments = new ()
        {
            { HoloMapNode.NodeDataType.AddCardAbility, 0f },
            { HoloMapNode.NodeDataType.BuildACard, 1f },
            { UnlockAscensionItemNodeData.UnlockItemsAscension, 0.6f },
            { HoloMapNode.NodeDataType.CreateTransformer, -1f },
            { HoloMapNode.NodeDataType.OverclockCard, -1f },
            { AscensionRecycleCardNodeData.AscensionRecycleCard, -2f }
        };

        public static int EncounterDifficulty
        {
            get
            {
                int tier = EventManagement.CompletedZones.Count;
                int modifier = AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.BaseDifficulty);
                return tier + modifier + (tier == 0 ? 0 : 1);
            }
        }

        [HarmonyPatch(typeof(Part3SaveData), nameof(Part3SaveData.GetDifficultyModifier))]
        [HarmonyPrefix]
        public static bool AscensionDifficultyModifierWorksDifferently(ref int __result)
        {
            if (SaveFile.IsAscension)
            {
                __result = 0;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(RunState), nameof(RunState.DifficultyModifier), MethodType.Getter)]
        [HarmonyPostfix]
        public static void AscensionRunStateDifficultyModifierWorksDifference(ref int __result)
        {
            if (SaveFile.IsAscension && P03AscensionSaveData.IsP03Run)
            {
                __result = 0;
            }
        }

        public static int UpgradePrice(HoloMapNode.NodeDataType nodeType)
        {
            float baseCost = 7 + (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.ExpensivePelts) ? 3f + 2f * CompletedZones.Count: 1f * CompletedZones.Count);

            if (CostAdjustments.ContainsKey(nodeType))
            {
                float adj = CostAdjustments[nodeType];
                if (adj != 0 && Math.Abs(adj) < 1)
                    baseCost *= adj; 
                else
                    baseCost += CostAdjustments[nodeType];
            }

            return UnityEngine.Mathf.RoundToInt(baseCost);
        }

        public static Tuple<int, int> CurrencyGainRange
        {
            get
            {
                int minExpectedUpgrades = AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.ExpensivePelts) ? 2 * CompletedZones.Count : 3 * CompletedZones.Count;
                int actualUpgrades = Part3SaveData.Data.deck.Cards.Select(c => c.mods.Count()).Sum();
                int upgradeDiff = Math.Max(0, minExpectedUpgrades - actualUpgrades - (Part3SaveData.Data.currency / 6));
                int low = 4 + CompletedZones.Count + 3 * upgradeDiff;
                int high = 8 + CompletedZones.Count + 3 * upgradeDiff;
                return new(low, high);
            }
        }

        [HarmonyPatch(typeof(BountyHunter), nameof(BountyHunter.OnDie))]
        [HarmonyPostfix]
        public static IEnumerator EarnCurrencyWhenBountyHunterDies(IEnumerator sequence)
        {
            yield return sequence;

            if (TurnManager.Instance.Opponent is P03AscensionOpponent) // don't do this on the final boss
                yield break;

            P03AnimationController.Face currentFace = P03AnimationController.Instance.CurrentFace;
            View currentView = ViewManager.Instance.CurrentView;
            yield return new WaitForSeconds(0.4f);
            int currencyGain = Part3SaveData.Data.BountyTier * 3;
            yield return P03AnimationController.Instance.ShowChangeCurrency(currencyGain, true);
            Part3SaveData.Data.currency += currencyGain;
            yield return new WaitForSeconds(0.2f);
            P03AnimationController.Instance.SwitchToFace(currentFace);
            yield return new WaitForSeconds(0.1f);
            if (ViewManager.Instance.CurrentView != currentView)
            {
                ViewManager.Instance.SwitchToView(currentView, false, false);
                yield return new WaitForSeconds(0.2f);
            }
        }

        public static int NumberOfLivesRemaining
        {
            get { return ModdedSaveManager.RunState.GetValueAsInt(P03Plugin.PluginGuid, "NumberOfLivesRemaining"); }
            set { ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "NumberOfLivesRemaining", value); }
        }

        public const int ENEMIES_TO_UNLOCK_BOSS = 4;
        public static int NumberOfZoneEnemiesKilled
        {
            get { return ModdedSaveManager.RunState.GetValueAsInt(P03Plugin.PluginGuid, "ZoneEnemiesKilled"); }
            set { ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "ZoneEnemiesKilled", value); }
        }

        public static List<string> CompletedZones
        {
            get
            {
                string zoneCsv = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "CompletedZones");
                if (zoneCsv == default(string))
                    return new List<string>();

                return zoneCsv.Split(',').ToList();
            }
        }

        public static void AddCompletedZone(StoryEvent storyEvent)
        {
            if (storyEvent == StoryEvent.ArchivistDefeated) AddCompletedZone("FastTravelMapNode_Undead");
            if (storyEvent == StoryEvent.CanvasDefeated) AddCompletedZone("FastTravelMapNode_Wizard");
            if (storyEvent == StoryEvent.TelegrapherDefeated) AddCompletedZone("FastTravelMapNode_Tech");
            if (storyEvent == StoryEvent.PhotographerDefeated) AddCompletedZone("FastTravelMapNode_Nature");
        }

        public static void AddCompletedZone(string id)
        {
            List<string> zones = CompletedZones;
            if (!zones.Contains(id))
                zones.Add(id);
            
            ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "CompletedZones", string.Join(",", zones));
        }

        public static List<string> VisitedZones
        {
            get
            {
                string zoneCsv = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "VisitedZones");
                if (zoneCsv == default(string))
                    return new List<string>();

                return zoneCsv.Split(',').ToList();
            }
        }
        public static void AddVisitedZone(string id)
        {
            List<string> zones = VisitedZones;
            if (!zones.Contains(id))
                zones.Add(id);
            
            ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "VisitedZones", string.Join(",", zones));
        }

        public static StoryEvent GetStoryEventForOpponent(Opponent.Type opponent)
        {
            if (opponent == Opponent.Type.PhotographerBoss)
                return StoryEvent.PhotographerDefeated;
            if (opponent == Opponent.Type.TelegrapherBoss)
                return StoryEvent.TelegrapherDefeated;
            if (opponent == Opponent.Type.CanvasBoss)
                return StoryEvent.CanvasDefeated;
            if (opponent == Opponent.Type.ArchivistBoss)
                return StoryEvent.ArchivistDefeated;

            return StoryEvent.WoodcarverDefeated;
        }

        [HarmonyPatch(typeof(ProgressionData), nameof(ProgressionData.LearnedMechanic))]
        [HarmonyPrefix]
        public static bool ForceMechanicsLearnd(MechanicsConcept mechanic, ref bool __result)
        {
            if (SaveFile.IsAscension && P03_MECHANICS.Contains(mechanic))
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(StoryEventsData), "SetEventCompleted")]
        [HarmonyPrefix]
        public static bool P03AscensionStoryCompleted(StoryEvent storyEvent)
        {
            if (SaveFile.IsAscension && P03AscensionSaveData.IsP03Run && P03AscensionSaveEvents.Contains(storyEvent))
            {
                ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, $"StoryEvent{storyEvent}", true);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(StoryEventsData), "EventCompleted")]
        [HarmonyPrefix]
        public static bool P03AscensionStoryData(ref bool __result, StoryEvent storyEvent)
        {
            if (SaveFile.IsAscension && P03AscensionSaveData.IsP03Run)
            {
                if (storyEvent == StoryEvent.ArchivistDefeated)
                {
                    __result = CompletedZones.Contains("FastTravelMapNode_Undead");
                    return false;
                }

                if (storyEvent == StoryEvent.CanvasDefeated) 
                {
                    __result = CompletedZones.Contains("FastTravelMapNode_Wizard");
                    return false;
                }

                if (storyEvent == StoryEvent.TelegrapherDefeated)
                {
                    __result = CompletedZones.Contains("FastTravelMapNode_Tech");
                    return false;
                }

                if (storyEvent == StoryEvent.PhotographerDefeated)
                {
                    __result = CompletedZones.Contains("FastTravelMapNode_Nature");
                    return false;
                }

                if ((int)storyEvent == (int)ALL_ZONE_ENEMIES_KILLED)
                {
                    __result = NumberOfZoneEnemiesKilled >= ENEMIES_TO_UNLOCK_BOSS;
                    //__result = true;
                    return false;
                }
                if ((int)storyEvent == (int)ALL_BOSSES_KILLED)
                {
                    __result = CompletedZones.Count >= 4;
                    return false;
                }
                if ((int)storyEvent == (int)HAS_DRAFT_TOKEN)
                {
                    __result = Part3SaveData.Data.deck.Cards.Any(card => card.name == CustomCards.DRAFT_TOKEN || card.name == CustomCards.RARE_DRAFT_TOKEN);
                    return false;
                }

                if (storyEvent == StoryEvent.GemsModuleFetched) // Simply going to this world 'completes' that story event for you
                {
                    __result = true;
                    return false;
                }

                if (storyEvent == StoryEvent.HoloTechTempleSatelliteActivated)
                {
                    __result = true;
                    return false;
                }

                if (P03AscensionSaveEvents.Contains(storyEvent))
                {
                    __result = ModdedSaveManager.SaveData.GetValueAsBoolean(P03Plugin.PluginGuid, $"StoryEvent{storyEvent}");
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(TurnManager), "CleanupPhase")]
        [HarmonyPrefix]
        public static void TrackVictories(ref TurnManager __instance)
        {
            if (__instance.Opponent.NumLives <= 0 || __instance.Opponent.Surrendered)
                NumberOfZoneEnemiesKilled = NumberOfZoneEnemiesKilled + 1;
        }

        [HarmonyPatch(typeof(AscensionSaveData), "NewRun")]
        [HarmonyPostfix]
        [HarmonyAfter(new string[] { "zorro.inscryption.infiniscryption.curses" })]
        public static void SortOutStartOfRun(ref AscensionSaveData __instance)
        {
            // Figure out the number of lives
            NumberOfLivesRemaining = __instance.currentRun.maxPlayerLives;
        }

        public static void FinishAscension(bool success=true)
		{
            P03Plugin.Log.LogInfo("Starting finale sequence");
			AscensionMenuScreens.ReturningFromSuccessfulRun = success;
            AscensionStatsData.TryIncrementStat(success ? AscensionStat.Type.Victories : AscensionStat.Type.Losses);

            if (success)
            {
                foreach (AscensionChallenge c in AscensionSaveData.Data.activeChallenges)
                    if (!AscensionSaveData.Data.conqueredChallenges.Contains(c))
                        AscensionSaveData.Data.conqueredChallenges.Add(c);

                if (!string.IsNullOrEmpty(AscensionSaveData.Data.currentStarterDeck) && !AscensionSaveData.Data.conqueredStarterDecks.Contains(AscensionSaveData.Data.currentStarterDeck))
                    AscensionSaveData.Data.conqueredStarterDecks.Add(AscensionSaveData.Data.currentStarterDeck);
            }

            // Delete the ascension save; the run is over            
            ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, P03AscensionSaveData.ASCENSION_SAVE_KEY, default(string)); 

            // Also delete the normal ascension current run just in case
            AscensionSaveData.Data.currentRun = null;

            if (EventManagement.CompletedZones.Count > 0)
                AscensionSaveData.Data.numRunsSinceReachedFirstBoss = 0;

            // Let's no longer force this to false
            // It should go false when the screen loads
            // and leaving it 'as is' should help the restart work.
            //P03AscensionSaveData.IsP03Run = false;

            Part3SaveData.Data.checkpointPos = new Part3SaveData.WorldPosition(GAME_OVER, 0, 0);

            SaveManager.SaveToFile(false);

            P03Plugin.Log.LogInfo("Loading ascension scene");
            SceneLoader.Load("Ascension_Configure");
		}
    }
}