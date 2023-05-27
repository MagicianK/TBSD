using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RangeFinder
{
    //[ServerRpc(RequireOwnership = false)]
    //public static void GetTilesRangeServerRpc(Vector2Int pos, int range, out List<TileCube> tiles)
    //{
    //    GetTilesRangeClientRpc(pos, range, out tiles);
    //}

    //[ClientRpc]
    //public static void GetTilesRangeClientRpc(Vector2Int pos, int range, out List<TileCube> tiles)
    //{
    //    tiles = new List<TileCube>();
    //    if (Board.instance.map.ContainsKey(pos))
    //    {
    //        TileCube startingTile = Board.instance.map[pos];
    //        tiles = GetTilesRange(startingTile, range);
    //    }
    //    else
    //    {
    //        Debug.Log("Position does not exist!");
    //    }
    //}

    public List<TileCube> GetTilesRange(Vector2Int startingTile, int range)
    {
        TileCube startTile = BoardManager.instance.GetTileAtPosition(startingTile);
        var inRangeTiles = new List<TileCube>();
        int stepCount = 0;

        inRangeTiles.Add(startTile);

        var tileForPreviousStep = new List<TileCube>();
        tileForPreviousStep.Add(startTile);

        while (stepCount < range)
        {
            var surroundingTiles = new List<TileCube>();

            foreach (var item in tileForPreviousStep)
            {
                if (item)
                    surroundingTiles.AddRange(BoardManager.instance.GetNeighbourTiles(item.coord.Value));
            }
            inRangeTiles.AddRange(surroundingTiles);
            tileForPreviousStep = surroundingTiles.Distinct().ToList();
            stepCount++;
        }

        return inRangeTiles.Distinct().ToList();
    }

}