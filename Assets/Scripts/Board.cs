using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : NetworkBehaviour
{
    private static Board _instance;

    public static Board instance
    { get { return _instance; } }

    private Camera currentCamera;
    public TileCube tileCubePrefab;
    public GameObject groundTilesContainer;
    //[SerializeField] private Tilemap tilemap;
    private Vector2Int currentHover;

    public bool isFilled = false;
    public Dictionary<Vector2Int, TileCube> map;

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

    private void Start()
    {
        map = new Dictionary<Vector2Int, TileCube>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Bounds:");
        SpawnTilesServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnTilesServerRpc()
    {
        SpawnTilesClientRpc();
    }

    [ClientRpc]
    private void SpawnTilesClientRpc()
    {
        var tileMap = gameObject.GetComponentInChildren<Tilemap>();
        BoundsInt bounds = tileMap.cellBounds;
        Debug.Log(bounds);

        for (int y = bounds.max.y; y > bounds.min.y; y--)
        {
            for (int z = bounds.min.z; z < bounds.max.z; z++)
            {
                for (int x = bounds.min.x; x < bounds.max.x; x++)
                {
                    var tileLocation = new Vector3Int(x, y, z);
                    var tilePosition = new Vector2Int(x, y);

                    if (tileMap.HasTile(tileLocation) && !map.ContainsKey(tilePosition))
                    {
                        var tileCube = Instantiate(tileCubePrefab, groundTilesContainer.transform);

                        var cellWorldPosition = tileMap.GetCellCenterWorld(tileLocation);

                        tileCube.transform.position = new Vector3(cellWorldPosition.x, cellWorldPosition.y, cellWorldPosition.z);
                        tileCube.GetComponent<MeshRenderer>().sortingOrder = tileMap.GetComponent<TilemapRenderer>().sortingOrder;
                        tileCube.gridLocation.Value = tileLocation;
                        map.Add(tilePosition, tileCube);
                    }
                }
            }
        }

        isFilled = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.current;
            return;
        }
    }

    // Just returns neighbour tiles alongside to the selected tile
    public List<TileCube> GetNeighbourTiles(TileCube currentTile)
    {
        List<TileCube> neighbours = new List<TileCube>();

        // TOP
        Vector2Int locationToCheck = new Vector2Int(currentTile.gridLocation.Value.x, currentTile.gridLocation.Value.y + 1);

        if (map.ContainsKey(locationToCheck))
        {
            neighbours.Add(map[locationToCheck]);
        }

        // BOTTOM
        locationToCheck = new Vector2Int(currentTile.gridLocation.Value.x, currentTile.gridLocation.Value.y - 1);

        if (map.ContainsKey(locationToCheck))
        {
            neighbours.Add(map[locationToCheck]);
        }

        // RIGHT
        locationToCheck = new Vector2Int(currentTile.gridLocation.Value.x + 1, currentTile.gridLocation.Value.y);

        if (map.ContainsKey(locationToCheck))
        {
            neighbours.Add(map[locationToCheck]);
        }

        // LEFT
        locationToCheck = new Vector2Int(currentTile.gridLocation.Value.x - 1, currentTile.gridLocation.Value.y);

        if (map.ContainsKey(locationToCheck))
        {
            neighbours.Add(map[locationToCheck]);
        }

        return neighbours;
    }
}