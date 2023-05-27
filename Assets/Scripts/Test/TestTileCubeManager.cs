using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class TestTileCubeManager : NetworkBehaviour
{
    public NetworkVariable<bool> isChecked = new NetworkVariable<bool>(false);
    
    [SerializeField]
    private TileCube singleTileCubePrefab;
    void OnIsCheckedChanged(bool newValue)
    {
        isChecked.Value = newValue;
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;
        GameObject go = Instantiate(singleTileCubePrefab.gameObject, new Vector3(0,0,0), Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn();
        isChecked.OnValueChanged += (prev, curr) =>
        {
            OnIsCheckedChanged(curr);
        };
        if (IsHost)
            go.GetComponent<TileCube>().isBlocked.Value = true;
        if (IsClient && !IsHost)
        {
            Debug.LogWarning("In Client");
            CheckServerRpc();
        }
    }

    [ClientRpc]
    public void CheckClientRpc()
    {
        isChecked.Value = true;
    }
    [ServerRpc]
    public void CheckServerRpc()
    {
        isChecked.Value = true;
    }
}
