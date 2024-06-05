using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

/**
 * CHANGES:
 *      Made draw(), directionIndex(), directions, and directioncoords public.
 *      Made the centers of non-ground floors be empty instead of filled.
 *      Moved Start() contents to Generate().
 *      Commented-out draw() in Generate(). Now called in MazeRenderer.Start().
 **/

public class MazeGenerator : MonoBehaviour
{
    string[][,][] maze;

    // Initialize in the editor
    public int width;
    public int height;
    public int centerWidth;
    public int centerHeight;
    public int optionalSeed;
    public int numFloors;
    public float[] digitPercentages;
    public int[] digitMinimums;
    public float addPercentage;
    public int addMinCount;
    public float subtractPercentage;
    public int subtractMinCount;
    public float multiplyPercentage;
    public int multiplyMinCount;
    public float dividePercentage;
    public int divideMinCount;
    public bool usePitTraps;
    public float pitPercentage;
    private static int pitTrapsMinCount = 0;  // Can change to public if desired
    private static int poofTrapsMinCount = 0;  // Can change to public if desired
    public bool usePoofTraps;
    public float poofPercentage;
    public bool useChasers;
    public float chaserPercentage;
    public int chaserMinCount;
    public bool usePatrollers;
    public float patrollerPercentage;
    public int patrollerMinCount;
    public bool useHunters;
    public float hunterPercentage;
    public int hunterMinCount;
    public float healthPercentage;
    public float speedPercentage;
    public float powerPercentage;
    public int healthMinCount;
    public int speedMinCount;
    public int powerMinCount;

    // Initialize here or in Start()
    public static readonly string EMPTY = " ";
    public static readonly string WALL = "#";
    public static readonly string FLOOR = "·";
    public static readonly string ADD = "+";
    public static readonly string SUBTRACT = "−";
    public static readonly string MULTIPLY = "×";
    public static readonly string DIVIDE = "÷";
    public static readonly string PIT_TRAP = "↓";
    public static readonly string POOF_TRAP = "⌖";
    public static readonly string HEALTH_BOOST = "♥"; //♡
    public static readonly string SPEED_BOOST = "⇪"; //↑
    public static readonly string POWER_BOOST = "✮"; //★
    public static readonly string CHASER = "C";
    public static readonly string PATROLLER = "P";
    public static readonly string HUNTER = "H";
    public static readonly string[] digits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };  // For convenience
    private int[] digitCounts = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly string[] operators = { ADD, SUBTRACT, MULTIPLY, DIVIDE };
    private float[] operatorPercentages;
    private int[] operatorCounts = { 0, 0, 0, 0 };
    private int[] operatorMinimums;
    static readonly string[] traps = { PIT_TRAP, POOF_TRAP };
    private float[] trapPercentages;
    private int[] trapCounts = { 0, 0 };
    private int[] trapMinimums;
    static readonly string[] enemies = { CHASER, PATROLLER, HUNTER };
    private float[] enemyPercentages;
    private int[] enemyCounts = { 0, 0, 0 };
    private int[] enemyMinimums;
    public static readonly string[] boosts = { HEALTH_BOOST, SPEED_BOOST, POWER_BOOST };
    public static readonly Color[] boostColors = { Color.red, Color.green, Color.yellow };
    private float[] boostPercentages;
    private int[] boostCounts = { 0, 0, 0 };
    private int[] boostMinumums;
    private string[][] specialFloorTypes = { digits, operators, traps, enemies, boosts };
    private float[][] specialFloorPercentages;
    private int[][] specialFloorCounts;
    private int[][] specialFloorMinimums;
    int startingFloor = 0;
    public (int, int) start;
    public static readonly (int, int) CENTER = (0, 0);
    public static readonly (int, int) LEFT = (-1, 0);
    public static readonly (int, int) RIGHT = (1, 0);
    public static readonly (int, int) UP = (0, 1);
    public static readonly (int, int) DOWN = (0, -1);
    public static readonly (int, int)[] DIRECTION_COORDS = { LEFT, RIGHT, UP, DOWN };
    private string[][,][] grids;
    public menuParams menuParameters;

    // grids has the following structure:
    //  dim 0 : floors
    //  dim 1 and dim 2 : width and height coordinates (respectively) of cells in the corresponding floor
    //  dim 3 : { "<tileTypeString>", "<tileTypeString>", "<tileTypeString>", "<tileTypeString>", "<tileTypeString>" }
    // Use grids[floor][w, h][directionIndex(direction)] to index the appropriate string.

    public static int directionIndex((int, int) direction)
    {
        if(direction.Equals(CENTER)) { return 0; }
        return 1 + Array.IndexOf(DIRECTION_COORDS, direction);
    }

    private bool isInCircle(int width, int height, int x, int y)
    {
        return Mathf.Pow(x - ((width - 1) / 2), 2) + Mathf.Pow(y - ((height - 1) / 2), 2) <= ((width - 1) / 2) * ((height - 1) / 2);
    }

    private bool isInCenter(int width, int height, int centerWidth, int centerHeight, int x, int y)
    {
        int centerWideStart = ((width - 1) / 2) - (centerWidth / 2);
        int centerWideEnd = ((width - 1) / 2) + (centerWidth / 2);
        int centerHighStart = ((height - 1) / 2) - (centerHeight / 2);
        int centerHighEnd = ((height - 1) / 2) + (centerHeight / 2);
        return (x >= centerWideStart) && (x <= centerWideEnd) && (y >= centerHighStart) && (y <= centerHighEnd);
    }

    private string getTileType(string[][] typeGroups, float[][] typePercentages)
    {
        /**
         * DO NOT INCLUDE FLOOR TYPE
         * Percentages should obviously add up to <= 100.0, but higher values are functionally non-erroneous.
         **/
        float roll = UnityEngine.Random.value * 100;
        float previousPercent = 0.0f;
        for (int i = 0; i < typeGroups.Length; i++)
        {
            float currentPercent = typePercentages[i].Sum();
            if (roll < previousPercent + currentPercent)
            {
                float subRoll = UnityEngine.Random.value * currentPercent;
                float previousSubPercent = 0.0f;
                for (int j = 0; j < typePercentages[i].Length; j++)
                {
                    if (subRoll < previousSubPercent + typePercentages[i][j])
                    {
                        return typeGroups[i][j];
                    }
                    previousSubPercent += typePercentages[i][j];
                }
                break;
            }
            previousPercent += currentPercent;
        }
        return FLOOR;
    }

    private (int, int)[] getUnvisitedNeighbors(bool[,] visited, (int, int) cell)
    {
        List<(int, int)> neighbors = new List<(int, int)>();
        foreach ((int, int) direction in DIRECTION_COORDS)
        {
            int newX = cell.Item1 + direction.Item1;
            int newY = cell.Item2 + direction.Item2;
            if ((newX >= 0) && (newX < visited.GetLength(0)) && (newY >= 0) && (newY < visited.GetLength(1)) &&
                !visited[newX, newY])
            {
                neighbors.Add((cell.Item1 + direction.Item1, cell.Item2 + direction.Item2));
            }
        }
        return neighbors.ToArray();
    }

    private void removeWall(string[,][] grid, int w, int h, (int, int) direction)
    {
        grid[w, h][directionIndex(direction)] = FLOOR;
    }

    private bool isInBounds(int x, int y, int minX, int maxX, int minY, int maxY)
    {
        return !((x < minX) || (x > maxX) || (y < minY) || (y > maxY));
    }

    private string[,][] makeGrid(int width, int height, int centerWidth, int centerHeight, bool pits, bool poofs, int floor, string[,][] floor_above)
    {
        // Initialize
        string[,][] grid = new string[width, height][];
        bool[,] visited = new bool[width, height];
        for (int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                grid[i, j] = new string[5];
                grid[i, j][directionIndex(CENTER)] = isInCircle(width, height, i, j) ? getTileType(specialFloorTypes, specialFloorPercentages) : EMPTY;
                for(int a = 0; a < specialFloorTypes.Length; a++)
                {
                    if (specialFloorTypes[a].Contains(grid[i, j][directionIndex(CENTER)]))
                    {
                        specialFloorCounts[a][Array.IndexOf(specialFloorTypes[a], grid[i, j][directionIndex(CENTER)])]++;
                    }
                }
                Array.Fill(grid[i, j], grid[i, j][directionIndex(CENTER)].Equals(EMPTY) ? EMPTY : WALL, 1, 4);
                visited[i, j] = !isInCircle(width, height, i, j) ? true : false;  // Ensures algorithm will never try to remove an outside or center wall
            }
        }

        // Carve the center area
        int centerWideStart = ((width - 1) / 2) - (centerWidth / 2);
        int centerWideEnd = ((width - 1) / 2) + (centerWidth / 2);
        int centerHighStart = ((height - 1) / 2) - (centerHeight / 2);
        int centerHighEnd = ((height - 1) / 2) + (centerHeight / 2);

        // Make teleporters for utility in center area
        grid[centerWideStart, centerHighStart][directionIndex(CENTER)] = POOF_TRAP;
        grid[centerWideStart, centerHighEnd][directionIndex(CENTER)] = POOF_TRAP;
        grid[centerWideEnd, centerHighStart][directionIndex(CENTER)] = POOF_TRAP;
        grid[centerWideEnd, centerHighEnd][directionIndex(CENTER)] = POOF_TRAP;

        // Remove all walls from center area
        for (int i = centerWideStart; i <= centerWideEnd; i++)
        {
            for(int j = centerHighStart; j <= centerHighEnd; j++)
            {
                if (isInCenter(width, height, centerWidth, centerHeight, i - 1, j))
                {
                    removeWall(grid, i, j, LEFT);
                }
                if (isInCenter(width, height, centerWidth, centerHeight, i + 1, j))
                {
                    removeWall(grid, i, j, RIGHT);
                }
                if (isInCenter(width, height, centerWidth, centerHeight, i, j - 1))
                {
                    removeWall(grid, i, j, DOWN);
                }
                if (isInCenter(width, height, centerWidth, centerHeight, i, j + 1))
                {
                    removeWall(grid, i, j, UP);
                }
                visited[i, j] = true;
            }
        }
        // Open the center by removing the wall of the middlemost cell in each wall of the center area
        removeWall(grid, (width - 1) / 2, centerHighStart, DOWN);
        removeWall(grid, (width - 1) / 2, centerHighStart - 1, UP);
        removeWall(grid, (width - 1) / 2, centerHighEnd, UP);
        removeWall(grid, (width - 1) / 2, centerHighEnd + 1, DOWN);
        removeWall(grid, centerWideStart, (height - 1) / 2, LEFT);
        removeWall(grid, centerWideStart - 1, (height - 1) / 2, RIGHT);
        removeWall(grid, centerWideEnd, (height - 1) / 2, RIGHT);
        removeWall(grid, centerWideEnd + 1, (height - 1) / 2, LEFT);

        // Carve the maze
        Stack<(int, int)> stack = new Stack<(int, int)>();
        // Start from the four openings to the center area
        stack.Push((centerWideEnd, (height - 1) / 2));
        stack.Push((centerWideStart, (height - 1) / 2));
        stack.Push(((width - 1) / 2, centerHighEnd));
        stack.Push(((width - 1) / 2, centerHighStart));  // This is pushed last so it's popped first; it's closest to the lowest cell, which is where the player should start
        int timeSinceTrap = 0;
        while (stack.Count > 0)
        {
            (int, int) current = stack.Pop();
            (int, int)[] neighbors = getUnvisitedNeighbors(visited, current);
            if (neighbors.Length > 0)
            {
                timeSinceTrap++;
                stack.Push(current);
                var chosen = neighbors[(int)(UnityEngine.Random.value * neighbors.Length)];
                visited[chosen.Item1, chosen.Item2] = true;
                removeWall(grid, current.Item1, current.Item2, (chosen.Item1 - current.Item1, chosen.Item2 - current.Item2));
                removeWall(grid, chosen.Item1, chosen.Item2, (current.Item1 - chosen.Item1, current.Item2 - chosen.Item2));
                stack.Push(chosen);
            }
            else
            {
                timeSinceTrap = 0;
            }
        }

        // Remove unused traps, traps in intersections, pit traps on floor 0, poof traps below pit traps,
        // and all special tiles at the starting cell
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                int openNeighbors = 0;
                foreach((int, int) direction in DIRECTION_COORDS)
                {
                    if(grid[i, j][directionIndex(direction)].Contains(FLOOR))
                    {
                        openNeighbors++;
                    }
                }
                if((!pits && grid[i, j][directionIndex(CENTER)].Contains(PIT_TRAP)) ||
                    (!poofs && grid[i, j][directionIndex(CENTER)].Contains(POOF_TRAP) && !isInBounds(i, j, centerWideStart, centerWideEnd, centerHighStart, centerHighEnd)) ||
                    ((grid[i, j][directionIndex(CENTER)].Contains(PIT_TRAP) || grid[i, j][directionIndex(CENTER)].Contains(POOF_TRAP)) && (openNeighbors > 2)) ||
                    ((floor == 0) && grid[i, j][directionIndex(CENTER)].Contains(PIT_TRAP)) ||
                    (grid[i, j][directionIndex(CENTER)].Contains(POOF_TRAP) && (floor_above != null) && floor_above[i, j][directionIndex(CENTER)].Contains(PIT_TRAP)) ||
                    ((i, j).Equals(start) && (floor == startingFloor)))
                {
                    grid[i, j][directionIndex(CENTER)] = FLOOR;
                }
                // remove hunters from non-ground floors
                if(floor > 0 && grid[i, j][directionIndex(CENTER)] == HUNTER)
                {
                    grid[i, j][directionIndex(CENTER)] = FLOOR;
                }
            }
        }

        if(floor > 0)
        {
            // Empty center if not the bottom floor.
            for(int i = centerWideStart; i <= centerWideEnd; i++)
            {
                for(int j = centerHighStart; j <= centerHighEnd; j++)
                {
                    grid[i, j][directionIndex(CENTER)] = EMPTY;
                }
            }
        }

        if (floor == 0)
        {
            // Remove all enemies from center of bottom floor.
            for (int i = centerWideStart; i <= centerWideEnd; i++)
            {
                for (int j = centerHighStart; j <= centerHighEnd; j++)
                {
                    if (grid[i, j][directionIndex(CENTER)] == CHASER || grid[i, j][directionIndex(CENTER)] == PATROLLER || grid[i, j][directionIndex(CENTER)] == HUNTER)
                    {
                        grid[i, j][directionIndex(CENTER)] = FLOOR;
                    }
                }
            }
        }

        return grid;
    }

    private bool checkMinimums(int[][] minimums, int[][] counts)
    {
        for (int i = 0; i < minimums.Length; i++)
        {
            for (int j = 0; j < minimums[i].Length; j++)
            {
                if (counts[i][j] < minimums[i][j]) { return false; }
            }
        }
        return true;
    }

    public void draw(string[][,][] grids)
    {
        // Prints a text representation of the maze to Debug
        // Copy/paste the output into a unispace editor to see the maze
        // Ignore the inside corner-walls in the center area; they're not represented in the underlying array
        for(int floor = 0; floor < grids.Length; floor++)
        {
            string floorString = "";
            for(int i = 0; i < grids[floor].GetLength(0); i++)
            {
                string[] lines = new string[4];
                for(int j = 0; j < grids[floor].GetLength(1); j++)
                {
                    string[] cell = grids[floor][i, j];
                    string tileType = cell[directionIndex(CENTER)];
                    string cornerChar = tileType.Equals(EMPTY) ? EMPTY : WALL;
                    lines[0] += cornerChar                  + cell[directionIndex(LEFT)]    + cell[directionIndex(LEFT)]    + cornerChar;
                    lines[1] += cell[directionIndex(DOWN)]  + cell[directionIndex(CENTER)]  + cell[directionIndex(CENTER)]  +  cell[directionIndex(UP)];
                    lines[2] += cell[directionIndex(DOWN)]  + cell[directionIndex(CENTER)]  + cell[directionIndex(CENTER)]  +  cell[directionIndex(UP)];
                    lines[3] += cornerChar                  + cell[directionIndex(RIGHT)]   + cell[directionIndex(RIGHT)]   + cornerChar;
                }
                foreach(string line in lines)
                {
                    floorString += "\n" + line;
                }
            }
            Debug.Log(floorString);
        }
    }

    public string[][,][] Generate()
    {
        numFloors = menuParameters.numFloors;
        usePoofTraps = menuParameters.useProofTraps;
        usePitTraps = menuParameters.useFloorTraps;
        if (optionalSeed > 0)
        {
            UnityEngine.Random.InitState(optionalSeed);
        }
        // Widths and heights must be odd for a good circle.
        if (width % 2 == 0)
        {
            width++;
        }
        if (height % 2 == 0)
        {
            height++;
        }
        if (centerWidth % 2 == 0)
        {
            centerWidth++;
        }
        if (centerHeight % 2 == 0)
        {
            centerHeight++;
        }
        operatorPercentages = new[] { addPercentage, subtractPercentage, multiplyPercentage, dividePercentage };
        operatorMinimums = new[] { addMinCount, subtractMinCount, multiplyMinCount, divideMinCount };
        trapPercentages = new[] { pitPercentage, poofPercentage };
        trapMinimums = new[] { pitTrapsMinCount, poofTrapsMinCount };
        enemyPercentages = new[] { chaserPercentage, patrollerPercentage, hunterPercentage };
        enemyMinimums = new[] { chaserMinCount, patrollerMinCount, hunterMinCount };
        boostPercentages = new[] { healthPercentage, speedPercentage, powerPercentage };
        boostMinumums = new[] { healthMinCount, speedMinCount, powerMinCount };
        specialFloorPercentages = new[] { digitPercentages, operatorPercentages, trapPercentages, enemyPercentages, boostPercentages };
        specialFloorCounts = new[] { digitCounts, operatorCounts, trapCounts, enemyCounts, boostCounts };
        specialFloorMinimums = new[] { digitMinimums, operatorMinimums, trapMinimums, enemyMinimums, boostMinumums };
        start = ((width - 1) / 2, 0);

        List<string[,][]> gridsList = new List<string[,][]>();
        // Each layer generated separately so we can split parameters between layers later if desired
        string[,][] floor_above = null;
        for (int floor = numFloors - 1; floor >= 0; floor--)
        {
            string[,][] grid;
            do
            {
                foreach (int[] countArray in specialFloorCounts)
                {
                    Array.Fill(countArray, 0);
                }
                grid = makeGrid(width, height, centerWidth, centerHeight, usePitTraps, usePoofTraps, floor, floor_above);
            }
            while (!checkMinimums(specialFloorMinimums, specialFloorCounts));
            gridsList.Add(grid);
            floor_above = grid;
        }
        gridsList.Reverse();
        grids = gridsList.ToArray();
        maze = grids;
        //draw(grids);
        return maze;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
