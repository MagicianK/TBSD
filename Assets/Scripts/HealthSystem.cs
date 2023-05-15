using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour, IDamagable
{
    Unit unit;
    private int health;
    int team;
    private void Start() {
        if(TryGetComponent<Unit>(out Unit unit))
            this.unit = unit;
        this.team = unit.team;
        this.health = unit.maxHealth;
    }
    public int GetPreyTeam(){
        return this.team;
    }
    public TileCube GetStandingOnTile()
    {
        return unit.standingOn;
    }
    public void TakeDamage(int damage){
        this.health -= damage;
        if(this.health <= 0)
            Destroy(gameObject);
    }
}
