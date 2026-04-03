using System;
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
    public bool IsFront { get => _isFront; set => _isFront = value; }
    
    public ItemSO dataItem;
    
    public Transform itemPos;
    public Transform itemPosFront;

    public int itemId;
    
    public bool haveAnItem = false;
    NetworkObject currentItem = null;
    private GameObject visualInstance;
    
    [Header("UI")]
    [SerializeField] private GameObject itemUI;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Sprite[] iconSpriteList;

    public NetworkVariable<int> currentItemId = new(-1);
    public NetworkVariable<bool> hasItem = new();

    private bool _isFront = false;

    //Actions
    public Action OnRollItem;
    public Action OnDropItem;
    
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            gameObject.name += " " + NetworkManager.LocalClientId;
            itemUI.SetActive(false);
        }
    }

    public void Update()
    {
        _isFront = Input.GetKey(KeyCode.LeftControl);
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DropItem();
        }
    }

    public void DropItem()
    {
        if (!IsOwner) return;
        
        if (!GameManager.instance.isStarting.Value)
            return;
        
        if (_isFront)
            DropItemServerRpc(transform.forward, itemPosFront.position, itemPosFront.rotation, true);
        else
            DropItemServerRpc(-transform.forward, itemPos.position, itemPos.rotation, false);
    }

    [Rpc(SendTo.Server)]
    public void DropItemServerRpc(Vector3 direction, Vector3 pos, Quaternion rot, bool isFront)
    {
        if (!haveAnItem) return;

        currentItem = Instantiate(dataItem.itemList[itemId].prefab, pos, rot).GetComponent<NetworkObject>();
        currentItem.TryRemoveParent();
        currentItem.Spawn();
        
        if (currentItem != null)
        {
            currentItem.GetComponent<IItem>().DropItem(direction, GetComponent<NetworkObject>(), isFront);
            currentItem.GetComponent<ItemFactory>().SetPlayerThrowId(OwnerClientId);
            currentItem = null;
        }

        hasItem.Value = false;
        currentItemId.Value = -1;

        DropItemClientRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void DropItemClientRpc()
    {
        StopAllCoroutines();

        OnDropItem?.Invoke();

        if (visualInstance != null)
        {
            Destroy(visualInstance);
            visualInstance = null;
        }

        haveAnItem = false;
        itemId = -1;

        if (IsOwner)
        {
            StartCoroutine(DropItemUI());
        }
    }
    
    
    [Rpc(SendTo.Everyone)]
    void PickUpNewItemClientRpc(int itemIndex)
    {
        StartCoroutine(GetNewItemCoroutine(itemIndex));
    }

    IEnumerator GetNewItemCoroutine(int finalIndex)
    {
        haveAnItem = true;
        
        if (IsOwner)
        {
            OnRollItem?.Invoke();
            
            itemUI.SetActive(true);
            itemUI.transform.localScale = Vector3.zero;

            float elapsed = 0;

            while (elapsed < 0.25f)
            {
                elapsed += Time.deltaTime;
                itemUI.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsed / 0.25f);
                yield return null;
            }
            
            int[] indexList = new int[15];
            int maxIndex = 4;

            for (int i = 0; i < indexList.Length; i++)
            {
                indexList[i] = (i % maxIndex);
            }
            
            indexList[^1] = finalIndex;

            float finalIntervalTime = 0.2f;
            
            for (int i = 0; i < indexList.Length; i++)
            {
                itemIcon.sprite = iconSpriteList[indexList[i]];
                yield return new WaitForSeconds(finalIntervalTime);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.25f + (15 * 0.2f));
        }
        
        if (haveAnItem)
        {
            itemId = finalIndex;
            SpawnVisual(finalIndex);
        }
    }
    
    public void GiveItemServerSide(int itemIndex)
    {
        if (!IsServer) return;
        if (haveAnItem) return;

        hasItem.Value = true;
        currentItemId.Value = itemIndex;

        PickUpNewItemClientRpc(itemIndex);
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

    void SpawnVisual(int id)
    {
        if (visualInstance != null)
        {
            Destroy(visualInstance);
        }
        
        Item item = dataItem.itemList[id];

        visualInstance = Instantiate(item.prefabVisual, itemPos.position, itemPos.rotation);
        visualInstance.transform.SetParent(itemPos);
    }
}