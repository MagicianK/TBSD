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

    public List<TileCube> GetTilesRange(TileCube startingTile, int range)
    {
        var inRangeTiles = new List<TileCube>();
        int stepCount = 0;

        inRangeTiles.Add(startingTile);

        var tileForPreviousStep = new List<TileCube>();
        tileForPreviousStep.Add(startingTile);

        while (stepCount < range)
        {
            var surroundingTiles = new List<TileCube>();

            foreach (var item in tileForPreviousStep)
            {
                //Debug.Log("Item " + item.grid2DLocation);
                if (item)
                    surroundingTiles.AddRange(BoardManager.instance.GetNeighbourTiles(item));
            }
            inRangeTiles.AddRange(surroundingTiles);
            tileForPreviousStep = surroundingTiles.Distinct().ToList();
            stepCount++;
        }

        return inRangeTiles.Distinct().ToList();
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }
}