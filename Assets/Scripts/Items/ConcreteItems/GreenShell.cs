using System;
using System.Collections;
using Items;
using Unity.Netcode;
using UnityEngine;

public class GreenShell : ItemFactory, IItem
{
    public Rigidbody rb;
    public float timeStun;
    public float force = 10;
    public GameObject model;
    
    Vector3 gravity = new Vector3(0, 9.8f, 0);
    
    public void DropItem(Vector3 direction, NetworkObject Thrower = null)
    {
        if (Thrower != null && playerThrowId == Thrower.NetworkObjectId)
            return;
        
        transform.SetParent(null);
        rb.isKinematic = false;
        
        transform.LookAt(direction);
        
        rb.AddForce(direction * force, ForceMode.Impulse);
        
        Destroy(gameObject, 10f);
    }

    private void Update()
    {
        model.transform.Rotate(0, 60 * Time.deltaTime, 0, Space.World);
    }

    private void FixedUpdate()
    {
        if (transform.parent != null) return;

        rb.linearVelocity -= gravity;
        rb.linearVelocity = rb.linearVelocity.normalized * force;
    }
    
    public override void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.TryGetComponent<KartController>(out _))
        {
            Vector3 normal = collision.contacts[0].normal;

            Vector3 newDirection = Vector3.Reflect(rb.linearVelocity.normalized, normal);

            rb.linearVelocity = newDirection * force;
        }
    }

    public override void ApplyEffect(KartController controller)
    {
        Debug.Log("Applying Carapace");
        controller.TakeBanana(timeStun);
        Destroy(gameObject);
    }
}