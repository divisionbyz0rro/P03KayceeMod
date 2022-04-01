using HarmonyLib;
using DiskCardGame;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class StupidDecalFix
    {
        [HarmonyPatch(typeof(DiskScreenCardDisplayer), nameof(DiskScreenCardDisplayer.DisplayInfo))]
        [HarmonyPrefix]
        private static void AddThirdDecal(ref DiskScreenCardDisplayer __instance)
        {
            if (__instance.gameObject.transform.Find("Decal_Good") == null)
            {
                GameObject portrait = __instance.portraitRenderer.gameObject;
                GameObject decalFull = __instance.decalRenderers[1].gameObject;
                GameObject decalGood = GameObject.Instantiate(decalFull, decalFull.transform.parent);
                decalGood.name = "Decal_Good";
                decalGood.transform.localPosition = portrait.transform.localPosition;
                decalGood.transform.localScale = new(1.2f, 1f, 0f);
                __instance.decalRenderers.Add(decalGood.GetComponent<Renderer>());
            }
        }
    }
}