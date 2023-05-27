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
}

public class MouseController : NetworkBehaviour
{
    // Server authorative base creation
    // Create bases locally and then in a server 

    public StateMachine mouseStateMachine = new StateMachine();
    public Unit selectedUnit { get; set; }
    public int team;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;
        if (IsHost)
            team = 0;
        if (IsClient && !IsHost)
            team = 1;
    }
    
    private void Update()
    {
        if (!IsOwner)
            return;

        mouseStateMachine.Update();
    }

    // Requires isOwner beyond within the body of the function
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