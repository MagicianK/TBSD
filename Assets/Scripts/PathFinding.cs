using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    public static List<TileCube> FindPath(TileCube start, TileCube end)
    {
        List<TileCube> openList = new List<TileCube>();
        List<TileCube> closedList = new List<TileCube>();
        openList.Add(start);
        while(openList.Count > 0)
        {
            TileCube currentTile = openList.OrderBy(x => x.F).First();

            openList.Remove(currentTile);
            closedList.Add(currentTile);

            if(currentTile == end)
            {
                return GetFinishedList(start, end);
            }

            var neighbourTiles = Board.instance.GetNeighbourTiles(currentTile);

            foreach (var neighbour in neighbourTiles)
            {
                if(neighbour.isBlocked || 
                closedList.Contains(neighbour))
                    {
                        continue;
                    }
                neighbour.G = GetBlockDistance(start, neighbour);
                neighbour.H = GetBlockDistance(end, neighbour);

                neighbour.previous = currentTile;

                if (!openList.Contains(neighbour))
                {
                    openList.Add(neighbour);
                }
            }
        }
        return new List<TileCube>();
    }

    private static List<TileCube> GetFinishedList(TileCube start, TileCube end)
    {
        List<TileCube> finishedList = new List<TileCube>();

        TileCube currentTile = end;
        while(currentTile != start)
        {
            finishedList.Add(currentTile);
            currentTile = currentTile.previous;
        }
        finishedList.Reverse();
        return finishedList;
    }

    private static int GetBlockDistance(TileCube start, TileCube neighbour)
    {
        return Mathf.Abs(start.gridLocation.x - neighbour.gridLocation.x) + Mathf.Abs(start.gridLocation.z - neighbour.gridLocation.z);
    }
}
