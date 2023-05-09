using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IState
{
    public void Enter();
    public void Execute();
    public void Exit();
}
public class StateMachine
{
    public IState currentState;
 
    public void ChangeState(IState newState)
    {
        if (currentState != null)
            currentState.Exit();
 
        currentState = newState;
        currentState.Enter();
    }
 
    public void Update()
    {
        if (currentState != null) currentState.Execute();
    }
}
public class GameManager : MonoBehaviour
{
    [SerializeField]private Player player0;
    [SerializeField]private Player player1;
    private Player playerBase0;
    private Player playerBase1;
    private static GameManager _instance;
    public static GameManager instance
    { get { return _instance; } }

    private void Awake() {
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
       
    }

    private void Update() {
 
    }
}
