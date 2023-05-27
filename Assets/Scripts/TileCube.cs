using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TileCube : NetworkBehaviour
{
    // TileCube should store G, H, F for PathFinder
    // TileCube should store isBlocked for PathFinder
    // TileCube should store coord 
    public int G;
    public int H;
    public int F
    { get { return G + H; } }
    public Vector2Int previous;


    public NetworkVariable<bool> isBlocked = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone);
    public NetworkVariable<Vector2Int> coord = new NetworkVariable<Vector2Int>(default, NetworkVariableReadPermission.Everyone);
    MouseController mouseController;

    public Material defaultMaterial;
    public Material rangeShowMaterial;
    public Material darkDefaultMaterial;


    
    [ServerRpc(RequireOwnership = false)]
    public void InitServerRpc(bool isOffset, Vector2Int coord)
    {
        this.coord.Value = coord;
        gameObject.GetComponent<MeshRenderer>().material = isOffset ? darkDefaultMaterial : defaultMaterial;
        InitClientRpc(isOffset, coord);
    }
    [ClientRpc]
    public void InitClientRpc(bool isOffset, Vector2Int coord)
    {
        this.coord.Value = coord;
        gameObject.GetComponent<MeshRenderer>().material = isOffset ? darkDefaultMaterial : defaultMaterial;
    }


    private void Start()
    {
        mouseController = NetworkManager.LocalClient.PlayerObject.GetComponent<MouseController>();
        gameObject.layer = LayerMask.NameToLayer("Tile");
        gameObject.GetComponent<MeshRenderer>().material = defaultMaterial;
    }
    private void Update()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Tile"))
        {
            DrawDefault();
        }
        if (gameObject.layer == LayerMask.NameToLayer("RangeShow"))
        {
            DrawBlue();
        }
    }
    public void DrawBlue()
    {
        gameObject.GetComponent<MeshRenderer>().material = rangeShowMaterial;
    }

    public void DrawDefault()
    {
        gameObject.GetComponent<MeshRenderer>().material = defaultMaterial;
    }
    public void ChangeLayer(LayerMask newLayer)
    {
        gameObject.layer = newLayer;
    }
}