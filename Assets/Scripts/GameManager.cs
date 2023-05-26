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

    public override void OnNetworkSpawn()
    {
        //turnSystem = new TurnSystem();
        //turnSystem.text = text; // Temporary solution

        //if (IsServer)
        //    StartCoroutine(CreatePlayerBases());
    }

    private void Start()
    {
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreatePlayerBasesServerRpc()
    {
        CreatePlayerBasesClientRpc();
    }

    [ClientRpc]
    private void CreatePlayerBasesClientRpc()
    {
        Debug.Log("Spawning bases");
        
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
}