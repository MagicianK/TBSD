using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New UnitData", menuName = "Unit Data", order = 51)]
public class UnitData : ScriptableObject, INetworkSerializable
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

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref unitName);
        serializer.SerializeValue(ref description);
        serializer.SerializeValue(ref goldCost);
        serializer.SerializeValue(ref movementRange);
        serializer.SerializeValue(ref health);
    }
}