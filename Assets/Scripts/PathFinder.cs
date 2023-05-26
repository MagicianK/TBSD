using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public List<TileCube> FindPath(TileCube start, TileCube end)
    {
        List<TileCube> openList = new List<TileCube>();
        List<TileCube> closedList = new List<TileCube>();
        openList.Add(start);
        while (openList.Count > 0)
        {
            TileCube currentTile = openList.OrderBy(x => x.F).First();

            openList.Remove(currentTile);
            closedList.Add(currentTile);

            if (currentTile == end)
            {
                return GetFinishedList(start, end);
            }

            var neighbourTiles = Board.instance.GetNeighbourTiles(currentTile);

            foreach (var neighbour in neighbourTiles)
            {
                if (neighbour.isBlocked ||
                closedList.Contains(neighbour))
                {
                    continue;
                }
                neighbour.G = GetBlockDistance(start, neighbour);
                neighbour.H = GetBlockDistance(end, neighbour);

                neighbour.previous = currentTile.grid2DLocation;

                if (!openList.Contains(neighbour))
                {
                    openList.Add(neighbour);
                }
            }
        }
        return new List<TileCube>();
    }

    private List<TileCube> GetFinishedList(TileCube start, TileCube end)
    {
        List<TileCube> finishedList = new List<TileCube>();

        TileCube currentTile = end;
        int counter = 0;
        while (currentTile != start && counter < 18)
        {
            finishedList.Add(currentTile);
            Debug.Log(currentTile.previous);
            currentTile = Board.instance.map[currentTile.previous];
            counter++;
        }
        finishedList.Reverse();
        return finishedList;
    }

    private int GetBlockDistance(TileCube start, TileCube neighbour)
    {
        return Mathf.Abs(start.gridLocation.x - neighbour.gridLocation.x) + Mathf.Abs(start.gridLocation.z - neighbour.gridLocation.z);
    }
}