using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseIdle : IState
{
    Player owner;
 
    public BaseIdle(Player owner) { this.owner = owner; }
    
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

public class BaseSelected : IState
{
    Player owner;
    
    public BaseSelected(Player owner) { this.owner = owner; }
    
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
    // Start is called before the first frame update
    void Start()
    {
        stateMachine.ChangeState(new BaseIdle(this));
        health = unitData.Health;
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
        stateMachine.ChangeState(new BaseSelected(this));
    }
}
