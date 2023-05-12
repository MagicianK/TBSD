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
        owner.standingOn.unit = null;
        owner.standingOn.isBlocked = false;
    }
 
    public void Execute()
    {
        owner.MoveAlongPath();
    }
 
    public void Exit()
    {
        owner.ClearRange();
        Debug.Log("UNIT IS NOT MOVING");
    }
}

public class UnitInChargeState : IState
{
    Unit owner;
 
    public UnitInChargeState(Unit owner) { this.owner = owner; }
    
    public void Enter()
    {
        Debug.Log("Unit is in charge");
        owner.GetInRangeTiles();
    }
 
    public void Execute()
    {
        if (Input.GetMouseButtonUp(0))
        {
            IDamagable prey = null;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                prey = hit.collider.gameObject.GetComponent<IDamagable>();
                Debug.Log("YOU HIT SOMEONE " + prey.GetType());
            }
            
            if(prey != null && owner.inRangeTiles.Contains(prey.GetStandingOnTile()))
            {
                prey.TakeDamage(1);
                owner.stateMachine.ChangeState(new UnitSelected(owner));
            }
        }
    }
    
    public void Exit()
    {
        owner.ClearRange();
        Debug.Log("Unit is cool from charge");
    }
}
public class UnitPrepareToMove : IState
{
    Unit owner;
 
    public UnitPrepareToMove(Unit owner) { this.owner = owner; }
    
    public void Enter()
    {
        Debug.Log("Unit is preparing to move");
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
                CanIgoThere(tc);
            }
        }
    }
    private void CanIgoThere(TileCube tc)
    {
        if(owner.inRangeTiles.Contains(tc) && !tc.isBlocked){
            owner.path = PathFinding.FindPath(owner.standingOn, tc);
            owner.stateMachine.ChangeState(new UnitMoveState(owner));
        }
    }
    public void Exit()
    {
        Debug.Log("Unit is prepared to move");
    }
}

public class UnitSelected : IState
{
    Unit owner;
 
    public UnitSelected(Unit owner) { this.owner = owner; }
    
    public void Enter()
    {
        Debug.Log("Unit is selected");
    }
 
    public void Execute()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            owner.stateMachine.ChangeState(new UnitPrepareToMove(owner));
        }
        else if(Input.GetKeyDown(KeyCode.A))
        {
            owner.stateMachine.ChangeState(new UnitInChargeState(owner));
        }
    }
 
    public void Exit()
    {
        Debug.Log("Unit is unselected");
    }
}
public class Unit : MonoBehaviour, IDamagable, IHealable
{

    public StateMachine stateMachine = new StateMachine();
    public Player team;
    public TileCube standingOn { get; set; }    
    public List<TileCube> path { get; set; }
    public List<TileCube> inRangeTiles { get; private set; }
    [SerializeField]
    private UnitData unitData;
    private int health;
    private int maxHealth;
    private const float MOVEMENT_ANIMATION_SPEED = 10f;
    private void Awake()
    {
        maxHealth = unitData.Health;
        health = maxHealth;
        path = new List<TileCube>();
        inRangeTiles = new List<TileCube>();
    }
    public TileCube GetStandingOnTile(){
        return standingOn;
    }
    private void Start()
    {
    }

    private void OnMouseDown() {
        // If mouse is in Idle state unit can be selected
        if(MouseController.instance.mouseStateMachine.currentState is Idle)
        {
            stateMachine.ChangeState(new UnitSelected(this));
            MouseController.instance.selectedUnit = this;
            MouseController.instance.mouseStateMachine.ChangeState(new OnUnitState());
        }
    }
    private void Update()
    {
        stateMachine.Update();
    }

    // Clears range of attack or movement
    public void ClearRange()
    {
        foreach (var item in inRangeTiles)
        {
            item.ChangeLayer(LayerMask.NameToLayer("Tile"));
        }
    }

    // Moves the Unit along retrieved path from PathFinding script
    public void MoveAlongPath()
    {
        var step = MOVEMENT_ANIMATION_SPEED * Time.deltaTime;

        var yIndex = path[0].transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, path[0].transform.position, step);
        transform.position = new Vector3(transform.position.x, yIndex, transform.position.z);

        if (Vector3.Distance(transform.position, path[0].transform.position) < 0.00001f)
        {
            PositionCharacterOnTile(path[0]);
            path.RemoveAt(0);
        }
        if (path.Count == 1){
            standingOn = path[0];
            standingOn.unit = this;
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

        inRangeTiles = RangeFinder.GetTilesRange(standingOn, unitData.MovementRange);

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
    }

    public void TakeDamage(int damage){
        this.health -= damage;
        if(this.health <= 0)
            Destroy(gameObject);
    }

    public void TakeHeal(int heal){
        if (this.health != this.maxHealth)
            this.health += heal;
    }
}