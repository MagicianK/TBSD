using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HealthSystem : NetworkBehaviour, IDamagable
{
    public NetworkVariable<int> health = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone);
    Unit unit;
    [ServerRpc]
    public void SetHealthServerRpc()
    {
        health.Value = unit.unitData.Health;
    }
    private void Start() {
        if(TryGetComponent<Unit>(out Unit unit))
            this.unit = unit;
        
        SetHealthServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage){
        this.health.Value -= damage;
        if (IsDead()){
            unit.DestroyServerRpc();
        }
    }
    public void TakeDamage(int damage)
    {
        TakeDamageServerRpc(damage);
    }
    public bool IsDead()
    {
        return this.health.Value <= 0;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeHealServerRpc(int heal)
    {
        this.health.Value += heal;
        if(this.health.Value > unit.unitData.Health)
            this.health.Value = unit.unitData.Health;
    }
    public void TakeHeal(int heal)
    {
        TakeHealServerRpc(heal);
    }

    
    public int GetPreyTeam()
    {
        return unit.team.Value;
    }

    public Vector2Int WhereAmI()
    {
        return unit.standingOn.Value;
    }
}
