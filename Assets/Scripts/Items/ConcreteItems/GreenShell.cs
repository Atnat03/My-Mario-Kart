using System;
using System.Collections;
using Items;
using Unity.Netcode;
using UnityEngine;

public class GreenShell : ItemFactory, IItem
{
    [Header("Shell Settings")]
    public Rigidbody rb;
    public float timeStun = 1.5f;
    public float force = 10f;
    public GameObject model;
    
    [Header("Physics Settings")]
    public float verticalBounceMultiplier = 0.7f;
    public float minSpeedBeforeDestroy = 0.5f;
    public float immunityDuration = 0.3f;
    
    private ulong throwerId;
    private bool hasBeenThrown = false;
    private bool hasHit = false;
    private float throwTime;
    private int bounceCount = 0;
    private const int maxBounces = 10;
    
    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }
    
    public void DropItem(Vector3 direction, NetworkObject Thrower, bool isFront = false)
    {
        if (Thrower != null)
        {
            throwerId = Thrower.NetworkObjectId;
            playerThrowId = throwerId;
        }
        
        transform.SetParent(null);
        
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        hasBeenThrown = true;
        hasHit = false;
        throwTime = Time.time;
        bounceCount = 0;
        
        Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z).normalized;
        
        rb.linearVelocity = horizontalDirection * force;
        
        if (horizontalDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(horizontalDirection);
        
        Destroy(gameObject, 10f);
    }

    private void Update()
    {
        if (model != null)
            model.transform.Rotate(0, 60 * Time.deltaTime, 0, Space.World);
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;
        if (!hasBeenThrown || hasHit) return;

        if (rb.linearVelocity.sqrMagnitude < minSpeedBeforeDestroy * minSpeedBeforeDestroy)
        {
            DestroyShell();
            return;
        }

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (horizontalVelocity.sqrMagnitude > 0.0001f)
        {
            Vector3 constantHorizontal = horizontalVelocity.normalized * force;

            rb.linearVelocity = new Vector3(
                constantHorizontal.x,
                rb.linearVelocity.y,
                constantHorizontal.z
            );
            
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(horizontalVelocity),
                10f * Time.fixedDeltaTime
            );
        }
    }
    
    public override void OnCollisionEnter(Collision collision)
    {
        if (!IsServer || hasHit) return;

        if (collision.collider.TryGetComponent<KartController>(out KartController kart))
        {
            NetworkObject netObj = collision.collider.GetComponent<NetworkObject>();
            if (netObj != null && netObj.NetworkObjectId == throwerId)
            {
                if (Time.time - throwTime < immunityDuration)
                {
                    Physics.IgnoreCollision(collision.collider, GetComponent<Collider>(), true);
                    return;
                }
            }

            hasHit = true;
            ApplyEffect(kart);
            return;
        }

        if (collision.contacts.Length > 0)
        {
            Vector3 normal = collision.contacts[0].normal;
            Vector3 velocity = rb.linearVelocity;

            Vector3 reflected = Vector3.Reflect(velocity, normal);

            Vector3 horizontalReflected = new Vector3(reflected.x, 0, reflected.z);
            
            if (horizontalReflected.sqrMagnitude > 0.01f)
            {
                horizontalReflected = horizontalReflected.normalized * force;
            }
            else
            {
                horizontalReflected = new Vector3(-velocity.x, 0, -velocity.z).normalized * force;
            }
            
            rb.linearVelocity = new Vector3(
                horizontalReflected.x,
                Mathf.Max(reflected.y * verticalBounceMultiplier, -20f),
                horizontalReflected.z
            );
            
            if (horizontalReflected.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(horizontalReflected);
            }
        }
    }

    public override void ApplyEffect(KartController controller)
    {
        if (!IsServer) return;

        controller.TakeBanana(timeStun);
        
        if (controller.OwnerClientId != playerThrowId)
        {
            GameManager.instance.AddScore(playerThrowId, _scoreGain);
        }

        DestroyShell();
    }
    
    private void DestroyShell()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        if (hasBeenThrown && !hasHit)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, rb != null ? rb.linearVelocity : Vector3.forward);
        }
    }
}