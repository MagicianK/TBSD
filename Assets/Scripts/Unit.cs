using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public int team;
    public TileCube standingOn { get; private set; }
    public TileCube focusedTile;
    public List<TileCube> path { get; set; }
    public MouseController mouse { get; private set; }
    public List<TileCube> inRangeTiles { get; private set; }

    private RangeFinder rangeFinder;
    public bool isMoving { get; set; } = false;
    public bool isChosen { get; set; } = false;
    public int movementRange;
    public float speed;

    private void Awake()
    {
        rangeFinder = new RangeFinder();
        path = new List<TileCube>();
        inRangeTiles = new List<TileCube>();
    }

    private void Start()
    {
    }

    // For now it only works for movement and showing available tiles for the unit
    private void Update()
    {
        if (isChosen)
        {
            GetInRangeTiles();

            if (path.Count > 0 && inRangeTiles.Contains(focusedTile))
            {
                isMoving = true;
                MoveAlongPath();
            }
        }
    }

    // Deletes selected state to the unit
    public void Deselect()
    {
        isChosen = false;
        foreach (var item in inRangeTiles)
        {
            item.ChangeLayer(LayerMask.NameToLayer("Tile"));
        }
    }

    // Assigns selected state to the unit
    public void Select(TileCube tile)
    {
        isChosen = true;
        standingOn = tile;

        GetInRangeTiles();
    }

    // Moves the Unit along retrieved path from PathFinding script
    public void MoveAlongPath()
    {
        var step = speed * Time.deltaTime;

        var yIndex = path[0].transform.position.y;
        standingOn.unit = null;
        standingOn.isBlocked = false;
        transform.position = Vector3.MoveTowards(transform.position, path[0].transform.position, step);
        transform.position = new Vector3(transform.position.x, yIndex, transform.position.z);

        if (Vector3.Distance(transform.position, path[0].transform.position) < 0.00001f)
        {
            PositionCharacterOnTile(path[0]);
            path.RemoveAt(0);
        }

        if (path.Count == 0)
        {
            GetInRangeTiles();
            isMoving = false;
        }
    }

    // Returns a list of tiles that are available tiles to go for the unit
    // Also sets those tiles to the "RangeShow" layer 
    public List<TileCube> GetInRangeTiles()
    {
        foreach (var item in inRangeTiles)
        {
            if (item.gameObject.layer != LayerMask.NameToLayer("Hover"))
                item.ChangeLayer(LayerMask.NameToLayer("Tile"));
        }
        if (rangeFinder != null)
            inRangeTiles = rangeFinder.GetTilesRange(standingOn, movementRange);

        foreach (var item in inRangeTiles)
        {
            if (item.gameObject.layer != LayerMask.NameToLayer("Hover"))
                item.ChangeLayer(LayerMask.NameToLayer("RangeShow"));
        }
        return inRangeTiles;
    }

    // Assigns standing tile to the unit
    public void PositionCharacterOnTile(TileCube tile)
    {
        transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z);
        //unit.GetComponent<MeshRenderer>().sortingOrder = tile.GetComponent<MeshRenderer>().sortingOrder;
        standingOn = tile;
        tile.unit = this;
    }
}