using System.Collections.Generic;
using DiskCardGame;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    public class HoloMapBlueprint
    {
        public static readonly int NO_SPECIAL = 0;
        public static readonly int LEFT_BRIDGE = 1;
        public static readonly int RIGHT_BRIDGE = 2;
        public static readonly int FULL_BRIDGE = 4;
        public static readonly int NORTH_BUILDING_ENTRANCE = 8;
        public static readonly int NORTH_GATEWAY = 16;
        public static readonly int NORTH_CABIN = 32;
        public static readonly int LOWER_TOWER_ROOM = 64;
        public static readonly int LANDMARKER = 128;
        public static readonly int BROKEN_GENERATOR = 256;
        public static readonly int MYCOLOGIST_WELL = 512;

        public static readonly int BATTLE = 0;
        public static readonly int TRADE = 1;

        public int randomSeed;
        public int x;
        public int y;
        public int arrowDirections;
        public int specialDirection;
        public int secretDirection;
        public int specialDirectionType;
        public Opponent.Type opponent;
        public HoloMapNode.NodeDataType upgrade;
        public int specialTerrain;
        public int blockedDirections;
        public StoryEvent blockEvent;
        public int battleTerrainIndex;
        public int encounterDifficulty;
        public bool isSecretRoom;

        public EventManagement.SpecialEvent dialogueEvent;

        public int distance; // used only for generation - doesn't get saved or parsed
        public int color;

        public override string ToString()
        {
            return $"[{randomSeed},{x},{y},{arrowDirections},{specialDirection},{specialDirectionType},{encounterDifficulty},{(int)opponent},{(int)upgrade},{specialTerrain},{blockedDirections},{(int)blockEvent},{battleTerrainIndex},{color},{(int)dialogueEvent},{secretDirection},{isSecretRoom}]";
        }

        public HoloMapBlueprint(int randomSeed) { this.randomSeed = randomSeed; this.upgrade = HoloMapSpecialNode.NodeDataType.MoveArea; }

        public HoloMapBlueprint(string parsed)
        {
            string[] split = parsed.Replace("[", "").Replace("]", "").Split(',');
            this.randomSeed = int.Parse(split[0]);
            x = int.Parse(split[1]);
            y = int.Parse(split[2]);
            arrowDirections = int.Parse(split[3]);
            specialDirection = int.Parse(split[4]);
            specialDirectionType = int.Parse(split[5]);
            encounterDifficulty = int.Parse(split[6]);
            opponent = (Opponent.Type)int.Parse(split[7]);
            upgrade = (HoloMapSpecialNode.NodeDataType)int.Parse(split[8]);
            specialTerrain = int.Parse(split[9]);
            blockedDirections = int.Parse(split[10]);
            blockEvent = (StoryEvent)int.Parse(split[11]);
            battleTerrainIndex = int.Parse(split[12]);
            color = int.Parse(split[13]);

            // From this point forward, extensions to the blueprint HAVE TO CHECK AND HAVE DEFAULTS
            // because we have to be backwards compatible
            dialogueEvent = (EventManagement.SpecialEvent)(split.Length > 14 ? int.Parse(split[14]) : 0);
            secretDirection = split.Length > 15 ? int.Parse(split[15]) : 0;
            isSecretRoom = split.Length > 16 ? bool.Parse(split[16]) : false;
        }

        public bool EligibleForUpgrade
        {
            get
            {
                return this.opponent == Opponent.Type.Default && this.upgrade == HoloMapNode.NodeDataType.MoveArea && (this.specialTerrain & LANDMARKER) == 0;
            }
        }

        public bool EligibleForDialogue
        {
            get
            {
                return this.dialogueEvent == EventManagement.SpecialEvent.None;
            }
        }

        public bool IsDeadEnd
        {
            get
            {
                return this.arrowDirections == RunBasedHoloMap.NORTH || 
                       this.arrowDirections == RunBasedHoloMap.SOUTH || 
                       this.arrowDirections == RunBasedHoloMap.WEST || 
                       this.arrowDirections == RunBasedHoloMap.EAST;
            }
        }

        public int NumberOfArrows
        {
            get
            {
                return (((this.arrowDirections & RunBasedHoloMap.NORTH) != 0) ? 1 : 0) +
                       (((this.arrowDirections & RunBasedHoloMap.SOUTH) != 0) ? 1 : 0) +
                       (((this.arrowDirections & RunBasedHoloMap.EAST) != 0) ? 1 : 0) +
                       (((this.arrowDirections & RunBasedHoloMap.WEST) != 0) ? 1 : 0);
            }
        }

        public List<string> DebugString
        {
            get
            {
                List<string> retval = new();
                string code = ((this.specialTerrain & LANDMARKER) != 0) ? "L" : this.opponent != Opponent.Type.Default ? "B" : this.specialDirection != RunBasedHoloMap.BLANK ? "E" : this.upgrade != HoloMapSpecialNode.NodeDataType.MoveArea ? "U" : " ";
                retval.Add("#---#");
                retval.Add((this.arrowDirections & RunBasedHoloMap.NORTH) != 0 ? $"|{this.color}| |" : $"|{this.color}  |");
                retval.Add("|" + ((this.arrowDirections & RunBasedHoloMap.WEST) != 0 ? $"-{code}" : $" {code}") + ((this.arrowDirections & RunBasedHoloMap.EAST) != 0 ? "-|" : " |"));
                retval.Add((this.arrowDirections & RunBasedHoloMap.SOUTH) != 0 ? "| | |" : "|   |");
                retval.Add("#---#");
                return retval;
            }
        }

        public string KeyCode
        {
            get
            {
                return $"{x},{y}";
            }
        }
    }
}