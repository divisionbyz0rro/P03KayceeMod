using System;
using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    public class RegionGeneratorData
    {
        public int regionCode;

        public string[] encounters;

        public string[] terrainRandoms;

        public string[] objectRandoms;

        public string wall;

        public Dictionary<int, Tuple<Vector3, Vector3>> wallOrientations;

        public Color lightColor;

        public Color mainColor;

        public HoloMapSpecialNode.NodeDataType defaultReward;

        public string[][] terrain;

        public string[][] landmarks;

        public HoloMapArea.AudioLoopsConfig audioConfig;

        public GameObject screenPrefab;

        public Dictionary<int, string[]> wallPrefabs;

        public RegionGeneratorData(int regionCode)
        {
            this.regionCode = regionCode;
            switch (regionCode)
            {
                case RunBasedHoloMap.NEUTRAL:
                    this.encounters = new string[] { "neutral_alarmbots", "neutral_bombsandshields", "neutral_bridgebattle", "neutral_minecarts", "neutral_sentrywall", "neutral_swapbots"};
                    this.terrainRandoms = new string[] { "NeutralEastMain_4/Scenery/HoloGrass_Small (1)", "NeutralEastMain_4/Scenery/HoloGrass_Patch (1)" };
                    this.objectRandoms = new string[] { "StartingIslandBattery/Scenery/HoloMeter","StartingIslandBattery/Scenery/HoloDrone_Broken","StartingIslandJunction/Scenery/HoloBotPiece_Leg","StartingIslandJunction/Scenery/HoloBotPiece_Head","NeutralEastMain_4/Scenery/HoloDrone_1", "NeutralEastMain_4/Scenery/HoloDrone_1", "NeutralEastMain_4/Scenery/HoloDrone_1", "NeutralEastMain_4/Scenery/HoloPowerPoll_1" };
                    this.wall = "NeutralEastMain_4/Scenery/HoloWall (1)";
                    this.lightColor = new Color(0.5802f, 0.8996f, 1f);
                    this.mainColor = new Color(0f, 0.5157f, 0.6792f);
                    this.defaultReward = HoloMapNode.NodeDataType.AddCardAbility;
                    wallOrientations = new();
                    wallOrientations.Add(RunBasedHoloMap.NORTH, new(new(.08f, -.18f, 2.02f), new(7.4407f, 179.305f, .0297f)));
                    wallOrientations.Add(RunBasedHoloMap.SOUTH, new(new(.08f, -.18f, -2.02f), new(7.4407f, 359.2266f, .0297f)));
                    wallOrientations.Add(RunBasedHoloMap.WEST, new(new(-3.2f, -.18f, -.4f), new(7.4407f, 89.603f, .0297f)));
                    wallOrientations.Add(RunBasedHoloMap.EAST, new(new(3.2f, -.18f, -.4f), new(7.4407f, 270.359f, .0297f)));
                    this.audioConfig = HoloMapArea.AudioLoopsConfig.Default;
                    this.screenPrefab = Resources.Load<GameObject>("prefabs/map/holomapscreens/HoloMapScreen_DefaultGround");
                    break;
                case RunBasedHoloMap.MAGIC:
                    this.encounters = new string[] { "wizard_bigripper", "wizard_gemexploder", "wizard_shieldgems" };
                    this.terrainRandoms = new string[] { "WizardEntrance/Scenery/HoloDebris (1)" };
                    this.objectRandoms = new string[] { "WizardSidePath_1/Scenery/HoloRock_Floating", "WizardMainPath_2/Scenery/HoloRock_Floating (2)", "WizardSidePath_1/Scenery/HoloRock_Floating", "WizardMainPath_2/Scenery/HoloRock_Floating (2)", "WizardEntrance/Scenery/HoloGemGreen", "WizardEntrance/Scenery/HoloGemOrange", "WizardEntrance/Scenery/HoloGemBlue" };
                    this.defaultReward = HoloMapNode.NodeDataType.AttachGem;
                    this.lightColor = new Color(0.5802f, 0.8996f, 1f);
                    this.mainColor = new Color(0.1802f, 0.2778f, 0.5094f);
                    this.terrain = new string[][] {
                        new string[] { null, null, null, null, null, null, null, null, null, null } // There's no terrain in the magic world
                    };
                    this.landmarks = new string[][] {
                        new string[] { "WizardMainPath_3/Scenery/HoloGenerator" },
                        new string[] { "WizardMainPath_3/Scenery/HoloSlime_Pile_1", "WizardMainPath_3/Scenery/HoloSlime_Pile_2", "WizardMainPath_3/Scenery/HoloSlimePipe", "WizardMainPath_3/Scenery/HoloSlime_Pile_1 (1)" },
                        new string[] { "WizardSidePath_3/Scenery/HoloSword", "WizardSidePath_3/Scenery/HoloSword (1)", "WizardSidePath_3/Scenery/HoloSword (2)", "WizardSidePath_1/Scenery/HoloSword", "WizardSidePath_1/Scenery/HoloSword (1)", },
                        new string[] { "TempleWizardEntrance/Scenery/Gem", "TempleWizardEntrance/Scenery/Gem (2)", "TempleWizardEntrance/Scenery/Gem (3)", "TempleWizardMain1/Scenery/Gem", "TempleWizardMain1/Scenery/Gem (2)", "TempleWizardMain1/Scenery/Gem (3)", "TempleWizardMain2/Scenery/Gem (2)", "WizardMainPath_6/Scenery/HoloGemBlue/Gem"  },
                    };
                    this.audioConfig = HoloMapArea.AudioLoopsConfig.Wizard;
                    this.screenPrefab = Resources.Load<GameObject>("prefabs/map/holomapscreens/HoloMapScreen_DefaultGround");
                    break;
                case RunBasedHoloMap.NATURE:
                    this.terrainRandoms = new string[] { "NatureMainPath_2/Scenery/HoloGrass_Foliage", "NatureMainPath_2/Scenery/HoloDebris" };
                    this.objectRandoms = new string[] { "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloCage_1" };
                    this.wall = "NatureMainPath_2/Scenery/HoloTree_3";
                    this.defaultReward = HoloMapNode.NodeDataType.CreateTransformer;
                    this.lightColor = new Color(.4078f, .698f, .4549f);
                    this.mainColor = new Color(.4431f, .448f, .1922f);
                    wallOrientations = new();
                    wallOrientations.Add(RunBasedHoloMap.NORTH, new(new(.08f, -.18f, 2.02f), new(270f, 179.305f, .0297f)));
                    wallOrientations.Add(RunBasedHoloMap.SOUTH, new(new(.08f, -.18f, -2.02f), new(270f, 359.2266f, .0297f)));
                    wallOrientations.Add(RunBasedHoloMap.WEST, new(new(-3.2f, -.18f, -.4f), new(270f, 89.603f, .0297f)));
                    wallOrientations.Add(RunBasedHoloMap.EAST, new(new(3.2f, -.18f, -.4f), new(270f, 270.359f, .0297f)));
                    this.encounters = new string[] { "nature_battransformers", "nature_beartransformers", "nature_hounds" };
                    this.terrain = new string[][] {
                        new string[] { null, "Tree_Hologram", null, "Tree_Hologram", null, null, "Tree_Hologram", null, "Tree_Hologram", null },
                        new string[] { "Tree_Hologram_SnowCovered", null, "Tree_Hologram_SnowCovered", null, "Tree_Hologram_SnowCovered", "Tree_Hologram_SnowCovered", null, "Tree_Hologram_SnowCovered", null, "Tree_Hologram_SnowCovered" }
                    };
                    this.landmarks = new string[][] {
                        new string[] { "NatureMainPath_2/Scenery/HoloGateway" },
                        new string[] { "NatureEntrance/Scenery/HoloRock_3", "NatureEntrance/Scenery/HoloGenerator", "NatureEntrance/Scenery/HoloLamp" },
                        new string[] { "NatureEntrance/Scenery/HoloGenerator/Generator_Cylinder", "NatureSidePath/Scenery/Generator_Cylinder (1)", "NatureSidePath/Scenery/Generator_Cylinder (2)", "NatureSidePath/Scenery/Generator_Cylinder (3)" },
                        new string[] { "NatureEntrance/Scenery/HoloLamp", "NatureMainPath_4/Scenery/HoloLamp", "NatureMainPath_10/Scenery/HoloLamp"}
                    };
                    this.audioConfig = HoloMapArea.AudioLoopsConfig.Nature;
                    this.screenPrefab = Resources.Load<GameObject>("prefabs/map/holomapscreens/HoloMapScreen_WetlandsGround");
                    break;
                case RunBasedHoloMap.TECH:
                    this.encounters = new string[] { "tech_attackconduits", "tech_giftcells", "tech_splintercells" };
                    this.terrainRandoms = new string[] { };
                    this.objectRandoms = new string[] {  };
                    this.terrain = new string[][] {
                        new string[] { null, null, null, null, null, null, null, "ConduitTower", null, null},
                        new string[] { null, null, null, null, null, null, null, null, null, null }
                    };
                    this.lightColor = new Color(0.6934f, 0.9233f, 1f);
                    this.mainColor = new Color(0.4413f, 0.5221f, 0.5472f);
                    this.defaultReward = HoloMapNode.NodeDataType.BuildACard;
                    this.landmarks = new string[][] {
                        new string[] { "NeutralWestTechGate/Scenery/Generator_Turbine", "NeutralWestTechGate/Scenery/Generator_Turbine (1)" },
                        new string[] { "NeutralWestMain_2/Scenery/HoloPowerPoll_1", "NeutralWestMain_2/Scenery/HoloPowerPoll_1 (1)"},
                        new string[] { "Center/Scenery/HoloTeslaCoil", "Center/Scenery/HoloMeter", "Center/Scenery/TerrainHologram_AnnoyTower"},
                        new string[] { "TechElevatorTop/Scenery/HoloTechPillar", "TechElevatorTop/Scenery/HoloTechPillar (1)", "TechElevatorTop/Scenery/HoloTechPillar (2)", "TechElevatorTop/Scenery/HoloTechPillar (3)"},
                    };
                    this.wallPrefabs = new() {
                        [RunBasedHoloMap.WEST] = (new string[] { "TechEdge_W/Scenery/Railing"}),
                        [RunBasedHoloMap.NORTH] = (new string[] { "TechTower_NW/Scenery/HoloHandRail", "TechTower_NW/Scenery/HoloMapTurret", "TechTower_NW/Scenery/HoloMapTurret (1)"}),
                        [RunBasedHoloMap.EAST] = (new string[] { "TechEdge_E/Scenery/Railing"}),
                        [RunBasedHoloMap.SOUTH] = (new string[] { "TechTower_SW/Scenery/HoloHandRail", "TechTower_SW/Scenery/HoloMapTurret", "TechTower_SW/Scenery/HoloMapTurret (1)"})
                    };
                    this.audioConfig = HoloMapArea.AudioLoopsConfig.Tech;
                    this.screenPrefab = Resources.Load<GameObject>("prefabs/map/holomapscreens/HoloMapScreen_TechGround");
                    break;
                case RunBasedHoloMap.UNDEAD:
                    this.encounters = new string[] { "undead_bomblatchers", "undead_shieldlatchers", "undead_skeleswarm" };
                    this.terrainRandoms = new string[] { "UndeadMainPath_4/Scenery/HoloDebris", "UndeadMainPath_4/Scenery/HoloGrass_Patch", "UndeadMainPath_4/Scenery/HoloGrass_Patch", "UndeadMainPath_4/Scenery/HoloGrass_Patch" };
                    this.objectRandoms = new string[] { "UndeadMainPath_4/Scenery/HoloGravestone", "UndeadMainPath_4/Scenery/HoloTreeDead (2)", "UndeadSecretPath_1/Scenery/HoloShovel", "UndeadMainPath_4/Scenery/HoloDirtPile_2", "UndeadMainPath_4/Scenery/HoloZombieArm", "UndeadMainPath_4/Scenery/HoloTreeDead (2)" };
                    this.defaultReward = HoloMapNode.NodeDataType.OverclockCard;
                    this.lightColor = new Color(.1702f, .8019f, .644f);
                    this.mainColor = new Color(.0588f, .3608f, .3647f);
                    this.wall = null;
                    this.terrain = new string[][] {
                        new string[] { null, "DeadTree", null, null, null, null, null, null, null, "DeadTree" },
                        new string[] { null, null, null, null, null, null, "TombStone", null, "TombStone", null }
                    };
                    this.landmarks = new string[][] {
                        new string[] { "UndeadSmallDetour_2/Scenery/HoloTeslaCoil", "UndeadSmallDetour_2/Scenery/HoloTeslaCoil (1)", "UndeadSmallDetour_2/Scenery/HoloTeslaCoil (2)", "UndeadSmallDetour_2/Scenery/HoloTeslaCoil (3)", "UndeadSmallDetour_2/Scenery/HoloTeslaCoil (4)", "UndeadSmallDetour_2/Scenery/HoloMapLightning", "UndeadSmallDetour_2/Scenery/HoloMapLightning (2)", "UndeadSmallDetour_2/Scenery/HoloMapLightning (3)"},
                        new string[] { "TempleUndeadEntrance/Scenery/HoloCasket", "TempleUndeadEntrance/Scenery/HoloCasket (1)", "TempleUndeadEntrance/Scenery/HoloCasket (2)", "TempleUndeadEntrance/Scenery/HoloCasket (3)"},
                        new string[] { "TempleUndeadMain_1/Scenery/HoloLibrarianBot (1)", "TempleUndeadMain_1/Scenery/HoloLibrarianBot (2)"},
                        new string[] { "UndeadSmallDetour_2/Scenery/HoloGravestone", "UndeadSmallDetour_2/Scenery/HoloGravestone (1)", "UndeadSmallDetour_2/Scenery/HoloGravestone (2)", "UndeadSmallDetour_2/Scenery/HoloGravestone (4)"}
                    };
                    this.audioConfig = HoloMapArea.AudioLoopsConfig.Undead;
                    this.screenPrefab = Resources.Load<GameObject>("prefabs/map/holomapscreens/HoloMapScreen_WetlandsGround");
                    break;
                default:
                    break;
            }
        }
    }
}