using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IState
{
    public void Enter();
    public void Execute();
    public void Exit();
}

public interface IDamagable
{
    public void TakeDamage(int damage);
    public TileCube GetStandingOnTile();
}

public interface IHealable
{
    public void TakeHeal(int heal);
    public TileCube GetStandingOnTile();
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
    [SerializeField]
    private Player playerBase0;
    [SerializeField]
    private Player playerBase1;
    private static GameManager _instance;
    TileCube tc;
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
        StartCoroutine(CreatePlayerBases());

    }
    IEnumerator CreatePlayerBases()
    {
        while (!Board.instance.isFilled)
        {
            yield return null;
        }
        tc = Board.instance.map[new Vector2Int(-6, 1)];
        Instantiate(playerBase0);
        playerBase0.standingOn = tc;
        playerBase0.transform.position = tc.transform.position;
        tc.isBlocked = true;
        tc.player = playerBase0;


        tc = Board.instance.map[new Vector2Int(25, 1)];
        Instantiate(playerBase1);
        playerBase1.standingOn = tc;
        playerBase1.transform.position = tc.transform.position;
        tc.isBlocked = true;
        tc.player = playerBase1;
    }
 
    private void Update() {
    }
}
