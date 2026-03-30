using Items;
using Unity.Netcode;
using UnityEngine;

public class RedShell : ItemFactory, IItem
{
    public Rigidbody rb;
    public float timeStun = 1.5f;
    public float force = 28f;
    public GameObject model;
    public float radiusRaycast = 9f;

    private NetworkObject thrower;
    private Transform currentTarget;

    private float homingActivationTime = 0.5f;
    private float launchTime;
    private bool hasHit = false;
    private bool isActive = false;

    public void DropItem(Vector3 direction, NetworkObject Thrower)
    {
        if (Thrower != null && playerThrowId == Thrower.NetworkObjectId)
            return;
        
        this.thrower = Thrower;

        rb.isKinematic = false;
        transform.parent = null;

        direction = new Vector3(direction.x, 0f, direction.z).normalized;

        transform.LookAt(transform.position + direction);
        rb.linearVelocity = direction * force;

        launchTime = Time.time;
        isActive = true;

        currentTarget = null;

        Destroy(gameObject, 15f);
    }

    private void Update()
    {
        if (model != null)
            model.transform.Rotate(0, 120 * Time.deltaTime, 0, Space.World);
    }

    private void FixedUpdate()
    {
        if (!isActive || !IsServer) return;
        if (transform.parent != null) return;

        if (Time.time - launchTime < homingActivationTime)
        {
            rb.linearVelocity = transform.forward * force;
            return;
        }

        Collider[] cols = Physics.OverlapSphere(transform.position, radiusRaycast, LayerMask.GetMask("Player"));

        Transform bestTarget = null;
        float bestDistance = Mathf.Infinity;

        foreach (Collider c in cols)
        {
            GameObject playerObj = c.gameObject;

            if (thrower != null && playerObj == thrower.gameObject)
                continue;

            float distSq = (playerObj.transform.position - transform.position).sqrMagnitude;

            if (distSq < bestDistance)
            {
                bestDistance = distSq;
                bestTarget = playerObj.transform;
            }
        }

        currentTarget = bestTarget;

        Vector3 moveDir;

        if (currentTarget != null)
        {
            moveDir = (currentTarget.position - transform.position).normalized;
        }
        else
        {
            moveDir = transform.forward;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, 
                                              Quaternion.LookRotation(moveDir), 
                                              12f * Time.fixedDeltaTime);

        rb.linearVelocity = moveDir * force;
    }

    public override void OnCollisionEnter(Collision collision)
    {
        if (!IsServer || hasHit) return;

        if (collision.collider.TryGetComponent<KartController>(out KartController kart))
        {
            if (thrower != null && collision.collider.GetComponent<NetworkObject>() == thrower)
                return;

            hasHit = true;
            ApplyEffect(kart);
            return;
        }

        if (collision.contacts.Length > 0)
        {
            Vector3 normal = collision.contacts[0].normal;
            Vector3 newDirection = Vector3.Reflect(rb.linearVelocity.normalized, normal);
            
            rb.linearVelocity = newDirection * force;
            transform.rotation = Quaternion.LookRotation(newDirection);
        }
    }

    public override void ApplyEffect(KartController controller)
    {
        if (!IsServer) return;

        Debug.Log($"Red Shell a touché {controller.name}");
        controller.TakeBanana(timeStun);

        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn(true);
        else
            Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radiusRaycast);
    }
}