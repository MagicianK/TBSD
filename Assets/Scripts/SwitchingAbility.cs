using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SwitchingAbilityStates{
    public class Idle : IState
    {
        SwitchingAbility stateOwner;
    
        public Idle(SwitchingAbility stateOwner) { this.stateOwner = stateOwner; }
        
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
    public class Activated : IState
    {
        SwitchingAbility stateOwner;
    
        public Activated(SwitchingAbility stateOwner) { this.stateOwner = stateOwner; }
        
        public void Enter()
        {
            
        }
    
        public void Execute()
        {
            bool IsEqualTeam(int team)
            {
                return stateOwner.unit.team == team;
            }
            bool IsReachable(TileCube tile)
            {
                return stateOwner.unit.inRangeTiles.Contains(tile);
            }
            ISwitchable GetTarget()
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    return hit.collider.gameObject.GetComponent<ISwitchable>();
                }
                return null;
            }
            // Choose ally target to switch with
            if (Input.GetMouseButtonUp(0))
            {
                ISwitchable prey = GetTarget();
                
                if(prey != null && IsEqualTeam(prey.GetPreyTeam()) && IsReachable(prey.GetStandingOnTile())) 
                { 
                    TileCube dest = stateOwner.unit.standingOn; 
                    stateOwner.unit.Switch(prey.GetStandingOnTile());
                    prey.Switch(dest);
                    stateOwner.stateMachine.ChangeState(new Idle(stateOwner));
                    stateOwner.unit.stateMachine.ChangeState(new UnitStates.Selected(stateOwner.unit));
                    GameManager.instance.turnSystem.MakeTurn();
                }
                
            }
        }
        
        public void Exit()
        {
            Debug.Log("Ability exited");
        }
    }
}

// This ability is targetable
// It switches places with target ally unit 
// It use counts as a turn
public class SwitchingAbility : MonoBehaviour, IAbility
{
    public Unit unit;
    public StateMachine stateMachine = new StateMachine();
    private void Start() {
        stateMachine.ChangeState(new SwitchingAbilityStates.Idle(this));
    }
    public void Activate(Unit owner)
    {
        this.unit = owner;
        stateMachine.ChangeState(new SwitchingAbilityStates.Activated(this));
    }
    public void Deactivate()
    {
        stateMachine.ChangeState(new SwitchingAbilityStates.Idle(this));
    }
    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
    }
}
