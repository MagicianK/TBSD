using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MouseStates
{
    public class Idle : IState
    {
        MouseController stateOwner;
        public Idle(MouseController owner)
        {
            stateOwner = owner;
        }
        public void Enter()
        {
            SelectedView.instance.MoveTo(new Vector3(0, -5, 0));
        }

        public void Execute()
        {
            // if(Input.GetKeyDown(KeyCode.P))
            // {
            //     MouseController.instance.mouseStateMachine.ChangeState(new UnitPlaceState());
            // }
        }

        public void Exit()
        {
        }
    }

    public class OnPlayerBaseState : IState
    {
        MouseController stateOwner;
        public OnPlayerBaseState(MouseController owner)
        {
            stateOwner = owner;
        }

        public void Enter()
        {
        }

        public void Execute()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                stateOwner.mouseStateMachine.ChangeState(new MouseStates.Idle(stateOwner));
        }

        public void Exit()
        {
        }
    }

    public class OnUnitState : IState
    {
        MouseController stateOwner;
        public OnUnitState(MouseController owner)
        {
            stateOwner = owner;
        }
        public void Enter()
        {
        }

        public void Execute()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                stateOwner.mouseStateMachine.ChangeState(new MouseStates.Idle(stateOwner));
        }

        public void Exit()
        {
            Unit selectedUnit = stateOwner.selectedUnit;
            selectedUnit.stateMachine.ChangeState(new UnitStates.Idle(selectedUnit));
            selectedUnit.ClearRange();
            stateOwner.selectedUnit = null;
        }
    }

    // public class UnitPlaceState : IState
    // {
    //     public void Enter()
    //     {
    //         if (MouseController.instance.selectedUnit != null)
    //         {
    //             MouseController.instance.selectedUnit.ClearRange();
    //             MouseController.instance.selectedUnit = null;
    //         }
    //         Debug.Log("YOU WANT TO PLACE UNIT");
    //     }

    //     public void Execute()
    //     {
    //         var focusedTileHit = Input.GetMouseButtonUp(0) ? MouseController.instance.GetFocusedTile() : null;
    //         if (Input.GetMouseButtonUp(0) && focusedTileHit.HasValue)
    //         {
    //             TileCube clickedTile = focusedTileHit.Value.collider.gameObject.GetComponent<TileCube>();
    //             MouseController.instance.CreateUnit(clickedTile);
    //             MouseController.instance.mouseStateMachine.ChangeState(new MouseStates.OnUnitState());
    //         }
    //     }

    //     public void Exit()
    //     {
    //         Unit selectedUnit = MouseController.instance.selectedUnit;
    //         selectedUnit.stateMachine.ChangeState(new UnitStates.Selected(selectedUnit));
    //         Debug.Log("IDLE");
    //     }
    // }
}

public class MouseController : NetworkBehaviour
{
    // Change state of the mouse in UPDATE()
    // Handle changes in UPDATE()
    public StateMachine mouseStateMachine = new StateMachine();

    //private static MouseController _instance;

    //public static MouseController instance
    //{ get { return _instance; } }

    public GameObject cursor;
    public Unit selectedUnit;
    [SerializeField] private Unit unitPrefab;
    public Unit unitToPlace;
    private TileCube clickedTile;
    public Unit clickedUnit;
    public int team;
    public BaseFactory baseFactory;
    
    //private void Awake()
    //{
    //    if (_instance != null && _instance != this)
    //    {
    //        Destroy(this.gameObject);
    //    }
    //    else
    //    {
    //        _instance = this;
    //    }
    //}

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;
        baseFactory = GetComponent<BaseFactory>();
        if (IsHost)
        {
            this.team = 0;
            StartCoroutine(WaitAndCreate(baseFactory.CreateFirstBaseClientRpc, this));
            //StartCoroutine(WaitAndCreateLocally(baseFactory.CreateFirstBase, this));
        }
        else if (IsClient && !IsHost)
        {
            this.team = 1;
            StartCoroutine(WaitAndCreate(baseFactory.CreateSecondBaseServerRpc, this));
            StartCoroutine(WaitAndCreateLocally(baseFactory.CreateSecondBase, this));
        }
    }
    private void Start()
    {
        if (!IsOwner)
            return;

        mouseStateMachine.ChangeState(new MouseStates.Idle(this));
    }
    public IEnumerator WaitAndCreateLocally(BaseFactory.CreateBaseX createBaseX, MouseController mc)
    {
        while (!Board.instance.isFilled)
        {
            yield return null;
        }
        createBaseX(mc);
    }
    public IEnumerator WaitAndCreate(BaseFactory.CreateBaseXRpc createBaseXRpc, NetworkBehaviourReference nbr)
    {
        while (!BoardManager.instance.isFilled)
        {
            yield return null;
        }
        createBaseXRpc(nbr);
    }

    private void LateUpdate()
    {
        if (!IsOwner)
            return;

        // Get clicked Tile or Unit
        var focusedTileHit = Input.GetMouseButtonUp(0) ? GetFocusedTile() : null;
        var focusedUnitHit = Input.GetMouseButtonUp(0) ? GetFocusedUnit() : null;

        // Handle Unit click
        if (focusedUnitHit != null && focusedUnitHit.HasValue)
        {
            clickedUnit = focusedUnitHit.Value.collider.GetComponentInParent<Unit>();
        }

        // Handle TileCube click
        if (focusedTileHit != null && focusedTileHit.HasValue)
        {
            clickedTile = focusedTileHit.Value.collider.gameObject.GetComponent<TileCube>();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!IsOwner)
            return;

        mouseStateMachine.Update();
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