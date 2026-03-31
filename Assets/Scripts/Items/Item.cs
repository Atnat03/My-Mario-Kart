using System;
using Unity.Netcode;
using UnityEngine;

namespace Items
{
    [Serializable]
    public class Item
    {
        public int id;
        public string name;
        public GameObject prefab;
        public GameObject prefabVisual;
    }

    public interface IItem
    {
        public void DropItem(Vector3 direction,NetworkObject Thrower, bool isFront);
        public void ApplyEffect(KartController controller);
    }

    [RequireComponent(typeof(Rigidbody))]
    public abstract class ItemFactory : NetworkBehaviour
    {
        protected ulong playerThrowId;

        [SerializeField] protected int _scoreGain = 10;
        
        public void SetPlayerThrowId(ulong playerThrowID) => playerThrowId = playerThrowID;
        
        public virtual void OnCollisionEnter(Collision other)
        {
            if (other.collider.TryGetComponent(out KartController controller))
            {
                ApplyEffect(controller);
            }
        }
        
        public virtual void ApplyEffect(KartController controller)
        { }
    }
}