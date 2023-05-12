using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RangeFinder
{
    public static List<TileCube> GetTilesRange(TileCube startingTile, int range)
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
                surroundingTiles.AddRange(Board.instance.GetNeighbourTiles(item));
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