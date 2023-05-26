using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


// This is a component of a player gameobject
public class BaseFactory : NetworkBehaviour
{
    [SerializeField]
    private PlayerBase playerBasePrefab0;

    [SerializeField]
    private PlayerBase playerBasePrefab1;

    public PlayerBase playerBase0;
    public PlayerBase playerBase1;
    public delegate void CreateBaseXRpc(NetworkBehaviourReference nbr);
    public delegate void CreateBaseX(MouseController mc);
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void CreateFirstBase(MouseController mc)
    {
        TileCube tc1 = BoardManager.instance.GetTileAtPosition(new Vector2Int(5, 5));
        playerBase0 = Instantiate(playerBasePrefab0);
        playerBase0.NetworkObject.Spawn();
        Debug.Log("Spawned " + playerBase0.NetworkObjectId);

        playerBase0.standingOn = tc1;
        playerBase0.transform.position = tc1.transform.position;
        playerBase0.location2D.Value = tc1.coord.Value;
        playerBase0.team = 0;
        BoardManager.instance.BlockTileServerRpc(tc1.coord.Value);
        //tc1.isBlocked = true;
        //tc1.player = playerBase0;
        playerBase0.mouseController = mc;
        Board.instance.mouseController = mc;
    }
    [ClientRpc]
    public void CreateFirstBaseClientRpc(NetworkBehaviourReference nbr)
    {
        TileCube tc1 = BoardManager.instance.GetTileAtPosition(new Vector2Int(5, 5));
        playerBase0 = Instantiate(playerBasePrefab0);
        playerBase0.NetworkObject.Spawn();
        Debug.Log("Spawned " + playerBase0.NetworkObjectId);
         
        playerBase0.standingOn = tc1;
        playerBase0.transform.position = tc1.transform.position;
        playerBase0.location2D.Value = tc1.coord.Value;
        playerBase0.team = 0;
        BoardManager.instance.BlockTileServerRpc(tc1.coord.Value);
        //tc1.isBlocked = true;
        //tc1.player = playerBase0;
        //if (nbr.TryGet<MouseController>(out MouseController mc))
        //{
        //    playerBase0.mouseController = mc;
        //    Board.instance.mouseController = mc;
        //}
    }
    [ClientRpc]
    public void CreateSecondBaseClientRpc(NetworkBehaviourReference nbr)
    {
        
    }
    [ServerRpc(RequireOwnership = false)]
    public void CreateSecondBaseServerRpc(NetworkBehaviourReference nbr)
    {
        TileCube tc2 = BoardManager.instance.GetTileAtPosition(new Vector2Int(2, 1));
        playerBase1 = Instantiate(playerBasePrefab1);
        playerBase1.NetworkObject.Spawn();
        Debug.Log("Spawned " + playerBase1.NetworkObjectId);

        playerBase1.standingOn = tc2;
        playerBase1.location2D.Value = tc2.coord.Value;
        playerBase1.transform.position = tc2.transform.position;
        playerBase1.team = 1;
        BoardManager.instance.BlockTileServerRpc(tc2.coord.Value);
        //tc2.isBlocked = true;
        //tc2.player = playerBase1;
        //if (nbr.TryGet<MouseController>(out MouseController mc))
        //{
        //    playerBase1.mouseController = mc;
        //    Board.instance.mouseController = mc;
        //}
    }
    public void CreateSecondBase(MouseController mc)
    {
        TileCube tc2 = BoardManager.instance.GetTileAtPosition(new Vector2Int(2, 1));
        playerBase1 = Instantiate(playerBasePrefab1);
        //playerBase1.NetworkObject.Spawn();
        Debug.Log("Spawned " + playerBase1.NetworkObjectId);

        playerBase1.standingOn = tc2;
        playerBase1.location2D.Value = tc2.coord.Value;
        playerBase1.transform.position = tc2.transform.position;
        playerBase1.team = 1;
        //tc2.isBlocked = true;
        BoardManager.instance.BlockTileServerRpc(tc2.coord.Value);
        
    }
}
