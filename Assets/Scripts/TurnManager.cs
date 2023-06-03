using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Netcode;


public class TurnManager : NetworkBehaviour
{
    public NetworkVariable<int> currentTeam = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone);
    public const int maxTurns = 3;
    public NetworkVariable<int> turns = new NetworkVariable<int>(maxTurns, NetworkVariableReadPermission.Everyone);
    public Text text;

    private static TurnManager _instance;

    public static TurnManager instance
    { get { return _instance; } }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartTurnServerRpc()
    {
        turns.Value -= 1;
    }
    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc()
    {
        Debug.Log("Made turn");
        if (turns.Value <= 0)
        {
            currentTeam.Value = (currentTeam.Value == 0) ? 1 : 0;
            turns.Value = maxTurns;
            text.text = "Team Turn: " + currentTeam;
            //MouseController.instance.mouseStateMachine.ChangeState(new MouseStates.Idle()); 
        }
    }
}
