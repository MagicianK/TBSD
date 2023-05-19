using MouseStates;
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
            GameManager.instance.turnSystem.MakeTurn();
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
                }

                if (prey != null && prey.GetPreyTeam() != stateOwner.team && stateOwner.inRangeTiles.Contains(prey.GetStandingOnTile()))
                    stateOwner.stateMachine.ChangeState(new Attack(stateOwner, prey));
            }
        }

        public void Exit()
        {
            stateOwner.ClearRange();
        }
    }

    public class PrepareToMove : IMouseState
    {
        private Unit stateOwner;

        public PrepareToMove(MouseController controller, Unit stateOwner) : base(controller)
        { this.stateOwner = stateOwner; }

        public override void Enter()
        {
            stateOwner.GetInRangeTiles();
            Debug.Log("Prepare to move");
        }

        public override void Execute()
        {
            if (Input.GetMouseButtonUp(0))
            {
                var focusedHit = controller.GetFocusedTile();
                if (focusedHit.HasValue)
                {
                    TileCube tc = focusedHit.Value.collider.gameObject.GetComponent<TileCube>();
                    CanIgoThere(tc);
                }
            }
        }

        private void CanIgoThere(TileCube tc)
        {
            if (stateOwner.inRangeTiles.Contains(tc) && !tc.isBlocked)
            {
                stateOwner.path = PathFinding.FindPath(stateOwner.standingOn, tc);
                stateOwner.stateMachine.ChangeState(new Moving(stateOwner));
            }
        }

        public override void Exit()
        {
        }
    }

    public class Selected : IState
    {
        private Unit owner;

        public Selected(Unit owner)
        { this.owner = owner; }

        public void Enter()
        {
            SelectedView.instance.MoveTo(owner.transform.position);
        }

        public void Execute()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                owner.stateMachine.ChangeState(new PrepareToMove(owner.mouseController, owner));
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                owner.stateMachine.ChangeState(new InCharge(owner));
            }
        }

        public void Exit()
        {
        }
    }
}

public class Unit : NetworkBehaviour, INetworkSerializable
{
    public Player owner;
    public StateMachine stateMachine = new StateMachine();
    public int team;
    public TileCube standingOn;
    public List<TileCube> path { get; set; }
    public List<TileCube> inRangeTiles { get; private set; }
    [SerializeField]
    private UnitData unitData;

    public MouseController mouseController;

    public int maxHealth;
    private Color startColor;
    private const float MOVEMENT_ANIMATION_SPEED = 10f;

    public void InitValues(int team, Player owner)
    {
        this.team = team;
        this.owner = owner;
    }

    private void Awake()
    {
        maxHealth = unitData.Health;
        path = new List<TileCube>();
        inRangeTiles = new List<TileCube>();
    }

    private void Start()
    {
        startColor = GetComponentInChildren<Renderer>().material.color;
    }

    private void OnMouseDown()
    {
        if (IsOwner)
        {
            Debug.Log("Clicked by owner");
        }
        /*
        // If mouse is in Idle state unit can be selected
        if (MouseController.instance.mouseStateMachine.currentState is MouseStates.Idle)
        {
            stateMachine.ChangeState(new UnitStates.Selected(this));
            MouseController.instance.selectedUnit = this;
            MouseController.instance.mouseStateMachine.ChangeState(new MouseStates.OnUnitState());
        }
        */
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

    public override void OnNetworkDespawn()
    {
        if (IsServer)
            owner.units.Remove(this);
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
        if (path.Count == 1)
        {
            standingOn = path[0];
            standingOn.unit = this;
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

        RangeFinder.GetTilesRangeServerRpc(standingOn.grid2DLocation, unitData.MovementRange, out List<TileCube> tempList);
        inRangeTiles = tempList;

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

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref owner);
        serializer.SerializeValue(ref team);
        serializer.SerializeValue(ref standingOn);
        serializer.SerializeValue(ref unitData);
        serializer.SerializeValue(ref mouseController);
        serializer.SerializeValue(ref maxHealth);
        serializer.SerializeValue(ref startColor);
    }
}