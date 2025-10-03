using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public string itemName;
        public GameObject itemPrefab;
        public int quantity;
        public Sprite itemIcon;
    }

    public List<InventorySlot> slots = new List<InventorySlot>();
    public int inventorySize = 20;
}