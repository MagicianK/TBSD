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

public class GameManager : NetworkBehaviour, INetworkSerializable
{
    [SerializeField]
    private Player playerBasePrefab0;

    [SerializeField]
    private Player playerBasePrefab1;

    public Player playerBase0;
    public Player playerBase1;
    public TurnSystem turnSystem;
    public Text text;
    private static GameManager _instance;
    public NetworkVariable<bool> isPlayerBasesCreated = new NetworkVariable<bool>();

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
            isPlayerBasesCreated.Value = false;
        }
    }

    public override void OnNetworkSpawn()
    {
        turnSystem = new TurnSystem();
        turnSystem.text = text; // Temporary solution

        if (IsServer)
            StartCoroutine(CreatePlayerBases());
    }

    private void Start()
    {
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreatePlayerBasesServerRpc()
    {
    }

    [ClientRpc]
    private void CreatePlayerBasesClientRpc()
    {
        Debug.Log("Spawning bases");
        TileCube tc1 = Board.instance.map[new Vector2Int(-6, 1)];
        playerBase0 = Instantiate(playerBasePrefab0);
        playerBase0.NetworkObject.Spawn();

        playerBase0.standingOn = tc1;
        playerBase0.transform.position = tc1.transform.position;
        playerBase0.location2D = tc1.grid2DLocation;
        playerBase0.team.Value = 0;
        tc1.isBlocked = true;
        tc1.player = playerBase0;

        TileCube tc2 = Board.instance.map[new Vector2Int(25, 1)];
        playerBase1 = Instantiate(playerBasePrefab1);
        playerBase1.NetworkObject.Spawn();

        playerBase1.standingOn = tc2;
        playerBase1.location2D = tc2.grid2DLocation;
        playerBase1.transform.position = tc2.transform.position;
        playerBase0.team.Value = 1;
        tc2.isBlocked = true;
        tc2.player = playerBase1;

        isPlayerBasesCreated.Value = true;
    }

    private IEnumerator CreatePlayerBases()
    {
        while (!Board.instance.isFilled)
        {
            yield return null;
        }

        CreatePlayerBasesClientRpc();
    }

    private void Update()
    {
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerBase0);
        serializer.SerializeValue(ref playerBase1);
    }
}