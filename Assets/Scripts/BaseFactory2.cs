using UnityEngine;
using Unity.Netcode;
public class BaseFactory2 : Factory
{
    [SerializeField]
    private PlayerBase BasePrefab2;

    public override IProduct GetProduct(Vector2Int pos)
    {
        TileCube tile = BoardManager.instance.GetTileAtPosition(pos);
        BoardManager.instance.BlockTileServerRpc(tile.coord.Value);
        GameObject instance = Instantiate(BasePrefab2.gameObject, tile.transform.position, Quaternion.identity);
        instance.GetComponent<NetworkObject>().Spawn();
        PlayerBase newProduct = instance.GetComponent<PlayerBase>();
        newProduct.standingOn = tile;
        newProduct.Initialize();

        return newProduct;
    }
}