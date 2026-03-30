using System.Collections;
using Items;
using Unity.Netcode;
using UnityEngine;

public class Banana : ItemFactory, IItem
{
    public Rigidbody rb;
    public float timeStun;
    public float force = 10;
    
    public void DropItem(Vector3 direction, NetworkObject Thrower = null)
    {
        rb.isKinematic = false;
        
        direction += Vector3.up * 0.5f;
        
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    public override void ApplyEffect(KartController controller)
    {
        if (!IsServer) return;
        
        NetworkObject n = controller.GetComponent<NetworkObject>();
        
        GameManager.instance.AddScore(playerThrowId, 10);

        Debug.Log("Applying banana");

        controller.TakeBanana(timeStun);

        GetComponent<NetworkObject>().Despawn(true);
    }
}
