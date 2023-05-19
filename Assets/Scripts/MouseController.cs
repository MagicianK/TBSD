using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MouseStates
{
    public abstract class IMouseState : IState
    {
        public MouseController controller;

        public IMouseState(MouseController controller)
        {
            this.controller = controller;
        }

        public virtual void Enter()
        {
        }

        public virtual void Execute()
        {
        }

        public virtual void Exit()
        {
        }
    }

    public class Idle : IState
    {
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

    public class OnPlayerBaseState : IMouseState
    {
        private Player playerBase;

        public OnPlayerBaseState(MouseController controller, Player playerBase) : base(controller)
        {
            this.playerBase = playerBase;
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                controller.mouseStateMachine.ChangeState(new MouseStates.Idle());
        }

        public override void Exit()
        {
            playerBase.stateMachine.ChangeState(new PlayerBaseStates.Idle(playerBase));
        }
    }

    public class OnUnitState : IMouseState
    {
        public OnUnitState(MouseController controller) : base(controller)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                controller.mouseStateMachine.ChangeState(new MouseStates.Idle());
        }

        public override void Exit()
        {
            Unit selectedUnit = controller.selectedUnit;
            selectedUnit.stateMachine.ChangeState(new UnitStates.Idle(selectedUnit));
            selectedUnit.ClearRange();
            controller.selectedUnit = null;
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

public class MouseController : NetworkBehaviour, INetworkSerializable
{
    // Change state of the mouse in UPDATE()
    // Handle changes in UPDATE()
    public StateMachine mouseStateMachine = new StateMachine();

    public GameObject cursor;
    public Unit selectedUnit;
    [SerializeField] private Unit unitPrefab;
    public Unit unitToPlace;
    private TileCube clickedTile;
    public Unit clickedUnit;

    private void Awake()
    {
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        mouseStateMachine.ChangeState(new MouseStates.Idle());

        StartCoroutine(AssignPlayers());
    }

    private IEnumerator AssignPlayers()
    {
        while (!GameManager.instance.isPlayerBasesCreated.Value)
        {
            yield return null;
        }
        AssignPlayersServerRpc();
    }

    [ServerRpc]
    public void AssignPlayersServerRpc()
    {
        AssignPlayersClientRpc();
    }

    [ClientRpc]
    public void AssignPlayersClientRpc()
    {
        Debug.Log("Assigning Players");
        if (IsHost)
        {
            Debug.Log("Assigned Host");
            GameManager.instance.playerBase0.mouseController = this;
        }
        else
        {
            Debug.Log("Assigning Client");
            GameManager.instance.playerBase1.mouseController = this;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
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

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
    }
}