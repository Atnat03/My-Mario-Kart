using System;
using System.Collections;
using Items;
using MyPrint;
using Unity.Netcode;
using UnityEngine;
using Console = MyPrint.Console;

public class Bomb : ItemFactory, IItem
{
    public Rigidbody rb;
    public float timeStun;
    public float force = 10;
    public float radiusExplosion = 3;
    public float timeToExplose = 3f;
    [SerializeField] private ParticleSystem particlesExplode;
    [SerializeField] private Animator _animator;

    private bool startTimer = false;
    
    private NetworkVariable<float> elapsedTimeToExplose = new NetworkVariable<float>(0);
    
    private void Update()
    {
        if(!IsServer) return;
        
        if (elapsedTimeToExplose.Value > 0)
        {
            elapsedTimeToExplose.Value -= Time.deltaTime;

            if (elapsedTimeToExplose.Value <= 0)
            {
                Explode();
            }
        }
    }

    public void DropItem(Vector3 direction, NetworkObject Thrower = null, bool isFront = false)
    {
        DropItemRpc(direction, isFront);
    }

    [Rpc(SendTo.Everyone)]
    private void DropItemRpc(Vector3 direction, bool isFront)
    {
        rb.isKinematic = false;

        if (isFront)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
            rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);
        }
        else
        {
            direction.z *= 0.2f;
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }

    public override void ApplyEffect(KartController controller)
    {
        if (!IsServer) return;

        Explode();
    }

    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radiusExplosion, LayerMask.GetMask("Player"));

        if (colliders.Length > 0)
        {
            GameManager.instance.AddScore(playerThrowId, _scoreGain);

            foreach (Collider col in colliders)
            {
                if (col.TryGetComponent(out KartController kartController))
                {
                    kartController.TakeBanana(timeStun);
                }
            }
        }

        ExplosionParticleClientRpc();
        
        GetComponent<NetworkObject>().Despawn(true);
    }

    public override void OnCollisionEnter(Collision other)
    {
        if (!IsServer) return;
        if (startTimer) return;

        startTimer = true;
        elapsedTimeToExplose.Value = timeToExplose;

        StartMeshClientRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void StartMeshClientRpc()
    {
        _animator.SetTrigger("Explose");
    }

    [Rpc(SendTo.Everyone)]
    private void ExplosionParticleClientRpc()
    {
        Instantiate(particlesExplode, transform.position, Quaternion.identity);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radiusExplosion);
    }
}
