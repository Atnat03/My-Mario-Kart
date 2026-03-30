using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Items/ItemSO")]
    public class ItemSO : ScriptableObject
    {
        public List<Item> itemList = new List<Item>();
    }
}