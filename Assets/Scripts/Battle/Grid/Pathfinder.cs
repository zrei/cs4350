using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kindly keep this updated :)
/// Can be used for more broad categories e.g. NORMAL means all units can cross
/// Something like FLYERS may mean only flying units can cross
/// </summary>
public enum TileType
{
    NORMAL,
    IMPASSABLE
}

/// <summary>
/// Class packaging data on a single tile
/// </summary>
public class TileData
{
    public TileType TileType => m_CurrTileEffect == null ? TileType.NORMAL : m_CurrTileEffect.TileType;
    // this follows the assumption that units cannot cross over the line, otherwise this requires information on the alliance
    public bool m_IsOccupied;
    public Unit m_CurrUnit;
    public TileEffect m_CurrTileEffect;

    public TileData(bool isOccupied)
    {
        m_IsOccupied = isOccupied;
        m_CurrUnit = null;
        m_CurrTileEffect = null;
    }

    public void Tick(float passedTime, VoidEvent clearEvent)
    {
        if (m_CurrTileEffect != null)
        {
            m_CurrTileEffect.Tick(passedTime);
            if (m_CurrTileEffect.IsEmpty)
            {
                clearEvent?.Invoke();
                m_CurrTileEffect = null;
            }
        }
    }

    public bool TryApplyEffect(InflictedTileEffect tileEffect)
    {
        if (m_CurrTileEffect != null && m_CurrTileEffect.IsPermanent)
            return false;

        if (m_CurrTileEffect != null && tileEffect.m_TileEffect == m_CurrTileEffect.m_TileEffectSO)
        {
            m_CurrTileEffect.TopUp(tileEffect.m_InitialTime);
        }
        else
        {
            m_CurrTileEffect = new TileEffect(tileEffect.m_TileEffect, tileEffect.m_InitialTime);
        }

        return true;
    }

    public bool TryApplyEffectOnUnit()
    {
        if (!m_IsOccupied || m_CurrTileEffect == null)
            return false;

        m_CurrTileEffect.ApplyEffects(m_CurrUnit);
        return true;
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

    public void Tick()
    {
        for (int r = 0; r < NUM_ROWS; ++r)
        {
            for (int c = 0; c < NUM_COLS; ++c)
            {
                if (tileMap[r, c].m_CurrTileEffect != null)
                {

                }
            }
        }
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
    public static HashSet<PathNode> ReachablePoints(MapData map, CoordPair startPoint, int movementRange, bool canSwapSquares, params TileType[] traversableTiles)
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

            q.Enqueue((new PathNode(coordinates.MoveLeft(), point), remainingMovement - 1));
            q.Enqueue((new PathNode(coordinates.MoveRight(), point), remainingMovement - 1));
            q.Enqueue((new PathNode(coordinates.MoveDown(), point), remainingMovement - 1));
            q.Enqueue((new PathNode(coordinates.MoveUp(), point), remainingMovement - 1));
        }

        return canTraverse;
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
        if (!Array.Exists(traversableTiles, x => x == tile.TileType))
            return false;
        return true;
    }
}