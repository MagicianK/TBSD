using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlayerBaseStates{
    public class Idle : IState
    {
        Player owner;
    
        public Idle(Player owner) { this.owner = owner; }
        
        public void Enter()
        {
            Debug.Log("Base Idle");
        }
    
        public void Execute()
        {

        }
    
        public void Exit()
        {
            Debug.Log("Base is active");
        }
    }

    public class Selected : IState
    {
        Player owner;
        
        public Selected(Player owner) { this.owner = owner; }
        
        public void Enter()
        {
            Debug.Log("Base selected");
        }
    
        public void Execute()
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("Base standing on: " + owner.GetStandingOnTile().gridLocation);
                List<TileCube> tiles = RangeFinder.GetTilesRange(owner.GetStandingOnTile(), 1);

                foreach (var tile in tiles)
                {
                    if(!tile.isBlocked){
                        MouseController.instance.CreateUnit(tile);
                        break;
                    }
                }
            }
        }
    
        public void Exit()
        {
            Debug.Log("Base unselected");
        }
    }
}
public class Player : MonoBehaviour, IDamagable
{
    public Unit unit1;
    public Unit unit2;
    public Unit unit3;

    public int points = 5000;
    public StateMachine stateMachine = new StateMachine();
    public int turnCredits = 100;
    [SerializeField]
    private UnitData unitData;
    private int health;
    public TileCube standingOn;
    Color startColor;
    // Start is called before the first frame update
    void Start()
    {
        stateMachine.ChangeState(new PlayerBaseStates.Idle(this));
        health = unitData.Health;
        startColor = GetComponentInChildren<Renderer>().material.color;
    }

    private void Update() {
        stateMachine.Update();
    }
    public TileCube GetStandingOnTile(){
        return standingOn;
    }
    public void TakeDamage(int damage){
        this.health -= damage;
        if(this.health <= 0)
            Destroy(gameObject);
    }
    public void takeTurnCredits(int turnCreditWeight)
    {

    }

    private void OnMouseDown() {
        Debug.Log("You clicked base");
        if (MouseController.instance.mouseStateMachine.currentState is MouseStates.Idle){
            MouseController.instance.mouseStateMachine.ChangeState(new MouseStates.OnPlayerBaseState(this));
            stateMachine.ChangeState(new PlayerBaseStates.Selected(this));
        }
    }
    private void OnMouseEnter() {
        GetComponentInChildren<Renderer>().material.color = Color.white;
    }
    private void OnMouseExit() {
       GetComponentInChildren<Renderer>().material.color = startColor; 
    }
}
