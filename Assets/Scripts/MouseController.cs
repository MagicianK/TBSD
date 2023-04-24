using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MouseController : NetworkBehaviour
{
    public GameObject cursor;
    public NetworkVariable<int> idForUnit = new NetworkVariable<int>(100);

    // public GameObject cursor;
    public GameObject currentHover { get; private set; }

    public GameObject currentClicked { get; private set; }
    private PathFinding pathfinder;
    [SerializeField] private Unit unitPrefab;
    private Unit unit;

    public override void OnNetworkSpawn()
    {
        idForUnit.OnValueChanged += (int previousValue, int newValue) =>
        {
        };
    }

    // Start is called before the first frame update
    private void Start()
    {
        currentHover = null;
        currentClicked = null;

        pathfinder = new PathFinding();
    }

    private void LateUpdate()
    {
        if (!IsOwner)
            return;

        if (unit && unit.isMoving)
            return;

        var focusedTileHit = GetFocusedTile();
        var focusedUnitHit = GetFocusedUnit();
        if (focusedUnitHit.HasValue)
        {
            if (Input.GetKeyDown(KeyCode.F))
                Debug.Log("Focused tile: " + focusedTileHit.Value.collider.GetComponent<TileCube>().GetUnitInfo());
            Unit currentUnit = focusedUnitHit.Value.collider.GetComponentInParent<Unit>();
            if (!Input.GetMouseButtonUp(0))
                return;

            if (currentUnit)
            {
                chooseUnit(Board.instance.map[currentUnit.standingOn]);
            }
        }

        if (focusedTileHit.HasValue)
        {
            TileCube tileCube = focusedTileHit.Value.collider.gameObject.GetComponent<TileCube>();
            GameObject tileObj = focusedTileHit.Value.collider.gameObject;

            if (tileCube == null)
                return;

            if (!Input.GetMouseButtonUp(0))
                return;

            Board.instance.map.TryGetValue(tileCube.grid2DLocation, out tileCube);

            foreach (var mapVal in Board.instance.map)
            {
                Debug.Log(mapVal);
            }

            Debug.Log(tileCube);
            if (tileCube.unitId != -1)
            {
                chooseUnit(tileCube);
            }
            else if (unit == null || unit.inRangeTiles == null || !unit.inRangeTiles.Contains(tileCube))
            {
                CreateUnit(tileCube);
            }
            else if (unit && unit.isChosen && !unit.isMoving)
            {
                cursor.transform.position = new Vector3(tileCube.transform.position.x, tileCube.transform.position.y + 0.55f, tileCube.transform.position.z);
                unit.focusedTile = tileCube;
                tileObj.layer = LayerMask.NameToLayer("Clicked");
                unit.path = pathfinder.FindPath(Board.instance.map[unit.standingOn], tileCube);
            }
        }
    }

    private void chooseUnit(TileCube tileCube)
    {
        if (unit)
            unit.Deselect();

        Board.instance.unitIdToUnit.TryGetValue(tileCube.unitId, out unit);

        if (unit != null)
            unit.Select(tileCube);
        else
            Debug.Log("ChooseUnit: tileCube.unitId is wrong");
    }

    private void CreateUnit(TileCube tileCube)
    {
        if (unit)
            unit.Deselect();

        CreateUnitServerRpc(tileCube.grid2DLocation);
        chooseUnit(tileCube);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreateUnitServerRpc(Vector2Int location)
    {
        CreateUnitClientRpc(location);
    }

    [ClientRpc]
    private void CreateUnitClientRpc(Vector2Int location)
    {
        var tileMap = Board.instance.map;
        if (tileMap.ContainsKey(location))
        {
            var tile = tileMap[location];
            unit = Instantiate(unitPrefab).GetComponent<Unit>();
            unit.PositionCharacterOnTile(tile);
            unit.NetworkObject.Spawn();

            tile.unitId = idForUnit.Value;
            unit.uniqueId = idForUnit.Value;
            Board.instance.unitIdToUnit.Add(idForUnit.Value, unit);
            idForUnit.Value++;
        }
        else
        {
            Debug.Log("No location :(");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!IsOwner)
            return;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask previousLayer = LayerMask.NameToLayer("Tile");
        if (Physics.Raycast(ray, out hit, LayerMask.GetMask("Tile")))
        {
            if (currentHover == null)
            {
                currentHover = hit.collider.gameObject;
                previousLayer = currentHover.layer;
                hit.collider.gameObject.layer = LayerMask.NameToLayer("Hover");
            }
            if (currentHover != hit.collider.gameObject)
            {
                currentHover.layer = previousLayer;
                currentHover = hit.collider.gameObject;
                hit.collider.gameObject.layer = LayerMask.NameToLayer("Hover");
            }
        }
        else
        {
            if (currentHover != null && currentHover.layer == LayerMask.NameToLayer("Hover"))
            {
                currentHover.layer = LayerMask.NameToLayer("Tile");
                currentHover = null;
            }
        }
    }

    public RaycastHit? GetFocusedTile()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, LayerMask.GetMask("Hover")))
        {
            return hit;
        }
        return null;
    }

    public RaycastHit? GetFocusedUnit()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, LayerMask.GetMask("Unit")))
        {
            return hit;
        }
        return null;
    }
}