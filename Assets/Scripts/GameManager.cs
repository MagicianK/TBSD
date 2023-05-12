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

    private Player player0;
    private Player player1;
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
        StartCoroutine(CreatePlayerBases());

    }
    IEnumerator CreatePlayerBases()
    {
        while (!Board.instance.isFilled)
        {
            yield return null;
        }
        TileCube tc1 = Board.instance.map[new Vector2Int(-6, 1)];
        player0 = Instantiate(playerBase0);
        player0.standingOn = tc1;
        player0.transform.position = tc1.transform.position;
        tc1.isBlocked = true;
        tc1.player = player0;


        TileCube tc2 = Board.instance.map[new Vector2Int(25, 1)];
        player1 = Instantiate(playerBase1);
        player1.standingOn = tc2;
        player1.transform.position = tc2.transform.position;
        tc2.isBlocked = true;
        tc2.player = player1;
    }
 
    private void Update() {
    }
}
