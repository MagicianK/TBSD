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
    public NetworkVariable<int> allTurns = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone);
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
    private void Start() {
        currentTeam.OnValueChanged += OnTurnChanged;
    }
    void OnTurnChanged(int prev, int curr)
    {
        text.text = "Team turn: " + curr;
    }
    [ServerRpc(RequireOwnership = false)]
    public void IncrementAllTurnsServerRpc()
    {
        allTurns.Value++;
    }
    [ServerRpc(RequireOwnership = false)]
    public void StartTurnServerRpc()
    {
        if(!IsOwner)
            return;
        turns.Value -= 1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc()
    {
        if(!IsOwner)
            return;
        Debug.Log("Made turn");
        if (turns.Value <= 0)
        {
            currentTeam.Value = (currentTeam.Value == 0) ? 1 : 0;
            turns.Value = maxTurns;
        }
        
    }
}
