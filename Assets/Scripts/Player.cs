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

        }
    
        public void Execute()
        {

        }
    
        public void Exit()
        {
 
        }
    }

    public class Selected : IState
    {
        Player owner;
        
        public Selected(Player owner) { this.owner = owner; }
        
        public void Enter()
        {
            SelectedView.instance.MoveTo(owner.transform.position);
        }
    
        public void Execute()
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                //Debug.Log("Base standing on: " + owner.GetStandingOnTile().gridLocation);
                List<TileCube> tiles = RangeFinder.GetTilesRange(owner.GetStandingOnTile(), 1);

                foreach (var tile in tiles)
                {
                    if(!tile.isBlocked){
                        owner.CreateUnit(tile, owner.unit1prefab);
                        break;
                    }
                }
            }
            if(Input.GetKeyDown(KeyCode.Alpha2))
            {
                //Debug.Log("Base standing on: " + owner.GetStandingOnTile().gridLocation);
                List<TileCube> tiles = RangeFinder.GetTilesRange(owner.GetStandingOnTile(), 1);

                foreach (var tile in tiles)
                {
                    if(!tile.isBlocked){
                        owner.CreateUnit(tile, owner.unit2prefab);
                        break;
                    }
                }
            }
        }
    
        public void Exit()
        {
            //SelectedView.instance.MoveTo(owner.transform.position);
        }
    }
}
public class Player : MonoBehaviour, IDamagable
{
    public Unit unit1prefab;
    public Unit unit2prefab;
    public Unit unit3prefab;
    public int team;
    public int points = 5000;
    public StateMachine stateMachine = new StateMachine();
    public Unit unitToPlace;
    [SerializeField]
    private UnitData unitData;
    public List<Unit> units;
    private int health;
    public TileCube standingOn;
    public Color startColor;
    // Start is called before the first frame update
    void Start()
    {
        stateMachine.ChangeState(new PlayerBaseStates.Idle(this));
        health = unitData.Health;
        startColor = GetComponentInChildren<Renderer>().material.color;
        units = new List<Unit>();
    }

    private void Update() {
        stateMachine.Update();
        if (GameManager.instance.turnSystem.currentTeam != this.team)
        {
            foreach (Unit unit in units)
            {
                unit.stateMachine.ChangeState(new UnitStates.Idle(unit));
                unit.GetComponent<Unit>().enabled = false;
                unit.GetComponentInChildren<Renderer>().material.color = Color.gray;
            }
        }
        else{
            foreach (Unit unit in units)
            {
                unit.GetComponent<Unit>().enabled = true;
                unit.GetComponentInChildren<Renderer>().material.color = startColor;
            }
        }
    }
    private void OnDestroy() {
        Debug.Log($"Player {this.team - 1}");
    }
    public TileCube GetStandingOnTile(){
        return standingOn;
    }
    
    public int GetPreyTeam(){
        return this.team;
    }
    public void TakeDamage(int damage){
        this.health -= damage;
        if(this.health <= 0)
            Destroy(gameObject);
    }
    
    public void CreateUnit(TileCube tileCube, Unit unitPrefab)
    {
        unitToPlace = Instantiate(unitPrefab);
        unitToPlace.standingOn = tileCube;
        tileCube.isBlocked = true;
        unitToPlace.transform.position = tileCube.transform.position;
        unitToPlace.InitValues(this.team, this);
        MouseController.instance.selectedUnit = unitToPlace;
        units.Add(unitToPlace);
        unitToPlace = null;
    }
    private void OnMouseDown() {

        // Enables selection state only if Mouse state is Idle and it is their turn
        if (MouseController.instance.mouseStateMachine.currentState is MouseStates.Idle &&
            GameManager.instance.turnSystem.currentTeam == this.team){
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
