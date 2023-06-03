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
            {
                stateOwner.mouseStateMachine.ChangeState(new MouseStates.Idle(stateOwner));
                Debug.LogWarning("Exited");
            }
        }
        public void Exit()
        {
            stateOwner.playerBase.stateMachine.ChangeState(new PlayerBaseStates.Idle(stateOwner.playerBase));
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

    public StateMachine mouseStateMachine = new StateMachine();
    public Unit selectedUnit { get; set; }
    public NetworkVariable<int> team = new NetworkVariable<int>();
    public TileCube clickedTile;
    public PlayerBase playerBase;
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
        
        mouseStateMachine.Update();
    }

    [ServerRpc]
    void SetTeamServerRpc(int team)
    {
        this.team.Value = team;
    }
}