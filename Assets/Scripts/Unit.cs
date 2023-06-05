using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;

namespace UnitStates
{
    public class Death : IState
    {
        private Unit stateOwner;

        public Death(Unit owner)
        { this.stateOwner = owner; }

        public void Enter()
        {
            stateOwner.owner.units.Remove(stateOwner);
        }

        public void Execute()
        {
        }

        public void Exit()
        {
        }
    }

    public class Idle : IState
    {
        private Unit stateOwner;

        public Idle(Unit stateOwner)
        { this.stateOwner = stateOwner; }

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
        private Unit stateOwner;

        public Moving(Unit stateOwner)
        { this.stateOwner = stateOwner; }

        public void Enter()
        {
            BoardManager.instance.UnblockTileServerRpc(stateOwner.standingOn.Value);
            TurnManager.instance.StartTurnServerRpc();
        }

        public void Execute()
        {

            stateOwner.MoveAlongPath();
        }

        public void Exit()
        {
            TurnManager.instance.EndTurnServerRpc();
            Debug.Log("Stopped moving");
            //GameManager.instance.turnSystem.MakeTurn();
            stateOwner.ClearRange();
        }
    }

    public class Attack : IState
    {
        private Unit stateOwner;
        private IDamagable prey;

        public Attack(Unit stateOwner, IDamagable prey)
        {
            this.stateOwner = stateOwner;
            this.prey = prey;
        }

        public void Enter()
        {
            stateOwner.GetInRangeTiles();
        }

        public void Execute()
        {
            prey.TakeDamage(5);
            stateOwner.stateMachine.ChangeState(new Selected(stateOwner));
        }

        public void Exit()
        {
            //GameManager.instance.turnSystem.MakeTurn();
            stateOwner.ClearRange();
        }
    }

    public class InCharge : IState
    {
        private Unit stateOwner;

        public InCharge(Unit stateOwner)
        { this.stateOwner = stateOwner; }

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
                    Debug.LogWarning("Where Am I: " + prey.WhereAmI());
                    Debug.LogWarning("Prey team: " + prey.GetPreyTeam());
                    Debug.LogWarning("Prey: " + prey);
                }

                if (prey != null && 
                    prey.GetPreyTeam() != stateOwner.team.Value && 
                    stateOwner.inRangeTiles.Contains(BoardManager.instance.GetTileAtPosition(prey.WhereAmI())))
                    stateOwner.stateMachine.ChangeState(new Attack(stateOwner, prey));
            }
        }

        public void Exit()
        {
            stateOwner.ClearRange();
        }
    }

    public class PrepareToMove : IState
    {
        private Unit stateOwner;

        public PrepareToMove(Unit stateOwner)
        { this.stateOwner = stateOwner; }
        
        public void Enter()
        {
            stateOwner.GetInRangeTiles();
            Debug.Log("Prepare to move");
        }

        public void Execute()
        {
            if (Input.GetMouseButtonDown(0))
            {
                TileCube tc = BoardManager.instance.GetTileAtPosition(Cursor.instance.coord);
                CanIgoThere(tc);
            }
            if(Input.GetKeyDown(KeyCode.Escape))
                stateOwner.stateMachine.ChangeState(new Selected(stateOwner));
        }

        private void CanIgoThere(TileCube tc)
        {
            if (stateOwner.inRangeTiles.Contains(tc) && !tc.isBlocked.Value)
            {
                List<TileCube> path = new List<TileCube>();
                stateOwner.FindPath(tc, ref path);
                stateOwner.path = path;
                if(path.Count > 0)
                    stateOwner.stateMachine.ChangeState(new Moving(stateOwner));
            }
        }

        public void Exit()
        {
        }
    }
    public class OnAbilityState : IState
    {
        Unit stateOwner;
    
        public OnAbilityState(Unit stateOwner) { this.stateOwner = stateOwner; }
        
        public void Enter()
        {
            stateOwner.ability.Activate(stateOwner);
        }
    
        public void Execute()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
                stateOwner.stateMachine.ChangeState(new Selected(stateOwner));
                
        }
    
        public void Exit()
        {
            stateOwner.ability.Deactivate();
        }
    }
    public class Selected : IState
    {
        private Unit owner;

        public Selected(Unit owner)
        { this.owner = owner; }

        public void Enter()
        {
            Debug.LogWarning("Owner is null?: " + (owner == null));
            SelectedView.instance.MoveTo(owner.transform.position);
        }

        public void Execute()
        {
            
            if (Input.GetKeyDown(KeyCode.M))
            {
                owner.stateMachine.ChangeState(new PrepareToMove(owner));
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                owner.stateMachine.ChangeState(new InCharge(owner));
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                owner.stateMachine.ChangeState(new OnAbilityState(owner));
            }
        }

        public void Exit()
        {
        }
    }
}

public class Unit : NetworkBehaviour, ISwitchable
{
    const float MOVEMENT_ANIMATION_SPEED = 10f;

    // Local data
    public PlayerBase owner;
    public StateMachine stateMachine = new StateMachine();
    public NetworkVariable<Vector2Int> standingOn = new NetworkVariable<Vector2Int>(default, NetworkVariableReadPermission.Everyone);
    public List<TileCube> path { get; set; }
    public List<TileCube> inRangeTiles { get; private set; }
    public PathFinder pathFinder;
    public NetworkVariable<int> team = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public IAbility ability;
    
    [SerializeField]
    public UnitData unitData;
    MouseController mouseController;
    Color startColor;
    HealthSystem healthSystem;


    public void InitValues(Vector2Int pos)
    {
        ChangePositionServerRpc(pos);
    }
    private void Start()
    {
        ability = GetComponent<IAbility>();
        healthSystem = GetComponent<HealthSystem>();
        pathFinder = new PathFinder();
        path = new List<TileCube>();
        startColor = GetComponentInChildren<Renderer>().material.color;
        inRangeTiles = new List<TileCube>();
        if (IsOwner)
            mouseController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<MouseController>();
    }
    public void FindPath(TileCube tc, ref List<TileCube> path)
    {

        path = pathFinder.FindPath(inRangeTiles, BoardManager.instance.GetTileAtPosition(standingOn.Value), tc);
    }
    private void OnMouseDown()
    {
        if (!IsOwner)
            return;
        // If mouse is in Idle state unit can be selected
        if (mouseController.mouseStateMachine.currentState is MouseStates.Idle)
        {
            stateMachine.ChangeState(new UnitStates.Selected(this));
            mouseController.selectedUnit = this;
            mouseController.mouseStateMachine.ChangeState(new MouseStates.OnUnitState(mouseController));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTeamServerRpc(int team)
    {
        this.team.Value = team;
    }
    private void OnMouseEnter()
    {
        GetComponentInChildren<Renderer>().material.color = Color.white;
    }

    private void OnMouseExit()
    {
        GetComponentInChildren<Renderer>().material.color = startColor;
    }

    private void Update()
    {
        stateMachine.Update();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRpc()
    {
        Destroy(gameObject);
        NetworkObject.Despawn();
    }
    public override void OnNetworkDespawn()
    {
        Debug.Log("Ive been Destroyed");
    }

    // Clears range of attack or movement
    public void ClearRange()
    {
        foreach (var item in inRangeTiles)
        {
            item.ChangeLayer(LayerMask.NameToLayer("Tile"));
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void ChangePositionServerRpc(Vector2Int pos)
    {
        standingOn.Value = pos;
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
        if (path.Count == 1)
        {
            ChangePositionServerRpc(path[0].coord.Value);
            //standingOn.unit = this;
            BoardManager.instance.BlockTileServerRpc(standingOn.Value);
        }
        if (path.Count == 0)
        {
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

        //RangeFinder.GetTilesRangeServerRpc(standingOn.grid2DLocation, unitData.MovementRange, out List<TileCube> tempList);
        //inRangeTiles = tempList;
        inRangeTiles = RangeFinder.GetTilesRange(standingOn.Value, unitData.MovementRange);
        
        //inRangeTiles = RangeFinder.GetTilesRange(standingOn, unitData.MovementRange);
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

    public TileCube WhereAmI()
    {
        return BoardManager.instance.GetTileAtPosition(standingOn.Value);
    }

    public int GetPreyTeam()
    {
        return team.Value;
    }

    public Vector2Int GetStandingOnTile()
    {
        return standingOn.Value;
    }

    public void Switch(Vector2Int dest)
    {
        ChangePositionServerRpc(dest);
        TileCube tile = BoardManager.instance.GetTileAtPosition(dest);
        transform.position = new Vector3(tile.transform.position.x, this.transform.position.y, tile.transform.position.z);
    }
}