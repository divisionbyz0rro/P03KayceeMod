using HarmonyLib;
using DiskCardGame;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class FastTravelManagement
    {
        [HarmonyPatch(typeof(HoloMapWaypointNode), "OnEnable")]
        [HarmonyPostfix]
        public static void AlwaysDisableHintUI(ref HoloMapWaypointNode __instance)
        {
            if (SaveFile.IsAscension)
                Traverse.Create(__instance).Field("fastTravelHint").GetValue<GameObject>().SetActive(false);
        }

        private static readonly Dictionary<string, int> fastTravelNodes = new()
        {
            { "FastTravelMapNode_Wizard", RunBasedHoloMap.MAGIC },
            { "FastTravelMapNode_Undead", RunBasedHoloMap.UNDEAD },
            { "FastTravelMapNode_Nature", RunBasedHoloMap.NATURE },
            { "FastTravelMapNode_Tech", RunBasedHoloMap.TECH }
        };
        
        [HarmonyPatch(typeof(FastTravelNode), "OnCursorSelectEnd")]
        [HarmonyPrefix]
        public static bool FastTravelInAscensionMode(ref FastTravelNode __instance)
        {
            // In ascension mode, fast travel is different
            // We will NOT fast travel to the world owned by the fast travel node
            // Instead, we will dynamically create a world based on that node
            if (SaveFile.IsAscension)
            {
                EventManagement.AddVisitedZone(__instance.gameObject.name);

                Traverse nodeTraverse = Traverse.Create(__instance);
                P03Plugin.Log.LogInfo($"SetHoveringEffectsShown");
                nodeTraverse.Method("SetHoveringEffectsShown", new Type[] { typeof(bool) }).GetValue(false);
                P03Plugin.Log.LogInfo($"OnSelected");
                nodeTraverse.Method("OnSelected").GetValue();
                HoloGameMap.Instance.ToggleFastTravelActive(false, false);
                HoloMapAreaManager.Instance.CurrentArea.OnAreaActive();
                HoloMapAreaManager.Instance.CurrentArea.OnAreaEnabled();

                string worldId = RunBasedHoloMap.GetAscensionWorldID(fastTravelNodes[__instance.gameObject.name]);
                Tuple<int, int> pos = RunBasedHoloMap.GetStartingSpace(fastTravelNodes[__instance.gameObject.name]);
                Part3SaveData.WorldPosition worldPosition = new(worldId, pos.Item1, pos.Item2);

                HoloMapAreaManager.Instance.StartCoroutine(HoloMapAreaManager.Instance.DroneFlyToArea(worldPosition, false));
                Part3SaveData.Data.checkpointPos = worldPosition;

                EventManagement.NumberOfZoneEnemiesKilled = 0;

                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(FastTravelNode), nameof(FastTravelNode.OnFastTravelActive))]
        [HarmonyPostfix]
        public static void SetFastTravelNodeActive(ref FastTravelNode __instance)
        {
            if (SaveFile.IsAscension)
                __instance.gameObject.SetActive(fastTravelNodes.Keys.Contains(__instance.gameObject.name) && !EventManagement.CompletedZones.Contains(__instance.gameObject.name));
        }

        [HarmonyPatch(typeof(HoloMapWaypointNode), nameof(HoloMapWaypointNode.OnCursorSelectEnd))]
        [HarmonyPrefix]
        public static void SetFastTravelNodesVisible()
        {
            if (SaveFile.IsAscension)
            {
                foreach (Transform trans in HoloGameMap.Instance.fastTravelMap.gameObject.transform)
                {
                    bool active = trans.gameObject.name == "WireframeGeo" || (fastTravelNodes.Keys.Contains(trans.gameObject.name) && !EventManagement.CompletedZones.Contains(trans.gameObject.name));
                    trans.gameObject.SetActive(active);
                }
            }
        }

        private static bool isDroneFlying = false;

        [HarmonyPatch(typeof(HoloMapAreaManager), "DroneFlyToArea")]
        public static class ManageDroneFlying
        {
            [HarmonyPrefix]
            public static void SetDroneFlying()
            {
                P03Plugin.Log.LogInfo("Drone flying = true");
                FastTravelManagement.isDroneFlying = true;
            }

            [HarmonyPostfix]
            public static IEnumerator SetDroneNotFlying(IEnumerator sequence)
            {
                P03Plugin.Log.LogInfo("Drone flying = false");
                yield return sequence;
                FastTravelManagement.isDroneFlying = false;
            }
        }

        [HarmonyPatch(typeof(HoloGameMap), "UpdateColors")]
        [HarmonyPrefix]
        public static bool ManuallySetMapColorsIfDroneFlying(ref HoloGameMap __instance)
        {
            if (SaveFile.IsAscension && FastTravelManagement.isDroneFlying)
            {
                P03Plugin.Log.LogInfo("Setting map colors after drone flight");
                HoloMapArea currentArea = HoloMapAreaManager.Instance.CurrentArea;
                Traverse mapTrav = new Traverse(__instance);
                mapTrav.Method("SetSceneColors", new Type[] { typeof(Color), typeof(Color)}).GetValue(currentArea.MainColor, currentArea.LightColor);
                mapTrav.Method("SetSceneryColors", new Type[] { typeof(Color), typeof(Color)}).GetValue(GameColors.Instance.blue, GameColors.Instance.gold);
                mapTrav.Method("SetNodeColors", new Type[] { typeof(Color), typeof(Color)}).GetValue(GameColors.Instance.darkBlue, GameColors.Instance.brightBlue);
                return false;
            }
            return true;
        }

        public static IEnumerator ReturnToHomeBase() 
        {
            // We do our own special sequence when you complete a boss
            // ... we just play the drone and move you back to the hub world.
            
            yield return new WaitUntil(() => HoloMapAreaManager.Instance.CurrentArea != null);
            HoloGameMap.Instance.ToggleFastTravelActive(false, false);
            HoloMapAreaManager.Instance.CurrentArea.OnAreaActive();
            HoloMapAreaManager.Instance.CurrentArea.OnAreaEnabled();

            string worldId = RunBasedHoloMap.GetAscensionWorldID(RunBasedHoloMap.NEUTRAL);
            Tuple<int, int> pos = RunBasedHoloMap.GetStartingSpace(RunBasedHoloMap.NEUTRAL);
            Part3SaveData.WorldPosition worldPosition = new(worldId, pos.Item1, pos.Item2);

            HoloMapAreaManager.Instance.StartCoroutine(HoloMapAreaManager.Instance.DroneFlyToArea(worldPosition, false));
            Part3SaveData.Data.checkpointPos = worldPosition;

            yield return new WaitForSeconds(1.75f);
        }

        [HarmonyPatch(typeof(HoloGameMap), "ToggleFastTravelActive")]
        [HarmonyPrefix]
        public static void LogThisStupidError(ref HoloGameMap __instance)
        {
            Traverse mapTraverse = Traverse.Create(__instance);
            P03Plugin.Log.LogInfo($"Fast travel map {mapTraverse.Field("fastTravelMap").GetValue<HoloFastTravelMap>()}");
            P03Plugin.Log.LogInfo($"Current area {HoloMapAreaManager.Instance.CurrentArea}");
            P03Plugin.Log.LogInfo($"Current area gameobject {HoloMapAreaManager.Instance.CurrentArea.gameObject}");
            P03Plugin.Log.LogInfo($"Current area marker {HoloMapPlayerMarker.Instance}");
            P03Plugin.Log.LogInfo($"Current area marker gameobject {HoloMapPlayerMarker.Instance.gameObject}");
        }
    }
}