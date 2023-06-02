using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class CameraDestroyer : NetworkBehaviour
{
    private void Update() {
        if(NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
            Destroy(gameObject);
    }
}
