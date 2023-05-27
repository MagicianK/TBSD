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
}

public class MouseController : NetworkBehaviour
{
    // Server authorative base creation
    // Create bases locally and then in a server 

    public StateMachine mouseStateMachine = new StateMachine();
    public Unit selectedUnit { get; set; }
    public NetworkVariable<int> team = new NetworkVariable<int>();
    public TileCube clickedTile;
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;
        if (IsHost)
            team.Value = 0;
        if (IsClient && !IsHost)
            SetTeamServerRpc(1);
    }
    private void Start()
    {
        mouseStateMachine.ChangeState(new MouseStates.Idle(this));
    }
    private void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetMouseButtonDown(0))
            clickedTile = GetFocusedTile().Value.collider.gameObject.GetComponent<TileCube>();
        
        mouseStateMachine.Update();
    }

    [ServerRpc]
    void SetTeamServerRpc(int team)
    {
        this.team.Value = team;
    }
    // Теперь эта функция не нужна думаю
    public RaycastHit? GetFocusedTile()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, LayerMask.GetMask("Tile")))
        {
            return hit;
        }
        return null;
    }

    // Ноль референсов я не знаю
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