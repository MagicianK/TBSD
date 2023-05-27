using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
// Waits for player connect and creates player bases for all players
public class Waiter : NetworkBehaviour
{
    BaseFactory1 baseFactory1;
    BaseFactory2 baseFactory2;

    PlayerBase playerBase1;
    PlayerBase playerBase2;

    void Start()
    {
        baseFactory1 = GetComponent<BaseFactory1>();
        baseFactory2 = GetComponent<BaseFactory2>();
    }
    [ServerRpc(RequireOwnership = false)]
    void DespawnServerRpc()
    {
        NetworkObject.Despawn();
    }
    void Update()
    {
        if (!IsOwner)
            return;
        //IsClientsReadyServerRpc(out bool isClientsReady);
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            playerBase1 = (PlayerBase)baseFactory1.GetProduct(new Vector2Int(1, 5));
            playerBase2 = (PlayerBase)baseFactory2.GetProduct(new Vector2Int(5, 5));

            IReadOnlyList<NetworkClient> clients = NetworkManager.Singleton.ConnectedClientsList;


            playerBase1.team = clients[0].PlayerObject.GetComponent<MouseController>().team;
            playerBase2.team = clients[1].PlayerObject.GetComponent<MouseController>().team;

            DespawnServerRpc();
        }
    }
}
