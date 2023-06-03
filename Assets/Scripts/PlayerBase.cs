using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace PlayerBaseStates
{
    public class Idle : IState
    {
        private PlayerBase owner;

        public Idle(PlayerBase owner)
        { this.owner = owner; }

        public void Enter()
        {
        }

        public void Execute()
        {
        }

        public void Exit()
        {
        }
    }

    public class Selected : IState
    {
        private PlayerBase stateOwner;

        public Selected(PlayerBase owner)
        { this.stateOwner = owner; }

        public void Enter()
        {
        }

        public void Execute()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //Debug.Log("Base standing on: " + owner.GetStandingOnTile().gridLocation);
                //Debug.Log("Checking if owner is null " + stateOwner.location2D.Value);
                //RangeFinder.GetTilesRangeServerRpc(owner.location2D.Value, 1, out List<TileCube> tiles);
                List<TileCube> tiles = RangeFinder.GetTilesRange(stateOwner.standingOn.Value, 1);

                foreach (var tile in tiles)
                {
                    Debug.Log("Tile " + tile);
                    if (tile && !tile.isBlocked.Value)
                    {
                        Debug.Log("Creating Unit");
                        BoardManager.instance.BlockTileServerRpc(tile.coord.Value);
                        stateOwner.CreateUnitServerRpc(tile.coord.Value, stateOwner.OwnerClientId);
                        
                        break;
                    }
                }
            }
        }

        public void Exit()
        {
        }
    }
}

public class PlayerBase : NetworkBehaviour, IDamagable, IProduct
{
    public Unit unit1prefab;
    public Unit unit2prefab;
    public Unit unit3prefab;
    public NetworkVariable<int> team = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone);
    public StateMachine stateMachine = new StateMachine();
    [SerializeField]
    public UnitData unitData;

    public MouseController mouseController;
    public List<Unit> units;
    
    // ! Needs to be NetworkVariable
    private int health;
    public RangeFinder rangeFinder = new RangeFinder();
    public NetworkVariable<Vector2Int> standingOn = new NetworkVariable<Vector2Int>(default, NetworkVariableReadPermission.Everyone);
    private Color startColor;
    public string ProductName { get => productName; set => productName = value; }

    public void Initialize()
    {
        gameObject.name = productName;
    }

    // Start is called before the first frame update
    private void Start()
    {
        if(IsOwner)
            GetComponentInChildren<Renderer>().material.color = Color.red;
        stateMachine.ChangeState(new PlayerBaseStates.Idle(this));
        health = unitData.Health;
        startColor = GetComponentInChildren<Renderer>().material.color;
        units = new List<Unit>();
    }

    private void Update()
    {
        // ! Temporary solution
        // ? how to assign mouseController to it in a better way?
        if (!mouseController){
            mouseController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<MouseController>();

        }
        stateMachine.Update();
        
        // TODO: Connect with TurnManager
        if (TurnManager.instance.currentTeam.Value != this.team.Value)
        {
            foreach (Unit unit in units)
            {
                if(unit == null){
                    units.Remove(unit);
                    continue;
                }
                unit.stateMachine.ChangeState(new UnitStates.Idle(unit));
                unit.GetComponent<Unit>().enabled = false;
                unit.GetComponentInChildren<Renderer>().material.color = Color.gray;
            }
        }
        else
        {
            foreach (Unit unit in units)
            {
                if(unit == null){
                    units.Remove(unit);
                    continue;
                }
                unit.GetComponent<Unit>().enabled = true;
                unit.GetComponentInChildren<Renderer>().material.color = startColor;
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log($"Player {(this.team.Value == 1 ? 0 : 1)} won!!!");
        if(IsServer)
            NetworkManager.SceneManager.LoadScene("GameOver", UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

    public Vector2Int GetStandingOnTile()
    {
        return standingOn.Value;
    }

    public int GetPreyTeam()
    {
        return this.team.Value;
    }

    // ! Needs to be network synchronized
    public void TakeDamage(int damage)
    {
        this.health -= damage;
        if (this.health <= 0)
            Destroy(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CreateUnitServerRpc(Vector2Int pos, ulong clientId)
    {
        GameObject unitToPlace = Instantiate(unit1prefab.gameObject);
        unitToPlace.GetComponent<Unit>().NetworkObject.SpawnWithOwnership(clientId);

        CreateUnitClientRpc(pos, unitToPlace.GetComponent<Unit>().NetworkObject.NetworkObjectId);

    }
    [ClientRpc]
    public void CreateUnitClientRpc(Vector2Int pos, ulong networkId)
    {
        CreateUnit(pos, networkId);
    }   

    public void CreateUnit(Vector2Int pos, ulong networkId)
    {
        Unit unitCreation = null;
        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units)
        {
            if(unit.NetworkObjectId == networkId)
                unitCreation = unit;
        }
        unitCreation.InitValues(pos);
        unitCreation.gameObject.transform.position = BoardManager.instance.GetTileAtPosition(pos).transform.position;
        unitCreation.team.Value = team.Value;
        mouseController.selectedUnit = unitCreation;
        this.units.Add(unitCreation);
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetTeamServerRpc(int team)
    {
        this.team.Value = team;
    }
    private void OnMouseDown()
    {
        if (!IsOwner)
            return;
        
        // Enables selection state only if Mouse state is Idle and it is their turn
        if (mouseController.mouseStateMachine.currentState is MouseStates.Idle)
        //GameManager.instance.turnSystem.currentTeam == this.team
        {
            mouseController.playerBase = this;
            mouseController.mouseStateMachine.ChangeState(new MouseStates.OnPlayerBaseState(mouseController));
            stateMachine.ChangeState(new PlayerBaseStates.Selected(this));
        }
    }

    private void OnMouseEnter()
    {
        GetComponentInChildren<Renderer>().material.color = Color.white;
    }

    private void OnMouseExit()
    {
        GetComponentInChildren<Renderer>().material.color = startColor;
    }

    public Vector2Int WhereAmI()
    {
        return standingOn.Value;
    }
}