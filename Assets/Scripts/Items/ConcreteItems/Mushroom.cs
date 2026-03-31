using Unity.Netcode;
using UnityEngine;

namespace Items.ConcreteItems
{
    public class Mushroom : ItemFactory, IItem
    {
        public void DropItem(Vector3 direction, NetworkObject thrower = null, bool isFront = false)
        {
            if (thrower == null) return;

            ulong playerId = thrower.OwnerClientId;

            Debug.Log("Take mushroom");

            EatMushroomClientRpc(playerId);

            GetComponent<NetworkObject>().Despawn(true);
        }
        
        [Rpc(SendTo.Everyone)]
        public void EatMushroomClientRpc(ulong playerId)
        {
            if (NetworkManager.LocalClientId != playerId) return;

            NetworkObject playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerId);

            if (playerObj == null) return;

            KartController controller = playerObj.GetComponent<KartController>();

            controller.TakeMushroom();
        }
        
    }
}