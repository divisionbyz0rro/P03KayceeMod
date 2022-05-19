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
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Cards;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class EventManagement
    {
        public enum SpecialEvent
        {
            None = 0,
            GoobertQuest = 1,
            ProspectorQuest = 2,
            WhiteFlag = 3,
            DeckSize = 4,
            Smuggler = 5,
            FullyUpgraded = 6,
            ListenToTheRadio = 7,
            ILoveBones = 8,
            TippedScales = 9,
            BrokenGeneratorQuest = 10,
            PowerUpTheTower = 11,
            Donation = 12,
            DonationPartTwo = 13,
            SmugglerPartTwo = 14
        }

        public enum SpecialReward
        {
            None = 0,
            GemifyTwoRandomCards = 1,
            RandomCardGainsUndying = 2
        }

        public static List<StoryEvent> P03AscensionSaveEvents = new();
        public static List<StoryEvent> P03RunBasedStoryEvents = new();

        internal static StoryEvent NewStory(string code, bool save = false, bool run = false)
        {
            StoryEvent se = GuidManager.GetEnumValue<StoryEvent>(P03Plugin.PluginGuid, code);
            if (save)
                P03AscensionSaveEvents.Add(se);
            if (run)
                P03RunBasedStoryEvents.Add(se);
            return se;
        }

        public static readonly StoryEvent ALL_ZONE_ENEMIES_KILLED = NewStory("AllZoneEnemiesKilled");
        public static readonly StoryEvent ALL_BOSSES_KILLED = NewStory("AllBossesKilled");
        public static readonly StoryEvent HAS_DRAFT_TOKEN = NewStory("HasDraftToken");
        public static readonly StoryEvent SAW_P03_INTRODUCTION = NewStory("SawP03Introduction", save:true);
        public static readonly StoryEvent GOLLY_NFT = NewStory("GollyNFTIntro", save:true);
        public static readonly StoryEvent DEFEATED_P03 = NewStory("DefeatedP03");    
        public static readonly StoryEvent ONLY_ONE_BOSS_LIFE = NewStory("P03AscensionOneBossLife", save:true);    
        public static readonly StoryEvent OVERCLOCK_CHANGES = NewStory("P03AscensionOverclock", save:true);   
        public static readonly StoryEvent TRANSFORMER_CHANGES = NewStory("P03AscensionTransformer", save:true);   
        public static readonly StoryEvent HAS_DEFEATED_P03 = NewStory("HasDefeatedP03", save:true);   
        public static readonly StoryEvent USED_LIFE_ITEM = NewStory("HasUsedLifeItem", save:true);  
        public static readonly StoryEvent SAW_NEW_ORB = NewStory("P03HammerOrb", save:true);  

        public const string GAME_OVER = "GameOverZone";

        internal static readonly StoryEvent TALKED_TO_GOOBERTS_FRIEND = NewStory("SpecialEvent01", run:true);
        internal static readonly StoryEvent SAW_GOOBERT_AT_SHOP_NODE = NewStory("SpecialEvent02", run:true);
        internal static readonly StoryEvent GOT_GOOBERT_MAP_HINT = NewStory("SpecialEvent03", run:true);
        internal static readonly StoryEvent SAW_BOUNTY_HUNTER_MEDAL = NewStory("SpecialEvent04", run:true);
        internal static readonly StoryEvent SAVED_GOOBERT = NewStory("SpecialEvent05", run:true);
        internal static readonly StoryEvent FLUSHED_GOOBERT = NewStory("SpecialEvent06", run:true);
        internal static readonly StoryEvent LOST_GOOBERT = NewStory("SpecialEvent07", run:true);
        internal static readonly StoryEvent BOUGHT_GOOBERT = NewStory("SpecialEvent08", run:true);
        internal static readonly StoryEvent DID_NOT_BUY_GOOBERT = NewStory("SpecialEvent09", run:true);
        internal static readonly StoryEvent HAS_NO_GOOBERT = NewStory("SpecialEvent10", run:true);
        internal static readonly StoryEvent HAS_BOUNTY_HUNTER_BRAIN = NewStory("SpecialEvent11", run:true);
        internal static readonly StoryEvent READY_TO_UPGRADE_BRAIN = NewStory("SpecialEvent12", run:true);
        internal static readonly StoryEvent TOLD_ABOUT_BRAIN_UPGRADE = NewStory("SpecialEvent13", run:true);
        internal static readonly StoryEvent WHITE_FLAG_START = NewStory("SpecialEvent14", run:true);
        internal static readonly StoryEvent WHITE_FLAG_COMPLETED = NewStory("SpecialEvent15", run:true);
        internal static readonly StoryEvent DECK_SIZE_START = NewStory("SpecialEvent16", run:true);
        internal static readonly StoryEvent DECK_SIZE_FINISH = NewStory("SpecialEvent17");
        internal static readonly StoryEvent SMUGGLER_INTRO = NewStory("SpecialEvent18", run:true);
        internal static readonly StoryEvent SMUGGLER_ACCEPTED = NewStory("SpecialEvent19", run:true);
        internal static readonly StoryEvent HAS_CONTRABAND = NewStory("SpecialEvent20");
        internal static readonly StoryEvent HAS_FULLY_UPGRADED = NewStory("SpecialEvent21");
        internal static readonly StoryEvent RADIO_INTRO = NewStory("SpecialEvent22", run:true);
        internal static readonly StoryEvent RADIO_ACCEPTED = NewStory("SpecialEvent23", run:true);
        internal static readonly StoryEvent RADIO_IN_PROGRESS = NewStory("SpecialEvent24");
        internal static readonly StoryEvent RADIO_FAILED = NewStory("SpecialEvent25");
        internal static readonly StoryEvent RADIO_SUCCEEDED = NewStory("SpecialEvent26");
        internal static readonly StoryEvent I_HAVE_THREE_BONES = NewStory("SpecialEvent27");
        internal static readonly StoryEvent TIPPED_SCALES_INTRO = NewStory("SpecialEvent28", run:true);
        internal static readonly StoryEvent TIPPED_SCALES_ACCEPTED = NewStory("SpecialEvent29", run:true);
        internal static readonly StoryEvent TIPPED_SCALES_COMPLETED = NewStory("SpecialEvent30");
        internal static readonly StoryEvent SMUGGLER_COMPLETE = NewStory("SpecialEvent31", run:true);
        internal static readonly StoryEvent GENERATOR_INTRO = NewStory("SpecialEvent32", run:true);
        internal static readonly StoryEvent GENERATOR_SUCCESS = NewStory("SpecialEvent33", run:true);
        internal static readonly StoryEvent GENERATOR_FAILURE = NewStory("SpecialEvent34", run:true);
        internal static readonly StoryEvent GENERATOR_READY = NewStory("SpecialEvent35");
        internal static readonly StoryEvent RADIO_COMPLETED = NewStory("SpecialEvent36");
        internal static readonly StoryEvent POWER_INTRO = NewStory("SpecialEvent37", run:true);
        internal static readonly StoryEvent POWER_ACCEPTED = NewStory("SpecialEvent38", run:true);
        internal static readonly StoryEvent POWER_IN_PROGRESS = NewStory("SpecialEvent39");
        internal static readonly StoryEvent POWER_FAILED = NewStory("SpecialEvent40");
        internal static readonly StoryEvent POWER_SUCCEEDED = NewStory("SpecialEvent41");
        internal static readonly StoryEvent POWER_COMPLETED = NewStory("SpecialEvent42");
        internal static readonly StoryEvent DONATION_INTRO = NewStory("SpecialEvent43", run:true);
        internal static readonly StoryEvent CANNOT_DONATE = NewStory("SpecialEvent44");
        internal static readonly StoryEvent DONATED = NewStory("SpecialEvent45", run: true);
        internal static readonly StoryEvent DONATION_REWARD = NewStory("SpecialEvent46", run: true);
        internal static readonly StoryEvent MYCO_ENTRY_APPROVED = NewStory("SpecialEvent47", run: true);
        internal static readonly StoryEvent MYCO_ENTRY_DENIED = NewStory("SpecialEvent48", run: true);
        internal static readonly StoryEvent MYCO_DEFEATED = NewStory("SpecialEvent49", run: true);
        internal static readonly StoryEvent TIPPED_SCALES_REWARD = NewStory("SpecialEvent50", run: true);


        private static void MarkRandomEventAsSelected(SpecialEvent se)
        {
            string eventString = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "AssignedSpecialEvents");
            if (string.IsNullOrEmpty(eventString))
                eventString = se.ToString();
            else
                eventString += "," + se.ToString();
            ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "AssignedSpecialEvents", eventString);
        }

        internal static List<SpecialEvent> SelectedRandomEvents
        {
            get
            {
                string eventString = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "AssignedSpecialEvents");
                if (string.IsNullOrEmpty(eventString))
                    return new();

                return eventString.Split(',').Select(s => (SpecialEvent)Enum.Parse(typeof(SpecialEvent), s)).ToList();
            }
        }

        internal static void SetDialogueForSpecialEvent(this HoloMapConditionalDialogueNode node, SpecialEvent se)
        {
            node.dialogueRules.Clear();

            if (se == SpecialEvent.GoobertQuest)
            {
                if (CompletedZones.Count == 1)
                {
                    node.SetDialogueRule("P03LostGoobert", "MY FRIEND IS LOST", se, preRequisite:LOST_GOOBERT);
                    node.SetDialogueRule("P03GoobertHome", "YOU FOUND HIM!", se, preRequisite:BOUGHT_GOOBERT, antiPreRequisite:HAS_NO_GOOBERT, completeStory:SAVED_GOOBERT, completedCurrencyReward:13, loseItemReward:GoobertHuh.ItemData.name, completeAfter:true);
                }

                if (CompletedZones.Count == 0)
                {
                    node.SetDialogueRule("P03LostGoobert", "MY FRIEND IS LOST", se, preRequisite:LOST_GOOBERT);
                    node.SetDialogueRule("P03DidNotBuyGoobert", "MY FRIEND IS LOST", se, preRequisite:DID_NOT_BUY_GOOBERT);
                    node.SetDialogueRule("P03WhereIsGoobert", "MY FRIEND IS LOST", se, antiPreRequisite:SAW_GOOBERT_AT_SHOP_NODE, completeStory:TALKED_TO_GOOBERTS_FRIEND);
                    node.SetDialogueRule($"P03FoundGoobert{GoobertDropoffZone}", "YOU FOUND HIM!", se, preRequisite:BOUGHT_GOOBERT, antiPreRequisite:HAS_NO_GOOBERT, completeStory:GOT_GOOBERT_MAP_HINT, completeAfter:true);
                }
            }
            if (se == SpecialEvent.ProspectorQuest)
            {
                node.SetDialogueRule("P03ProspectorPrepareGold", "GOLD!", se, preRequisite:HAS_BOUNTY_HUNTER_BRAIN, completeStory:TOLD_ABOUT_BRAIN_UPGRADE);
                node.SetDialogueRule("P03ProspectorReplaceGold", "GOLD!", se, preRequisite:READY_TO_UPGRADE_BRAIN, completeAfter:true, completedCardReward:CustomCards.BOUNTY_HUNTER_SPAWNER, loseCardReward:CustomCards.BRAIN);
                node.SetDialogueRule("P03ProspectorWantGold", "GOLD!", se);
            }
            if (se == SpecialEvent.WhiteFlag)
            {
                node.SetDialogueRule("P03WhiteFlagSetup", "DO THEY GIVE UP?", se, antiPreRequisite:WHITE_FLAG_COMPLETED, completeStory:WHITE_FLAG_START);
                node.SetDialogueRule("P03WhiteFlagReward", "THEY DO GIVE UP", se, preRequisite:WHITE_FLAG_COMPLETED, completedCardReward:CustomCards.UNC_TOKEN, completeAfter:true);
            }
            if (se == SpecialEvent.DeckSize)
            {
                node.SetDialogueRule("P03DeckSizeSetup", "ITS TOO SMALL", se, antiPreRequisite:DECK_SIZE_FINISH, completeStory:DECK_SIZE_START);
                node.SetDialogueRule("P03DeckSizeReward", "ITS JUST RIGHT", se, preRequisite:DECK_SIZE_FINISH, completeAfter:true, completedCurrencyReward:(int)Mathf.Floor(CurrencyGainRange.Item2*1.5f));
            }
            if (se == SpecialEvent.Smuggler)
            {
                node.SetDialogueRule("P03SmugglerSetup", "SSH - COME OVER HERE", se, antiPreRequisite:SMUGGLER_INTRO, completeStory: SMUGGLER_INTRO);
                node.SetDialogueRule("P03SmugglerAccepted", "LETS DO THIS", se, preRequisite:SMUGGLER_INTRO, completeStory: SMUGGLER_ACCEPTED, completedCardReward:CustomCards.CONTRABAND, completeAfter: true);
            }
            if (se == SpecialEvent.SmugglerPartTwo)
            {
                node.SetDialogueRule("P03SmugglerComplete", "SSH - BRING IT HERE", se, preRequisite:HAS_CONTRABAND, completeStory:SMUGGLER_COMPLETE, completeAfter:true, completedCardReward:CustomCards.UNC_TOKEN, loseCardReward:CustomCards.CONTRABAND);
                node.SetDialogueRule("P03SmugglerFailed", "WHERE DID IT GO?", se, antiPreRequisite:SMUGGLER_COMPLETE, completeAfter: true, completeStory:SMUGGLER_COMPLETE);
            }
            if (se == SpecialEvent.FullyUpgraded)
            {
                node.SetDialogueRule("P03FullyUpgradedFail", "SHOW ME POWER", se, antiPreRequisite:HAS_FULLY_UPGRADED);
                node.SetDialogueRule("P03FullyUpgradedSuccess", "SHOW ME POWER", se, preRequisite:HAS_FULLY_UPGRADED, completeAfter:true, completedCurrencyReward:(int)Mathf.Floor(CurrencyGainRange.Item2*1.5f));
            }
            if (se == SpecialEvent.ListenToTheRadio)
            {
                node.SetDialogueRule("P03RadioQuestStart", "LETS DO SCIENCE", se, antiPreRequisite:RADIO_INTRO, completeStory:RADIO_INTRO);
                node.SetDialogueRule("P03RadioQuestAccepted", "LETS DO SCIENCE", se, antiPreRequisite:RADIO_COMPLETED, preRequisite:RADIO_INTRO, completeStory:RADIO_ACCEPTED, completedCardReward:CustomCards.RADIO_TOWER);
                node.SetDialogueRule("P03RadioQuestInProgress", "LETS DO SCIENCE", se, preRequisite:RADIO_IN_PROGRESS);
                node.SetDialogueRule("P03RadioQuestFailed", "YOU BROKE IT?", se, preRequisite:RADIO_FAILED, completeAfter: true);
                node.SetDialogueRule("P03RadioQuestSucceeded", "A WIN FOR SCIENCE", se, preRequisite:RADIO_SUCCEEDED, completeAfter:true, loseCardReward:CustomCards.RADIO_TOWER, completedCardReward:CustomCards.UNC_TOKEN);
            }
            if (se == SpecialEvent.ILoveBones)
            {
                node.SetDialogueRule("P03ILoveBones", "I LOVE BONES!!", se, antiPreRequisite:I_HAVE_THREE_BONES);
                node.SetDialogueRule("P03ILoveBonesSuccess", "I LOVE BONES!!!!", se, preRequisite:I_HAVE_THREE_BONES, completedCardReward:CustomCards.SKELETON_LORD, completeAfter:true);
            }
            if (se == SpecialEvent.TippedScales)
            {
                node.SetDialogueRule("P03TooEasyQuest", "TOO EASY...", se, antiPreRequisite:TIPPED_SCALES_INTRO, completeStory:TIPPED_SCALES_INTRO);
                node.SetDialogueRule("P03TooEasyAccepted", "TOO EASY...", se, preRequisite:TIPPED_SCALES_INTRO, completeStory:TIPPED_SCALES_ACCEPTED);
                node.SetDialogueRule("P03TooEasyInProgress", "TOO EASY...", se, preRequisite:TIPPED_SCALES_ACCEPTED, antiPreRequisite:TIPPED_SCALES_COMPLETED);
                node.SetDialogueRule("P03TooEasyComplete", "TOO EASY...", se, antiPreRequisite: TIPPED_SCALES_REWARD, preRequisite:TIPPED_SCALES_COMPLETED, completeAfter:true, specialReward:SpecialReward.RandomCardGainsUndying, completeStory:TIPPED_SCALES_REWARD);
            }
            if (se == SpecialEvent.BrokenGeneratorQuest)
            {
                node.SetDialogueRule("P03DamageRaceFailed", "OH NO...", se, preRequisite:GENERATOR_FAILURE, completeAfter:true);
                node.SetDialogueRule("P03DamageRaceSuccess", "PHEW!", se, preRequisite:GENERATOR_SUCCESS);
                node.SetDialogueRule("P03DamageRaceIntro", "HELP!", se, completeStory:GENERATOR_INTRO);
            }
            if (se == SpecialEvent.PowerUpTheTower)
            {
                node.SetDialogueRule("P03PowerQuestStart", "LOOKING FOR A JOB?", se, antiPreRequisite:POWER_INTRO, completeStory:POWER_INTRO);
                node.SetDialogueRule("P03PowerQuestAccepted", "LOOKING FOR A JOB?", se, antiPreRequisite:POWER_COMPLETED, preRequisite:POWER_INTRO, completeStory:POWER_ACCEPTED, completedCardReward:CustomCards.POWER_TOWER);
                node.SetDialogueRule("P03PowerQuestInProgress", "GET BACK TO WORK", se, preRequisite:POWER_IN_PROGRESS);
                node.SetDialogueRule("P03PowerQuestFailed", "YOU BROKE IT?", se, preRequisite:POWER_FAILED, completeAfter: true);
                node.SetDialogueRule("P03PowerQuestSucceeded", "HERE'S YOUR PAYMENT", se, preRequisite:POWER_SUCCEEDED, completeAfter:true, loseCardReward:CustomCards.POWER_TOWER, completedCurrencyReward:CurrencyGainRange.Item2);
            }
            if (se == SpecialEvent.Donation)
            {
                node.SetDialogueRule("P03DonationIntro", "SPARE SOME CASH?", se, antiPreRequisite:DONATION_INTRO, completeStory:DONATION_INTRO);
                node.SetDialogueRule("P03DonationComplete", "SPARE SOME CASH?", se, antiPreRequisite:CANNOT_DONATE, preRequisite:DONATION_INTRO, completeStory:DONATED, completedCurrencyReward:-10, completeAfter:true);
                node.SetDialogueRule("P03DonationNotEnough", "SPARE SOME CASH?", se, antiPreRequisite:DONATED, preRequisite:DONATION_INTRO);
            }
            if (se == SpecialEvent.DonationPartTwo)
            {
                node.SetDialogueRule("P03DonationReward", "THANK YOU!", se, antiPreRequisite:DONATION_REWARD, preRequisite:DONATED, completeAfter:true, specialReward:SpecialReward.GemifyTwoRandomCards, completeStory:DONATION_REWARD);
            }
        }

        // The first special event always appears in color zone 1; the others explicitly don't appear in color zone 1
        internal static List<Tuple<SpecialEvent, Predicate<HoloMapBlueprint>>> GetSpecialEventForZone(RunBasedHoloMap.Zone zone)
        {
            List<Tuple<SpecialEvent, Predicate<HoloMapBlueprint>>> events = new ();

            int completedZoneCount = CompletedZones.Count;

            // Randomized special events
            List<SpecialEvent> possibles = new() {
                SpecialEvent.WhiteFlag,
                SpecialEvent.DeckSize,
                SpecialEvent.Smuggler,
                SpecialEvent.ListenToTheRadio,
                SpecialEvent.TippedScales,
                SpecialEvent.PowerUpTheTower
            };

            List<SpecialEvent> alreadyAssigned = SelectedRandomEvents;
            possibles.RemoveAll(se => alreadyAssigned.Contains(se));

            // This code checks quests against map count
            // Some can only happen in certain maps
            if (CurrentZone == RunBasedHoloMap.Zone.Undead || CompletedZones.Any(z => z.ToLowerInvariant().EndsWith("undead")))
                possibles.Add(SpecialEvent.ILoveBones);

            if (completedZoneCount >= 3)
            {
                possibles.Remove(SpecialEvent.Smuggler);
                possibles.Add(SpecialEvent.FullyUpgraded);
            }            

            if (P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("Event"))
            {
                try
                {
                    int idx = P03Plugin.Instance.DebugCode.ToLowerInvariant().IndexOf("event[");
                    int eidx = P03Plugin.Instance.DebugCode.ToUpperInvariant().IndexOf("]");
                    string substr = P03Plugin.Instance.DebugCode.Substring(idx + 6, eidx - idx - 6);
                    SpecialEvent dEvent = (SpecialEvent)Enum.Parse(typeof(SpecialEvent), substr);

                    events.Add(new (dEvent, bp => bp.color == 1 && !bp.isSecretRoom)); // randomly selected events should appear in the first color
                    MarkRandomEventAsSelected(dEvent);
                    possibles.Clear();
            
                } 
                catch (Exception ex)
                {
                    P03Plugin.Log.LogWarning($"Could not parse special event from debug string! {ex}");                    
                }
            }
            if (possibles.Count > 0)
            {
                SpecialEvent randomEvent = possibles[SeededRandom.Range(0, possibles.Count, P03AscensionSaveData.RandomSeed)];
                events.Add(new (randomEvent, bp => bp.color == 1 && !bp.isSecretRoom)); // randomly selected events should appear in the first color
                MarkRandomEventAsSelected(randomEvent);
            }
            
            // Special, special events

            if (StoryEventsData.EventCompleted(RADIO_ACCEPTED) && !StoryEventsData.EventCompleted(RADIO_COMPLETED))
                events.Add(new (SpecialEvent.ListenToTheRadio, bp => bp.color != 1 && !bp.IsDeadEnd && !bp.isSecretRoom));

            if (StoryEventsData.EventCompleted(POWER_ACCEPTED) && !StoryEventsData.EventCompleted(POWER_COMPLETED))
                events.Add(new (SpecialEvent.PowerUpTheTower, bp => bp.color != 1 && !bp.IsDeadEnd && !bp.isSecretRoom));

            if (StoryEventsData.EventCompleted(SMUGGLER_ACCEPTED) && !StoryEventsData.EventCompleted(SMUGGLER_COMPLETE))
                events.Add(new (SpecialEvent.SmugglerPartTwo, bp => bp.color != 1 && bp.IsDeadEnd && !bp.isSecretRoom));

            if (StoryEventsData.EventCompleted(DONATED) && !StoryEventsData.EventCompleted(DONATION_REWARD))
                events.Add(new (SpecialEvent.DonationPartTwo, bp => bp.color != 1 && !bp.IsDeadEnd && !bp.isSecretRoom));

            if (StoryEventsData.EventCompleted(TIPPED_SCALES_ACCEPTED) && !StoryEventsData.EventCompleted(TIPPED_SCALES_REWARD))
                events.Add(new (SpecialEvent.TippedScales, bp => bp.color == 2 && !bp.IsDeadEnd && !bp.isSecretRoom));
            
            if (completedZoneCount == 0 && !P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("goobert"))
                events.Add(new (SpecialEvent.GoobertQuest, bp => bp.isSecretRoom));

            if (completedZoneCount == 1 && !P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("goobert"))
                events.Add(new (SpecialEvent.BrokenGeneratorQuest, bp => bp.isSecretRoom));

            if (completedZoneCount == 1 && !P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("goobert"))
                if (zone == GoobertDropoffZone && StoryEventsData.EventCompleted(BOUGHT_GOOBERT))
                    events.Add(new (SpecialEvent.GoobertQuest, bp => bp.color != 1 && !bp.isSecretRoom));

            if (completedZoneCount > 0 && !P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("goobert") && Part3SaveData.Data.deck.Cards.Any(c => c.name.Equals(CustomCards.BRAIN)))
                events.Add(new (SpecialEvent.ProspectorQuest, bp => bp.color != 1 && !bp.isSecretRoom));

            return events;
        }

        public class NPCDescriptor
        {
            public string faceCode;
            public CompositeFigurine.FigurineType head;
            public CompositeFigurine.FigurineType arms;
            public CompositeFigurine.FigurineType body;

            public NPCDescriptor(string code)
            {
                string[] pieces = code.Split('|');
                faceCode = pieces[0];
                head = (CompositeFigurine.FigurineType)Enum.Parse(typeof(CompositeFigurine.FigurineType), pieces[1]);
                arms = (CompositeFigurine.FigurineType)Enum.Parse(typeof(CompositeFigurine.FigurineType), pieces[2]);
                body = (CompositeFigurine.FigurineType)Enum.Parse(typeof(CompositeFigurine.FigurineType), pieces[3]);
            }
        }

        internal static bool SawCredits
        {
            get { return ModdedSaveManager.SaveData.GetValueAsBoolean(P03Plugin.PluginGuid, "SawCredits"); }
            set { ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, "SawCredits", value); }
        }

        internal static NPCDescriptor GetDescriptorForNPC(SpecialEvent se)
        {
            string descriptorString = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, $"NPC{se}");
            if (!string.IsNullOrEmpty(descriptorString))
                return new NPCDescriptor(descriptorString);

            string faceCode = P03ModularNPCFace.GeneratedNPCFaceCode();

            int randomSeed = P03AscensionSaveData.RandomSeed + 350;
            CompositeFigurine.FigurineType head = (CompositeFigurine.FigurineType)SeededRandom.Range(0, (int)CompositeFigurine.FigurineType.NUM_FIGURINES, randomSeed++);
            CompositeFigurine.FigurineType arms = (CompositeFigurine.FigurineType)SeededRandom.Range(0, (int)CompositeFigurine.FigurineType.NUM_FIGURINES, randomSeed++);
            CompositeFigurine.FigurineType body = (CompositeFigurine.FigurineType)SeededRandom.Range(0, (int)CompositeFigurine.FigurineType.NUM_FIGURINES, randomSeed++);

            if (se == SpecialEvent.ProspectorQuest)
                head = arms = body = CompositeFigurine.FigurineType.Prospector;

            string newDescriptor = $"{faceCode}|{head}|{arms}|{body}";
            ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, $"NPC{se}", newDescriptor);
            NumberOfGeneratedNPCs += 1;
            return new NPCDescriptor(newDescriptor);
        }

        public static int NumberOfGeneratedNPCs
        {
            get { return ModdedSaveManager.RunState.GetValueAsInt(P03Plugin.PluginGuid, "NumberOfGeneratedNPCs"); }
            set { ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "NumberOfGeneratedNPCs", value); }
        }

        internal static void GrantSpecialReward(SpecialReward reward)
        {
            if (reward == SpecialReward.None)
                return;

            if (reward == SpecialReward.GemifyTwoRandomCards)
            {
                List<CardInfo> cards = Part3SaveData.Data.deck.Cards.Where(c => !c.Gemified).ToList();
                int randomSeed = P03AscensionSaveData.RandomSeed;
                while (cards.Count > 2)
                    cards.RemoveAt(SeededRandom.Range(0, cards.Count, randomSeed++));
                
                foreach (CardInfo card in cards)
                {
                    CardModificationInfo mod = new() { gemify = true };
                    Part3SaveData.Data.deck.ModifyCard(card, mod);
                }
            }

            if (reward == SpecialReward.RandomCardGainsUndying)
            {
                List<CardInfo> cards = Part3SaveData.Data.deck.Cards.Where(c => !c.HasAbility(Ability.DrawCopyOnDeath)).ToList();
                if (cards.Count == 0)
                    return;

                CardInfo target = cards[SeededRandom.Range(0, cards.Count, P03AscensionSaveData.RandomSeed)];
                CardModificationInfo mod = new(Ability.DrawCopyOnDeath);
                Part3SaveData.Data.deck.ModifyCard(target, mod);
            }            
        }

        public const int POWER_TURNS = 4;
        public const int RADIO_TURNS = 5;

        public static int RadioUpkeepCount
        {
            get { return ModdedSaveManager.RunState.GetValueAsInt(P03Plugin.PluginGuid, "RadioUpkeepCount"); }
            set { ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "RadioUpkeepCount", value); }
        }

        public static int PowerUpkeepCount
        {
            get { return ModdedSaveManager.RunState.GetValueAsInt(P03Plugin.PluginGuid, "PowerUpkeepCount"); }
            set { ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "PowerUpkeepCount", value); }
        }

        public static int ChallengeBattlesWon
        {
            get { return ModdedSaveManager.RunState.GetValueAsInt(P03Plugin.PluginGuid, "ChallengeBattlesWon"); }
            set { ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "ChallengeBattlesWon", value); }
        }

        public static Part3SaveData.WorldPosition MycologistReturnPosition
        {
            get
            {
                string key = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "MycologistReturnPosition");
                if (string.IsNullOrEmpty(key))
                    throw new InvalidOperationException("Trying to get the mycologist return position when it has never been set!");

                string[] pieces = key.Split('|');
                return new (pieces[0], int.Parse(pieces[1]), int.Parse(pieces[2]));
            }
            set
            {
                string key = $"{value.worldId}|{value.gridX}|{value.gridY}";
                ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "MycologistReturnPosition", key);
            }
        }

        public static List<CardInfo> MycologistTestSubjects
        {
            get
            {
                List<CardInfo> retval = new();
                string key = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "MycologistTestSubjects");
                if (string.IsNullOrEmpty(key))
                    return retval;

                string[] pieces = key.Split('%');
                retval.AddRange(pieces.Select(CustomCards.ConvertCodeToCard));
                return retval;
            }
        }

        public static void AddMycologistsTestSubject(CardInfo info)
        {
            string subjectCode = CustomCards.ConvertCardToCompleteCode(info);
            string currentKey = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "MycologistTestSubjects");
            if (string.IsNullOrEmpty(currentKey))
                currentKey = subjectCode;
            else
                currentKey += "%" + subjectCode;
            ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "MycologistTestSubjects", currentKey);
        }

        public static RunBasedHoloMap.Zone CurrentZone
        {
            get  
            {
                try
                {
                    return RunBasedHoloMap.Building ? RunBasedHoloMap.BuildingZone : RunBasedHoloMap.GetRegionCodeFromWorldID(HoloMapAreaManager.Instance.CurrentWorld.name);
                }
                catch (Exception ex)
                {
                    return RunBasedHoloMap.Zone.Neutral;
                }
            }
        }

        internal static int DeckSizeTarget
        {
            get
            {
                int retval = ModdedSaveManager.RunState.GetValueAsInt(P03Plugin.PluginGuid, "DeckSizeTarget");
                if (retval > 0)
                    return retval;

                retval = Part3SaveData.Data.deck.Cards.Count + 2;
                ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "DeckSizeTarget", retval);
                return retval;
            }
        }

        internal static RunBasedHoloMap.Zone GoobertDropoffZone
        {
            get
            {
                string dropoff = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "Dropoff");
                if (!string.IsNullOrEmpty(dropoff))
                    return (RunBasedHoloMap.Zone)Enum.Parse(typeof(RunBasedHoloMap.Zone), dropoff);

                List<RunBasedHoloMap.Zone> zones = new () 
                { 
                    RunBasedHoloMap.Zone.Magic, RunBasedHoloMap.Zone.Nature, RunBasedHoloMap.Zone.Tech, RunBasedHoloMap.Zone.Undead
                };
                zones.Remove(CurrentZone);
                RunBasedHoloMap.Zone assignedDropoff = zones[SeededRandom.Range(0, zones.Count, P03AscensionSaveData.RandomSeed)];
                ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "Dropoff", assignedDropoff);
                return assignedDropoff;
            }
        }

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
            MechanicsConcept.PhotographerTakeSnapshot,
            MechanicsConcept.DamageRaceBattle
        };

        public static readonly StoryEvent[] P03_ALWAYS_TRUE_STORIES = new StoryEvent[]
        {
            StoryEvent.LukeVOBeatLeshyAgain,
            StoryEvent.LukeVODieAlready,
            StoryEvent.LukeVOLeshyRematch,
            StoryEvent.LukeVOMantisGod,
            StoryEvent.LukeVOPart3Shit,
            StoryEvent.LukeVOPart3Yes,
            StoryEvent.LukeVOPart3Wtf,
            StoryEvent.LukeVOPart3File,
            StoryEvent.LukeVOOPCard,
            StoryEvent.LukeVONewRunAfterVictory,
            StoryEvent.LukeVOPart1Vision,
            StoryEvent.LukeVOPart2Bonelord,
            StoryEvent.LukeVOPart2Grimora,
            StoryEvent.LukeVOPart3CloseWin
        };

        private static readonly Dictionary<HoloMapNode.NodeDataType, float> CostAdjustments = new ()
        {
            { HoloMapNode.NodeDataType.AddCardAbility, 0f },
            { HoloMapNode.NodeDataType.BuildACard, 1f },
            { UnlockAscensionItemNodeData.UnlockItemsAscension, 0.5f },
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

        public static IEnumerator SayDialogueOnce(string dialogueId, StoryEvent eventTracker)
        {
            if (!StoryEventsData.EventCompleted(eventTracker))
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent(dialogueId, TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                StoryEventsData.SetEventCompleted(eventTracker);
            }
            yield break;
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
        public static IEnumerator EarnCurrencyWhenBountyHunterDies(IEnumerator sequence, PlayableCard killer, BountyHunter __instance)
        {
            yield return sequence;

            if (!SaveFile.IsAscension || TurnManager.Instance.Opponent is P03AscensionOpponent) // don't do this on the final boss
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

            // Don't spawn the brain in the following situations:
            if (killer != null && (killer.HasAbility(Ability.Deathtouch) || killer.HasAbility(Ability.SteelTrap)))
                yield break;

            // Spawn at most one per run
            if (StoryEventsData.EventCompleted(SAW_BOUNTY_HUNTER_MEDAL))
                yield break;

            // This can only happen on the very first bounty hunter of the run. You get exactly one shot
            if (Part3SaveData.Data.bountyHunterMods.Count != 1)
                yield break;

            // Get the brain but take the ability off of it
            CardInfo brain = CardLoader.GetCardByName(CustomCards.BRAIN);
            yield return BoardManager.Instance.CreateCardInSlot(brain, (__instance.Card as PlayableCard).Slot, 0.15f, true);
            StoryEventsData.SetEventCompleted(SAW_BOUNTY_HUNTER_MEDAL);
        }

        public static int NumberOfLivesRemaining
        {
            get { return ModdedSaveManager.RunState.GetValueAsInt(P03Plugin.PluginGuid, "NumberOfLivesRemaining"); }
            set { ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "NumberOfLivesRemaining", value); }
        }

        public const int ENEMIES_TO_UNLOCK_BOSS = 4;
        public static int NumberOfZoneEnemiesKilled
        {
            get 
            { 
                string key = $"{CurrentZone}_EnemiesKilled";
                return ModdedSaveManager.RunState.GetValueAsInt(P03Plugin.PluginGuid, key); 
            }
            set 
            {
                string key = $"{CurrentZone}_EnemiesKilled";
                ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, key, value); 
            }
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
            if (SaveFile.IsAscension && P03AscensionSaveData.IsP03Run && P03RunBasedStoryEvents.Contains(storyEvent))
            {
                ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, $"StoryEvent{storyEvent}", true);
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
                if (P03_ALWAYS_TRUE_STORIES.Contains(storyEvent))
                {
                    __result = true;
                    return false;
                }

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

                if (storyEvent == CANNOT_DONATE)
                {
                    __result = Part3SaveData.Data.currency < 10;
                    return false;
                }

                if (storyEvent == DECK_SIZE_FINISH)
                {
                    __result = Part3SaveData.Data.deck.Cards.Count >= DeckSizeTarget;
                    return false;
                }

                if (storyEvent == HAS_BOUNTY_HUNTER_BRAIN)
                {
                    __result = Part3SaveData.Data.deck.Cards.Any(c => c.name.Equals(CustomCards.BRAIN));
                    return false;
                }

                if (storyEvent == HAS_CONTRABAND)
                {
                    __result = Part3SaveData.Data.deck.Cards.Any(c => c.name.Equals(CustomCards.CONTRABAND));
                    return false;
                }

                if (storyEvent == TIPPED_SCALES_COMPLETED)
                {
                    __result = ChallengeBattlesWon >= 5;
                    return false;
                }

                if (storyEvent == READY_TO_UPGRADE_BRAIN)
                {
                    __result = StoryEventsData.EventCompleted(HAS_BOUNTY_HUNTER_BRAIN) && StoryEventsData.EventCompleted(TOLD_ABOUT_BRAIN_UPGRADE);
                    return false;
                }

                if (storyEvent == HAS_FULLY_UPGRADED)
                {
                    __result = Part3SaveData.Data.deck.Cards.Any(c =>
                        c.HasAbility(Ability.Transformer) &&
                        c.HasAbility(NewPermaDeath.AbilityID) &&
                        c.Gemified
                    );
                    return false;
                }

                if (storyEvent == GENERATOR_READY)
                {
                    __result = !StoryEventsData.EventCompleted(GENERATOR_FAILURE) &&
                               !StoryEventsData.EventCompleted(GENERATOR_SUCCESS);
                    return false;
                }

                if (storyEvent == I_HAVE_THREE_BONES)
                {
                    __result = Part3SaveData.Data.deck.Cards.Where(c => c.HasAbility(Ability.Brittle) || c.HasAbility(NewPermaDeath.AbilityID)).Count() >= 3;
                    return false;
                }

                if (storyEvent == RADIO_IN_PROGRESS)
                {
                    __result = StoryEventsData.EventCompleted(RADIO_ACCEPTED) &&
                               RadioUpkeepCount < RADIO_TURNS &&
                               Part3SaveData.Data.deck.Cards.Any(c => c.name.Equals(CustomCards.RADIO_TOWER));
                    return false;
                }

                if (storyEvent == RADIO_FAILED)
                {
                    __result = StoryEventsData.EventCompleted(RADIO_ACCEPTED) &&
                               RadioUpkeepCount < RADIO_TURNS &&
                               !Part3SaveData.Data.deck.Cards.Any(c => c.name.Equals(CustomCards.RADIO_TOWER));
                    return false;
                }

                if (storyEvent == RADIO_SUCCEEDED)
                {
                    __result = StoryEventsData.EventCompleted(RADIO_ACCEPTED) && RadioUpkeepCount >= RADIO_TURNS;
                    return false;
                }

                if (storyEvent == RADIO_COMPLETED)
                {
                    __result = StoryEventsData.EventCompleted(RADIO_SUCCEEDED) || StoryEventsData.EventCompleted(RADIO_FAILED);
                    return false;
                }

                if (storyEvent == POWER_IN_PROGRESS)
                {
                    __result = StoryEventsData.EventCompleted(POWER_ACCEPTED) &&
                               PowerUpkeepCount < POWER_TURNS &&
                               Part3SaveData.Data.deck.Cards.Any(c => c.name.Equals(CustomCards.POWER_TOWER));
                    return false;
                }

                if (storyEvent == POWER_FAILED)
                {
                    __result = StoryEventsData.EventCompleted(POWER_ACCEPTED) &&
                               PowerUpkeepCount < POWER_TURNS &&
                               !Part3SaveData.Data.deck.Cards.Any(c => c.name.Equals(CustomCards.POWER_TOWER));
                    return false;
                }

                if (storyEvent == POWER_SUCCEEDED)
                {
                    __result = StoryEventsData.EventCompleted(POWER_ACCEPTED) && PowerUpkeepCount >= POWER_TURNS;
                    return false;
                }

                if (storyEvent == POWER_COMPLETED)
                {
                    __result = StoryEventsData.EventCompleted(POWER_SUCCEEDED) || StoryEventsData.EventCompleted(POWER_FAILED);
                    return false;
                }

                if (P03AscensionSaveEvents.Contains(storyEvent))
                {
                    __result = ModdedSaveManager.SaveData.GetValueAsBoolean(P03Plugin.PluginGuid, $"StoryEvent{storyEvent}");
                    return false;
                }

                if (P03RunBasedStoryEvents.Contains(storyEvent))
                {
                    __result = ModdedSaveManager.RunState.GetValueAsBoolean(P03Plugin.PluginGuid, $"StoryEvent{storyEvent}");
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(TurnManager), "CleanupPhase")]
        [HarmonyPrefix]
        public static void TrackVictories(ref TurnManager __instance)
        {
            if (!SaveFile.IsAscension)
                return;

            // NOTE! In the prefix, the calculation for 'player won' hasn't happened yet
            // So we have to manually do all the checks for what constitutes 'victory'

            if (__instance.SpecialSequencer is DamageRaceBattleSequencer drbs)
            {
                if (drbs.damageDealt >= DamageRaceBattleSequencer.DAMAGE_TO_SUCCEED)
                {
                    StoryEventsData.SetEventCompleted(GENERATOR_SUCCESS);
                }
                else
                {
                    // We don't want failure in the generator to actually cause the player to lose
                    Part3SaveData.Data.playerLives += 1;
                    StoryEventsData.SetEventCompleted(GENERATOR_FAILURE);
                }
            }

            if (__instance.SpecialSequencer is not DamageRaceBattleSequencer)
            {
                if (__instance.Opponent.NumLives <= 0 || __instance.Opponent.Surrendered)
                {
                    NumberOfZoneEnemiesKilled = NumberOfZoneEnemiesKilled + 1;

                    if (StoryEventsData.EventCompleted(TIPPED_SCALES_ACCEPTED))
                        ChallengeBattlesWon += 1;
                }
            }

            if (StoryEventsData.EventCompleted(WHITE_FLAG_START) && __instance.Opponent.Surrendered)
                StoryEventsData.SetEventCompleted(WHITE_FLAG_COMPLETED);
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPostfix]
        private static IEnumerator TippedScalesQuest(IEnumerator sequence)
        {
            yield return sequence;

            if (SaveFile.IsAscension)
            {
                if (StoryEventsData.EventCompleted(TIPPED_SCALES_ACCEPTED) && !StoryEventsData.EventCompleted(TIPPED_SCALES_COMPLETED))
                {
                    yield return LifeManager.Instance.ShowDamageSequence(1, 1, true, 0.125f, null, 0f, false);
                }
            }
        }

        private static bool SlotHasBrain(CardSlot slot)
        {
            if (slot.Card != null && slot.Card.Info.name.Equals(CustomCards.BRAIN))
                return true;

            Card queueCard = BoardManager.Instance.GetCardQueuedForSlot(slot);
            if (queueCard != null && queueCard.Info.name.Equals(CustomCards.BRAIN))
                return true;

            return false;
        }

        [HarmonyPatch(typeof(TurnManager), "CleanupPhase")]
        [HarmonyPostfix]
        public static IEnumerator AcquireBrain(IEnumerator sequence)
        {
            if (SaveFile.IsAscension)
            {
                if (BoardManager.Instance.opponentSlots.Any(SlotHasBrain))
                {
                    yield return TextDisplayer.Instance.PlayDialogueEvent("P03BountyHunterBrain", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                    CardInfo brain = CardLoader.GetCardByName(CustomCards.BRAIN);
                    brain.mods = new();
                    brain.mods.Add(new(Ability.DrawRandomCardOnDeath));
                    Part3SaveData.Data.deck.AddCard(brain);
                }
            }

            yield return sequence;
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
            AscensionMenuScreens.ReturningFromFailedRun = !success;
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
            //AscensionSaveData.Data.currentRun = null;

            if (EventManagement.CompletedZones.Count > 0)
                AscensionSaveData.Data.numRunsSinceReachedFirstBoss = 0;

            // Let's no longer force this to false
            // It should go false when the screen loads
            // and leaving it 'as is' should help the restart work.
            //P03AscensionSaveData.IsP03Run = false;

            Part3SaveData.Data.checkpointPos = new Part3SaveData.WorldPosition(GAME_OVER, 0, 0);

            SaveManager.SaveToFile(false);

            P03Plugin.Log.LogInfo("Loading ascension scene");

            if (SawCredits || !success)
                SceneLoader.Load("Ascension_Configure");
            else
                SceneLoader.Load("Ascension_Credits");
		}
    }
}