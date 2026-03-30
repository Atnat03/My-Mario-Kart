using System.Collections;
using Items;
using MyPrint;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
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
    
    [Header("UI")]
    [SerializeField] private GameObject itemUI;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Sprite[] iconSpriteList;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            gameObject.name += " " + NetworkManager.LocalClientId;
            itemUI.SetActive(false);
        }
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
        
        itemId = i;

        StartCoroutine(GetNewItemCoroutine(i));


        haveAnItem.Value = true;
    }

    IEnumerator GetNewItemCoroutine(int finalIndex)
    {
        itemUI.SetActive(true);
        
        itemUI.transform.localScale = Vector3.zero;

        float elapsed = 0;

        while (elapsed < 0.25f)
        {
            elapsed += Time.deltaTime;
            itemUI.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsed / 0.25f);
            yield return null;
        }
        
        int[] indexList = new int[10];

        int ind = 0;
        
        int maxIndex = 4;

        for (int i = 0; i < indexList.Length; i++)
        {
            indexList[i] = (i % maxIndex);
        }
        
        indexList[^1] = finalIndex;

        float finalIntervalTime = 0.25f;
        
        Console.PrintList(indexList, ColorConsole.Orange);
        
        for(int i = 0 ; i < indexList.Length ; i++)
        {
            itemIcon.sprite = iconSpriteList[indexList[i]];
            
            yield return new WaitForSeconds(finalIntervalTime);
        }
        
        SpawnVisualClientRpc(itemId);
    }

    IEnumerator DropItemUI()
    {
        float elapsed = 0.25f;
        
        while (elapsed > 0)
        {
            elapsed -= Time.deltaTime;
            itemUI.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsed / 0.25f);
            yield return null;
        }
        
        itemUI.SetActive(false);
    }
    
    
    [Rpc(SendTo.Everyone)]
    void SpawnVisualClientRpc(int itemId)
    {
        Item item = dataItem.itemList[itemId];

        visualInstance = Instantiate(item.prefabVisual, itemPos.position, itemPos.rotation);
        visualInstance.transform.SetParent(itemPos);
    }
}
