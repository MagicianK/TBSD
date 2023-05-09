using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Unit is selected
// Unit is in state moving 
// Unit is in state wants to move
// Unit is in state wants to attack
// 

public class UnitIdle : IState
{
    Unit owner;
 
    public UnitIdle(Unit owner) { this.owner = owner; }
    
    public void Enter()
    {
        Debug.Log("UNIT IS IDLE");

    }
 
    public void Execute()
    {
    }
 
    public void Exit()
    {
        Debug.Log("UNIT IS ACTIVE!!!!");
    }
}

public class UnitMoveState : IState
{
    Unit owner;
 
    public UnitMoveState(Unit owner) { this.owner = owner; }
    
    public void Enter()
    {
        Debug.Log("UNIT IS MOVING");
        //owner.GetInRangeTiles();
    }
 
    public void Execute()
    {
        owner.MoveAlongPath();
    }
 
    public void Exit()
    {
        owner.Deselect();
        Debug.Log("UNIT IS NOT MOVING");
    }
}

public class UnitPrepareToMove : IState
{
    Unit owner;
 
    public UnitPrepareToMove(Unit owner) { this.owner = owner; }
    
    public void Enter()
    {
        owner.GetInRangeTiles();
    }
 
    public void Execute()
    {
        if (Input.GetMouseButtonUp(0))
        {
            var focusedHit = MouseController.instance.GetFocusedTile();
            if (focusedHit.HasValue)
            {
                TileCube tc = focusedHit.Value.collider.gameObject.GetComponent<TileCube>();
                if(owner.inRangeTiles.Contains(tc)){
                    owner.path = PathFinding.FindPath(owner.standingOn, tc);
                    owner.stateMachine.ChangeState(new UnitMoveState(owner));
                }
            }
        }
    }
 
    public void Exit()
    {
        Debug.Log("exiting test state");
    }
}

public class UnitSelected : IState
{
    Unit owner;
 
    public UnitSelected(Unit owner) { this.owner = owner; }
    
    public void Enter()
    {
        Debug.Log("entering test state");
    }
 
    public void Execute()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            owner.stateMachine.ChangeState(new UnitPrepareToMove(owner));
        }
    }
 
    public void Exit()
    {
        Debug.Log("exiting test state");
    }
}
public class Unit : MonoBehaviour
{
    public StateMachine stateMachine = new StateMachine();
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
    public float speed = 10.0f;

    private void Awake()
    {
        rangeFinder = new RangeFinder();
        path = new List<TileCube>();
        inRangeTiles = new List<TileCube>();
    }

    private void Start()
    {
    }

    
    private void Update()
    {
        stateMachine.Update();
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
            stateMachine.ChangeState(new UnitSelected(this));
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