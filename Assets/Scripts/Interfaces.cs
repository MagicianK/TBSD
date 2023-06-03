using UnityEngine;
public interface IDamagable
{
    public void TakeDamage(int damage);
    public int GetPreyTeam();
    public Vector2Int WhereAmI();
}

public interface IHealable
{
    public void TakeHeal(int heal);
}