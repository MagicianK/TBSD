using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    public GameObject cursor;

    // public GameObject cursor;
    public GameObject currentHover { get; private set; }

    public GameObject currentClicked { get; private set; }
    private PathFinding pathfinder;
    [SerializeField] private Unit unitPrefab;
    private Unit unit;

    // Start is called before the first frame update
    private void Start()
    {
        currentHover = null;
        currentClicked = null;
        pathfinder = new PathFinding();
    }

    private void LateUpdate()
    {
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
            else if (unit == null || unit.inRangeTiles == null || !unit.inRangeTiles.Contains(tileCube))
            {
                CreateUnit(tileCube);
            }
            else if (unit && unit.isChosen && !unit.isMoving)
            {
                cursor.transform.position = new Vector3(tileCube.transform.position.x, tileCube.transform.position.y + 0.55f, tileCube.transform.position.z);
                cursor.GetComponent<Cursor>().SetFocusedTile(tileCube);
                unit.focusedTile = tileCube;
                tileObj.layer = LayerMask.NameToLayer("Clicked");
                unit.path = pathfinder.FindPath(unit.standingOn, tileCube);
            }
        }
    }

    private void chooseUnit(TileCube tileCube)
    {
        if (unit)
            unit.Deselect();

        unit = tileCube.unit;
        unit.Select(tileCube);
    }

    private void CreateUnit(TileCube tileCube)
    {
        if (unit)
            unit.Deselect();

        unit = Instantiate(unitPrefab).GetComponent<Unit>();
        unit.PositionCharacterOnTile(tileCube);
        chooseUnit(tileCube);
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