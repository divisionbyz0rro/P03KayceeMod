using HarmonyLib;
using DiskCardGame;
using System.Collections.Generic;
using System.Linq;
using InscryptionAPI.Saves;
using System;
using Infiniscryption.P03KayceeRun.Sequences;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static partial class RunBasedHoloMap
    {
        private static readonly int[][] NSEW = new int[][] { new int[]{ 0, -1 }, new int[]{0, 1}, new int[]{1, 0}, new int[]{-1, 0}};

        private static IEnumerable<HoloMapBlueprint> AdjacentToQuadrant(this HoloMapBlueprint[,] map, int x, int y)
        {
            int minX = x <= 2 ? 0 : 3;
            int minY = y <= 2 ? 0 : 3;
            int maxX = x <= 2 ? 2 : 5;
            int maxY = y <= 2 ? 2 : 5;

            return NSEW.Where(p => x + p[0] >= minX &&
                                   y + p[1] >= minY &&
                                   x + p[0] <= maxX &&
                                   y + p[1] <= maxY)
                       .Select(p => map[x + p[0], y + p[1]]);
        }

        private static int OppositeDirection(int direction)
        {
            int retval = 0;
            if ((direction & NORTH) != 0)
                retval |= SOUTH;
            if ((direction & SOUTH) != 0)
                retval |= NORTH;
            if ((direction & EAST) != 0)
                retval |= WEST;
            if ((direction & WEST) != 0)
                retval |= EAST;
            return retval;
        }

        private static IEnumerable<HoloMapBlueprint> AdjacentToQuadrant(this HoloMapBlueprint[,] map, HoloMapBlueprint node)
        {
            return map.AdjacentToQuadrant(node.x, node.y);
        }
        
        private static IEnumerable<HoloMapBlueprint> AdjacentTo(this HoloMapBlueprint[,] map, int x, int y)
        {
            return NSEW.Where(p => x + p[0] >= 0 &&
                                   y + p[1] >= 0 &&
                                   x + p[0] < map.GetLength(0) &&
                                   y + p[1] < map.GetLength(1))
                       .Select(p => map[x + p[0], y + p[1]]);
        }

        private static Tuple<int, int> GetAdjacentLocation(this HoloMapBlueprint node, int direction)
        {
            int x = direction == WEST ? node.x - 1 : direction == EAST ? node.x + 1 : node.x;
            int y = direction == NORTH ? node.y - 1 : direction == SOUTH ? node.y + 1 : node.y;
            if (x <= 0 || x >= 6 || y <= 0 || y >= 6)
                return null;
            return new(x, y);
        }

        private static HoloMapBlueprint GetAdjacentNode(this HoloMapBlueprint node, HoloMapBlueprint[,] map, int direction)
        {
            int x = direction == WEST ? node.x - 1 : direction == EAST ? node.x + 1 : node.x;
            int y = direction == NORTH ? node.y - 1 : direction == SOUTH ? node.y + 1 : node.y;
            if (x < 0 || y < 0 || x >= map.GetLength(0) || y >= map.GetLength(1))
                return null;

            return map[x,y];
        }

        private static List<HoloMapBlueprint> GetPointOfInterestNodes(this List<HoloMapBlueprint> nodes, Func<HoloMapBlueprint, bool> filter = null)
        {
            Func<HoloMapBlueprint, bool> activeFilter = (filter == null) ? ((HoloMapBlueprint i) => true) : filter;
            List<HoloMapBlueprint> deadEndPOI = nodes.Where(activeFilter).Where(bp => bp.IsDeadEnd && bp.EligibleForUpgrade).ToList();
            if (deadEndPOI.Count > 0)
                return deadEndPOI;
            else
                return nodes.Where(activeFilter).Where(bp => bp.EligibleForUpgrade).ToList();
        }

        private static HoloMapBlueprint GetRandomPointOfInterest(this List<HoloMapBlueprint> nodes, Func<HoloMapBlueprint, bool> filter = null, int randomSeed = -1)
        {
            if (randomSeed != -1)
                UnityEngine.Random.InitState(randomSeed);

            List<HoloMapBlueprint> possibles = nodes.GetPointOfInterestNodes(filter: filter);
            return possibles.Count == 0 ? null : possibles[UnityEngine.Random.Range(0, possibles.Count)];
        }

        private static IEnumerable<HoloMapBlueprint> AdjacentTo(this HoloMapBlueprint[,] map, HoloMapBlueprint node)
        {
            return map.AdjacentTo(node.x, node.y);
        }

        // private static void PaintQuadrant(HoloMapBlueprint[,] map, int x, int y, int color)
        // {
        //     if (map[x, y] == null)
        //     {
        //         foreach (HoloMapBlueprint adjNode in map.AdjacentTo(x, y))
        //         {
        //             if (adjNode != null)
        //             {
        //                 PaintQuadrant(map, adjNode.x, adjNode.y, color);
        //                 return;
        //             }
        //         }
        //     }

        //     // Staying within the given quadrant, paint all adjacent nodes the same color as you
        //     map[x, y].color = color;

        //     foreach (HoloMapBlueprint adjNode in map.AdjacentToQuadrant(x, y))
        //         if (adjNode != null && adjNode.color == 0)
        //             PaintQuadrant(map, adjNode.x, adjNode.y, color);
        // }

        // private static void FixPaint(HoloMapBlueprint[,] map, int x, int y)
        // {
        //     if (map[x,y] != null && map[x,y].color <= 0)
        //     {
        //         foreach (HoloMapBlueprint adj in map.AdjacentTo(x, y))
        //         {
        //             if (adj != null && adj.color > 0)
        //             {
        //                 map[x,y].color = adj.color;
        //                 return;
        //             }
        //         }
        //     }
        // }

        private static int DirTo(this HoloMapBlueprint start, HoloMapBlueprint end)
        {
            int retval = BLANK;
            retval = retval | (start.x == end.x ? 0 : start.x < end.x ? EAST : WEST);
            retval = retval | (start.y == end.y ? 0 : start.y < end.y ? SOUTH : NORTH);
            return retval;
        }

        private static void CrawlQuadrant(HoloMapBlueprint[,] map, int color)
        {
            List<HoloMapBlueprint> possibles = new();
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++)
                    if (map[i,j] != null && map[i,j].color == color)
                        possibles.Add(map[i, j]);

            HoloMapBlueprint startNode = possibles[UnityEngine.Random.Range(0, possibles.Count)];
            CrawlQuadrant(map, startNode.x, startNode.y);
        }

        private static void CrawlQuadrant(HoloMapBlueprint[,] map, int x, int y)
        {
            // Find all adjacent uncrawled nodes
            List<HoloMapBlueprint> uncrawled = map.AdjacentTo(x, y)
                                                  .Where(bp => bp != null)
                                                  .Where(bp => map[x,y].color == bp.color)
                                                  .Where(bp => bp.arrowDirections == BLANK)
                                                  .ToList();

            if (uncrawled.Count == 0)
                return;            

            // Pick a random adjacent uncrawled node
            HoloMapBlueprint current = map[x,y];
            HoloMapBlueprint next = uncrawled[UnityEngine.Random.Range(0, uncrawled.Count)];
            current.arrowDirections = current.arrowDirections | current.DirTo(next);
            next.arrowDirections = next.arrowDirections | next.DirTo(current);

            CrawlQuadrant(map, next.x, next.y);
            
            // double check this one again
            CrawlQuadrant(map, x, y);
        }

        private static void ConnectQuadrants(HoloMapBlueprint[,] map, int region)
        {
            // This is too hard to generalize, although maybe I'll come up with a way to do it?
            int v = region == MAGIC ? 3 : 2;
            
            for (int i = 2; i >= 0; i--)
            {
                if (map[i, v] != null && map[i, v+1] != null)
                {
                    map[i, v].arrowDirections = map[i, v].arrowDirections | SOUTH;
                    map[i, v+1].arrowDirections = map[i, v+1].arrowDirections | NORTH;
                    break;
                }
            }

            for (int i = 3; i <= 5; i++)
            {
                if (map[i, v] != null && map[i, v+1] != null)
                {
                    map[i, v].arrowDirections = map[i, v].arrowDirections | SOUTH;
                    map[i, v+1].arrowDirections = map[i, v+1].arrowDirections | NORTH;
                    break;
                }
            }

            for (int j = v; j >= 0; j--)
            {
                if (map[2, j] != null && map[3, j] != null)
                {
                    map[2, j].arrowDirections = map[2, j].arrowDirections | EAST;
                    map[3, j].arrowDirections = map[3, j].arrowDirections | WEST;
                    break;
                }
            }

            for (int j = v+1; j <= 5; j++)
            {
                if (map[2, j] != null && map[3, j] != null)
                {
                    map[2, j].arrowDirections = map[2, j].arrowDirections | EAST;
                    map[3, j].arrowDirections = map[3, j].arrowDirections | WEST;
                    break;
                }
            }
        }

        private static void DiscoverAndTrimDeadEnds(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes)
        {
            // Look for all 'dead ends' - that is, nodes where you can only move in one direction
            // The goal of this is to find pointless hallways; that is, a path that simply leads to a dead end with nothing interesting.
            // There's no need to have to walk through a hallway just to get to a dead end
            // This trims those by removing the dead end and turning the hallway into the dead end.

            // It should make the maps feel smaller. Which is good, actually. There's not a lot to do on these maps.

            // We ignore the first node, because that's the starting node. And we can't risk killing the starting node
            List<HoloMapBlueprint> possibles = nodes.Where(bp => bp.NumberOfArrows == 1 && bp != nodes[0]).ToList();
            int i = 0;
            foreach (HoloMapBlueprint deadEnd in possibles)
            {
                HoloMapBlueprint adjacent = deadEnd.GetAdjacentNode(map, deadEnd.arrowDirections);
                
                // If the node leading into a dead end only has two directions
                // And the color of the dead end has more than two nodes
                // We kill the dead end and make the hall leading into it into a dead end
                if (adjacent.NumberOfArrows == 2 && nodes.Where(bp => bp.color == deadEnd.color).Count() > 2)
                {
                    // Kill the arrow going into the dead end node
                    // Right, so, the arrow going to the dead end will be the opposite direction of the arrow leaving the dead end
                    // We AND with the complement to get rid of it
                    adjacent.arrowDirections &= ~OppositeDirection(deadEnd.arrowDirections);

                    // Now just delete the node
                    map[deadEnd.x, deadEnd.y] = null;
                    nodes.Remove(deadEnd);
                }

                i++;
                if (i >= 2)
                    break;
            }
        }

        private static void DiscoverAndCreateLandmarks(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes)
        {
            for (int c = 1; c <= 4; c++)
            {
                List<HoloMapBlueprint> possibles = nodes.Where(bp => bp.color == c && bp.NumberOfArrows >= 3).ToList();
                if (possibles.Count == 0)
                    possibles = nodes.Where(bp => bp.color == c && bp.NumberOfArrows == 2).ToList();
                if (possibles.Count == 0)
                    possibles = nodes.Where(bp => bp.color == c).ToList();

                HoloMapBlueprint landmarkNode = possibles[UnityEngine.Random.Range(0, possibles.Count)];
                landmarkNode.specialTerrain |= HoloMapBlueprint.LANDMARKER;
            }
        }

        private static void DiscoverAndCreateBridge(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes, int region)
        {
            if (region == NATURE)
                return; // Nature doesn't have bridges

            // This is a goofy one. We're looking for a section on the map where the area could be a bridge.
            // If so, roll the dice and make a bridge
            float bridgeOdds = 0.95f;
            List<HoloMapBlueprint> bridgeNodes = nodes.Where(bp => bp.arrowDirections == (EAST | WEST)).ToList();
            while (bridgeNodes.Count > 0 && bridgeOdds > 0f)
            {
                HoloMapBlueprint bridge = bridgeNodes[UnityEngine.Random.Range(0, bridgeNodes.Count)];
                if (UnityEngine.Random.value < bridgeOdds)
                {
                    bridge.specialTerrain |= HoloMapBlueprint.FULL_BRIDGE;
                    map[bridge.x-1, bridge.y].specialTerrain |= HoloMapBlueprint.LEFT_BRIDGE;
                    map[bridge.x+1, bridge.y].specialTerrain |= HoloMapBlueprint.RIGHT_BRIDGE;
                    bridgeOdds -= 0.25f;
                }
                bridgeNodes.Remove(bridge);
            }
        }

        private static bool DiscoverAndCreateTrade(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes)
        {
            // The goal here is to find battles that exist in hallways; that is, battles where the room that
            // are in when you click the arrow to fight only has one way in.
            // Then we make the entrace to THAT room force you to trade your way in

            // So again: the room must have a 'special' arrow, the type must be enemy, and there can be only two arrows
            // One would be the special direction, and one would be the path backward
            List<HoloMapBlueprint> possibles = nodes.Where(bp => bp.specialDirection != 0 && bp.specialDirectionType == 0 && bp.NumberOfArrows == 2).ToList();

            if (possibles.Count == 0)
                return false; // This means we couldn't find a spot for the

            HoloMapBlueprint battleNode = possibles[UnityEngine.Random.Range(0, possibles.Count)];

            // Find the way back. It's the arrow directions with the special direction removed
            int directionBack = battleNode.arrowDirections & ~battleNode.specialDirection;
            
            // Find the node in that direction
            HoloMapBlueprint prevNode = GetAdjacentNode(battleNode, map, directionBack);

            // Set the special direction and special type
            prevNode.specialDirection = OppositeDirection(directionBack);
            prevNode.specialDirectionType = HoloMapBlueprint.TRADE;

            return true;
        }

        private static bool ForceTrade(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes)
        {
            // This gets called when we couldn't naturally fit a trade in the map
            // Here, we pick a random encounter from the map and move it to make room for a trade
            List<HoloMapBlueprint> possibles = nodes.Where(bp => bp.specialDirection != 0 && bp.specialDirectionType == 0 && bp.color != 1).ToList();

            while (possibles.Count > 0)
            {
                HoloMapBlueprint oldBattleNode = possibles[UnityEngine.Random.Range(0, possibles.Count)];
                possibles.Remove(oldBattleNode);

                // Find the node that the battle leads you to
                HoloMapBlueprint newBattleNode = GetAdjacentNode(oldBattleNode, map, oldBattleNode.specialDirection);

                // Find a location in any direction that is null
                foreach (int dir in DIR_LOOKUP.Keys)
                {
                    Tuple<int, int> xy = GetAdjacentLocation(newBattleNode, dir);

                    if (xy == null)
                        continue;

                    if (map[xy.Item1, xy.Item2] == null)
                    {
                        // Good. We found a spot
                        HoloMapBlueprint brandNewNode = new(newBattleNode.randomSeed + 1000);
                        brandNewNode.x = xy.Item1;
                        brandNewNode.y = xy.Item2;

                        nodes.Add(brandNewNode);
                        map[xy.Item1, xy.Item2] = brandNewNode;

                        // Give the new node the upgrade that was hiding behind the battle
                        brandNewNode.upgrade = newBattleNode.upgrade;
                        newBattleNode.upgrade = HoloMapNode.NodeDataType.MoveArea;

                        // Make all arrows match up
                        brandNewNode.arrowDirections = brandNewNode.DirTo(newBattleNode);
                        newBattleNode.arrowDirections |= newBattleNode.DirTo(brandNewNode);
                        newBattleNode.specialDirection = newBattleNode.DirTo(brandNewNode);

                        // Make the old battle node into trade node
                        oldBattleNode.specialDirectionType = HoloMapBlueprint.TRADE;
                        
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool DiscoverAndCreateEnemyEncounter(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes, int tier, int region, HoloMapSpecialNode.NodeDataType reward, int color = -1)
        {
            // The goal here is to find four rooms that have only one entrance
            // Then back out to the first spot that doesn't have a choice
            // Then put an enemy encounter there
            // And put something of interest in the 

            HoloMapBlueprint enemyNode = null;
            HoloMapBlueprint rewardNode = null;
            if (color == nodes[0].color && nodes[0].NumberOfArrows == 1) // This bit only works if there's only one way out of the starting node
            {
                // If this is the region you start in, we do the work a little bit differently.
                // We walk until we find the first node with a choice
                enemyNode = nodes[0];
                rewardNode = enemyNode.GetAdjacentNode(map, enemyNode.arrowDirections);
                for (int i = 0; i < 3; i++)
                {
                    if (rewardNode.NumberOfArrows == 2)
                    {
                        int dirToEnemyNode = DirTo(rewardNode, enemyNode);
                        int dirToNextRewardNode = rewardNode.arrowDirections & ~dirToEnemyNode;
                        HoloMapBlueprint nextRewardNode = rewardNode.GetAdjacentNode(map, dirToNextRewardNode);
                        enemyNode = rewardNode;
                        rewardNode = nextRewardNode;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                rewardNode = nodes.GetRandomPointOfInterest(bp => (bp.color == color || color == -1) && bp.IsDeadEnd);
            }

            if (rewardNode != null)
            {
                enemyNode = enemyNode ?? rewardNode.GetAdjacentNode(map, rewardNode.arrowDirections);
                enemyNode.specialDirection = DirTo(enemyNode, rewardNode);
                enemyNode.encounterDifficulty = EventManagement.EncounterDifficulty;

                // 50% change of terrain
                if (UnityEngine.Random.value < 0.5f)
                    enemyNode.battleTerrainIndex = UnityEngine.Random.Range(0, REGION_DATA[region].terrain.Length) + 1;

                rewardNode.upgrade = reward;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void DiscoverAndCreateCanvasBoss(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes)
        {
            // We need two spaces vertically to make this work.
            HoloMapBlueprint bossIntroRoom = null;
            for (int j = map.GetLength(1) - 1; j >= 2; j--)
            {
                for(int i = map.GetLength(0) - 1; i >= 0; i--)
                {
                    if (map[i,j] != null && map[i,j-1] == null && map[i,j-2] == null)
                    {
                        bossIntroRoom = map[i,j];
                        break;
                    }
                }
                if (bossIntroRoom != null)
                    break;
            }

            // Now we've found the space
            bossIntroRoom.specialTerrain |= HoloMapBlueprint.NORTH_BUILDING_ENTRANCE;
            bossIntroRoom.arrowDirections |= NORTH;
            bossIntroRoom.blockedDirections |= NORTH;
            bossIntroRoom.blockEvent = EventManagement.ALL_ZONE_ENEMIES_KILLED;

            // We need to create the special lower tower room
            HoloMapBlueprint lowerTowerRoom = new(0);
            lowerTowerRoom.specialTerrain = HoloMapBlueprint.LOWER_TOWER_ROOM;
            lowerTowerRoom.arrowDirections = NORTH | SOUTH;
            lowerTowerRoom.x = bossIntroRoom.x;
            lowerTowerRoom.y = bossIntroRoom.y - 1;
            lowerTowerRoom.color = bossIntroRoom.color;
            map[lowerTowerRoom.x, lowerTowerRoom.y] = lowerTowerRoom;
            nodes.Add(lowerTowerRoom);

            HoloMapBlueprint bossRoom = new(0);
            bossRoom.opponent = Opponent.Type.CanvasBoss;
            bossRoom.arrowDirections = SOUTH;
            bossRoom.x = bossIntroRoom.x;
            bossRoom.y = bossIntroRoom.y - 2;
            bossRoom.color = bossIntroRoom.color;
            map[bossRoom.x, bossRoom.y] = bossRoom;
            nodes.Add(bossRoom);

        }

        private static void DiscoverAndCreateBossRoom(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes, int region)
        {
            if (region == NEUTRAL)
                return;

            if (region == MAGIC)
            {
                DiscoverAndCreateCanvasBoss(map, nodes);
                return;
            }

            // We need a room that has a blank room above it
            // And does not have the same color as the starting room
            List<HoloMapBlueprint> bossPossibles = nodes.Where(bp => bp.y >= 1 && map[bp.x, bp.y - 1] == null && bp.color != nodes[0].color).ToList();
            HoloMapBlueprint bossIntroRoom = bossPossibles[UnityEngine.Random.Range(0, bossPossibles.Count)];
            bossIntroRoom.specialTerrain |= (region == NATURE ? HoloMapBlueprint.NORTH_CABIN : HoloMapBlueprint.NORTH_BUILDING_ENTRANCE);
            bossIntroRoom.arrowDirections |= NORTH;
            bossIntroRoom.blockedDirections |= NORTH;
            bossIntroRoom.blockEvent = EventManagement.ALL_ZONE_ENEMIES_KILLED;

            HoloMapBlueprint bossRoom = new(bossIntroRoom.randomSeed + 200 * bossIntroRoom.x);
            bossRoom.x = bossIntroRoom.x;
            bossRoom.y = bossIntroRoom.y - 1;
            bossRoom.opponent = (region == UNDEAD) ? Opponent.Type.ArchivistBoss : (region == NATURE ? Opponent.Type.PhotographerBoss : Opponent.Type.TelegrapherBoss);
            bossRoom.arrowDirections |= SOUTH;
            bossRoom.color = bossIntroRoom.color;

            map[bossRoom.x, bossRoom.y] = bossRoom;
            nodes.Add(bossRoom);
        }

        private static List<HoloMapBlueprint> BuildHubBlueprint(int seed)
        {
            List<HoloMapBlueprint> retval = new();
            retval.Add(new(seed) { upgrade = HoloMapWaypointNode.NodeDataType.FastTravel, x=0, y=1, arrowDirections = NORTH });
            retval.Add(new(seed) { opponent = Opponent.Type.P03Boss, x=0, y=0, arrowDirections = SOUTH });

            return retval;
        }

        private static void LogBlueprint(HoloMapBlueprint[,] bpBlueprint)
        {
            // Log to the file for debug purposes
            for (int j = 0; j < bpBlueprint.GetLength(1); j++)
            {
                List<string> lines = new() { "", "", "", "", ""};
                for (int i = 0; i < bpBlueprint.GetLength(0); i++)
                    for (int s = 0; s < lines.Count; s++)
                        lines[s] += bpBlueprint[i, j] == null ? "     " : bpBlueprint[i, j].DebugString[s];
                for (int s = 0; s < lines.Count; s++)
                    P03Plugin.Log.LogInfo(lines[s]);
            }  
        }

        public static void ShapeMapForRegion(HoloMapBlueprint[,] bpBlueprint, int region)
        {
            int x, y;
            if (region == TECH)
            {
                // Set the corners empty
                bpBlueprint[0,0] = bpBlueprint[0,1] = bpBlueprint[1,0] = null;
                bpBlueprint[0,4] = bpBlueprint[0,5] = bpBlueprint[1,5] = null;
                bpBlueprint[4,0] = bpBlueprint[5,0] = bpBlueprint[5,1] = null;
                bpBlueprint[4,5] = bpBlueprint[5,5] = bpBlueprint[5,4] = null;
                
                // Randomly chop some rooms
                // Chop one of the middle rooms
                bpBlueprint[UnityEngine.Random.Range(2,4), UnityEngine.Random.Range(2,4)] = null;
                
                // Randomly chop a corner
                int[] corners = new int[] { 1, 4 };
                bpBlueprint[corners[UnityEngine.Random.Range(0, 2)], corners[UnityEngine.Random.Range(0, 2)]] = null;

                // Randomly chop an interior side
                x = UnityEngine.Random.Range(1, 5);
                y = x == 1 || x == 4 ? UnityEngine.Random.Range(2, 4) : corners[UnityEngine.Random.Range(0, 2)];
                bpBlueprint[x, y] = null;
            }
            if (region == MAGIC)
            {
                // Take off the entire top two rows
                for (int i = 0; i < 6; i++)
                {
                    bpBlueprint[i, 0] = null;

                    if (i != 2 && i != 3)
                        bpBlueprint[i, 1] = null;
                }

                // Take out one of the middle two segments
                x = UnityEngine.Random.value < 0.5f ? 2 : 3;
                bpBlueprint[x, 3] = bpBlueprint[x, 4] = null;

                // Take out a corner
                x = UnityEngine.Random.value < 0.5f ? 0 : 5;
                y = x == 0 ? 5 : UnityEngine.Random.value < 0.5f ? 2 : 5;
                bpBlueprint[x, y] = null;
            }
            if (region == NATURE)
            {
                int offset = UnityEngine.Random.Range(0, 4);
                for (int i = 0; i < 3; i++)
                {
                    bpBlueprint[i + offset, 0] = null;
                    if (i == 1)
                    {
                        bpBlueprint[i + offset, 1] = null;
                        if (UnityEngine.Random.value < 0.5f)
                            bpBlueprint[i + offset - 1, 1] = null;
                        else
                            bpBlueprint[i + offset + 1, 1] = null;
                    }
                }
                if (offset <= 1)
                    bpBlueprint[0, 5] = null;
                else
                    bpBlueprint[0, 0] = null;

                offset = offset <= 1 ? 2 : 0;
                offset = UnityEngine.Random.value < 0.5f ? offset : offset + 1;

                for (int i = 0; i < 3; i++)
                {
                    bpBlueprint[i + offset, 5] = null;
                    if (i == 1)
                    {
                        bpBlueprint[i + offset, 4] = null;
                        if (UnityEngine.Random.value < 0.5f)
                            bpBlueprint[i + offset - 1, 4] = null;
                        else
                            bpBlueprint[i + offset + 1, 4] = null;
                    }
                }
                if (offset <= 1)
                    bpBlueprint[5, 5] = null;
                else
                    bpBlueprint[5, 0] = null;                
            }
            if (region == UNDEAD)
            {
                bool pointUp = UnityEngine.Random.value < 0.5f;

                int[] ys = pointUp ? new int[] { 5, 4, 1, 0 } : new int[] { 0, 1, 4, 5 }; 

                bpBlueprint[1, ys[0]] = bpBlueprint[2, ys[0]] = bpBlueprint[3, ys[0]] = bpBlueprint[4, ys[0]] =  null;
                bpBlueprint[2, ys[1]] = bpBlueprint[3, ys[1]] = null;
                bpBlueprint[0, ys[2]] = bpBlueprint[5, ys[2]] = null;
                bpBlueprint[0, ys[3]] = bpBlueprint[1, ys[3]] = bpBlueprint[4, ys[3]] = bpBlueprint[5, ys[3]] = null;

                bpBlueprint[UnityEngine.Random.Range(2, 4), pointUp ? 2 : 3] = null;
            }

            int v = region == MAGIC ? 4 : 3;
            for (int i = 0; i < bpBlueprint.GetLength(0); i++)
                for (int j = 0; j < bpBlueprint.GetLength(1); j++)
                    if (bpBlueprint[i,j] != null)
                        bpBlueprint[i,j].color = i < 3 ? j < v ? 1 : 2 : j < v ? 3 : 4;
        }

        private static List<HoloMapBlueprint> BuildBlueprint(int order, int region, int seed, int stackDepth = 0)
        {
            string blueprintKey = $"ascensionBlueprint{order}{region}";
            string savedBlueprint = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, blueprintKey);

            if (savedBlueprint != default(string))
                return savedBlueprint.Split('|').Select(s => new HoloMapBlueprint(s)).ToList();

            if (region == NEUTRAL)
                return BuildHubBlueprint(seed);

            UnityEngine.Random.InitState(seed);

            // Start with a 6x6 grid
            HoloMapBlueprint[,] bpBlueprint = new HoloMapBlueprint[6,6];
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j ++)
                    bpBlueprint[i, j] = new HoloMapBlueprint(seed + 10*i + 100*j) { x = i, y = j, arrowDirections = BLANK };

            // Reshape for region
            ShapeMapForRegion(bpBlueprint, region);

            // Crawl and mark each quadrant.
            for (int i = 1; i <= 4; i++)
                CrawlQuadrant(bpBlueprint, i);

            // Set up the connections between quadrants
            ConnectQuadrants(bpBlueprint, region);

            // Figure out the starting space
            HoloMapBlueprint startSpace = bpBlueprint[0, 2];
            List<HoloMapBlueprint> retval = new() { startSpace };
            for (int i = 0; i < bpBlueprint.GetLength(0); i++)
                for (int j = 0; j < bpBlueprint.GetLength(1); j ++)
                    if (bpBlueprint[i, j] != null && bpBlueprint[i, j] != startSpace)
                        retval.Add(bpBlueprint[i, j]);

            // Make sure that the tech zone adds the conduit to the side deck
            //if (region == TECH)
            startSpace.upgrade = TradeChipsNodeData.TradeChipsForCards;
            
            // Do some special sequencing
            DiscoverAndCreateLandmarks(bpBlueprint, retval);
            DiscoverAndCreateBossRoom(bpBlueprint, retval, region);
            DiscoverAndCreateBridge(bpBlueprint, retval, region);


            // Add four enemy encounters and rewards
            int seedForChoice = seed * 2 + 10;

            List<int> colorsWithoutEnemies = new() { 1, 2, 3, 4 };
            int numberOfEncountersAdded = 0;
            while (colorsWithoutEnemies.Count > 0)
            {
                UnityEngine.Random.InitState(seedForChoice + colorsWithoutEnemies.Count * 1000);
                int colorToUse = colorsWithoutEnemies[UnityEngine.Random.Range(0, colorsWithoutEnemies.Count)];
                HoloMapSpecialNode.NodeDataType type = colorsWithoutEnemies.Count <= 2 ? HoloMapSpecialNode.NodeDataType.AddCardAbility : REGION_DATA[region].defaultReward;
                if (DiscoverAndCreateEnemyEncounter(bpBlueprint, retval, order, region, type, colorToUse))
                    numberOfEncountersAdded += 1;
                colorsWithoutEnemies.Remove(colorToUse);
            }

            int remainingEncountersToAdd = EventManagement.ENEMIES_TO_UNLOCK_BOSS - numberOfEncountersAdded;
            for (int i = 0; i < remainingEncountersToAdd; i++)
                if (DiscoverAndCreateEnemyEncounter(bpBlueprint, retval, order, region, REGION_DATA[region].defaultReward))
                    numberOfEncountersAdded += 1;

            P03Plugin.Log.LogInfo($"I have created {numberOfEncountersAdded} enemy encounters");

            // Add one trade node
            bool traded = DiscoverAndCreateTrade(bpBlueprint, retval);
            P03Plugin.Log.LogInfo($"Created a trade node? {traded}");

            if (!traded)
            {
                traded = ForceTrade(bpBlueprint, retval);
                P03Plugin.Log.LogInfo($"Forcing a trade. Successful? {traded}");
            }

            // Add four card choice nodes
            P03Plugin.Log.LogInfo($"Adding upgrades");
            int cardChoiceNodes = 0;
            for (int i = 1; i < 5; i++) // one for each color 1-4
            {
                HoloMapBlueprint node = retval.GetRandomPointOfInterest(bp => bp.color == i);
                if (node != null)
                {
                    node.upgrade = HoloMapSpecialNode.NodeDataType.CardChoice;
                    cardChoiceNodes += 1;
                }
            }
            for (int i = cardChoiceNodes; i < 4; i++) // Just in case we couldn't find a valid point of interest in every quadrant
                retval.GetRandomPointOfInterest().upgrade = HoloMapSpecialNode.NodeDataType.CardChoice;

            // And now we're just going to add one more regional upgrade
            retval.GetRandomPointOfInterest().upgrade = REGION_DATA[region].defaultReward;

            for (int i = 0; i < 2; i++)
                retval.GetRandomPointOfInterest().upgrade = UnlockAscensionItemNodeData.UnlockItemsAscension;

            retval.GetRandomPointOfInterest().upgrade = AscensionRecycleCardNodeData.AscensionRecycleCard;

            // Add two hidden currency nodes
            P03Plugin.Log.LogInfo($"Adding hidden currency nodes");
            for (int i = 0; i < 2; i++)
            {
                HoloMapBlueprint tbp2 = retval.GetRandomPointOfInterest();
                if (tbp2 != null)
                    tbp2.upgrade = HoloMapSpecialNode.NodeDataType.GainCurrency;
            }

            // Add one of each of the default upgrades for each completed zone
            foreach (int cRegion in CompletedRegions)
            {
                HoloMapBlueprint tbp2 = retval.GetRandomPointOfInterest();
                if (tbp2 != null)
                    tbp2.upgrade = REGION_DATA[cRegion].defaultReward;
            }

            LogBlueprint(bpBlueprint);

            if (!IsBlueprintValid(retval))
            {
                if (stackDepth == 500)
                    throw new InvalidOperationException("Could not generate a valid map after 500 attempts - something has gone horribly wrong!");
                retval = BuildBlueprint(order, region, seed + 25, stackDepth + 1);
            }

            savedBlueprint = string.Join("|", retval.Select(b => b.ToString()));
            ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, blueprintKey, savedBlueprint);
            SaveManager.SaveToFile();
            return retval;
        }

        public static void TravelMap(HoloMapBlueprint current, List<HoloMapBlueprint> map)
        {
            for (int idx = 0; idx < 4; idx++)
            {
                int dir = idx == 0 ? NORTH : idx == 1 ? SOUTH : idx == 2 ? EAST : WEST;
                int xDelta = NSEW[idx][0];
                int yDelta = NSEW[idx][1];
                if ((current.arrowDirections & dir) != 0)
                {
                    HoloMapBlueprint node = map.FirstOrDefault(b => b.x == current.x + xDelta && b.y == current.y + yDelta);
                    if (node != null)
                    {
                        map.Remove(node);
                        TravelMap(node, map);
                    }
                }
            }
        }

        public static bool IsBlueprintValid(List<HoloMapBlueprint> blueprint)
        {
            // Make sure we can travel the entire map
            List<HoloMapBlueprint> bpCopy = new(blueprint);
            TravelMap(bpCopy[0], bpCopy);

            if (bpCopy.Count > 0)
            {
                P03Plugin.Log.LogInfo($"Map failed validation - could not visit entire map from start. {bpCopy.Count} nodes remaining");
                return false;
            }

            // Make sure there is a boss node
            if (!blueprint.Any(bp => bp.opponent != Opponent.Type.Default))
            {
                P03Plugin.Log.LogInfo("Map failed validation - no boss");
                return false;
            }

            // Make sure there are four enemy nodes
            if (blueprint.Where(bp => bp.specialDirection != 0 && bp.specialDirectionType == HoloMapBlueprint.BATTLE).Count() < 4)
            {
                P03Plugin.Log.LogInfo("Map failed validation - not enough enemy encounters");
                return false;
            }

            return true;
        }
    }
}