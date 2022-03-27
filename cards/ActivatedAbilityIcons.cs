using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public static class ActivatedAbilityIcons
    {
        [HarmonyPatch(typeof(CardAbilityIcons), "SetColorOfDefaultIcons")]
        [HarmonyPostfix]
        public static void MoveAndRecolorActivatedAbilities(ref CardAbilityIcons __instance)
        {
            if (SaveManager.SaveFile.IsPart3)
            {
                List<GameObject> defaultIconGroups = __instance.defaultIconGroups;
                foreach (GameObject group in defaultIconGroups)
                {
                    if (group.activeSelf)
                    {
                        foreach (AbilityIconInteractable abilityIconInteractable in group.GetComponentsInChildren<AbilityIconInteractable>())
                        {
                            AbilityInfo info = AbilitiesUtil.GetInfo(abilityIconInteractable.Ability);
                            if (info.activated)
                            {
                                abilityIconInteractable.SetColor(Color.white);
                            }
                        }
                    }
                }
            }
        }
    }
}