using HarmonyLib;
using DiskCardGame;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Infiniscryption.P03KayceeRun.Sequences;
using Infiniscryption.P03KayceeRun.Helpers;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static partial class RunBasedHoloMap
    {
        // The holographic map is absolutely bonkers
        // Each screen that you see on the map is called an 'area'
        // Think of it like a region on the paper game map.
        // Each area has nodes on it. Those nodes do things.
        // What's crazy is that the arrows on the edge of the map that you think of as just UI elements,
        // those are actually map nodes. The arrow itself contains all the data of the encounter and behavior.
        // The encounter data, etc, is not stored in the map area - it's stored on the arrow of the adjacent map area.

        private static readonly Dictionary<string, HoloMapWorldData> worldDataCache = new Dictionary<string, HoloMapWorldData>();

        private static GameObject defaultPrefab;
        private static GameObject neutralHoloPrefab;

        private static Dictionary<Opponent.Type, GameObject> BossPrefabs = new();
        public static Dictionary<HoloMapNode.NodeDataType, GameObject> SpecialNodePrefabs = new();
        private static Dictionary<int, GameObject[]> SpecialTerrainPrefabs = new();
        private static Dictionary<int, GameObject> ArrowPrefabs = new();
        
        public const int NEUTRAL = 0;
        public const int TECH = 1;
        public const int UNDEAD = 2;
        public const int NATURE = 3;
        public const int MAGIC = 4;

        private static readonly Dictionary<int, RegionGeneratorData> REGION_DATA = new();

        private static Dictionary<string, GameObject> objectLookups = new();

        private static GameObject HOLO_NODE_BASE;
        private static GameObject HOVER_HOLO_NODE_BASE;
        private static GameObject BLOCK_ICON;

        public static readonly int EMPTY = -1;
        public static readonly int BLANK = 0;
        public static readonly int NORTH = 1;
        public static readonly int EAST = 2;
        public static readonly int SOUTH = 4;
        public static readonly int WEST = 8;
        public static readonly int ENEMY = 16;
        public static readonly int COUNTDOWN = 32;
        public static readonly int ALL_DIRECTIONS = NORTH | EAST | SOUTH | WEST;
        private static readonly Dictionary<int, string> DIR_LOOKUP = new() {{SOUTH, "S"}, {WEST, "W"}, {NORTH, "N"}, {EAST, "E"}};
        private static readonly Dictionary<int, LookDirection> LOOK_MAPPER = new() {{SOUTH, LookDirection.South}, {NORTH, LookDirection.North}, {EAST, LookDirection.East}, {WEST, LookDirection.West}};
        private static readonly Dictionary<string, LookDirection> LOOK_NAME_MAPPER = new () 
        {
            {"MoveArea_S", LookDirection.South}, {"MoveArea_N", LookDirection.North}, {"MoveArea_E", LookDirection.East}, {"MoveArea_W", LookDirection.West},
            {"MoveArea_W (NORTH)", LookDirection.North}, {"MoveArea_W (SOUTH)", LookDirection.South}, {"MoveArea_E (NORTH)", LookDirection.North}, {"MoveArea_E (SOUTH)", LookDirection.South}
        };
        
        private static IEnumerable<int> GetDirections(int compound, bool inclusive=true)
        {
            if (inclusive)
            {
                if ((compound & NORTH) != 0) yield return NORTH;
                if ((compound & EAST) != 0) yield return EAST;
                if ((compound & SOUTH) != 0) yield return SOUTH;
                if ((compound & WEST) != 0) yield return WEST;
                yield break;
            }

            yield return NORTH | WEST;
            yield return NORTH | EAST;
            yield return SOUTH | WEST;
            yield return SOUTH | EAST;
            if ((compound & NORTH) == 0) yield return NORTH;
            if ((compound & EAST) == 0) yield return EAST;
            if ((compound & SOUTH) == 0) yield return SOUTH;
            if ((compound & WEST) == 0) yield return WEST;
        }

        private static GameObject GetGameObject(string singleMapKey)
        {
            if (singleMapKey == default(string))
                return null;
            string holoMapKey = singleMapKey.Split('/')[0];
            string findPath = singleMapKey.Replace($"{holoMapKey}/", "");
            return GetGameObject(holoMapKey, findPath);
        }

        private static GameObject[] GetGameObject(string[] multiMapKey)
        {
            return multiMapKey.Select(s => GetGameObject(s)).ToArray();
        }

        private static GameObject GetGameObject(string holomap, string findPath)
        {
            string key = $"{holomap}/{findPath}";
            if (objectLookups.ContainsKey(key))
            {
                GameObject dictval = objectLookups[key];
                if (dictval == null)
                    objectLookups.Remove(key);
                else
                    return objectLookups[key];
            }

            P03Plugin.Log.LogInfo($"Getting {holomap} / {findPath} ");
            GameObject resource = Resources.Load<GameObject>($"prefabs/map/holomapareas/HoloMapArea_{holomap}");
            GameObject retval = resource.transform.Find(findPath).gameObject;

            objectLookups.Add(key, retval);

            return retval;
        }

        public static void AddReplace<K, V>(this Dictionary<K, V> dict, K key, Func<V> getValue)
        {
            // I want to verify that these game objects are still alive
            // If they're not, I want to recreate them
            // But I don't want to create them unless I need to
            // So this helper takes a Func that creates them to delay building them until it's necessary

            if (dict.ContainsKey(key))
            {
                V oldValue = dict[key];
                if (oldValue != null)
                {
                    P03Plugin.Log.LogInfo($"I already have a {key.ToString()}");
                    return;
                }
                
                dict.Remove(key);
            }

            P03Plugin.Log.LogInfo($"I need to create a {key.ToString()}");
            dict.Add(key, getValue());
        }

        private static void Initialize()
        {
            P03Plugin.Log.LogInfo("Initializing world data");

            REGION_DATA.Clear(); // All of the actual region data is in the region data class itself
            for (int i = 0; i < 5; i++)
                REGION_DATA.Add(i, new(i));

            HOLO_NODE_BASE = HOLO_NODE_BASE ?? GetGameObject("StartingIslandJunction", "Scenery/HoloNodeBase");
            HOVER_HOLO_NODE_BASE = HOVER_HOLO_NODE_BASE ?? GetGameObject("Shop", "Scenery/HoloDrone_HoldingPlatform_Undead");
            BLOCK_ICON = BLOCK_ICON ?? GetGameObject("UndeadShortcut_Exit", "HoloStopIcon");

            defaultPrefab = Resources.Load<GameObject>("prefabs/map/holomapareas/holomaparea");
            P03Plugin.Log.LogInfo($"Default prefab is {defaultPrefab}");

            // Boss prefabs
            BossPrefabs.AddReplace(Opponent.Type.ArchivistBoss, () => Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleUndeadBoss"));
            BossPrefabs.AddReplace(Opponent.Type.PhotographerBoss, () => Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleNatureBoss"));
            BossPrefabs.AddReplace(Opponent.Type.TelegrapherBoss, () => Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleTech_1"));
            BossPrefabs.AddReplace(Opponent.Type.CanvasBoss, () => Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleWizardBoss"));

            // Special node prefabs
            SpecialNodePrefabs.AddReplace(HoloMapSpecialNode.NodeDataType.CardChoice, () => GetGameObject("StartingIslandJunction", "Nodes/CardChoiceNode3D"));
            SpecialNodePrefabs.AddReplace(HoloMapSpecialNode.NodeDataType.AddCardAbility, () => GetGameObject("Shop", "Nodes/ShopNode3D_AddAbility"));
            SpecialNodePrefabs.AddReplace(HoloMapSpecialNode.NodeDataType.OverclockCard, () => GetGameObject("Shop", "Nodes/ShopNode3D_Overclock"));
            SpecialNodePrefabs.AddReplace(HoloMapSpecialNode.NodeDataType.CreateTransformer, () => GetTransformerNode());
            SpecialNodePrefabs.AddReplace(HoloMapSpecialNode.NodeDataType.AttachGem, () => GetGameObject("Shop", "Nodes/ShopNode3D_AttachGem"));
            SpecialNodePrefabs.AddReplace(HoloMapSpecialNode.NodeDataType.RecycleCard, () => GetGameObject("NeutralWestMain_1", "Nodes/RecycleCardNode3D"));
            SpecialNodePrefabs.AddReplace(HoloMapSpecialNode.NodeDataType.BuildACard, () => GetGameObject("Shop", "Nodes/ShopNode3D_BuildACard"));
            SpecialNodePrefabs.AddReplace(HoloMapSpecialNode.NodeDataType.GainCurrency, () => GetGameObject("NatureMainPath_3", "Nodes/CurrencyGainNode3D"));
            SpecialNodePrefabs.AddReplace(HoloMapSpecialNode.NodeDataType.ModifySideDeckConduit, () => GetGameObject("TechEntrance", "Nodes/ModifySideDeckNode3D"));
            SpecialNodePrefabs.AddReplace(TradeChipsNodeData.TradeChipsForCards, () => GetDraftNode());
            SpecialNodePrefabs.AddReplace(UnlockAscensionItemNodeData.UnlockItemsAscension, () => GetItemNode());
            SpecialNodePrefabs.AddReplace(AscensionRecycleCardNodeData.AscensionRecycleCard, () => GetRecycleNode());
            SpecialNodePrefabs.AddReplace(HoloMapSpecialNode.NodeDataType.BossBattle, () => GetGameObject("TempleWizardBoss", "Nodes/BossNode3D"));

            // Special terrain prefabs
            SpecialTerrainPrefabs.AddReplace(HoloMapBlueprint.RIGHT_BRIDGE, () => new GameObject[] { GetGameObject("UndeadMainPath_4", "Scenery/HoloBridge_Entrance") });
            SpecialTerrainPrefabs.AddReplace(HoloMapBlueprint.LEFT_BRIDGE, () => new GameObject[] { GetGameObject("UndeadMainPath_3", "Scenery/HoloBridge_Entrance") });
            SpecialTerrainPrefabs.AddReplace(HoloMapBlueprint.FULL_BRIDGE, () => new GameObject[] { GetGameObject("NeutralEastMain_2", "Scenery") });
            SpecialTerrainPrefabs.AddReplace(HoloMapBlueprint.NORTH_BUILDING_ENTRANCE, () => GetGameObject(new string[] { "UndeadMainPath_4/Scenery/SM_Bld_Wall_Exterior_04", "UndeadMainPath_4/Scenery/SM_Bld_Wall_Exterior_04 (1)", "UndeadMainPath_4/Scenery/SM_Bld_Wall_Doorframe_02" }));
            SpecialTerrainPrefabs.AddReplace(HoloMapBlueprint.NORTH_GATEWAY, () => new GameObject[] { GetGameObject("NatureMainPath_2", "Scenery/HoloGateway") });
            SpecialTerrainPrefabs.AddReplace(HoloMapBlueprint.NORTH_CABIN, () => new GameObject[] { GetGameObject("TempleNature_4", "Scenery/Cabin")});

            // Let's instantiate the battle arrow prefabs
            ArrowPrefabs = new();
            ArrowPrefabs.AddReplace(EAST | ENEMY, () => GetGameObject("neutraleastmain_3", "Nodes/MoveArea_E"));
            ArrowPrefabs.AddReplace(SOUTH | ENEMY, () => GetGameObject("UndeadEntrance", "Nodes/MoveArea_S"));
            ArrowPrefabs.AddReplace(NORTH | ENEMY, () => GetGameObject("naturemainpath_2", "Nodes/MoveArea_N"));
            ArrowPrefabs.AddReplace(WEST | ENEMY, () => GetGameObject("neutralwestmain_2", "Nodes/MoveArea_W"));

            ArrowPrefabs.AddReplace(WEST | COUNTDOWN, () => GetGameObject("natureentrance", "Nodes/MoveArea_W"));
            ArrowPrefabs.AddReplace(SOUTH | COUNTDOWN, () => GetGameObject("wizardmainpath_3", "Nodes/MoveArea_S"));

            // This generates 'pseudo-prefab' objects
            // We will have one for each zone
            // Each random node will randomly turn scenery nodes on and off
            // And will set the arrows appropriately.
            neutralHoloPrefab = GameObject.Instantiate(defaultPrefab);
            neutralHoloPrefab.SetActive(false);
        }

        private static List<int> CompletedRegions
        {
            get
            {
                return EventManagement.CompletedZones.Select( s=>
                    s.EndsWith("Undead") ? UNDEAD :
                    s.EndsWith("Wizard") ? MAGIC : 
                    s.EndsWith("Tech") ? TECH :
                    s.EndsWith("Nature") ? NATURE :
                    NEUTRAL).ToList();
            }
        }

        private static float DistanceFromCenter(this Tuple<float, float> p)
        {
            return Mathf.Sqrt(Mathf.Pow(p.Item1, 2) + Mathf.Pow(p.Item2, 2));
        }

        private static int DistanceComparer(Tuple<float, float> a, Tuple<float, float> b)
        {
            float da = a.DistanceFromCenter();
            float db = b.DistanceFromCenter();
            if (da == db)
                return 0;
            if (da > db)
                return 1;
            return -1;
        }

        private static float[] MULTIPLIERS = new float[] { 0.33f, 0.66f };
        private static List<Tuple<float, float>> GetSpotsForQuadrant(int quadrant)
        {
            float minX = ((quadrant & WEST) != 0) ? -3.2f : ((quadrant & EAST) != 0) ? 1.1f : -1.1f;
            float maxX = ((quadrant & WEST) != 0) ? -1.1f : ((quadrant & EAST) != 0) ? 3.2f : 1.1f;
            float minZ = ((quadrant & NORTH) != 0) ? 1.1f : ((quadrant & SOUTH) != 0) ? -2.02f : -1.1f;
            float maxZ = ((quadrant & NORTH) != 0) ? 2.02f : ((quadrant & SOUTH) != 0) ? -1.1f : 1.1f;
            
            List<Tuple<float, float>> retval = new();
            foreach (float m in MULTIPLIERS)
                foreach (float n in MULTIPLIERS)
                    retval.Add(new(minX + m * (maxX - minX) - .025f + .05f * UnityEngine.Random.value, minZ + n * (maxZ - minZ) - .025f + .05f * UnityEngine.Random.value));

            retval.Sort(DistanceComparer);

            return retval;
        }

        private static GameObject GetItemNode()
        {
            GameObject baseObject = GetGameObject("Shop", "Nodes/ShopNode3D_ShieldGenItem");
            GameObject retval = GameObject.Instantiate(baseObject);

            // Turn this into a trade node
            HoloMapSpecialNode nodeData = retval.GetComponentInChildren<HoloMapSpecialNode>();
            nodeData.nodeType = UnlockAscensionItemNodeData.UnlockItemsAscension;
            nodeData.repeatable = false;

            retval.SetActive(false);

            return retval;
        }

        private static GameObject GetRecycleNode()
        {
            GameObject baseObject = GetGameObject("TechTower_NW", "Nodes/ShopNode3D_Recycle");
            GameObject retval = GameObject.Instantiate(baseObject);

            // Turn this into a trade node
            HoloMapSpecialNode nodeData = retval.GetComponentInChildren<HoloMapSpecialNode>();
            nodeData.nodeType = AscensionRecycleCardNodeData.AscensionRecycleCard;
            nodeData.repeatable = false;

            retval.SetActive(false);

            return retval;
        }

        private static GameObject GetTransformerNode()
        {
            GameObject baseObject = GetGameObject("Shop", "Nodes/ShopNode3D_Transformer");
            GameObject retval = GameObject.Instantiate(baseObject);

            // Turn this into a trade node
            HoloMapSpecialNode nodeData = retval.GetComponentInChildren<HoloMapSpecialNode>();
            nodeData.nodeType = AscensionTransformerCardNodeData.AscensionTransformCard;
            nodeData.repeatable = false;

            retval.SetActive(false);

            return retval;
        }

        private static GameObject GetDraftNode()
        {
            GameObject baseObject = GetGameObject("WizardMainPath_3", "Nodes/CardChoiceNode3D");
            GameObject retval = GameObject.Instantiate(baseObject);

            // Turn this into a trade node
            HoloMapSpecialNode nodeData = retval.GetComponent<HoloMapSpecialNode>();
            nodeData.nodeType = TradeChipsNodeData.TradeChipsForCards;
            nodeData.repeatable = true;

            retval.transform.Find("RendererParent/Renderer_2").gameObject.SetActive(false);
            retval.transform.localEulerAngles = new (0f, 0f, 0f);

            GameObject card = retval.transform.Find("RendererParent/Renderer").gameObject;
            card.transform.localPosition = new(-0.1f, 0.05f, -0.3f);
            card.transform.localScale = new (1f, 1f, .8f);
            card.transform.localEulerAngles = new(271f, 191f, 9f);

            GameObject second = GameObject.Instantiate(card, card.transform.parent);
            second.transform.localPosition = new(0.13f, -0.1f, -0.3f);
            nodeData.nodeRenderers.Add(second.GetComponent<Renderer>());

            GameObject third = GameObject.Instantiate(card, card.transform.parent);
            third.transform.localPosition = new(0f, -0.01f, -0.3f);
            nodeData.nodeRenderers.Add(third.GetComponent<Renderer>());

            // Add an 'active only if' flag
            // ActiveIfStoryFlag flag = retval.AddComponent<ActiveIfStoryFlag>();
            // Traverse.Create(flag).Field("storyFlag").SetValue(EventManagement.HAS_DRAFT_TOKEN);
            // Traverse.Create(flag).Field("activeIfConditionMet").SetValue(true);

            retval.SetActive(false);

            P03Plugin.Log.LogInfo($"Build draft node {retval}");
            return retval;
        }

        private static void BuildSpecialNode(HoloMapBlueprint blueprint, int regionId, Transform parent, Transform sceneryParent, float x, float z)
        {
            BuildSpecialNode(blueprint.upgrade, blueprint.specialTerrain, regionId, parent, sceneryParent, x, z);
        }

        private static HoloMapNode BuildSpecialNode(HoloMapNode.NodeDataType dataType, int specialTerrain, int regionId, Transform parent, Transform sceneryParent, float x, float z)
        {
            if (!SpecialNodePrefabs.ContainsKey(dataType))
                return null;

            P03Plugin.Log.LogInfo($"Adding {dataType.ToString()} at {x},{z}");

            GameObject defaultNode = SpecialNodePrefabs[dataType];

            P03Plugin.Log.LogInfo($"node is{defaultNode}");
            GameObject newNode = GameObject.Instantiate(defaultNode, parent);
            newNode.SetActive(true);

            HoloMapShopNode shopNode = newNode.GetComponent<HoloMapShopNode>();
            if (shopNode != null)
            {
                // This is a shop node but we want it to behave differently than the in-game shop nodes
                Traverse shopTraverse = Traverse.Create(shopNode);
                shopTraverse.Field("cost").SetValue(EventManagement.UpgradePrice(dataType));
                shopTraverse.Field("repeatable").SetValue(false);
                shopTraverse.Field("increasingCost").SetValue(false);
            }

            if (dataType == HoloMapSpecialNode.NodeDataType.GainCurrency)
            {
                newNode.transform.localPosition = new Vector3(x, newNode.transform.localPosition.y, z);
                HoloMapGainCurrencyNode nodeData = newNode.GetComponent<HoloMapGainCurrencyNode>();
                Traverse nodeTraverse = Traverse.Create(nodeData);
                nodeTraverse.Field("amount").SetValue(UnityEngine.Random.Range(EventManagement.CurrencyGainRange.Item1, EventManagement.CurrencyGainRange.Item2));
            }
            else
            {
                if (sceneryParent != null)
                {
                    float yVal = ((specialTerrain & HoloMapBlueprint.FULL_BRIDGE) == 0) ? newNode.transform.localPosition.y : .5f;
                    newNode.transform.localPosition = new Vector3(x, yVal, z);

                    yVal = ((specialTerrain & HoloMapBlueprint.FULL_BRIDGE) == 0) ? .1f : 1.33f;

                    GameObject nodeBasePrefab = ((specialTerrain & HoloMapBlueprint.FULL_BRIDGE) == 0) ? HOLO_NODE_BASE : HOVER_HOLO_NODE_BASE;
                    P03Plugin.Log.LogInfo($"nodebase is{nodeBasePrefab}");
                    GameObject nodeBase = GameObject.Instantiate(nodeBasePrefab, sceneryParent);
                    nodeBase.transform.localPosition = new Vector3(newNode.transform.localPosition.x, yVal, newNode.transform.localPosition.z);
                }
            }

            return newNode.GetComponent<HoloMapNode>();
        }

        private static GameObject BuildP03BossNode()
        {
            GameObject hubNodeBase = Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_StartingIslandTablets");
            GameObject retval = GameObject.Instantiate(hubNodeBase);

            Part3FinaleAreaSequencer sequencer = retval.GetComponent<Part3FinaleAreaSequencer>();
            Component.Destroy(sequencer);

            //AscensionFinaleSequencer newSequencer = retval.AddComponent<AscensionFinaleSequencer>();
            //newSequencer.enabled = true;

            HoloMapArea area = retval.GetComponent<HoloMapArea>();
            //Traverse.Create(area).Field("specialSequencer").SetValue(newSequencer);
            area.firstEnterDialogueId = "P03AscensionPreIntro";

            P03Plugin.Log.LogInfo("Building boss node");
            HoloMapBossNode bossNode = BuildSpecialNode(HoloMapNode.NodeDataType.BossBattle, HoloMapBlueprint.NO_SPECIAL, NEUTRAL, retval.transform.Find("Nodes"), null, 0f, 0f) as HoloMapBossNode;
            P03Plugin.Log.LogInfo($"Making boss invisible: { bossNode }");
            foreach (Renderer rend in bossNode.gameObject.GetComponentsInChildren<Renderer>())
                rend.enabled = false; // Hide the boss node visually - I don't want to see it

            bossNode.lootNodes = new();
            bossNode.bossAnim = null;
            bossNode.specialEncounterId = BossBattleSequencer.GetSequencerIdForBoss(BossManagement.P03FinalBossOpponent);

            P03Plugin.Log.LogInfo("Setting boss type");
            CardBattleNodeData data = bossNode.Data as CardBattleNodeData;
            data.specialBattleId = BossBattleSequencer.GetSequencerIdForBoss(BossManagement.P03FinalBossOpponent);

            area.bossNode = bossNode;
            area.activateBossOnEnter = true;

            retval.SetActive(false);
            return retval;
        }

        private static GameObject BuildHubNode()
        {
            GameObject hubNodeBase = Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_StartingIslandWaypoint");
            GameObject retval = GameObject.Instantiate(hubNodeBase);
            
            // We don't want the bottom arrow
            retval.transform.Find("Nodes/MoveArea_S").gameObject.SetActive(false);
            retval.transform.Find("Nodes/CurrencyGainNode3D").gameObject.SetActive(false);

            // We need to set a conditional up arrow
            HoloMapArea areaData = retval.GetComponent<HoloMapArea>();
            Traverse areaTrav = Traverse.Create(areaData);
            BlockDirections(retval, areaTrav, NORTH, EventManagement.ALL_BOSSES_KILLED);

            // We need to add the draft node
            Transform nodes = retval.transform.Find("Nodes");
            Transform scenery = retval.transform.Find("Scenery");
            HoloMapNode node = BuildSpecialNode(TradeChipsNodeData.TradeChipsForCards, 0, NEUTRAL, nodes, scenery, 1.5f, 0f);
            
            retval.SetActive(false);
            return retval;
        }

        private static void BlockDirections(GameObject area, Traverse areaTrav, int blocked, StoryEvent storyEvent)
        {
            P03Plugin.Log.LogInfo($"Blocking directions");
            List<GameObject> blockIcons = new();
            List<LookDirection> blockedDirections = new();
            foreach (int direction in GetDirections(blocked, true))
            {
                blockedDirections.Add(LOOK_MAPPER[direction]);

                GameObject blockIcon = GameObject.Instantiate(BLOCK_ICON, area.transform);
                blockIcons.Add(blockIcon);
                Vector3 pos = REGION_DATA[NEUTRAL].wallOrientations[direction].Item1;
                blockIcon.transform.localPosition = new (pos.x, 0.3f, pos.z);
                blockIcon.transform.localEulerAngles = REGION_DATA[NEUTRAL].wallOrientations[direction].Item2;
            }

            BlockDirectionsAreaSequencer sequencer = area.AddComponent<BlockDirectionsAreaSequencer>();
            Traverse blockTraverse = Traverse.Create(sequencer);
            blockTraverse.Field("stopIcons").SetValue(blockIcons);
            blockTraverse.Field("unblockStoryEvent").SetValue(storyEvent);
            blockTraverse.Field("blockedDirections").SetValue(blockedDirections);
            areaTrav.Field("specialSequencer").SetValue(sequencer);
        }

        private static void CleanBattleFromArrow(GameObject room, string direction)
        {
            GameObject southArrow = room.transform.Find($"Nodes/MoveArea_{direction}").gameObject;
            MoveHoloMapAreaNode southNode = southArrow.GetComponent<MoveHoloMapAreaNode>();
            Traverse southTraverse = Traverse.Create(southNode);
            southTraverse.Field("nodeType").SetValue(HoloMapNode.NodeDataType.MoveArea);
            southTraverse.Field("blueprintData").SetValue(null);
        }

        private static GameObject BuildLowerTowerRoom()
        {
            GameObject prefab = Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleWizardEntrance");
            GameObject retval = GameObject.Instantiate(prefab);

            // No dialogue
            HoloMapArea area = retval.GetComponent<HoloMapArea>();
            Traverse.Create(area).Field("firstEnterDialogueId").SetValue(null);

            // Kill the shop node:
            retval.transform.Find("Nodes/MoveArea_E").gameObject.SetActive(false);

            // Kill the open door
            retval.transform.Find("Scenery/Doorframe").gameObject.SetActive(false);

            // Remove the battle info from the West and South arrows
            CleanBattleFromArrow(retval, "W (NORTH)");
            CleanBattleFromArrow(retval, "S");

            // Fill the open door with a clone of the wall piece
            GameObject rightWall = retval.transform.Find("Scenery/RightWall").gameObject;
            GameObject newRightWall = GameObject.Instantiate(rightWall, rightWall.transform.parent);
            newRightWall.transform.localPosition = new Vector3(rightWall.transform.localPosition.x, rightWall.transform.localPosition.y, 0.45f);

            retval.SetActive(false);
            return retval;
        }

        private static EncounterBlueprintData GetBlueprintForRegion(int regionId, int color)
        {
            string encounterName = default(string);
            if (color == 1) // The first encounter pulls from neutral
            {
                string[] encounters = REGION_DATA[NEUTRAL].encounters;
                encounterName = encounters[UnityEngine.Random.Range(0, encounters.Length)];
            }
            else
            {
                encounterName = REGION_DATA[regionId].encounters[color - 2];
            }

             P03Plugin.Log.LogDebug($"Hi {encounterName}");

            // Use EncounterBlueprintHelper to get our custom representation of the encounter blueprint
            // and convert that to a blueprint the game understands
            return (new EncounterBlueprintHelper(DataHelper.GetResourceString(encounterName, "dat"))).AsBlueprint();
        }

        private static GameObject BuildMapAreaPrefab(int regionId, HoloMapBlueprint bp)
        {
            P03Plugin.Log.LogInfo($"Building gameobject for [{bp.x},{bp.y}]");

            if (bp.opponent == Opponent.Type.P03Boss)
            {
                GameObject retval = BuildP03BossNode();
                retval.name = $"ProceduralMapArea_{regionId}_{bp.x}_{bp.y})";
                return retval;
            }

            if (bp.opponent != Opponent.Type.Default)
            {
                GameObject retval = GameObject.Instantiate(BossPrefabs[bp.opponent]);
                if (bp.opponent == Opponent.Type.TelegrapherBoss)
                {
                    retval.transform.Find("Nodes/MoveArea_E").gameObject.SetActive(false);
                    retval.transform.Find("Nodes/MoveArea_W").gameObject.SetActive(false);
                    CleanBattleFromArrow(retval, "S");
                }

                FlyBackToCenterIfBossDefeated returnToCenter = retval.AddComponent<FlyBackToCenterIfBossDefeated>();
                retval.GetComponent<HoloMapArea>().specialSequencer = returnToCenter;

                // This is a bit of a CYA
                // We want to make sure the battle id and the opponent always match - this can get out of sync with some of our custom patches
                HoloMapBossNode bossNode = retval.GetComponentInChildren<HoloMapBossNode>();
                bossNode.specialEncounterId = BossBattleSequencer.GetSequencerIdForBoss(bp.opponent);
                CardBattleNodeData bossData = bossNode.Data as CardBattleNodeData;
                bossData.specialBattleId = BossBattleSequencer.GetSequencerIdForBoss(bp.opponent);

                P03Plugin.Log.LogInfo($"Setting special battle id {bossData.specialBattleId} for opponent {bp.opponent.ToString()}");

                retval.SetActive(false);
                retval.name = $"ProceduralMapArea_{regionId}_{bp.x}_{bp.y})";
                return retval;
            }

            if (bp.upgrade == HoloMapSpecialNode.NodeDataType.FastTravel)
                return BuildHubNode();

            if (bp.specialTerrain == HoloMapBlueprint.LOWER_TOWER_ROOM)
                return BuildLowerTowerRoom();

            P03Plugin.Log.LogInfo($"Instantiating base object {neutralHoloPrefab}");
            GameObject area = GameObject.Instantiate(neutralHoloPrefab);
            area.name = $"ProceduralMapArea_{regionId}_{bp.x}_{bp.y})";

            P03Plugin.Log.LogInfo($"Getting nodes");
            GameObject nodes = area.transform.Find("Nodes").gameObject;

            if (DIR_LOOKUP.ContainsKey(bp.specialDirection))
            {
                if (bp.specialDirectionType == HoloMapBlueprint.TRADE)
                {
                    GameObject arrowToReplace = area.transform.Find($"Nodes/MoveArea_{DIR_LOOKUP[bp.specialDirection]}").gameObject;
                    arrowToReplace.GetComponent<HoloMapNode>().nodeType = HoloMapNode.NodeDataType.MoveAreaTrade;
                }
                if (bp.specialDirectionType == HoloMapBlueprint.BATTLE)
                {
                    P03Plugin.Log.LogInfo($"Finding arrow to destroy");
                    GameObject arrowToReplace = area.transform.Find($"Nodes/MoveArea_{DIR_LOOKUP[bp.specialDirection]}").gameObject;
                    P03Plugin.Log.LogInfo($"Destroying arrow");
                    GameObject.DestroyImmediate(arrowToReplace);
                    
                    P03Plugin.Log.LogInfo($"Copying arrow");
                    GameObject newArrow = GameObject.Instantiate(ArrowPrefabs[bp.specialDirection | ENEMY], nodes.transform);
                    newArrow.name = $"MoveArea_{DIR_LOOKUP[bp.specialDirection]}";
                    HoloMapNode node = newArrow.GetComponent<HoloMapNode>();
                    Traverse nodeTraverse = Traverse.Create(node);
                    nodeTraverse.Field("blueprintData").SetValue(GetBlueprintForRegion(regionId, bp.color));
                    nodeTraverse.Field("encounterDifficulty").SetValue(bp.encounterDifficulty);
                    if ((bp.specialTerrain & HoloMapBlueprint.FULL_BRIDGE) != 0)
                        nodeTraverse.Field("bridgeBattle").SetValue(true);
                    
                    if (bp.battleTerrainIndex > 0 && (bp.specialTerrain & HoloMapBlueprint.FULL_BRIDGE) == 0)
                    {
                        string[] terrain = REGION_DATA[regionId].terrain[bp.battleTerrainIndex - 1];
                        nodeTraverse.Field("playerTerrain").SetValue(terrain.Take(5).Select(s => s == default(string) ? null : CardLoader.GetCardByName(s)).ToArray());
                        nodeTraverse.Field("opponentTerrain").SetValue(terrain.Skip(5).Select(s => s == default(string) ? null : CardLoader.GetCardByName(s)).ToArray());
                    }
                    else
                    {
                        nodeTraverse.Field("playerTerrain").SetValue(new CardInfo[5]);
                        nodeTraverse.Field("opponentTerrain").SetValue(new CardInfo[5]);
                    }
                }
            }

            P03Plugin.Log.LogInfo($"Setting arrows and walls active");
            Transform scenery = area.transform.Find("Scenery");
            if (REGION_DATA[regionId].wallPrefabs != null && REGION_DATA[regionId].wallPrefabs.Keys.Count > 0)
            {
                foreach (int key in DIR_LOOKUP.Keys)
                {
                    area.transform.Find($"Nodes/MoveArea_{DIR_LOOKUP[key]}").gameObject.SetActive((bp.arrowDirections & key) != 0);

                    if ((bp.arrowDirections & key) == 0)
                        foreach (string wallPrefabKey in REGION_DATA[regionId].wallPrefabs[key])
                            GameObject.Instantiate(GetGameObject(wallPrefabKey), scenery);
                }
            }
            else
            {
                GameObject wall = GetGameObject(REGION_DATA[regionId].wall);
                foreach (int key in DIR_LOOKUP.Keys)
                {
                    area.transform.Find($"Nodes/MoveArea_{DIR_LOOKUP[key]}").gameObject.SetActive((bp.arrowDirections & key) != 0);

                    // Walls
                    if (wall != null)
                    {
                        if ((bp.arrowDirections & key) == 0)
                        {
                            GameObject wallClone = GameObject.Instantiate(wall, scenery);
                            wallClone.transform.localPosition = REGION_DATA[regionId].wallOrientations[key].Item1;
                            wallClone.transform.localEulerAngles = REGION_DATA[regionId].wallOrientations[key].Item2;
                        }
                    }
                }
            }

            P03Plugin.Log.LogInfo($"Generating random scenery");

            // Add the landmarks if necessary
            if ((bp.specialTerrain & HoloMapBlueprint.LANDMARKER) != 0)
                foreach (string objId in REGION_DATA[regionId].landmarks[bp.color - 1])
                    GameObject.Instantiate(GetGameObject(objId), scenery);

            // Add the normal scenery
            // For each section of the board that doesn't have an arrow on it
            List<int> directions = GetDirections(bp.arrowDirections, false).ToList(); 
            bool firstQuadrant = true; 
            while(directions.Count > 0)
            {
                int dir = directions[UnityEngine.Random.Range(0, directions.Count)];
                directions.Remove(dir);

                List<Tuple<float, float>> sceneryLocations = GetSpotsForQuadrant(dir);

                bool firstObject = true;
                while (sceneryLocations.Count > 0)
                {
                    int spIdx = firstObject ? 0 : UnityEngine.Random.Range(0, sceneryLocations.Count);
                    Tuple<float, float> specialLocation = sceneryLocations[spIdx];
                    sceneryLocations.RemoveAt(spIdx);

                    if (firstQuadrant && firstObject && bp.upgrade != HoloMapSpecialNode.NodeDataType.MoveArea)
                    {
                        BuildSpecialNode(bp, regionId, nodes.transform, scenery.transform, specialLocation.Item1, specialLocation.Item2);
                        firstQuadrant = false;
                        firstObject = false;
                        continue;
                    }

                    if (firstObject && (bp.specialTerrain & HoloMapBlueprint.LANDMARKER) != 0)
                    {
                        firstObject = false;
                        continue;
                    }

                    string[] scenerySource = firstObject ? REGION_DATA[regionId].objectRandoms : REGION_DATA[regionId].terrainRandoms;

                    firstQuadrant = false;
                    firstObject = false;

                    if (scenerySource.Length == 0)
                        continue;

                    string sceneryKey = scenerySource[UnityEngine.Random.Range(0, scenerySource.Length)];
                    GameObject sceneryObject = GameObject.Instantiate(GetGameObject(sceneryKey), scenery);
                    sceneryObject.transform.localPosition = new Vector3(specialLocation.Item1, sceneryObject.transform.localPosition.y, specialLocation.Item2);
                    sceneryObject.transform.localEulerAngles = new Vector3(sceneryObject.transform.localEulerAngles.x, UnityEngine.Random.Range(0f, 360f), sceneryObject.transform.localEulerAngles.z);
                }
            }

            P03Plugin.Log.LogInfo($"Generating special terrain");
            foreach (int key in SpecialTerrainPrefabs.Keys)
                if ((bp.specialTerrain & key) != 0)
                    foreach (GameObject obj in SpecialTerrainPrefabs[key])
                        GameObject.Instantiate(obj, scenery);

            P03Plugin.Log.LogInfo($"Setting grid data");
            HoloMapArea areaData = area.GetComponent<HoloMapArea>();
            Traverse areaTrav = Traverse.Create(areaData);
            areaData.GridX = bp.x;
            areaData.GridY = bp.y;
            areaData.audioLoopsConfig = REGION_DATA[regionId].audioConfig;
            areaData.screenPrefab = REGION_DATA[regionId].screenPrefab;
            areaTrav.Field("mainColor").SetValue(REGION_DATA[regionId].mainColor);
            areaTrav.Field("lightColor").SetValue(REGION_DATA[regionId].mainColor);

            if (bp.blockedDirections != BLANK)
                BlockDirections(area, areaTrav, bp.blockedDirections, EventManagement.ALL_ZONE_ENEMIES_KILLED);

            // Give every node a unique id
            int nodeId = 10;
            foreach (MapNode node in area.GetComponentsInChildren<MapNode>())
                node.nodeId = node is MoveHoloMapAreaNode ? nodeId++ - 10 : nodeId++;

            area.SetActive(false);
            return area;
        }

        private static void ConnectArea(HoloMapWorldData.AreaData[,] map, HoloMapBlueprint bp)
        {
            GameObject area = map[bp.x, bp.y].prefab;

            if (area == null)
                return;

            HoloMapArea areaData = area.GetComponent<HoloMapArea>();

            // The index of DirectionNodes has to correspond to the integer value of the LookDirection enumeration
            areaData.DirectionNodes.Clear();
            for (int i = 0; i < 4; i++)
                areaData.DirectionNodes.Add(null);

            Transform nodes = area.transform.Find("Nodes");

            foreach (Transform arrow in nodes)
                if (arrow.gameObject.name.StartsWith("MoveArea"))
                    areaData.DirectionNodes[(int)LOOK_NAME_MAPPER[arrow.gameObject.name]] = arrow.gameObject.activeSelf ? arrow.gameObject.GetComponent<MoveHoloMapAreaNode>() : null;
        }

        public static string GetAscensionWorldID(int regionCode)
        {
            if (regionCode == NEUTRAL)
                return $"ascension_0_{regionCode}";

            return $"ascension_{EventManagement.CompletedZones.Count}_{regionCode}";
        }

        public static int GetRegionCodeFromWorldID(string worldId)
        {
            return int.Parse(worldId[worldId.Length - 1].ToString());
        }

        public static Tuple<int, int> GetStartingSpace(int regionCode)
        {
            return regionCode == NEUTRAL ? new(0, 1) : new(0, 2);
        }

        public static HoloMapWorldData GetAscensionWorldbyId(string id)
        {
            P03Plugin.Log.LogInfo($"Getting world for {id}");

            HoloMapWorldData data = ScriptableObject.CreateInstance<HoloMapWorldData>();
            data.name = id;

            string[] idSplit = id.Split('_');
            int regionCount = int.Parse(idSplit[1]);
            int regionCode = int.Parse(idSplit[2]);

            List<HoloMapBlueprint> blueprints = BuildBlueprint(regionCount, regionCode, P03AscensionSaveData.RandomSeed);

            int xDimension = blueprints.Select(b => b.x).Max() + 1;
            int yDimension = blueprints.Select(b => b.y).Max() + 1;

            data.areas = new HoloMapWorldData.AreaData[xDimension, yDimension];

            foreach(HoloMapBlueprint bp in blueprints)
            {
                GameObject mapArea = BuildMapAreaPrefab(regionCode, bp);

                if (regionCode != NEUTRAL)
                    Minimap.CreateMinimap(mapArea.transform, blueprints, $"ProceduralMapArea_{regionCode}");
                
                data.areas[bp.x, bp.y] = new() { prefab = mapArea };
            }

            // The second pass creates relationships between everything
            foreach(HoloMapBlueprint bp in blueprints)
                ConnectArea(data.areas, bp);

            return data;
        }

        [HarmonyPatch(typeof(HoloMapArea), "OnAreaActive")]
        [HarmonyPrefix]
        public static void ActivateObject(ref HoloMapArea __instance)
        {
            if (SaveFile.IsAscension && !__instance.gameObject.activeSelf)
                __instance.gameObject.SetActive(true);
        }

        private static bool ValidateWorldData(HoloMapWorldData data)
        {
            if (data == null || data.areas == null)
                return false;

            for (int i = 0; i < data.areas.GetLength(0); i++)
                for (int j = 0; j < data.areas.GetLength(1); j++)
                    if (data.areas[i,j] != null && data.areas[i,j].prefab != null)
                        return true;

            return false;
        }

        public static void ClearWorldData()
        {
            P03Plugin.Log.LogInfo("Clearing world data");

            // This completely clears the cache of game objects that we have access to
            foreach (var entry in worldDataCache)
                for (int i = 0; i < entry.Value.areas.GetLength(0); i++)
                    for (int j = 0; j < entry.Value.areas.GetLength(1); j++)
                        if (entry.Value.areas[i,j] != null && entry.Value.areas[i,j].prefab != null)
                            GameObject.DestroyImmediate(entry.Value.areas[i,j].prefab);

            
            worldDataCache.Clear();

            foreach(var entry in SpecialNodePrefabs)
                if (entry.Value != null && !objectLookups.Values.Contains(entry.Value))
                    GameObject.DestroyImmediate(entry.Value);
            SpecialNodePrefabs.Clear();

            foreach(var entry in ArrowPrefabs)
                if (entry.Value != null && !objectLookups.Values.Contains(entry.Value))
                    GameObject.DestroyImmediate(entry.Value);
            ArrowPrefabs.Clear();

            foreach(var entry in SpecialTerrainPrefabs)
                foreach (GameObject obj in entry.Value)
                    if (obj != null && !objectLookups.Values.Contains(obj))
                        GameObject.DestroyImmediate(obj);
            SpecialTerrainPrefabs.Clear();        
        }

        [HarmonyPatch(typeof(HoloMapDataLoader), "GetWorldById")]
        [HarmonyPrefix]
        private static bool PatchGetAscensionWorldById(ref HoloMapWorldData __result, string id)
        {
            if (id.ToLowerInvariant().StartsWith("ascension_"))
            {
                if (worldDataCache.ContainsKey(id) && ValidateWorldData(worldDataCache[id]))
                {
                    __result = worldDataCache[id];
                    return false;
                }

                Initialize();
                if (worldDataCache.ContainsKey(id))
                    worldDataCache.Remove(id);
                worldDataCache.Add(id, GetAscensionWorldbyId(id));
                __result = worldDataCache[id];
                return false;
            }                
            return true;        
        }
    }
}