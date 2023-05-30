using UnityEngine;
using Unity.Netcode;

// base factory for player 2
public class BaseFactory1 : Factory
{
    [SerializeField]
    private PlayerBase BasePrefab1;

    public override IProduct GetProduct(Vector2Int pos, ulong clientId)
    {
        TileCube tile = BoardManager.instance.GetTileAtPosition(pos);
        BoardManager.instance.BlockTileServerRpc(tile.coord.Value);
        GameObject instance = Instantiate(BasePrefab1.gameObject, tile.transform.position, Quaternion.identity);
        instance.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        PlayerBase newProduct = instance.GetComponent<PlayerBase>();
        newProduct.standingOn.Value = tile.coord.Value;
        newProduct.Initialize();

        return newProduct;
    }
}