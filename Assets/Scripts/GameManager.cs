using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public interface IState
{
    public void Enter();

    public void Execute();

    public void Exit();
}

public interface IDamagable
{
    public void TakeDamage(int damage);

    public Vector2Int GetStandingOnTile();

    public int GetPreyTeam();
}

public interface IHealable
{
    public void TakeHeal(int heal);
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

public class GameManager : NetworkBehaviour
{
    public Text text;
    private static GameManager _instance;

    public static GameManager instance
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
}