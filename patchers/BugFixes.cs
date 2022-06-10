using HarmonyLib;
using DiskCardGame;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    internal static class BugFixes
    {
        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.AddTemporaryMod))]
        [HarmonyPrefix]
        private static bool ProperlyRemoveSingletonTempMods(PlayableCard __instance, CardModificationInfo mod)
        {
            if (!string.IsNullOrEmpty(mod.singletonId))
            {
                CardModificationInfo cardModificationInfo = __instance.temporaryMods.Find(x => String.Equals(x.singletonId, mod.singletonId));
                if (cardModificationInfo != null)
                {
                    __instance.RemoveTemporaryMod(mod, true);
                }
            }
            __instance.temporaryMods.Add(mod);
            using (List<Ability>.Enumerator enumerator = mod.abilities.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Ability ability = enumerator.Current;
                    if (!__instance.temporaryMods.Exists((CardModificationInfo x) => x.negateAbilities.Contains(ability)))
                    {
                        __instance.TriggerHandler.AddAbility(ability);
                    }
                }
            }
            foreach (Ability ability2 in mod.negateAbilities)
            {
                __instance.TriggerHandler.RemoveAbility(ability2);
            }
            __instance.OnStatsChanged();
            return false;
        }

        [HarmonyPatch(typeof(Card), nameof(Card.SetInfo))]
        [HarmonyPrefix]
        private static void RemoveApperanceBehavioursBeforeSettingInfo(Card __instance, CardInfo info)
        {
            if (__instance.Info != null && !__instance.Info.name.Equals(info.name))
            {
                foreach (CardAppearanceBehaviour.Appearance appearance in __instance.Info.appearanceBehaviour)
                {
                    Type type = CustomType.GetType("DiskCardGame", appearance.ToString());
                    Component c = __instance.gameObject.GetComponent(type);
                    if (c != null)
                    {
                        GameObject.DestroyImmediate(c);
                    }
                }
                if (__instance.Anim is DiskCardAnimationController dcac)
                {
                    if (dcac.holoPortraitParent != null)
                    {
                        List<Transform> childrenToDelete = new();
                        foreach (Transform t in dcac.holoPortraitParent)
                            childrenToDelete.Add(t);

                        foreach (Transform t in childrenToDelete)
                            GameObject.DestroyImmediate(t.gameObject);

                        dcac.holoPortraitParent.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}