using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HealthSystem : MonoBehaviour, IDamagable
{
    private int health;
    Unit unit;

    private void Start() {
        if(TryGetComponent<Unit>(out Unit unit))
            this.unit = unit;
        
        health = unit.unitData.Health;
    }
    public void TakeDamage(int damage){
        this.health -= damage;
        if (IsDead()){
            unit.DestroyServerRpc();
        }
    }

    public bool IsDead()
    {
        return this.health <= 0;
    }

    public void TakeHeal(int heal)
    {
        this.health += heal;
        if(this.health > unit.unitData.Health)
            this.health = unit.unitData.Health;
    }

    public int GetPreyTeam()
    {
        return unit.team;
    }

    public Vector2Int WhereAmI()
    {
        return unit.standingOn;
    }
}
