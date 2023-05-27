using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// health кажется нужно сделать NetworkVariable?
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
    public Vector2Int GetStandingOnTile()
    {
        return unit.standingOn.coord.Value;
    }
    public void TakeDamage(int damage){
        this.health -= damage;
        if(this.health <= 0)
            Destroy(gameObject);
    }
}
