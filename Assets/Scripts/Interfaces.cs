using UnityEngine;
public interface IDamagable
{
    public void TakeDamage(int damage);
    public int GetPreyTeam();
    public Vector2Int WhereAmI();
}
public interface IPlaceable{
    
}
public interface IHealable
{
    public void TakeHeal(int heal);
}
public interface ISwitchable
{
    public int GetPreyTeam();
    public Vector2Int GetStandingOnTile();
    public void Switch(Vector2Int dest); 
}
public interface IAbility
{
    public void Activate(Unit owner);
    public void Deactivate();
}

public interface ICanBeDisabled
{
    public bool isDisabled();
    public void Disable();
    public void Enable();
    public int GetPreyTeam();
    public Vector2Int WhereAmI();
    public Vector2Int GetStandingOnTile();
}