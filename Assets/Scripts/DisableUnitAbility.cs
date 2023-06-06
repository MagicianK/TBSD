using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DisableUnitAbilityStates{
    public class Idle : IState
    {
        DisableUnitAbility stateOwner;
    
        public Idle(DisableUnitAbility stateOwner) { this.stateOwner = stateOwner; }
        
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
    public class Charge : IState
    {
        DisableUnitAbility stateOwner;
        ICanBeDisabled prey;
        public Charge(DisableUnitAbility stateOwner, ICanBeDisabled prey) { 
            this.stateOwner = stateOwner; 
            this.prey = prey; 
        }
        
        public void Enter()
        {
            TurnManager.instance.StartTurnServerRpc();
        }
    
        public void Execute()
        {
            prey.Disable();
            stateOwner.stateMachine.ChangeState(new Idle(stateOwner));
            stateOwner.unit.stateMachine.ChangeState(new UnitStates.Selected(stateOwner.unit)); 
        }
        
        public void Exit()
        {
            TurnManager.instance.EndTurnServerRpc();
        }
    }
    public class Activated : IState
    {
        DisableUnitAbility stateOwner;
    
        public Activated(DisableUnitAbility stateOwner) { this.stateOwner = stateOwner; }
        
        public void Enter()
        {
            stateOwner.unit.GetInRangeTiles();
        }
    
        public void Execute()
        {
            ICanBeDisabled GetTarget()
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    return hit.collider.gameObject.GetComponent<ICanBeDisabled>();
                }
                return null;
            }
            bool IsEqualTeam(int team)
            {
                return stateOwner.unit.team.Value == team;
            }
            bool IsReachable(Vector2Int pos)
            {
                return stateOwner.unit.inRangeTiles.Contains(BoardManager.instance.GetTileAtPosition(pos));
            }
            if(Input.GetMouseButtonDown(0)){
                ICanBeDisabled prey = GetTarget();
                if (prey != null && !IsEqualTeam(prey.GetPreyTeam()) && IsReachable(prey.GetStandingOnTile()))
                    stateOwner.stateMachine.ChangeState(new Charge(stateOwner, prey));
            }
        }
        
        public void Exit()
        {
            stateOwner.unit.ClearRange();
        }
    }
}

// This ability is targetable
// It switches places with target ally unit 
// It use counts as a turn
// PROBLEM: IT CAN SWITCH WITH ITSELF
public class DisableUnitAbility : MonoBehaviour, IAbility
{
    public Unit unit;
    public StateMachine stateMachine = new StateMachine();
    private void Start() {
        stateMachine.ChangeState(new DisableUnitAbilityStates.Idle(this));
        
    }
    public void Activate(Unit owner)
    {
        this.unit = owner;
        stateMachine.ChangeState(new DisableUnitAbilityStates.Activated(this));
    }
    public void Deactivate()
    {
        stateMachine.ChangeState(new DisableUnitAbilityStates.Idle(this));
    }

    void Update()
    {

        
        stateMachine.Update();
    }
}
