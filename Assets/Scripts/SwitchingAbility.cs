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
    public class Charge : IState
    {
        SwitchingAbility stateOwner;
        ISwitchable prey;
        public Charge(SwitchingAbility stateOwner, ISwitchable prey) { 
            this.stateOwner = stateOwner; 
            this.prey = prey; 
        }
        
        public void Enter()
        {
            TurnManager.instance.StartTurnServerRpc();
        }
    
        public void Execute()
        {
            Vector2Int dest = stateOwner.unit.standingOn.Value; 
            stateOwner.unit.Switch(prey.GetStandingOnTile());
            prey.Switch(dest);
            stateOwner.PlaySound();
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
        SwitchingAbility stateOwner;
    
        public Activated(SwitchingAbility stateOwner) { this.stateOwner = stateOwner; }
        
        public void Enter()
        {
            stateOwner.unit.GetInRangeTiles();
        }
    
        public void Execute()
        {
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
            bool IsEqualTeam(int team)
            {
                return stateOwner.unit.team.Value == team;
            }
            bool IsReachable(Vector2Int pos)
            {
                return stateOwner.unit.inRangeTiles.Contains(BoardManager.instance.GetTileAtPosition(pos));
            }
            if(Input.GetMouseButtonDown(0)){
                ISwitchable prey = GetTarget();
                if (prey != null && IsEqualTeam(prey.GetPreyTeam()) && IsReachable(prey.GetStandingOnTile()))
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
    public void PlaySound()
    {
        //FindObjectOfType<SoundPlayer>().Play("Switched");
    }
    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
    }
}
