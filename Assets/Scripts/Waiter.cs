using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// Waits for player connect and creates player bases for all players
public class Waiter : NetworkBehaviour
{
    private BaseFactory1 baseFactory1;
    private BaseFactory2 baseFactory2;

    private PlayerBase playerBase1;
    private PlayerBase playerBase2;
    private bool isBasesCreated = false;

    private void Start()
    {
        baseFactory1 = GetComponent<BaseFactory1>();
        baseFactory2 = GetComponent<BaseFactory2>();
    }

    [ServerRpc]
    private void DespawnServerRpc()
    {
        NetworkObject.DontDestroyWithOwner = true;
        NetworkObject.Despawn();
    }

    private void Update()
    {
        if (!IsOwner)
            return;
        //IsClientsReadyServerRpc(out bool isClientsReady);
        IReadOnlyList<NetworkClient> clients = NetworkManager.Singleton.ConnectedClientsList;
        if (clients.Count == 2)
        {
            playerBase1 = (PlayerBase)baseFactory1.GetProduct(new Vector2Int(1, 5), clients[0].ClientId);
            playerBase2 = (PlayerBase)baseFactory2.GetProduct(new Vector2Int(5, 5), clients[1].ClientId);

            playerBase1.team = clients[0].PlayerObject.GetComponent<MouseController>().team.Value;
            playerBase2.team = clients[1].PlayerObject.GetComponent<MouseController>().team.Value;

            this.enabled = false;
        }
    }
}