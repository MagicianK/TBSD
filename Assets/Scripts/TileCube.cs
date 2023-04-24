using Unity.Netcode;
using UnityEngine;

public class TileCube : NetworkBehaviour, INetworkSerializable
{
    public int G;
    public int H;
    public bool isBlocked;

    public int F
    { get { return G + H; } }

    public Unit unit;
    public int unitId = -1;
    public Vector2Int previous;
    public Vector3Int gridLocation;

    public Vector2Int grid2DLocation
    { get { return new Vector2Int(gridLocation.x, gridLocation.y); } }

    public Material hoverMaterial;
    public Material defaultMaterial;
    public Material clickedMaterial;
    public Material rangeShowMaterial;

    // Start is called before the first frame update
    public string GetUnitInfo()
    {
        if (unit != null)
            return unit.ToString();
        return "empty";
    }

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Tile");
    }

    public void DrawBlue()
    {
        gameObject.GetComponent<MeshRenderer>().material = rangeShowMaterial;
    }

    public void DrawDefault()
    {
        gameObject.GetComponent<MeshRenderer>().material = defaultMaterial;
    }

    // Update is called once per frame
    private void Update()
    {
        if (unit != null)
        {
            isBlocked = true;
        }
        if (gameObject.layer == LayerMask.NameToLayer("Hover"))
        {
            gameObject.GetComponent<MeshRenderer>().material = hoverMaterial;
        }
        if (gameObject.layer == LayerMask.NameToLayer("Tile"))
        {
            DrawDefault();
        }
        if (gameObject.layer == LayerMask.NameToLayer("Clicked"))
        {
            gameObject.GetComponent<MeshRenderer>().material = clickedMaterial;
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

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref unitId);
    }
}