using System;
using System.Collections.Generic;

// if trap in future? wwww
public enum TileType
{
    NORMAL
}

// this is under the assumption that you can't cross over the line
public struct Tile
{
    public TileType m_TileType;
    public bool m_IsOccupied;
}

public struct CoordPair
{
    public int row;
    public int col;

    public CoordPair MoveRight()
    {
        return new CoordPair(row, col + 1);
    }

    public CoordPair MoveUp()
    {
        return new CoordPair(row - 1, col);
    }

    public CoordPair MoveDown()
    {
        return new CoordPair(row + 1, col);
    }

    public CoordPair MoveLeft()
    {
        return new CoordPair(row, col - 1);
    }

    public CoordPair(int row, int col)
    {
        this.row = row;
        this.col = col;
    }
}

// not gonna link it to MonoBehaviour... yet.
public static class Pathfinder
{
    private const int NUM_ROWS = 5;
    private const int NUM_COLS = 5;

    private static Tile[,] Map;

    public static void InitialiseMap(Tile[,] map)
    {
        Map = map;
    }

    public static void UpdateMap(CoordPair point, Tile newTile)
    {
        Map[point.row, point.col] = newTile;
    }

    // if you like need me to return the entire thing then like sure i guess?
    public static HashSet<CoordPair> ReachableMap(CoordPair startPoint, int movementRange, bool canSwapSquares, params TileType[] traversableTiles)
    {
        HashSet<CoordPair> canTraverse = new HashSet<CoordPair>();
        Queue<(CoordPair, int)> q = new Queue<(CoordPair, int)>();
        q.Enqueue((startPoint, movementRange));

        while (q.Count > 0)
        {
            (CoordPair point, int remainingMovement) = q.Dequeue();
            if (canTraverse.Contains(point))
                continue;
            if (remainingMovement < 0)
                continue;
            if (!WithinBounds(point))
                continue;
            if (!IsTraversableTile(point, canSwapSquares, traversableTiles))
                continue;
            canTraverse.Add(point);

            q.Enqueue((point.MoveLeft(), remainingMovement - 1));
            q.Enqueue((point.MoveRight(), remainingMovement - 1));
            q.Enqueue((point.MoveDown(), remainingMovement - 1));
            q.Enqueue((point.MoveUp(), remainingMovement - 1));
        }

        return canTraverse;
    }

    private static bool IsTraversableTile(CoordPair point, bool canSwapSquares, params TileType[] traversableTiles)
    {
        Tile tile = Map[point.row, point.col];
        if (tile.m_IsOccupied && !canSwapSquares)
            return false;
        else if (!Array.Exists(traversableTiles, x => x == tile.m_TileType))
            return false;

        return true;
    }

    private static bool WithinBounds(CoordPair point)
    {
        return point.row >= 0 && point.row < NUM_ROWS && point.col >= 0 && point.col < NUM_COLS;
    }
}