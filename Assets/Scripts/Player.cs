using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace PlayerBaseStates
{
    public class Idle : IState
    {
        private Player owner;

        public Idle(Player owner)
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
        private Player owner;

        public Selected(Player owner)
        { this.owner = owner; }

        public void Enter()
        {
        }

        public void Execute()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //Debug.Log("Base standing on: " + owner.GetStandingOnTile().gridLocation);
                Debug.Log("Checking if owner is null " + owner.location2D.Value);
                RangeFinder.GetTilesRangeServerRpc(owner.location2D.Value, 1, out List<TileCube> tiles);

                foreach (var tile in tiles)
                {
                    Debug.Log("Tile " + tile);
                    if (tile && !tile.isBlocked)
                    {
                        owner.CreateUnitServerRpc(tile.grid2DLocation);
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

public class Player : NetworkBehaviour, IDamagable
{
    public Unit unit1prefab;
    public Unit unit2prefab;
    public Unit unit3prefab;
    public int team;
    public int points = 5000;
    public StateMachine stateMachine = new StateMachine();
    public Unit unitToPlace;
    [SerializeField]
    private UnitData unitData;

    public List<Unit> units;
    private int health;
    public TileCube standingOn;
    public NetworkVariable<Vector2Int> location2D = new NetworkVariable<Vector2Int>(default, NetworkVariableReadPermission.Everyone);
    private Color startColor;

    // Start is called before the first frame update
    private void Start()
    {
        stateMachine.ChangeState(new PlayerBaseStates.Idle(this));
        health = unitData.Health;
        startColor = GetComponentInChildren<Renderer>().material.color;
        units = new List<Unit>();
    }

    private void Update()
    {
        stateMachine.Update();
        if (GameManager.instance.turnSystem == null)
        {
            return;
        }

        if (GameManager.instance.turnSystem.currentTeam != this.team)
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

    public TileCube GetStandingOnTile()
    {
        return standingOn;
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
    public void CreateUnitServerRpc(Vector2Int pos)
    {
        if (Board.instance.map.ContainsKey(pos))
        {
            TileCube tileCube = Board.instance.map[pos];
            CreateUnit(tileCube);
        }
        else
        {
            Debug.Log("Position does not exist!");
        }
    }

    [ClientRpc]
    public void CreateUnitClientRpc(Vector2Int pos)
    {
        if (Board.instance.map.ContainsKey(pos))
        {
            TileCube tileCube = Board.instance.map[pos];
            CreateUnit(tileCube);
        }
        else
        {
            Debug.Log("Position does not exist!");
        }
    }

    public void CreateUnit(TileCube tileCube)
    {
        unitToPlace = Instantiate(unit1prefab);
        if (IsServer)
            unitToPlace.NetworkObject.Spawn();

        unitToPlace.standingOn = tileCube;
        tileCube.isBlocked = true;
        unitToPlace.transform.position = tileCube.transform.position;
        unitToPlace.InitValues(this.team, this);
        MouseController.instance.selectedUnit = unitToPlace;
        units.Add(unitToPlace);
        unitToPlace = null;
    }

    private void OnMouseDown()
    {
        // Enables selection state only if Mouse state is Idle and it is their turn
        if (MouseController.instance.mouseStateMachine.currentState is MouseStates.Idle &&
            GameManager.instance.turnSystem.currentTeam == this.team)
        {
            MouseController.instance.mouseStateMachine.ChangeState(new MouseStates.OnPlayerBaseState(this));
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