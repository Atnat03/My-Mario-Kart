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
    
    private Vector3 gravity = new Vector3(0, -9.81f, 0);
    private bool hasBeenThrown = false;
    
    public void DropItem(Vector3 direction, NetworkObject Thrower, bool isFront = false)
    {
        transform.SetParent(null);
        rb.isKinematic = false;
        hasBeenThrown = true;
        
        Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z).normalized;
        
        rb.linearVelocity = horizontalDirection * force;
        
        Destroy(gameObject, 10f);
    }

    private void Update()
    {
        model.transform.Rotate(0, 60 * Time.deltaTime, 0, Space.World);
    }

    private void FixedUpdate()
    {
        if (!hasBeenThrown) return;

        rb.AddForce(gravity, ForceMode.Acceleration);

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (horizontalVelocity.magnitude > 0.01f)
        {
            Vector3 constant = horizontalVelocity.normalized * force;

            rb.linearVelocity = new Vector3(
                constant.x,
                rb.linearVelocity.y,
                constant.z
            );
        }
    }
    
    public override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if (!collision.collider.TryGetComponent<KartController>(out _))
        {
            Vector3 normal = collision.contacts[0].normal;

            Vector3 velocity = rb.linearVelocity;

            Vector3 reflected = Vector3.Reflect(velocity, normal);

            rb.linearVelocity = reflected.normalized * force;
        }
    }

    public override void ApplyEffect(KartController controller)
    {
        Debug.Log("Applying Green Shell");
        controller.TakeBanana(timeStun);
        Destroy(gameObject);
    }
}