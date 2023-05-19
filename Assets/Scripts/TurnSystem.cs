using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Netcode;

public class TurnSystem : MonoBehaviour
{
    public NetworkVariable<int> currentTeam = new NetworkVariable<int>(0);
    public static NetworkVariable<int> maxTurns = new NetworkVariable<int>(3);
    public int turns = maxTurns.Value;
    public Text text;

    private void Start()
    {
        text.text = "Team Turn: " + currentTeam;
    }

    // Unit[] armyUnits = FindObjectsOfType(typeof(Unit)) as Unit[];

    public void MakeTurn()
    {
        turns -= 1;
        Debug.Log("Made turn");
        if (turns <= 0)
        {
            currentTeam.Value = (currentTeam.Value == 0) ? 1 : 0;
            turns = maxTurns.Value;
            text.text = "Team Turn: " + currentTeam;
            //MouseController.instance.mouseStateMachine.ChangeState(new MouseStates.Idle());
        }
    }
}