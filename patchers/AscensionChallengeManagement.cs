using HarmonyLib;
using DiskCardGame;
using System.Collections.Generic;
using InscryptionAPI.Helpers;
using System.Linq;
using UnityEngine;
using InscryptionAPI.Ascension;
using InscryptionAPI.Guid;
using System.Collections;
using System;
using InscryptionAPI.Saves;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class AscensionChallengeManagement
    {
        public static AscensionChallenge BOUNTY_HUNTER { get; private set; }
        public static AscensionChallenge BOMB_CHALLENGE { get; private set; }
        public static AscensionChallenge ENERGY_HAMMER { get; private set; }
        public static AscensionChallenge ALL_CONVEYOR { get; private set; }

        private static string CompatibleChallengeList
        {
            get 
            {
                return ModdedSaveManager.SaveData.GetValue(P03Plugin.PluginGuid, "P03CompatibleChallenges");
            }
        }

        public const int HAMMER_ENERGY_COST = 2;

        public static Dictionary<AscensionChallenge, AscensionChallengeInfo> PatchedChallengesReference;
        public static List<AscensionChallenge> ValidChallenges;

        public static void UpdateP03Challenges()
        {
            BOUNTY_HUNTER = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "HigherBounties");
            BOMB_CHALLENGE = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "ExplodingBots");
            ENERGY_HAMMER = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "EnergyHammer");
            ALL_CONVEYOR = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "AllConveyor");

            PatchedChallengesReference = new();

            PatchedChallengesReference.Add(
                AscensionChallenge.NoClover,
                new() {
                    challengeType = ALL_CONVEYOR,
                    title = "Overactive Factory",
                    description = "All regular battles are conveyor battles",
                    iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_conveyorbattle.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_conveyorbattle_active.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 10
                }
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.SubmergeSquirrels,
                new() {
                    challengeType = BOUNTY_HUNTER,
                    title = "Wanted Fugitive",
                    description = "Your bounty level is permanently increased by 1",
                    iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_bounthunter.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 10
                }
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.GrizzlyMode,
                new() {
                    challengeType = BOUNTY_HUNTER,
                    title = "Wanted Fugitive",
                    description = "Your bounty level is permanently increased by 1",
                    iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_bounthunter.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 10
                }
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.BossTotems,
                new() {
                    challengeType = BOMB_CHALLENGE,
                    title = "Explosive Bots",
                    description = "All non-vessel bots self destruct when they die",
                    iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_bomb.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_bombactivated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 5
                }
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.AllTotems,
                new() {
                    challengeType = ENERGY_HAMMER,
                    title = "Energy Hammer",
                    description = "The hammer now costs 2 energy to use",
                    iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_energyhammer.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_energyhammer_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 10
                }
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.NoHook,
                new() {
                    challengeType = AscensionChallenge.NoHook,
                    title = "No Remote",
                    description = "You do not start with Mrs. Bomb's Remote",
                    iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_nohook.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 5
                }
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.ExpensivePelts,
                new() {
                    challengeType = AscensionChallenge.ExpensivePelts,
                    title = "Pricey Upgrades",
                    description = "All upgrades cost more",
                    iconSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_expensivepelts"), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 5
                }
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.LessLives,
                new() {
                    challengeType = AscensionChallenge.LessLives,
                    title = "Single Life",
                    description = "You are limited to 1 life",
                    iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_oneup.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 30
                }
            );

            ValidChallenges = new() {
                AscensionChallenge.BaseDifficulty,
                AscensionChallenge.ExpensivePelts,
                AscensionChallenge.LessConsumables,
                AscensionChallenge.LessLives,
                AscensionChallenge.NoBossRares,
                AscensionChallenge.NoHook, 
                AscensionChallenge.StartingDamage,
                AscensionChallenge.WeakStarterDeck,
                AscensionChallenge.SubmergeSquirrels, // This gets replaced by BOUNTY_HUNTER - we mark it as valid so that we can calculate its unlock level properly
                BOUNTY_HUNTER,
                AscensionChallenge.BossTotems, // This gets replaced by BOMB_CHALLENGE - we mark it as valid so that we can calculate its unlock level properly
                BOMB_CHALLENGE,
                AscensionChallenge.AllTotems, // This gets replaced by ENERGY_HAMMER - we mark it as valid so that we can calculate its unlock level properly
                ENERGY_HAMMER,
                AscensionChallenge.NoClover,
                ALL_CONVEYOR
            };

            ChallengeManager.ModifyChallenges += delegate(List<ChallengeManager.FullChallenge> challenges)
            {
                if (P03AscensionSaveData.IsP03Run)
                    for (int i = 0; i < challenges.Count; i++)
                        if (PatchedChallengesReference.ContainsKey(challenges[i].Challenge.challengeType))
                            //challenges[i] = PatchedChallengesReference[challenges[i].challengeType];
                            challenges[i] = new () { 
                                Challenge = PatchedChallengesReference[challenges[i].Challenge.challengeType],
                                AppearancesInChallengeScreen = 1,
                                UnlockLevel = challenges[i].UnlockLevel
                            };

                return challenges;
            };
        }

        [HarmonyPatch(typeof(AscensionChallengeScreen), nameof(AscensionChallengeScreen.OnEnable))]
        [HarmonyPostfix]
        private static void HideLockedBossIcon(AscensionChallengeScreen __instance)
        {
            __instance.gameObject.GetComponentInChildren<ChallengeIconGrid>().Start();
        }

        // [HarmonyPatch(typeof(AscensionChallengePaginator), nameof(AscensionChallengePaginator.ShowVisibleChallenges))]
        // [HarmonyPostfix]
        // private static void MakeIconGridRecalc(AscensionChallengePaginator __instance)
        // {
        //     if (!AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(AscensionChallenge.FinalBoss, AscensionSaveData.Data.challengeLevel))
        //         __instance.gameObject.GetComponentInChildren<ChallengeIconGrid>().finalBossIcon.SetActive(false);
        // }

        [HarmonyPatch(typeof(ChallengeIconGrid), nameof(ChallengeIconGrid.Start))]
        [HarmonyPrefix]
        private static void DynamicSwapSize(ChallengeIconGrid __instance)
        {
            if (!AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(AscensionChallenge.FinalBoss, AscensionSaveData.Data.challengeLevel))
			{
				__instance.finalBossIcon.SetActive(false);
				float xStart = -1.65f;
				for (int i = 0; i < __instance.topRowIcons.Count; i++)
					__instance.topRowIcons[i].localPosition = new Vector2(xStart + (float)i * 0.55f, __instance.topRowIcons[i].localPosition.y);

				for (int j = 0; j < __instance.bottomRowIcons.Count; j++)
					__instance.bottomRowIcons[j].localPosition = new Vector2(xStart + (float)j * 0.55f, __instance.bottomRowIcons[j].localPosition.y);
			}
        }


        [HarmonyPatch(typeof(AscensionUnlockSchedule), nameof(AscensionUnlockSchedule.ChallengeIsUnlockedForLevel))]
        [HarmonyAfter(new string[] { "cyantist.inscryption.api" })]
        [HarmonyPostfix]
        public static void ValidP03Challenges(ref bool __result, AscensionChallenge challenge, int level)
        {
            if (ScreenManagement.ScreenState == CardTemple.Tech)
            {
                if (PatchedChallengesReference.Any(kvp => kvp.Value.challengeType == challenge))
                {
                    var kvp = PatchedChallengesReference.First(kvp => kvp.Value.challengeType == challenge);
                    if (kvp.Value.challengeType != kvp.Key)
                    {
                        __result = AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(kvp.Key, level);
                        return;
                    }
                }

                if (ValidChallenges.Contains(challenge))
                    return;

                var fullChallenge = ChallengeManager.AllChallenges.FirstOrDefault(fc => fc.Challenge.challengeType == challenge);
                if (fullChallenge != null)
                {
                    if (fullChallenge.Flags == null)
                    {
                        __result = false;
                        return;
                    }

                    if (!fullChallenge.Flags.Any(f => f != null && f.ToString().ToLowerInvariant().Equals("p03")))
                    {
                        __result = false;
                        return;
                    }
                }                
            }
        }

        [HarmonyPatch(typeof(Part3CardDrawPiles), nameof(Part3CardDrawPiles.AddModsToVessel))]
        [HarmonyPostfix]
        private static void AddSideDeckAbilitiesWithoutMesh(CardInfo info)
        {
            if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.SubmergeSquirrels))
            {
                if (info != null && !info.HasAbility(Ability.Submerge))
                {
                    CardModificationInfo abMod = new(Ability.Submerge);
                    abMod.sideDeckMod = true;
                    info.mods.Add(abMod);
                }
            }
        }

        private static bool CardShouldExplode(this PlayableCard card)
        {
            return !card.Info.name.ToLowerInvariant().Contains("vessel") && !card.Info.HasTrait(Trait.Terrain);
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.UpdateFaceUpOnBoardEffects))]
        [HarmonyPostfix]
        private static void ShowExplosiveEffect(ref PlayableCard __instance)
        {
            if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(BOMB_CHALLENGE) && __instance.CardShouldExplode())
            {
                __instance.Anim.SetExplosive(!__instance.Dead);
            }
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.AssignCardToSlot))]
        [HarmonyPostfix]
        private static IEnumerator AttachExplosivesToCard(IEnumerator sequence, PlayableCard card)
        {
            if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(BOMB_CHALLENGE) && card.CardShouldExplode())
            {
                // Make sure the card has the explosive trigger receiver
                ExplodeOnDeath[] comps = card.gameObject.GetComponentsInChildren<ExplodeOnDeath>();
                if (comps == null || !comps.Any(c => c.GetType() == typeof(ExplodeOnDeath)))
                {
                    //CardTriggerHandler.AddReceiverToGameObject<AbilityBehaviour>(Ability.ExplodeOnDeath.ToString(), card.gameObject);
                    card.TriggerHandler.AddAbility(Ability.ExplodeOnDeath);
                }
            }
            yield return sequence;
        }

        [HarmonyPatch(typeof(TargetSlotItem), nameof(TargetSlotItem.ActivateSequence))]
        public static class HammerPatch
        {
            [HarmonyPrefix]
            private static void Prefix(ref TargetSlotItem __instance, ref TargetSlotItem __state)
            {
                __state = __instance;
            }

            [HarmonyPostfix]
            private static IEnumerator Postfix(IEnumerator sequence, TargetSlotItem __state)
            {
                if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(ENERGY_HAMMER) && __state is HammerItem && TurnManager.Instance.IsPlayerTurn)
                {
                    if (ResourcesManager.Instance.PlayerEnergy < HAMMER_ENERGY_COST)
                    {
                        ChallengeActivationUI.Instance.ShowActivation(ENERGY_HAMMER);
                        __state.PlayShakeAnimation();
                        yield return new WaitForSeconds(0.2f);
                        yield break;
                    }
                }

                yield return sequence;
            }
        }

        [HarmonyPatch(typeof(HammerItem), nameof(HammerItem.OnValidTargetSelected))]
        public static class HammerPatchPostSelect
        {
            [HarmonyPrefix]
            private static void Prefix(ref HammerItem __instance, ref HammerItem __state)
            {
                __state = __instance;
            }

            [HarmonyPostfix]
            private static IEnumerator SpendHammerEnergy(IEnumerator sequence, HammerItem __state)
            {
                if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(ENERGY_HAMMER) && TurnManager.Instance.IsPlayerTurn)
                {
                    if (ResourcesManager.Instance.PlayerEnergy < HAMMER_ENERGY_COST)
                    {
                        ChallengeActivationUI.Instance.ShowActivation(ENERGY_HAMMER);
                        __state.PlayShakeAnimation();
                        yield return new WaitForSeconds(0.2f);
                        yield break;
                    }

                    ChallengeActivationUI.Instance.ShowActivation(ENERGY_HAMMER);
                    yield return ResourcesManager.Instance.SpendEnergy(HAMMER_ENERGY_COST);
                }

                yield return sequence;
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        [HarmonyPostfix]
        private static IEnumerator RotateCards(IEnumerator sequence, bool playerUpkeep)
        {
            yield return sequence;

            if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(ALL_CONVEYOR) && TurnManager.Instance.opponent is not Part3BossOpponent)
            {
                if (TurnManager.Instance.TurnNumber > 0 && playerUpkeep)
                {
                    if (TurnManager.Instance.TurnNumber == 1)
                        ChallengeActivationUI.Instance.ShowActivation(ALL_CONVEYOR);

                    yield return BoardManager.Instance.MoveAllCardsClockwise();
                }
            }
        }

        [HarmonyPatch(typeof(BoardStateSimulator), nameof(BoardStateSimulator.SimulateCombatPhase))]
        [HarmonyPrefix]
        private static void MakeAIRecognizeRotation(BoardState board, bool playerIsAttacker)
        {
            if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(ALL_CONVEYOR) && playerIsAttacker && TurnManager.Instance.opponent is not Part3BossOpponent)
            {
                // We need to rotate the board
                var anchorCard = board.playerSlots[0].card;
                for (int i = 1; i < board.playerSlots.Count; i++)
                    board.playerSlots[i-1].card = board.playerSlots[i].card;
                board.playerSlots[board.playerSlots.Count - 1].card = board.opponentSlots[board.opponentSlots.Count - 1].card;
                for (int i = board.opponentSlots.Count - 1; i > 0; i--)
                    board.opponentSlots[i].card = board.opponentSlots[i - 1].card;
                board.opponentSlots[0].card = anchorCard;
            }
        }

        [HarmonyPatch(typeof(BoardStateEvaluator), nameof(BoardStateEvaluator.EvaluateCard))]
        [HarmonyPostfix]
        private static void MakeAIPreferLeftmostBountyHunters(BoardState.CardState card, BoardState board, ref int __result)
        {
            if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(ALL_CONVEYOR) && TurnManager.Instance.opponent is not Part3BossOpponent)
            {
                if (board.opponentSlots.Contains(card.slot))
                {
                    if (card.info.mods.Any(m => m.bountyHunterInfo != null))
                    {
                        int bestSlot = card.HasAbility(Ability.SplitStrike) ? 1 : 0;
                        __result -= Math.Abs(board.opponentSlots.IndexOf(card.slot) - bestSlot);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(BoardManager3D), nameof(BoardManager3D.ShowSlots))]
        [HarmonyPrefix]
        private static void SetRotationSlotTextures()
        {
            if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(ALL_CONVEYOR) && TurnManager.Instance.opponent is not Part3BossOpponent)
            {
                for (int i = 0; i < BoardManager.Instance.opponentSlots.Count - 1; i++)
                    BoardManager.Instance.opponentSlots[i].SetTexture(Resources.Load<Texture2D>("art/cards/card_slot_left"));

                BoardManager.Instance.opponentSlots[BoardManager.Instance.opponentSlots.Count - 1].SetTexture(TextureHelper.GetImageAsTexture("cadslot_up.png", typeof(AscensionChallengeManagement).Assembly));

                for (int i = 1; i < BoardManager.Instance.playerSlots.Count; i++)
                    BoardManager.Instance.playerSlots[i].SetTexture(Resources.Load<Texture2D>("art/cards/card_slot_left"));

                BoardManager.Instance.playerSlots[0].SetTexture(TextureHelper.GetImageAsTexture("cadslot_up.png", typeof(AscensionChallengeManagement).Assembly));
            }
        }
    }
}