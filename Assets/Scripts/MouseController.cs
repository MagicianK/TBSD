using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class Idle : IState
{
    
    public void Enter()
    {
        Debug.Log("YOU ENTERED ON IDLE STATE");
    }
 
    public void Execute()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            MouseController.instance.mouseStateMachine.ChangeState(new UnitPlaceState());
        }
    }
 
    public void Exit()
    {

    }
}

public class OnUnitState : IState
{
    
    public void Enter()
    {
        Debug.Log("YOU ENTERED ON UNIT STATE");
    }
 
    public void Execute()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            MouseController.instance.mouseStateMachine.ChangeState(new Idle());
    }
 
    public void Exit()
    {
        MouseController.instance.selectedUnit.stateMachine.ChangeState(new UnitIdle(MouseController.instance.selectedUnit));
        MouseController.instance.selectedUnit.Deselect();
        MouseController.instance.selectedUnit = null;
    }
}
public class UnitPlaceState : IState
{
    
    public void Enter()
    {
        if (MouseController.instance.selectedUnit != null)
        {
            MouseController.instance.selectedUnit.Deselect();
            MouseController.instance.selectedUnit = null;
        }
        Debug.Log("YOU WANT TO PLACE UNIT");
    }
 
    public void Execute()
    {
        var focusedTileHit = Input.GetMouseButtonUp(0) ? MouseController.instance.GetFocusedTile() : null;
        if (Input.GetMouseButtonUp(0) && focusedTileHit.HasValue)
        {
            TileCube clickedTile = focusedTileHit.Value.collider.gameObject.GetComponent<TileCube>();
            MouseController.instance.CreateUnit(clickedTile);
            MouseController.instance.mouseStateMachine.ChangeState(new OnUnitState());
        }
    }
 
    public void Exit()
    {
        MouseController.instance.selectedUnit.stateMachine.ChangeState(new UnitSelected(MouseController.instance.selectedUnit));
        Debug.Log("IDLE");
    }
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
        mouseStateMachine.ChangeState(new Idle());
    }

    private void LateUpdate()
    {
        if (clickedUnit && clickedUnit.isMoving)
            return;

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

    public void CreateUnit(TileCube tileCube)
    {
        unitToPlace = Instantiate(unitPrefab).GetComponent<Unit>();
        unitToPlace.PositionCharacterOnTile(tileCube);
        unitToPlace.stateMachine.ChangeState(new UnitSelected(unitToPlace));
        selectedUnit = unitToPlace;
        unitToPlace = null;
    }

    // Update is called once per frame
    private void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, LayerMask.GetMask("Tile")))
        {
            ChangeCursorPos(hit.collider.transform);
        }
        mouseStateMachine.Update();
    }

    // Changes the red cursor to the place where mouse is pointed
    private void ChangeCursorPos(Transform newPlace)
    {
        cursor.transform.position = new Vector3(newPlace.position.x, cursor.transform.position.y, newPlace.position.z);
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