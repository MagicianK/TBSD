using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
[ExecuteInEditMode]
public class BoardManager : NetworkBehaviour
{
    private static BoardManager _instance;
    public static BoardManager instance
    { get { return _instance; } }

    [SerializeField] private int _width, _height;

    [SerializeField] private TileCube _tilePrefab;
    [SerializeField] private GameObject allTiles;
    //[SerializeField] private Transform _cam;

    private Dictionary<Vector2Int, TileCube> _tiles;
    //public NetworkVariable<bool> isFilled = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone);
    public bool isFilled = false;
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    public override void OnNetworkSpawn()
    {
        if(IsServer)
            GenerateGridServerRpc();
    }
    
    [ServerRpc]
    void GenerateGridServerRpc()
    {
        _tiles = new Dictionary<Vector2Int, TileCube>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, 0, y), Quaternion.identity, allTiles.transform);
                spawnedTile.name = $"Tile {x} {y}";

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.GetComponent<NetworkObject>().Spawn();

                spawnedTile.InitServerRpc(isOffset, new Vector2Int(x, y));
                _tiles[new Vector2Int(x, y)] = spawnedTile;
                
            }
        }
        isFilled = true;
        //_cam.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10);
    }

    [ServerRpc]
    public void UnblockTileServerRpc(Vector2Int coord)
    {
        if (!IsOwner)
            return;
        _tiles[coord].isBlocked.Value = false;

    }
    [ServerRpc]
    public void BlockTileServerRpc(Vector2Int coord)
    {
        if (!IsOwner)
            return;
        _tiles[coord].isBlocked.Value = true;
        //BlockTileClientRpc(coord);
    }
    public TileCube GetTileAtPosition(Vector2Int pos)
    {
        if (_tiles.TryGetValue(pos, out var tile)) return tile;
        return null;
    }

    public List<TileCube> GetNeighbourTiles(Vector2Int currentTile)
    {
        List<TileCube> neighbours = new List<TileCube>();

        // TOP
        Vector2Int locationToCheck = new Vector2Int(currentTile.x, currentTile.y + 1);

        if (_tiles.ContainsKey(locationToCheck))
        {
            neighbours.Add(_tiles[locationToCheck]);
        }

        // BOTTOM
        locationToCheck = new Vector2Int(currentTile.x, currentTile.y - 1);

        if (_tiles.ContainsKey(locationToCheck))
        {
            neighbours.Add(_tiles[locationToCheck]);
        }

        // RIGHT
        locationToCheck = new Vector2Int(currentTile.x + 1, currentTile.y);

        if (_tiles.ContainsKey(locationToCheck))
        {
            neighbours.Add(_tiles[locationToCheck]);
        }

        // LEFT
        locationToCheck = new Vector2Int(currentTile.x - 1, currentTile.y);

        if (_tiles.ContainsKey(locationToCheck))
        {
            neighbours.Add(_tiles[locationToCheck]);
        }

        return neighbours;
    }
}
