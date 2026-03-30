using Unity.Netcode;
using UnityEngine;

namespace Items.ConcreteItems
{
    public class Mushroom : ItemFactory, IItem
    {
        public void DropItem(Vector3 direction, NetworkObject Thrower = null)
        {
            if (Thrower == null) return;
            
            Debug.Log("Take mushroom");
            
            KartController controller = Thrower.GetComponent<KartController>();
            
            controller.TakeMushroom();
            
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}