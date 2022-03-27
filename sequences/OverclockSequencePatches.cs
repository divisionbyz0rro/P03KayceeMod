using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public static class OverclockSequencePatches
    {
        private static bool ChangeOverclockAbility = false;

        [HarmonyPatch(typeof(OverclockCardSequencer), nameof(OverclockCardSequencer.GetValidCards))]
        [HarmonyPostfix]
        public static void CannotDoubleSkeleclock(ref List<CardInfo> __result)
        {
            if (SaveFile.IsAscension)
            {
                __result.RemoveAll(ci => ci.HasAbility(NewPermaDeath.AbilityID));
            }
        }

        [HarmonyPatch(typeof(DeckInfo), nameof(DeckInfo.ModifyCard))]
        [HarmonyPrefix]
        public static void ReplaceOverclockAbility(CardModificationInfo mod)
        {
            if (ChangeOverclockAbility)
            {
                mod.abilities.Remove(Ability.PermaDeath);
                mod.abilities.Add(NewPermaDeath.AbilityID);
            }
        }

        [HarmonyPatch(typeof(OverclockCardSequencer), nameof(OverclockCardSequencer.OverclockCard))]
        [HarmonyPostfix]
        public static IEnumerator AddTutorialToSequence(IEnumerator sequence)
        {
            if (!SaveFile.IsAscension)
            {
                yield return sequence;
                yield break;
            }           

            bool hasShownTutorial = StoryEventsData.EventCompleted(EventManagement.OVERCLOCK_CHANGES);
            ChangeOverclockAbility = true;
            while (sequence.MoveNext())
            {
                if (!hasShownTutorial && sequence.Current is WaitForSeconds wfs && wfs.m_Seconds == 0.5f)
                {                
                    yield return TextDisplayer.Instance.PlayDialogueEvent("P03AscensionOverclock", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                    hasShownTutorial = true;
                    StoryEventsData.SetEventCompleted(EventManagement.OVERCLOCK_CHANGES);
                }
                yield return sequence.Current;
            }
            
            ChangeOverclockAbility = false;
            yield break;
        }

        [HarmonyPatch(typeof(OverclockCardSequencer), nameof(OverclockCardSequencer.OnAltSelectScreen))]
        [HarmonyPrefix]
        public static bool ShowAlternateRule()
        {
            if (ChangeOverclockAbility)
            {
                RuleBookController.Instance.OpenToAbilityPage(NewPermaDeath.AbilityID.ToString(), null, RuleBookController.Instance.Shown);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(OverclockCardSequencer), nameof(OverclockCardSequencer.OnCursorEnterScreen))]
        [HarmonyPrefix]
        public static bool ShowAlternateAbility(ref OverclockCardSequencer __instance)
        {
            if (ChangeOverclockAbility)
            {
                P03AddModFace component = P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.AddMod, false, true).GetComponent<P03AddModFace>();
                CardModificationInfo cardModificationInfo = new CardModificationInfo(NewPermaDeath.AbilityID);
                cardModificationInfo.attackAdjustment = 1;
                component.DisplayCardWithMod(__instance.selectionSlot.Card.Info, cardModificationInfo);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(P03AnimationController), nameof(P03AnimationController.SwitchToFace))]
        [HarmonyPostfix]
        public static void ShowAlternateAbilityFull(P03AnimationController.Face face, ref GameObject __result)
        {
            if (face == P03AnimationController.Face.OverclockIcon)
                if (ChangeOverclockAbility)
                    __result.GetComponent<P03AbilityFace>().SetAbility(NewPermaDeath.AbilityID);
        }
    }
}