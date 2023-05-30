using UnityEngine;
using Unity.Netcode;

// Base factory for player2
public class BaseFactory2 : Factory
{
    [SerializeField]
    private PlayerBase BasePrefab2;

    public override IProduct GetProduct(Vector2Int pos, ulong clientId)
    {
        TileCube tile = BoardManager.instance.GetTileAtPosition(pos);
        BoardManager.instance.BlockTileServerRpc(tile.coord.Value);
        GameObject instance = Instantiate(BasePrefab2.gameObject, tile.transform.position, Quaternion.identity);
        instance.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        PlayerBase newProduct = instance.GetComponent<PlayerBase>();
        newProduct.standingOn.Value = tile.coord.Value;
        newProduct.Initialize();

        return newProduct;
    }
}