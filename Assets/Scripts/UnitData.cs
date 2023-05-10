using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New UnitData", menuName = "Unit Data", order = 51)]
public class UnitData : ScriptableObject
{
    [SerializeField]
    private string unitName;
    [SerializeField]
    private string description;
    [SerializeField]
    private Sprite icon;
    [SerializeField]
    private int goldCost;
    [SerializeField]
    private int movementRange;
    [SerializeField]
    private int health;

        public string UnitName
    {
        get
        {
            return unitName;
        }
    }

    public string Description
    {
        get
        {
            return description;
        }
    }

    public Sprite Icon
    {
        get
        {
            return icon;
        }
    }

    public int GoldCost
    {
        get
        {
            return goldCost;
        }
    }
    public int MovementRange
    {
        get
        {
            return movementRange;
        }
    }
    public int Health
    {
        get
        {
            return health;
        }
    }
}