using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class GameOverManager : NetworkBehaviour
{
    [SerializeField]
    private Text text;
    private void Start() {
        if(NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<MouseController>().isLost.Value)
        {
            text.color = Color.red;
            text.text = "You lost!";
        }
        else{
            text.color = Color.cyan;
            text.text = "You won!";
        }

    }
}
