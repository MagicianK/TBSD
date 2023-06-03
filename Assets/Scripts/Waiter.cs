using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// Waits for player connect and creates player bases for all players
public class Waiter : NetworkBehaviour
{
    public const int BASE1_X = 0;
    public const int BASE1_Y = 10;
    public const int BASE2_X = 39;
    public const int BASE2_Y = 10;
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
            playerBase1 = (PlayerBase)baseFactory1.GetProduct(new Vector2Int(BASE1_X, BASE1_Y), clients[0].ClientId);
            playerBase2 = (PlayerBase)baseFactory2.GetProduct(new Vector2Int(BASE2_X, BASE2_Y), clients[1].ClientId);

            playerBase1.SetTeamServerRpc(0);
            playerBase2.SetTeamServerRpc(1);
            this.enabled = false;
        }
    }
}