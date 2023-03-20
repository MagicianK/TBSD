using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPlacer : MonoBehaviour
{
    public GameObject cursor;
    private Unit selectedUnit;
    public Unit unit1;
    public Unit unit2;
    public Unit unit3;

    // Start is called before the first frame update
    private void Start()
    {
    }

    private void UnitSelect()
    {
    }

    public void PlaceUnit()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        /*
         * if(Input.GetKeyDown(KeyCode.A)){
            selectedUnit = Instantiate(unit1).GetComponent<Unit>();;

            TileCube tile = cursor.GetComponent<Cursor>().GetFocusedTile();
            tile.unit = selectedUnit;
            selectedUnit.standingOn = tile;
            if (tile != null)
            {
                selectedUnit.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z);
            }
        }*/
    }
}