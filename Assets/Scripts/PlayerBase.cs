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
                List<TileCube> tiles = stateOwner.rangeFinder.GetTilesRange(stateOwner.standingOn.Value, 1);

                foreach (var tile in tiles)
                {
                    Debug.Log("Tile " + tile);
                    if (tile && !tile.isBlocked.Value)
                    {
                        Debug.Log("Creating Unit");
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
    public int team;
    public StateMachine stateMachine = new StateMachine();
    [SerializeField]
    private UnitData unitData;

    public MouseController mouseController;
    public List<Unit> units;
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
        if (!mouseController)
            mouseController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<MouseController>();
        stateMachine.Update();
        //if (GameManager.instance.turnSystem == null)
        //{
        //    return;
        //}

        // TODO: Turn Manager
        if (false)
        {
            foreach (Unit unit in units)
            {
                unit.stateMachine.ChangeState(new UnitStates.Idle(unit));
                unit.GetComponent<Unit>().enabled = false;
                unit.GetComponentInChildren<Renderer>().material.color = Color.gray;
            }
        }
        else
        {
            foreach (Unit unit in units)
            {
                unit.GetComponent<Unit>().enabled = true;
                unit.GetComponentInChildren<Renderer>().material.color = startColor;
            }
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"Player {this.team - 1}");
    }

    public Vector2Int GetStandingOnTile()
    {
        return standingOn.Value;
    }

    public int GetPreyTeam()
    {
        return this.team;
    }

    public void TakeDamage(int damage)
    {
        this.health -= damage;
        if (this.health <= 0)
            Destroy(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CreateUnitServerRpc(Vector2Int pos, ulong clientId)
    {
        if (BoardManager.instance.GetTileAtPosition(pos))
        {
            TileCube tileCube = BoardManager.instance.GetTileAtPosition(pos);
            CreateUnit(tileCube, clientId);
        }
        else
        {
            Debug.Log("Position does not exist!");
        }
    }

    [ClientRpc]
    public void CreateUnitClientRpc(Vector2Int pos)
    {
        if (BoardManager.instance.GetTileAtPosition(pos))
        {
            TileCube tileCube = BoardManager.instance.GetTileAtPosition(pos);
            CreateUnit(tileCube, NetworkManager.LocalClientId);
        }
        else
        {
            Debug.Log("Position does not exist!");
        }
    }

   
    // Страшное мессиво
    public void CreateUnit(TileCube tileCube, ulong clientId)
    {
        GameObject unitToPlace = Instantiate(unit1prefab.gameObject);
     
        Debug.LogWarning("Spawned with ownership for " + clientId);

        unitToPlace.GetComponent<Unit>().standingOn = tileCube;
        BoardManager.instance.BlockTileServerRpc(tileCube.coord.Value);
        unitToPlace.transform.position = tileCube.transform.position;

        // mouseController is given to unit here
        // but it work surely given only to the server side.
        // In the client unit does not have mouseController
        unitToPlace.GetComponent<Unit>().InitValues(this.team, this, mouseController);
        mouseController.selectedUnit = unitToPlace.GetComponent<Unit>();
        units.Add(unitToPlace.GetComponent<Unit>());
        if (IsServer)
            unitToPlace.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

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
}