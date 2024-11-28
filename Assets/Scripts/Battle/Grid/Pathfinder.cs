using System;
using System.Collections.Generic;
using UnityEngine;

// can add more tile types like trap or cliff in the future
// if calculations become exceedingly class dependent it should be given to the class
// to calculate
public enum TileType
{
    NORMAL
}

public enum MovementType
{
    UNRESTRICTED,
    DIAGONALS
}

/// <summary>
/// Class packaging data on a single tile
/// </summary>
public class TileData
{
    public TileType m_TileType;
    // this follows the assumption that units cannot cross over the line, otherwise this requires information on the alliance
    public bool m_IsOccupied;
    public Unit m_CurrUnit;

    public TileData(TileType tileType, bool isOccupied)
    {
        m_TileType = tileType;
        m_IsOccupied = isOccupied;
        m_CurrUnit = null;
    }
}

/// <summary>
/// Struct packaging data on all tiles in a map
/// (This is currently handling only one half of a battle - i.e. only
/// the player or enemy side)
/// </summary>
public class MapData 
{
    public const int NUM_ROWS = 5;
    public const int NUM_COLS = 5;

    private TileData[,] tileMap = new TileData[NUM_ROWS, NUM_COLS];

    public MapData(MapData map)
    {
        tileMap = map.tileMap;
    }

    public MapData(TileData[,] tiles)
    {
        if (tiles.GetLength(0) != NUM_ROWS || tiles.GetLength(1) != NUM_COLS)
        {
            Logger.Log(this.GetType().Name, $"Tiles has {tiles.GetLength(0)} rows and {tiles.GetLength(1)} cols", LogLevel.ERROR);
            return;
        }
        tileMap = tiles;
    }

    public void UpdateTile(CoordPair coordPair, TileData tile)
    {
        tileMap[coordPair.m_Row, coordPair.m_Col] = tile;
    }

    public TileData RetrieveTile(CoordPair coordPair)
    {
        return tileMap[coordPair.m_Row, coordPair.m_Col];
    }

    public static bool WithinBounds(CoordPair point)
    {
        return point.m_Row >= 0 && point.m_Row < NUM_ROWS && point.m_Col >= 0 && point.m_Col < NUM_COLS;
    }
}

[System.Serializable]
public struct CoordPair
{
    public int m_Row;
    public int m_Col;

    public CoordPair(int row, int col)
    {
        m_Row = row;
        m_Col = col;
    }

    public CoordPair MoveRight()
    {
        return new CoordPair(m_Row, m_Col + 1);
    }

    public CoordPair MoveUp()
    {
        return new CoordPair(m_Row - 1, m_Col);
    }

    public CoordPair MoveDown()
    {
        return new CoordPair(m_Row + 1, m_Col);
    }

    public CoordPair MoveLeft()
    {
        return new CoordPair(m_Row, m_Col - 1);
    }

    public CoordPair MoveDiagonalUpRight()
    {
        return new CoordPair(m_Row + 1, m_Col + 1);
    }

    public CoordPair MoveDiagonalDownRight()
    {
        return new CoordPair(m_Row - 1, m_Col + 1);
    }

    public CoordPair MoveDiagonalUpLeft()
    {
        return new CoordPair(m_Row + 1, m_Col - 1);
    }

    public CoordPair MoveDiagonalDownLeft()
    {
        return new CoordPair(m_Row - 1, m_Col - 1);
    }

    public CoordPair Offset(CoordPair offset)
    {
        return new CoordPair(m_Row + offset.m_Row, m_Col + offset.m_Col);
    }

    public int GetDistanceToPoint(CoordPair otherPoint)
    {
        return Mathf.Abs(otherPoint.m_Row - m_Row) + Mathf.Abs(otherPoint.m_Col - m_Col);
    }

    public override string ToString()
    {
        return $"({m_Row}, {m_Col})";
    }

    public override int GetHashCode()
    {
        return m_Row.GetHashCode() ^ m_Col.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is CoordPair)
        {
            CoordPair cast = (CoordPair) obj;
            return cast.m_Row == m_Row && cast.m_Col == m_Col;
        }
        return false;
    }
}

/// <summary>
/// To help with constructing shortest path
/// </summary>
public class PathNode
{
    public readonly CoordPair m_Coordinates;
    public readonly PathNode m_Parent;

    public bool HasParent => m_Parent != null;

    public PathNode(CoordPair coordinates, PathNode parent)
    {
        m_Coordinates = coordinates;
        m_Parent = parent;
    }
}

/// <summary>
/// Static class that helps with finding traversable tiles and finding the shortest
/// path to a tile
/// </summary>
public static class Pathfinder
{
    public static HashSet<PathNode> ReachablePoints(MapData map, CoordPair startPoint, int movementRange, bool canSwapSquares, MovementType movementType, params TileType[] traversableTiles)
    {
        HashSet<PathNode> canTraverse = new HashSet<PathNode>();
        HashSet<CoordPair> checkedTiles = new HashSet<CoordPair>();
        Queue<(PathNode, int)> q = new Queue<(PathNode, int)>();
        q.Enqueue((new PathNode(startPoint, null), movementRange));

        while (q.Count > 0)
        {
            (PathNode point, int remainingMovement) = q.Dequeue();
            CoordPair coordinates = point.m_Coordinates;

            if (checkedTiles.Contains(coordinates))
                continue;
            checkedTiles.Add(coordinates);
            if (remainingMovement < 0)
                continue;
            if (!MapData.WithinBounds(coordinates))
                continue;
            if (!IsTraversableTile(map, coordinates, traversableTiles))
                continue;

            if (!coordinates.Equals(startPoint) && (!map.RetrieveTile(coordinates).m_IsOccupied || canSwapSquares))
                canTraverse.Add(point);

            if (!coordinates.Equals(startPoint) && map.RetrieveTile(coordinates).m_IsOccupied && !GlobalSettings.AllowCrossingOverOccupiedSquares)
                continue;

            EnqueueNewPoints(point, movementType, remainingMovement, q);
        }

        return canTraverse;
    }

    private static void EnqueueNewPoints(PathNode currentPathNode, MovementType movementType, int currMovementRange, Queue<(PathNode,int)> queue)
    {
        CoordPair coordinates = currentPathNode.m_Coordinates;
        switch (movementType)
        {
            
            case MovementType.UNRESTRICTED:
                queue.Enqueue((new PathNode(coordinates.MoveLeft(), currentPathNode), currMovementRange - 1));
                queue.Enqueue((new PathNode(coordinates.MoveRight(), currentPathNode), currMovementRange - 1));
                queue.Enqueue((new PathNode(coordinates.MoveDown(), currentPathNode), currMovementRange - 1));
                queue.Enqueue((new PathNode(coordinates.MoveUp(), currentPathNode), currMovementRange - 1));
                break;
            case MovementType.DIAGONALS:
                queue.Enqueue((new PathNode(coordinates.MoveDiagonalDownLeft(), currentPathNode), currMovementRange - 1));
                queue.Enqueue((new PathNode(coordinates.MoveDiagonalDownRight(), currentPathNode), currMovementRange - 1));
                queue.Enqueue((new PathNode(coordinates.MoveDiagonalUpLeft(), currentPathNode), currMovementRange - 1));
                queue.Enqueue((new PathNode(coordinates.MoveDiagonalUpRight(), currentPathNode), currMovementRange - 1));
                break;
        }
    }

    public static bool TryPathfind(MapData map, CoordPair startPosition, CoordPair destination, out PathNode pathNode, params TileType[] traversableTiles)
    {
        HashSet<CoordPair> checkedTiles = new HashSet<CoordPair>();
        Queue<PathNode> q = new Queue<PathNode>();
        q.Enqueue(new PathNode(startPosition, null));

        while (q.Count > 0)
        {
            PathNode point = q.Dequeue();
            CoordPair coordinates = point.m_Coordinates;

            if (coordinates.Equals(destination))
            {
                pathNode = point;
                return true;
            }

            if (checkedTiles.Contains(coordinates))
                continue;
            checkedTiles.Add(coordinates);
            if (!MapData.WithinBounds(coordinates))
                continue;
            if (!IsTraversableTile(map, coordinates, traversableTiles))
                continue;

            q.Enqueue(new PathNode(coordinates.MoveLeft(), point));
            q.Enqueue(new PathNode(coordinates.MoveRight(), point));
            q.Enqueue(new PathNode(coordinates.MoveDown(), point));
            q.Enqueue(new PathNode(coordinates.MoveUp(), point));
        }

        pathNode = default;
        return false;
    }

    private static bool IsTraversableTile(MapData map, CoordPair point, params TileType[] traversableTiles)
    {
        TileData tile = map.RetrieveTile(point);
        if (!Array.Exists(traversableTiles, x => x == tile.m_TileType))
            return false;

        return true;
    }
}