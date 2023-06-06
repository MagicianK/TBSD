using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace PlayerBaseStates
{
    public class Idle : IState
    {
        private PlayerBase owner;

        public Idle(PlayerBase owner)
        { this.owner = owner; }

        public void Enter()
        {
            SelectedView.instance.MoveTo(new Vector3(0, -5, 0));
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
            SelectedView.instance.MoveTo(stateOwner.transform.position);
        }

        public void Execute()
        {
            void CreateUnit(int type)
            {
                List<TileCube> tiles = RangeFinder.GetTilesRange(stateOwner.standingOn.Value, 1);

                foreach (var tile in tiles)
                {
                    Debug.Log("Tile " + tile);
                    if (tile && !tile.isBlocked.Value)
                    {
                        Debug.Log("Creating Unit");
                        BoardManager.instance.BlockTileServerRpc(tile.coord.Value);
                        stateOwner.CreateUnitServerRpc(tile.coord.Value, stateOwner.OwnerClientId, type);

                        break;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                CreateUnit(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                CreateUnit(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                CreateUnit(3);
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
    public NetworkVariable<int> health = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone);

    public RangeFinder rangeFinder = new RangeFinder();
    public NetworkVariable<Vector2Int> standingOn = new NetworkVariable<Vector2Int>(default, NetworkVariableReadPermission.Everyone);
    private Color startColor;
    public NetworkVariable<bool> isChecked = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone);

    public string ProductName { get; set; }

    public void Initialize()
    {
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (IsOwner)
            GetComponentInChildren<Renderer>().material.color = Color.blue;
        else
            GetComponentInChildren<Renderer>().material.color = Color.red;
        stateMachine.ChangeState(new PlayerBaseStates.Idle(this));
        health.Value = unitData.Health;
        startColor = GetComponentInChildren<Renderer>().material.color;
        units = new List<Unit>();
    }

    public void OnTurnChanged(int prev, int curr)
    {
        // Debug.LogWarning($"wow {prev} -> {curr}");
        // if (curr != this.team.Value)
        // {
        //     foreach (Unit unit in units)
        //     {
        //         if(unit == null){
        //             units.Remove(unit);
        //             continue;
        //         }

        //         unit.stateMachine.ChangeState(new UnitStates.Idle(unit));
        //         unit.GetComponent<Unit>().enabled = false;
        //         unit.GetComponentInChildren<Renderer>().material.color = Color.gray;
        //     }

        // }
        // else{
        //     foreach (Unit unit in units)
        //     {
        //         if(unit == null){
        //             units.Remove(unit);
        //             continue;
        //         }
        //         if(unit.isDisabled() && !isChecked.Value)
        //         {
        //             unit.IncrementTurnCounterServerRpc();
        //         }

        //         unit.GetComponent<Unit>().enabled = true;
        //         unit.GetComponentInChildren<Renderer>().material.color = startColor;

        //     }
        // }
    }

    private void Update()
    {
        if (!IsOwner)
            return;
        // ! Temporary solution
        // ? how to assign mouseController to it in a better way?
        if (!mouseController)
        {
            mouseController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<MouseController>();
        }
        stateMachine.Update();
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckServerRpc()
    {
        isChecked.Value = true;
    }

    public void Uncheck()
    {
        ///UncheckServerRpc();
    }

    // [ServerRpc(RequireOwnership = false)]
    // public void CheckServerRpc()
    // {
    //     isChecked.Value = true;
    // }
    public void Check()
    {
        CheckServerRpc();
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log($"Player {(this.team.Value == 1 ? 0 : 1)} won!!!");
        mouseController.InLostServerRpc();
        TurnManager.instance.ChangeSceneServerRpc();
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
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        this.health.Value -= damage;
        if (this.health.Value <= 0){
            Destroy(gameObject);
            NetworkObject.Despawn();
        }
    }

    public void TakeDamage(int damage)
    {
        TakeDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CreateUnitServerRpc(Vector2Int pos, ulong clientId, int type)
    {
        GameObject unitToCreate = unit1prefab.gameObject;
        switch (type)
        {
            case 1:
                unitToCreate = unit1prefab.gameObject;
                break;

            case 2:
                unitToCreate = unit2prefab.gameObject;
                break;

            case 3:
                unitToCreate = unit3prefab.gameObject;
                break;
        }
        GameObject unitToPlace = Instantiate(unitToCreate, this.transform.position, this.transform.rotation);
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
            if (unit.NetworkObjectId == networkId)
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
        if (TurnManager.instance.currentTeam.Value != team.Value)
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
        if (!IsOwner)
            return;
        GetComponentInChildren<Renderer>().material.color = Color.white;
    }

    private void OnMouseExit()
    {
        if (!IsOwner)
            return;
        GetComponentInChildren<Renderer>().material.color = startColor;
    }

    public Vector2Int WhereAmI()
    {
        return standingOn.Value;
    }
}