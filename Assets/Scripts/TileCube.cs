using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCube : MonoBehaviour
{
    public Player player;
    public int G;
    public int H;
    public bool isBlocked;
    public int F {get {return G+H;}}
    public Unit unit;
    public TileCube previous;
    public Vector3Int gridLocation;
    public Vector2Int grid2DLocation {get {return new Vector2Int(gridLocation.x, gridLocation.z);}}
    public Material hoverMaterial;
    public Material defaultMaterial;
    public Material clickedMaterial;
    public Material rangeShowMaterial;
    public Material prevmaterial;
    // Start is called before the first frame update
    public string GetUnitInfo()
    {
        if(unit != null)
            return unit.ToString();
        return "empty";
    }
    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Tile");
        gameObject.GetComponent<MeshRenderer> ().material = defaultMaterial;
        prevmaterial = defaultMaterial;
    }
    private void OnMouseDown() {
        Debug.Log("grid location of cube: " + gridLocation);
        Debug.Log("Does cube has unit: " + ((unit != null) || (player != null)));
    }
    private void OnMouseEnter() {
        Transform trans = MouseController.instance.cursor.transform;
        Vector3 pos = new Vector3(gameObject.transform.position.x, trans.position.y, gameObject.transform.position.z);
        MouseController.instance.cursor.transform.position = pos;
    }
    public void DrawBlue()
    {
        gameObject.GetComponent<MeshRenderer> ().material = rangeShowMaterial;
    }
    public void DrawDefault()
    {
        gameObject.GetComponent<MeshRenderer> ().material = defaultMaterial;
    }
    // Update is called once per frame
    void Update()
    {
        if (unit != null)
        {
            isBlocked = true;
        }
        if (gameObject.layer == LayerMask.NameToLayer("Tile"))
        {
            DrawDefault();
        }
        if (gameObject.layer == LayerMask.NameToLayer("RangeShow"))
        {
            DrawBlue();
        }
    }
    public void ChangeLayer(LayerMask layer)
    {
        gameObject.layer = layer;
    }
}
