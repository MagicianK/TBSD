using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MouseStates
    {
    public class Idle : IState
    {
        
        public void Enter()
        {
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

    public class OnPlayerBaseState: IState
    {
        Player playerBase;
        public OnPlayerBaseState(Player playerBase){
            this.playerBase = playerBase;
        }
        public void Enter()
        {

        }
    
        public void Execute()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
                MouseController.instance.mouseStateMachine.ChangeState(new MouseStates.Idle());
        }
    
        public void Exit()
        {
            playerBase.stateMachine.ChangeState(new PlayerBaseStates.Idle(playerBase));
        }
    }
    public class OnUnitState : IState
    {
        
        public void Enter()
        {

        }
    
        public void Execute()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
                MouseController.instance.mouseStateMachine.ChangeState(new MouseStates.Idle());
        }
    
        public void Exit()
        {
            Unit selectedUnit = MouseController.instance.selectedUnit;
            selectedUnit.stateMachine.ChangeState(new UnitStates.Idle(selectedUnit));
            selectedUnit.ClearRange();
            MouseController.instance.selectedUnit = null;
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
public class MouseController : MonoBehaviour
{
    // Change state of the mouse in UPDATE()
    // Handle changes in UPDATE()
    public StateMachine mouseStateMachine = new StateMachine();
    private static MouseController _instance;
    public static MouseController instance
    { get { return _instance; } }

    public GameObject cursor;
    public Unit selectedUnit;
    [SerializeField] private Unit unitPrefab;
    public Unit unitToPlace;
    private TileCube clickedTile;
    public Unit clickedUnit;
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    // Start is called before the first frame update
    private void Start()
    {
        mouseStateMachine.ChangeState(new MouseStates.Idle());
    }
    private void LateUpdate()
    {
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

}