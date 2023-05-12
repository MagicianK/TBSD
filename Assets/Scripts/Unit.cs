using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace UnitStates{

    public class Death : IState
    {
        Unit stateOwner;
    
        public Death(Unit owner) { this.stateOwner = owner; }
        
        public void Enter()
        {
            stateOwner.owner.units.Remove(stateOwner);
        }
    
        public void Execute()
        {
            stateOwner.Dead();
        }
    
        public void Exit()
        {
        }
    }
    public class Idle : IState
    {
        Unit stateOwner;
    
        public Idle(Unit stateOwner) { this.stateOwner = stateOwner; }
        
        public void Enter()
        {

        }
    
        public void Execute()
        {
        }
    
        public void Exit()
        {
        }
    }

    // BUG: Exit() procedure called two times. 
    public class Moving : IState
    {
        Unit stateOwner;
    
        public Moving(Unit stateOwner) { this.stateOwner = stateOwner; }
        
        public void Enter()
        {
            stateOwner.standingOn.unit = null;
            stateOwner.standingOn.isBlocked = false;
        }
    
        public void Execute()
        {
            stateOwner.MoveAlongPath();
        }
    
        public void Exit()
        {
            Debug.Log("Stopped moving");
            GameManager.instance.turnSystem.MakeTurn();
            stateOwner.ClearRange();
        }
    }

    public class InCharge : IState
    {
        Unit stateOwner;
    
        public InCharge(Unit stateOwner) { this.stateOwner = stateOwner; }
        
        public void Enter()
        {
            stateOwner.GetInRangeTiles();
        }
    
        public void Execute()
        {
            //Attack
            if (Input.GetMouseButtonUp(0))
            {
                IDamagable prey = null;
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    prey = hit.collider.gameObject.GetComponent<IDamagable>();
                }
                
                if(prey != null && prey.GetPreyTeam() != stateOwner.team && stateOwner.inRangeTiles.Contains(prey.GetStandingOnTile()))
                {
                    prey.TakeDamage(5);
                    stateOwner.stateMachine.ChangeState(new Selected(stateOwner));
                }
            }
        }
        
        public void Exit()
        {
            GameManager.instance.turnSystem.MakeTurn();
            stateOwner.ClearRange();
        }
    }
    public class PrepareToMove : IState
    {
        Unit stateOwner;
    
        public PrepareToMove(Unit stateOwner) { this.stateOwner = stateOwner; }
        
        public void Enter()
        {
            stateOwner.GetInRangeTiles();
            Debug.Log("Prepare to move");
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
            if(stateOwner.inRangeTiles.Contains(tc) && !tc.isBlocked){
                stateOwner.path = PathFinding.FindPath(stateOwner.standingOn, tc);
                stateOwner.stateMachine.ChangeState(new Moving(stateOwner));
            }
        }
        public void Exit()
        {
        }
    }

    public class Selected : IState
    {
        Unit owner;
    
        public Selected(Unit owner) { this.owner = owner; }
        
        public void Enter()
        {

        }
    
        public void Execute()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                owner.stateMachine.ChangeState(new PrepareToMove(owner));
            }
            else if(Input.GetKeyDown(KeyCode.A))
            {
                owner.stateMachine.ChangeState(new InCharge(owner));
            }
        }
    
        public void Exit()
        {

        }
    }
}
public class Unit : MonoBehaviour, IDamagable, IHealable
{
    public Player owner;
    public StateMachine stateMachine = new StateMachine();
    public int team;
    public TileCube standingOn { get; set; }    
    public List<TileCube> path { get; set; }
    public List<TileCube> inRangeTiles { get; private set; }
    [SerializeField]
    private UnitData unitData;
    private int health;
    private int maxHealth;
    Color startColor;
    private const float MOVEMENT_ANIMATION_SPEED = 10f;
    public void InitValues(int team, Player owner)
    {
        this.team = team;
        this.owner = owner;
    }
    private void Awake()
    {
        maxHealth = unitData.Health;
        health = maxHealth;
        path = new List<TileCube>();
        inRangeTiles = new List<TileCube>();
    }

    public int GetPreyTeam(){
        return this.team;
    }
    public TileCube GetStandingOnTile(){
        return standingOn;
    }
    private void Start()
    {
        startColor = GetComponentInChildren<Renderer>().material.color;
    }

    private void OnMouseDown() {
        // If mouse is in Idle state unit can be selected
        if(MouseController.instance.mouseStateMachine.currentState is MouseStates.Idle)
        {
            stateMachine.ChangeState(new UnitStates.Selected(this));
            MouseController.instance.selectedUnit = this;
            MouseController.instance.mouseStateMachine.ChangeState(new MouseStates.OnUnitState());
        }
    }
    private void OnMouseEnter() {
        GetComponentInChildren<Renderer>().material.color = Color.white;
    }
    private void OnMouseExit() {
       GetComponentInChildren<Renderer>().material.color = startColor; 
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
        if (path.Count == 0){
            Debug.Log("Path counted 0");
            stateMachine.ChangeState(new UnitStates.Selected(this));
        }
    }

    // Returns a list of tiles that are available to go for the unit
    // Also sets those tiles to the "RangeShow" layer
    // This is inappropriate and ugly way to do this due to side effect of function, but at least it works 
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

    // Position unit to the tile 
    public void PositionCharacterOnTile(TileCube tile)
    {
        transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z);
    }

    public void TakeDamage(int damage){
        FindObjectOfType<SoundPlayer>().Play("Damaged");
        this.health -= damage;
        if(this.health <= 0)
            this.stateMachine.ChangeState(new UnitStates.Death(this));
    }
    public void Dead()
    {
        Destroy(gameObject);
    }
    public void TakeHeal(int heal){
        if (this.health != this.maxHealth)
            this.health += heal;
    }
}