using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public interface IProduct
{
    public string ProductName { get; set; }

    public void Initialize();
}

public abstract class Factory : NetworkBehaviour
{
    public abstract IProduct GetProduct(Vector2Int pos, ulong clientId);
}