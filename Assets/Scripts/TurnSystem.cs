using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class TurnSystem : MonoBehaviour
{
    public int currentTeam = 0;
    public static int maxTurns = 3;
    public int turns = maxTurns;
    public Text text;
    private void Start() {
        text.text = "Team Turn: " + currentTeam;
    }
   // Unit[] armyUnits = FindObjectsOfType(typeof(Unit)) as Unit[];

    public void MakeTurn()
    {
        turns -= 1;
        Debug.Log("Made turn");
        if(turns <= 0)
        {
            currentTeam = (currentTeam == 0) ? 1 : 0;
            turns = maxTurns;
            text.text = "Team Turn: " + currentTeam;
            MouseController.instance.mouseStateMachine.ChangeState(new MouseStates.Idle()); 
        }
    }
}
