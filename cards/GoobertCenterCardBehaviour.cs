using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class GoobertCenterCardBehaviour : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility AbilityID { get; private set; }

        static GoobertCenterCardBehaviour()
        {
            AbilityID = SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "GoobertCenterCardAppearance", typeof(GoobertCenterCardBehaviour)).Id;
        }

        private bool IsInMycoBoss => TurnManager.Instance.Opponent is MycologistAscensionBossOpponent;

        // I'm cheating and doing things in the 'respondsto' section so I guarnatee the slots are right
        public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard)
        {
            ResolveOpposingSlotsForTripleCard();
            return false;
        }

        // I'm cheating and doing things in the 'respondsto' section so I guarnatee the slots are right
        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
        {
            ResolveOpposingSlotsForTripleCard();
            return false;
        }

        public override bool RespondsToResolveOnBoard()
        {
            return true;
        }

        private float GetGoobertEntrySpeed()
        {
            return IsInMycoBoss ? 3f : 1f;
        }

        private float GetArmEntrySpeed()
        {
            return IsInMycoBoss ? 2f : 0.7f;
        }

        private CardModificationInfo GetExperimentModInfo()
        {
            CardModificationInfo info = new ();
            int randomAbilityCount = 0;
            foreach(CardInfo card in EventManagement.MycologistTestSubjects)
            {
                info.healthAdjustment += card.Health;
                info.attackAdjustment += card.Attack;
                if (card.Gemified)
                {
                    info.healthAdjustment += 2;
                    info.attackAdjustment += 1;
                }
                foreach (Ability ab in card.Abilities)
                {
                    if (ab == Ability.RandomAbility)
                    {
                        randomAbilityCount += 1;
                    }
                    else if (ab == Ability.Transformer)
                    {
                        CardModificationInfo beastTransformer = card.mods.FirstOrDefault(m => !string.IsNullOrEmpty(m.transformerBeastCardId));
                        if (beastTransformer != null)
                        {
                            info.healthAdjustment -= card.Health;
                            info.attackAdjustment -= card.Attack;
                            CardInfo transformer = CustomCards.ConvertCodeToCard(beastTransformer.transformerBeastCardId);
                            info.healthAdjustment += transformer.Health;
                            info.attackAdjustment += transformer.Attack;
                            info.abilities.AddRange(transformer.abilities.Where(a => a != Ability.Transformer));
                        }
                    }
                    else
                    {
                        if (!info.abilities.Contains(ab) || AbilitiesUtil.GetInfo(ab).canStack)
                            info.abilities.Add(ab);
                    }
                }
            }
            for (int i = 0; i < randomAbilityCount; i++)
            {
                List<Ability> possibles = AbilitiesUtil.GetLearnedAbilities(false, 0, 5, SaveManager.SaveFile.IsPart1 ? AbilityMetaCategory.Part1Modular : AbilityMetaCategory.Part3Modular);
                possibles.RemoveAll(a => info.abilities.Contains(a));
                info.abilities.Add(possibles[SeededRandom.Range(0, possibles.Count, P03AscensionSaveData.RandomSeed + i)]);
            }
            return info;
        }

        public override IEnumerator OnResolveOnBoard()
        {
            DiskCardAnimationController dcac = this.Card.Anim as DiskCardAnimationController;

            this.PlayableCard.RenderInfo.hidePortrait = true;
            this.PlayableCard.SetInfo(this.PlayableCard.Info);

            if (!IsInMycoBoss)
                ViewManager.Instance.SwitchToView(View.Default, false, true);

            // Get the goobert face
            GameObject goobert = GameObject.Instantiate(Resources.Load<GameObject>(GoobertHuh.ItemData.prefabId), dcac.holoPortraitParent);
            ConsumableItem itemcontroller = GoobertHuh.FixGameObject(goobert);
            GameObject.Destroy(itemcontroller);
            GameObject.Destroy(goobert.GetComponentInChildren<GooWizardAnimationController>());
            GameObject.Destroy(goobert.GetComponentInChildren<Animator>());
            goobert.transform.Find("GooWizardBottle/GooWizard/Bottle").gameObject.SetActive(false);
            goobert.transform.Find("GooWizardBottle/GooWizard/Cork").gameObject.SetActive(false);
            OnboardDynamicHoloPortrait.HolofyGameObject(goobert, GameColors.Instance.brightLimeGreen);

            Transform gooWizard = goobert.transform.Find("GooWizardBottle/GooWizard");
            gooWizard.localEulerAngles = new(90f, 0f, 0f);

            Vector3 target = new (-.1f, -.7f, .6f);
            goobert.transform.localPosition = target + Vector3.down;
            goobert.transform.localEulerAngles = new (46.2497f, 121.8733f, 222.0276f);
            Tween.LocalPosition(goobert.transform, target, GetGoobertEntrySpeed(), 0f);

            if (IsInMycoBoss)
                base.StartCoroutine(GooSpeakBackground());

            //this.Card.RenderInfo.hidePortrait = true;
            this.Card.SetInfo(this.Card.Info);
            this.Card.RenderCard();
            yield return new WaitForSeconds(GetGoobertEntrySpeed());

            if (IsInMycoBoss)
            {
                ViewManager.Instance.SwitchToView(View.Default, false, true);
                yield return TextDisplayer.Instance.PlayDialogueEvent("GooWTF2", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            }

            // Make room for the left and right halves of the card
            List<CardSlot> friendlySlots = BoardManager.Instance.GetSlots(!this.PlayableCard.OpponentCard);
            int mySlot = this.PlayableCard.Slot.Index;
            
            int leftSlot = mySlot - 1;
            yield return MakeSlotEmpty(friendlySlots[leftSlot], true);
            int rightSlot = mySlot + 1;
            yield return MakeSlotEmpty(friendlySlots[rightSlot], false);

            // Get the arm
            GameObject rightArm = GameObject.Instantiate(Resources.Load<GameObject>("prefabs/map/holomapscenery/HoloClaw"), dcac.holoPortraitParent);
            GameObject leftArm = GameObject.Instantiate(Resources.Load<GameObject>("prefabs/map/holomapscenery/HoloClaw"), dcac.holoPortraitParent);
            OnboardDynamicHoloPortrait.HolofyGameObject(rightArm, GameColors.Instance.brightLimeGreen);
            OnboardDynamicHoloPortrait.HolofyGameObject(leftArm, GameColors.Instance.brightLimeGreen);
            rightArm.transform.localPosition = new (-0.5f, -1f, 0f);
            leftArm.transform.localPosition = new (0.5f, -1f, 0f);
            rightArm.transform.localEulerAngles = new (0f, 0f, 270f);
            leftArm.transform.localEulerAngles = new (0f, 180f, 270f);
            Tween.LocalPosition(rightArm.transform, new Vector3(-0.5f, 0f, 0f), GetArmEntrySpeed(), 0f);
            Tween.LocalPosition(leftArm.transform, new Vector3(0.5f, 0f, 0f), GetArmEntrySpeed(), 0f);
            yield return new WaitForSeconds(GetArmEntrySpeed());

            SwapAnimationController(this.PlayableCard, true);

            // Rotate the arm into place
            Tween.LocalPosition(leftArm.transform, new Vector3(1.14f, -0.08f, 0f), 0.3f, 0f);
            Tween.LocalRotation(leftArm.transform, new Vector3(0f, 180f, 34f), 0.3f, 0f);
            Tween.LocalPosition(rightArm.transform, new Vector3(-1.14f, -0.08f, 0f), 0.3f, 0f);
            Tween.LocalRotation(rightArm.transform, new Vector3(0f, 0f, 34f), 0.3f, 0f);

            // Scale the cards
            Tween.LocalScale(this.gameObject.transform.Find("Anim/CardBase"), new Vector3(0.5263f, 2.5f, 1f), 0.3f, 0f);
            Tween.LocalScale(this.gameObject.transform.Find("Anim/ShieldEffect"), new Vector3(6f, 7.9f, 1.7f), 0.3f, 0f);
            Tween.LocalScale(this.gameObject.transform.Find("Anim/CardBase/HoloportraitParent"), new Vector3(.8f / 2.5f, 1f, 1f), 0.3f, 0f);
            Tween.LocalScale(this.gameObject.transform.Find("Anim/CardBase/Top/Name"), new Vector3(0.5f, -1f, 1f), 0.3f, 0f);

            this.PlayableCard.temporaryMods.Add(GetExperimentModInfo());
            this.PlayableCard.SetInfo(this.PlayableCard.Info);

            AudioController.Instance.PlaySound3D("mushroom_large_appear", MixerGroup.TableObjectsSFX, this.gameObject.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);

            yield return new WaitForSeconds(0.3f);

            AddMushroom(-1.78f, -.26f, dcac.holoPortraitParent);
            AddMushroom(-.97f, .2436f, dcac.holoPortraitParent);
            AddMushroom(1.7f, .4f, dcac.holoPortraitParent);
            AddMushroom(.8f, -.12f, dcac.holoPortraitParent);

            yield return new WaitForSeconds(1.5f);

            ResolveOpposingSlotsForTripleCard();

            if (!IsInMycoBoss)
                ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;

            yield break;
        }

        private void AddMushroom(float x, float z, Transform parent)
        {
            GameObject mushroom = GameObject.Instantiate(Resources.Load<GameObject>("prefabs/map/holomapscenery/HoloMushroom_1"), parent);
            OnboardDynamicHoloPortrait.HolofyGameObject(mushroom, GameColors.Instance.brightLimeGreen);
            Vector3 target = new (x, -.7f, z);
            mushroom.transform.localPosition = target + Vector3.down;
            Tween.LocalPosition(mushroom.transform, target, 1f, 0f);
        }

        private static void SwapAnimationController(PlayableCard card, bool isMain = false)
        {
            GameObject obj = card.gameObject;
            DiskCardAnimationController thisOldController = obj.GetComponent<DiskCardAnimationController>();
            var newThisAnim = obj.AddComponent<TripleDiskCardAnimationController>();
            newThisAnim.InitializeWith(thisOldController);
            GameObject.Destroy(thisOldController);

            // Update the dynamic stretchers in the live render cameras

            DiskScreenCardDisplayer displayer = CardRenderCamera.Instance.GetLiveRenderCamera(card.StatsLayer).GetComponentInChildren<DiskScreenCardDisplayer>();
            Transform abilityParent = displayer.gameObject.transform.Find("CardAbilityIcons_Part3");
            UpdateAllStretchers(abilityParent, card);

            Transform liveAbilityParent = card.gameObject.transform.Find("Anim/CardBase/Bottom/CardAbilityIcons_Part3_Invisible");
            UpdateAllStretchers(liveAbilityParent, card);            
        }

        private static void UpdateAllStretchers(Transform abilityParent, PlayableCard card)
        {
            for (int i = 1; i <= 12; i++)
            {
                string name = i == 1 ? "DefaultIcons_1Ability" : $"DefaultIcons_{i}Abilities";
                GameObject container = abilityParent.Find(name).gameObject;
                TripleCardRenderIcons.InverseStretch stretcher = container.GetComponent<TripleCardRenderIcons.InverseStretch>();
                if (stretcher != null)
                    stretcher.MatchingTransform = card.gameObject.transform.Find("Anim/CardBase");
            }
        }

        private IEnumerator MakeSlotEmpty(CardSlot slot, bool left=true)
        {
            if (slot.Card == null)
                yield break;

            List<CardSlot> friendlySlots = BoardManager.Instance.GetSlots(!slot.Card.OpponentCard);
            int index = slot.Index;
            int nextIndex = left ? index - 1 : index + 1;

            if (index < 0 || index >= friendlySlots.Count)
            {
                slot.Card.ExitBoard(0.1f, Vector3.down);
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            yield return MakeSlotEmpty(friendlySlots[nextIndex], left);

            yield return BoardManager.Instance.AssignCardToSlot(slot.Card, friendlySlots[nextIndex]);
        }

        private IEnumerator GooSpeakBackground()
        {
            yield return TextDisplayer.Instance.PlayDialogueEvent("GooWTF", TextDisplayer.MessageAdvanceMode.Auto, TextDisplayer.EventIntersectMode.Wait, null, null);
        }

        [HarmonyPatch(typeof(RenderStatsLayer), nameof(RenderStatsLayer.RenderCard))]
        [HarmonyPrefix]
        private static bool LiveRenderGooCard(CardRenderInfo info, ref RenderStatsLayer __instance)
        {
            if (__instance is DiskRenderStatsLayer drsl && info.baseInfo.specialAbilities.Contains(GoobertCenterCardBehaviour.AbilityID))
            {
                CardRenderCamera.Instance.LiveRenderCard(info, drsl, drsl.PlayableCard);
                return false;
            }
            return true;
        }

        private static bool IsInsideCombatPhase = false;

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.GetAdjacentSlots))]
        [HarmonyPrefix]
        private static bool GetAdjacentAccountingForTripleCard(CardSlot slot, ref List<CardSlot> __result)
        {
            if (!SaveManager.SaveFile.IsPart3)
                return true;

            if (IsInsideCombatPhase)
                return true;

            if (TurnManager.Instance != null && TurnManager.Instance.GameIsOver())
                return true;

            List<CardSlot> slotsToCheck = BoardManager.Instance.GetSlots(slot.IsPlayerSlot);
			
            if (slotsToCheck.Any(s => s.SlotHasTripleCard()))
            {
                __result = new();
                int sidx = slotsToCheck.IndexOf(slot);
                if (slot.SlotHasTripleCard())
                {
                    // The slots adjacent to a triple card are two in either direction
                    if (sidx >= 2)
                        __result.Add(slotsToCheck[sidx - 2]);
                    if (sidx + 2 < slotsToCheck.Count)
                        __result.Add(slotsToCheck[sidx + 2]);

                    return false;
                }
                
                // Okay - what if someone is asking about the "wasted" slots underneath the triple card?
                // Remember - the triple card is considered to be in the middle of its space
                // In this case, we return the answer as if you asked for that card
                if (sidx > 0 && slotsToCheck[sidx - 1].SlotHasTripleCard())
                {
                    __result = BoardManager.Instance.GetAdjacentSlots(slotsToCheck[sidx - 1]);
                    return false;
                }

                if (sidx + 1 < slotsToCheck.Count && slotsToCheck[sidx + 1].SlotHasTripleCard())
                {
                    __result = BoardManager.Instance.GetAdjacentSlots(slotsToCheck[sidx + 1]);
                    return false;
                }

                // Okay, so at this point we're just asking for a normal card
                // Check the right side
                if (sidx + 1 < slotsToCheck.Count)
                {
                    if (slotsToCheck[sidx + 1].Card != null) // If there's a card there, there's not a triple card adjacent
                    {
                        __result.Add(slotsToCheck[sidx + 1]);
                    }
                    else
                    {
                        // okay, there's not a card there - what if there's a triple card one more slot over:
                        if (sidx + 2 < slotsToCheck.Count)
                        {
                            if (slotsToCheck[sidx + 2].SlotHasTripleCard())
                                __result.Add(slotsToCheck[sidx + 2]);
                            else
                                __result.Add(slotsToCheck[sidx + 1]);
                        }
                        else
                        {
                            // Okay, there's not room for a triple card so:
                            __result.Add(slotsToCheck[sidx + 1]);
                        }
                    }
                }  

                // Check the left side:
                if (sidx > 0)
                {
                    if (slotsToCheck[sidx - 1].Card != null) // If there's a card there, there's not a triple card adjacent
                    {
                        __result.Add(slotsToCheck[sidx - 1]);
                    }
                    else
                    {;
                        // okay, there's not a card there - what if there's a triple card one more slot over:
                        if (sidx > 1)
                        {
                            if (slotsToCheck[sidx - 2].SlotHasTripleCard())
                                __result.Add(slotsToCheck[sidx - 2]);
                            else
                                __result.Add(slotsToCheck[sidx - 1]);
                        }
                        else
                        {
                            // Okay, there's not room for a triple card so:
                            __result.Add(slotsToCheck[sidx - 1]);
                        }
                    }
                }    
                
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.AllSlots), MethodType.Getter)]
        [HarmonyPostfix]
        private static void FilterOutCoveredSlots(ref List<CardSlot> __result)
        {
            if (!SaveManager.SaveFile.IsPart3)
                return;

            // This is a bit of a hacky coverup to deal with the fact that when you ask the table manager
            // to reset colors when the boss dies, sometimes you do this while there are still cards on the
            // table. So if the game is essentially over, we can just leave the list of slots alone

            if (TurnManager.Instance != null && TurnManager.Instance.GameIsOver())
                return;             

            __result.RemoveAll(cs => cs.SlotCoveredByTripleCard());
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.SacrificesCreateRoomForCard))]
        [HarmonyPrefix]
        private static bool EnsureRoomForTripleCard(PlayableCard card, ref bool __result)
        {
            if (!SaveManager.SaveFile.IsPart3)
                return true;

            if (card.Info.specialAbilities.Contains(GoobertCenterCardBehaviour.AbilityID))
            {
                int emptiesInARow = 0;
                for (int i = 0; i < BoardManager.Instance.playerSlots.Count; i++)
                {
                    if (BoardManager.Instance.playerSlots[i].Card == null)
                    {
                        emptiesInARow += 1;
                        if (emptiesInARow == 3)
                        {
                            __result = true;
                            return false;
                        }
                    }
                    else
                    {
                        emptiesInARow = 0;
                    }
                }
                __result = false;
                return false;
            }
            return true;
        }

        internal static void ResolveOpposingSlotsForTripleCard(List<CardSlot> slots, List<CardSlot> opposingSlots, bool reset)
        {
            if (opposingSlots.Any(s => s.SlotHasTripleCard()) && !reset)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (i > 0 && opposingSlots[i - 1].SlotHasTripleCard())
                        slots[i].opposingSlot = opposingSlots[i - 1];
                    else if (i + 1 < opposingSlots.Count && opposingSlots[i + 1].SlotHasTripleCard())
                        slots[i].opposingSlot = opposingSlots[i + 1];
                    else
                        slots[i].opposingSlot = opposingSlots[i];
                }
            }
            else
            {
                for (int i = 0; i < slots.Count; i++)
                    slots[i].opposingSlot = opposingSlots[i];
            }
        }

        internal static void DropHighlightedConduitsForTripleCard()
        {
            foreach (CardSlot slot in BoardManager.Instance.playerSlots.Concat(BoardManager.Instance.opponentSlots))
                slot.gameObject.transform.Find("ConduitBorder/GravityParticles").gameObject.SetActive(!slot.SlotHasTripleCard() && !slot.SlotCoveredByTripleCard());
        }

        internal static void ResolveOpposingSlotsForTripleCard(bool reset = false)
        {
            if (!SaveManager.SaveFile.IsPart3)
                return;

            ResolveOpposingSlotsForTripleCard(BoardManager.Instance.playerSlots, BoardManager.Instance.opponentSlots, reset);
            ResolveOpposingSlotsForTripleCard(BoardManager.Instance.opponentSlots, BoardManager.Instance.playerSlots, reset);
            DropHighlightedConduitsForTripleCard();
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        [HarmonyPrefix]
        private static void EnsureSlotsOnUpkeep()
        {
            if (!SaveManager.SaveFile.IsPart3)
                return;

            ResolveOpposingSlotsForTripleCard();
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoCombatPhase))]
        [HarmonyPostfix]
        private static IEnumerator EnsureSlotsOnCombat(IEnumerator sequence)
        {
            IsInsideCombatPhase = true;
            ResolveOpposingSlotsForTripleCard(reset:true);
            yield return sequence;
            IsInsideCombatPhase = false;
            ResolveOpposingSlotsForTripleCard(reset:false);
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPrefix]
        private static void EnsureSlotsOnCleanup()
        {
            if (!SaveManager.SaveFile.IsPart3)
                return;

            ResolveOpposingSlotsForTripleCard(reset:true);
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.AssignCardToSlot))]
        [HarmonyPostfix]
        private static IEnumerator HackyTripleCardReassignmentStrategy(IEnumerator sequence, PlayableCard card, CardSlot slot, float transitionDuration = 0.1f, Action tweenCompleteCallback = null, bool resolveTriggers = true)
        {
            if (SaveManager.SaveFile.IsPart3)
            {
                // If this is a triple card we might actually change which slot its being assigned to:
                if (card.Info.specialAbilities.Contains(GoobertCenterCardBehaviour.AbilityID))
                {
                    // First, let's see if we can fit in the new space:
                    List<CardSlot> container = BoardManager.Instance.GetSlots(slot.IsPlayerSlot);

                    if (slot.SlotCanHoldTripleCard(card))
                    {
                        yield return sequence;
                        yield break;
                    }

                    // Okay. We don't fit. Is there any place we can fit that will cover this slot?
                    for (int i = 1; i < container.Count - 1; i++)
                    {
                        if (container[i].SlotCanHoldTripleCard(card) && Math.Abs(i - slot.Index) <= 1)
                        {
                            yield return BoardManager.Instance.AssignCardToSlot(card, container[i], transitionDuration, tweenCompleteCallback, resolveTriggers);
                            yield break;
                        }
                    }

                    // We could not find a way to assign the card's location!
                    // We will move right back to the slot we were in before!
                    if (card.Slot != null)
                        yield return BoardManager.Instance.AssignCardToSlot(card, card.Slot, transitionDuration, tweenCompleteCallback, resolveTriggers);

                    yield break;
                }   

                // If you're trying to move into a slot covered by a triple card, let's see if we can't move you somewhere else
                if (slot.SlotCoveredByTripleCard() && card.Slot != null)
                {
                    // Okay, you're trying to move into a slot that's covered by a triple card. This probably
                    // happened because of some strafe shenanigans. Let's just choose a different place for you to move to
                    List<CardSlot> container = BoardManager.Instance.playerSlots;
                    if (!container.Contains(slot))
                        container = BoardManager.Instance.opponentSlots;

                    int step = slot.Index < card.Slot.Index ? -1 : 1;

                    int startIndex = card.Slot.Index + step;
                    while (startIndex > 0 && startIndex < container.Count)
                    {
                        CardSlot newSlot = container[startIndex];
                        if (newSlot.Card != null && !newSlot.SlotCoveredByTripleCard())
                        {
                            yield return BoardManager.Instance.AssignCardToSlot(card, newSlot, transitionDuration, tweenCompleteCallback, resolveTriggers);
                            yield break;
                        }
                        startIndex += step;
                    }
                    yield break;
                    
                }
            }

            yield return sequence;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetOpposingSlots))]
        [HarmonyPostfix]
        [HarmonyAfter("cyantist.inscryption.api")]
        private static void ReplaceAttacksOnOverlap(PlayableCard __instance, ref List<CardSlot> __result)
        {
            if (!SaveManager.SaveFile.IsPart3)
                return;

            // If you attack a slot we overlap, you should attack us
            List<CardSlot> slots = BoardManager.Instance.GetSlots(__instance.OpponentCard);
            CardSlot tripleSlotCard = slots.FirstOrDefault(s => s.SlotHasTripleCard());
            if (tripleSlotCard != null)
            {
                for (int i = 0; i < __result.Count; i++)
                {
                    if (__result[i].SlotCoveredByTripleCard())
                        __result[i] = tripleSlotCard;
                }
            }
        }

        [HarmonyPatch(typeof(Opponent), nameof(Opponent.QueuedCardIsBlocked))]
        [HarmonyPrefix]
        private static bool QueuedCardIsBlocked(PlayableCard queuedCard, ref bool __result)
        {
            if (!SaveManager.SaveFile.IsPart3)
                return true;

            // You can't put a queued card into a slot we overlap
            __result = queuedCard.QueuedSlot.Card != null || queuedCard.QueuedSlot.SlotCoveredByTripleCard();
            return false;
        }

        private static bool InVirtualSlot = false;

        [HarmonyPatch(typeof(CardTriggerHandler), nameof(CardTriggerHandler.RespondsToTrigger))]
        [HarmonyPostfix]
        private static void WouldHaveRespondedInAnotherSlot(CardTriggerHandler __instance, ref bool __result, Trigger trigger, object[] otherArgs)
        {
            if (!SaveManager.SaveFile.IsPart3)
                return;

            if (InVirtualSlot)
                return;

            if (__result)
                return;

            PlayableCard card = __instance.gameObject.GetComponent<PlayableCard>();

            if (card.Info.specialAbilities.Contains(GoobertCenterCardBehaviour.AbilityID))
            {
                if (card.Slot != null && !TurnManager.Instance.GameEnding)
                {
                    // Let's see if we would have responded in a different slot
                    List<CardSlot> overrideSlots = BoardManager.Instance.GetSlots(card.Slot.IsPlayerSlot);
                    CardSlot leftSlot = overrideSlots[card.Slot.Index - 1];
                    CardSlot rightSlot = overrideSlots[card.Slot.Index + 1];
                    CardSlot originalSlot = card.Slot;

                    InVirtualSlot = true;

                    try
                    {
                        P03Plugin.Log.LogDebug($"Triple card did not trigger in slot {originalSlot.Index} for trigger {trigger}");

                        card.slot = leftSlot;
                        if (__instance.RespondsToTrigger(trigger, otherArgs))
                        {
                            P03Plugin.Log.LogDebug($"Triple card does trigger in slot {leftSlot.Index} for trigger {trigger}");
                            __result = true;
                            return;
                        }
                        card.slot = rightSlot;
                        if (__instance.RespondsToTrigger(trigger, otherArgs))
                        {
                            P03Plugin.Log.LogDebug($"Triple card does trigger in slot {rightSlot.Index} for trigger {trigger}");
                            __result = true;
                            return;
                        }
                        P03Plugin.Log.LogDebug($"Triple card does not trigger in slot {leftSlot.Index} or {rightSlot.Index} for trigger {trigger}");
                        return;
                    }
                    finally
                    {
                        card.slot = originalSlot;
                        InVirtualSlot = false;
                    }
                }
            }
        }

        private static bool ReceiverRespondsToTriggerInAnyOfThreeSlots(PlayableCard card, Trigger trigger, TriggerReceiver receiver, object[] otherArgs)
        {
            if (GlobalTriggerHandler.ReceiverRespondsToTrigger(trigger, receiver, otherArgs))
                return true;

            List<CardSlot> overrideSlots = BoardManager.Instance.GetSlots(card.Slot.IsPlayerSlot);
            CardSlot leftSlot = overrideSlots[card.Slot.Index - 1];
            CardSlot rightSlot = overrideSlots[card.Slot.Index + 1];
            CardSlot originalSlot = card.Slot;

            try
            {
                card.slot = leftSlot;
                if (GlobalTriggerHandler.ReceiverRespondsToTrigger(trigger, receiver, otherArgs))
                    return true;

                card.slot = rightSlot;
                if (GlobalTriggerHandler.ReceiverRespondsToTrigger(trigger, receiver, otherArgs))
                    return true;
            }
            finally
            {
                card.slot = originalSlot;
            }

            return false;
        }

        [HarmonyPatch(typeof(CardTriggerHandler), nameof(CardTriggerHandler.OnTrigger))]
        [HarmonyPostfix]
        private static IEnumerator HandleTripleCardTrigger(IEnumerator sequence, CardTriggerHandler __instance, Trigger trigger, object[] otherArgs)
        {
            if (!SaveManager.SaveFile.IsPart3)
            {
                yield return sequence;
                yield break;
            }
                
            PlayableCard card = __instance.gameObject.GetComponent<PlayableCard>();
            if (!card.Info.specialAbilities.Contains(GoobertCenterCardBehaviour.AbilityID))
            {
                yield return sequence;
                yield break;
            }

            foreach (TriggerReceiver receiver in __instance.GetAllReceivers())
			{
                if (ReceiverRespondsToTriggerInAnyOfThreeSlots(card, trigger, receiver, otherArgs))
                	yield return GlobalTriggerHandler.Instance.TriggerSequence(trigger, receiver, otherArgs);
			}
			
            yield break;
        }
    }
}