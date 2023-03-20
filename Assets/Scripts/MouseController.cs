using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    public GameObject cursor;
    public float speed;
    public int movementRange = 3;

    // public GameObject cursor;
    public GameObject currentHover { get; private set; }

    public GameObject currentClicked { get; private set; }
    private PathFinding pathfinder;
    private RangeFinder rangeFinder;
    [SerializeField] private Unit unitPrefab;
    private Unit unit;
    private List<TileCube> path = new List<TileCube>();
    private List<TileCube> inRangeTiles = new List<TileCube>();

    // Start is called before the first frame update
    private void Start()
    {
        currentHover = null;
        currentClicked = null;
        pathfinder = new PathFinding();
        rangeFinder = new RangeFinder();
        path = new List<TileCube>();
    }

    private void LateUpdate()
    {
        inRangeTiles = unit == null ? inRangeTiles : GetInRangeTiles();
        var focusedTileHit = GetFocusedTile();
        var focusedUnitHit = GetFocusedUnit();
        if (focusedUnitHit.HasValue)
        {
            if(Input.GetKeyDown(KeyCode.F))
                Debug.Log("Focused tile: " + focusedTileHit.Value.collider.GetComponent<TileCube>().GetUnitInfo());
            Unit currentUnit = focusedUnitHit.Value.collider.GetComponentInParent<Unit>();
            if (!Input.GetMouseButtonUp(0))
                return;

            if (currentUnit)
            {
                chooseUnit(currentUnit.standingOn);
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

            if (tileCube.unit)
            {
                chooseUnit(tileCube);
            }
            else if (unit == null || inRangeTiles == null || !inRangeTiles.Contains(tileCube))
            {
                CreateUnit(tileCube);
            }
            else if (unit && unit.isChosen && !unit.isMoving)
            {
                cursor.transform.position = new Vector3(tileCube.transform.position.x, tileCube.transform.position.y + 0.55f, tileCube.transform.position.z);
                cursor.GetComponent<Cursor>().SetFocusedTile(tileCube);
                tileObj.layer = LayerMask.NameToLayer("Clicked");
                path = pathfinder.FindPath(unit.standingOn, tileCube);
            }
        }
    }

    private void chooseUnit(TileCube tileCube)
    {
        if (unit)
            unit.isChosen = false;

        unit = tileCube.unit;
        unit.isChosen = true;
    }

    private void CreateUnit(TileCube tileCube)
    {
        unit = Instantiate(unitPrefab).GetComponent<Unit>();
        PositionCharacterOnTile(tileCube);
        chooseUnit(tileCube);
    }

    private List<TileCube> GetInRangeTiles()
    {
        foreach (var item in inRangeTiles)
        {
            if (item.gameObject.layer != LayerMask.NameToLayer("Hover"))
                item.ChangeLayer(LayerMask.NameToLayer("Tile"));
        }

        inRangeTiles = rangeFinder.GetTilesRange(unit.standingOn, movementRange);

        foreach (var item in inRangeTiles)
        {
            if (item.gameObject.layer != LayerMask.NameToLayer("Hover"))
                item.ChangeLayer(LayerMask.NameToLayer("RangeShow"));
        }
        return inRangeTiles;
    }

    // CHANGE: Field isBlocked of TileCube is now changing before Unit movement
    private void MoveAlongPath()
    {
        var step = speed * Time.deltaTime;

        var yIndex = path[0].transform.position.y;
        unit.standingOn.unit = null;
        unit.standingOn.isBlocked = false;
        unit.transform.position = Vector3.MoveTowards(unit.transform.position, path[0].transform.position, step);
        unit.transform.position = new Vector3(unit.transform.position.x, yIndex, unit.transform.position.z);

        if (Vector3.Distance(unit.transform.position, path[0].transform.position) < 0.00001f)
        {
            PositionCharacterOnTile(path[0]);
            path.RemoveAt(0);
        }

        if (path.Count == 0)
        {
            GetInRangeTiles();
            unit.isMoving = false;
        }
    }

    private void PositionCharacterOnTile(TileCube tile)
    {
        unit.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z);
        //unit.GetComponent<MeshRenderer>().sortingOrder = tile.GetComponent<MeshRenderer>().sortingOrder;
        unit.standingOn = tile;
        tile.unit = unit;
    }

    // Update is called once per frame
    private void Update()
    {
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

        if (path.Count > 0 && inRangeTiles.Contains(cursor.GetComponent<Cursor>().GetFocusedTile()))
        {
            unit.isMoving = true;
            MoveAlongPath();
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