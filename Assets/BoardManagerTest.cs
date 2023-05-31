using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Do not delete this class. It is created to manage camera position to look on a board
[ExecuteInEditMode]
public class BoardManagerTest : MonoBehaviour
{
    [SerializeField] private int _width, _height;
    [SerializeField] private TileCube _tilePrefab;
    [SerializeField] private GameObject allTiles;
    private Dictionary<Vector2Int, TileCube> _tiles;
    private void Start()    
    {
        if (Application.IsPlaying(gameObject))
            Destroy(allTiles);
        _tiles = new Dictionary<Vector2Int, TileCube>();
        GenerateGrid();
    }
    private void GenerateGrid()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, 0, y), Quaternion.identity, allTiles.transform);
                spawnedTile.name = $"Tile {x} {y}";

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                _tiles[new Vector2Int(x, y)] = spawnedTile;
            }
        }
    }
}
