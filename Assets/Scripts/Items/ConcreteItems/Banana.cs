using System.Collections;
using Items;
using Unity.Netcode;
using UnityEngine;

public class Banana : ItemFactory, IItem
{
    public Rigidbody rb;
    public float timeStun;
    public float force = 10;

    public void DropItem(Vector3 direction, NetworkObject Thrower = null, bool isFront = false)
    {
        rb.isKinematic = false;
        
        if(isFront)
            direction += Vector3.up * 0.5f;
        else
        {
            direction.z *= 0.2f;
        }
        
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    public override void ApplyEffect(KartController controller)
    {
        if (!IsServer) return;
        
        NetworkObject n = controller.GetComponent<NetworkObject>();
        
        if(controller.OwnerClientId != playerThrowId)
            GameManager.instance.AddScore(playerThrowId, _scoreGain);
        
        Debug.Log("Applying banana");

        controller.TakeBanana(timeStun);

        GetComponent<NetworkObject>().Despawn(true);
    }
}
