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

    public TileCube GetStandingOnTile();

    public int GetPreyTeam();
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

public class GameManager : NetworkBehaviour
{
    [SerializeField]
    private Player playerBasePrefab0;

    [SerializeField]
    private Player playerBasePrefab1;

    private Player playerBase0;
    private Player playerBase1;
    public TurnSystem turnSystem;
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

    public override void OnNetworkSpawn()
    {
        turnSystem = new TurnSystem();
        turnSystem.text = text; // Temporary solution
        if (!IsServer)
            return;

        StartCoroutine(CreatePlayerBases());
    }

    private void Start()
    {
    }

    private IEnumerator CreatePlayerBases()
    {
        while (!Board.instance.isFilled)
        {
            yield return null;
        }
        Debug.Log("Spawning bases");
        TileCube tc1 = Board.instance.map[new Vector2Int(-6, 1)];
        playerBase0 = Instantiate(playerBasePrefab0);
        playerBase0.NetworkObject.Spawn();
        Debug.Log("Spawned " + playerBase0.NetworkObjectId);

        playerBase0.standingOn = tc1;
        playerBase0.transform.position = tc1.transform.position;
        playerBase0.team = 0;
        tc1.isBlocked = true;
        tc1.player = playerBase0;

        TileCube tc2 = Board.instance.map[new Vector2Int(25, 1)];
        playerBase1 = Instantiate(playerBasePrefab1);
        playerBase1.NetworkObject.Spawn();
        Debug.Log("Spawned " + playerBase1.NetworkObjectId);

        playerBase1.standingOn = tc2;
        playerBase1.transform.position = tc2.transform.position;
        playerBase1.team = 1;
        tc2.isBlocked = true;
        tc2.player = playerBase1;
    }

    private void Update()
    {
    }
}