using Items;
using Unity.Netcode;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class PlayerItem : NetworkBehaviour
{
    public ItemSO dataItem;
    
    public Transform itemPos;
    public Transform itemPosFront;

    public int itemId;
    
    public NetworkVariable<bool> haveAnItem = new(false);
    NetworkObject currentItem = null;
    private GameObject visualInstance;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
            gameObject.name += " " + NetworkManager.LocalClientId;
    }
    
    public void Update()
    {
        if (!IsOwner) return;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(Input.GetKey(KeyCode.LeftControl))
                DropItemServerRpc(transform.forward, itemPosFront.position, itemPosFront.rotation);
            else
            {
                DropItemServerRpc(-transform.forward, itemPos.position, itemPos.rotation);
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void DropItemServerRpc(Vector3 direction, Vector3 pos, Quaternion rot)
    {
        if (!haveAnItem.Value) return;

        currentItem = Instantiate(dataItem.itemList[itemId].prefab, pos, rot).GetComponent<NetworkObject>();
        currentItem.TryRemoveParent();
        currentItem.Spawn();
        
        
        if (currentItem != null)
        {
            currentItem.GetComponent<IItem>().DropItem(direction, GetComponent<NetworkObject>());
            
            currentItem.GetComponent<ItemFactory>().SetPlayerThrowId(OwnerClientId);
            
            currentItem = null;
        }

        haveAnItem.Value = false;
        itemId = -1;

        DropItemClientRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void DropItemClientRpc()
    {
        if (visualInstance != null)
        {
            Destroy(visualInstance);
            visualInstance = null;
        }
    }

    public void PickUpNewItem()
    {
        if (haveAnItem.Value) return;
        
        int i = Random.Range(0, dataItem.itemList.Count);
        
        itemId = 0;

        SpawnVisualClientRpc(itemId);

        haveAnItem.Value = true;
    }
    
    
    [Rpc(SendTo.Everyone)]
    void SpawnVisualClientRpc(int itemId)
    {
        Item item = dataItem.itemList[itemId];

        visualInstance = Instantiate(item.prefabVisual, itemPos.position, itemPos.rotation);
        visualInstance.transform.SetParent(itemPos);
    }
}
