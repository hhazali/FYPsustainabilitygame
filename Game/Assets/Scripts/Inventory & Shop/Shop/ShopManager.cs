using System.Collections.Generic;
using UnityEngine;

public enum ShopMode { Buy, Sell }

public class ShopManager : MonoBehaviour
{
    [SerializeField] private ShopSlot[] shopSlots;
    [SerializeField] private InventoryManager inventoryManager;

    public ShopMode currentMode { get; private set; }

    public void PopulateShopItems(List<ShopItems> shopItems, ShopMode mode)
    {
        currentMode = mode;

        for (int i = 0; i < shopItems.Count && i < shopSlots.Length; i++)
        {
            ShopItems shopItem = shopItems[i];
            shopSlots[i].Initialize(shopItem.itemSO, shopItem.price);
            shopSlots[i].gameObject.SetActive(true);
        }

        for (int i = shopItems.Count; i < shopSlots.Length; i++)
        {
            shopSlots[i].gameObject.SetActive(false);
        }
    }

    public void TryBuyItem(ItemSO itemSO, int price)
    {
        if (currentMode != ShopMode.Buy || itemSO == null)
            return;

        if (inventoryManager.gold >= price && HasSpaceForItem(itemSO))
        {
            inventoryManager.gold -= price;
            inventoryManager.goldText.text = inventoryManager.gold.ToString();
            inventoryManager.AddItem(itemSO, 1);
        }
    }

    public bool SellItem(ItemSO itemSO)
    {
        if (itemSO == null)
            return false;

        foreach (var slot in shopSlots)
        {
            if (slot.itemSO == itemSO)
            {
                inventoryManager.gold += slot.price;
                inventoryManager.goldText.text = inventoryManager.gold.ToString();
                return true; // Only return true if item was found in SellShop
            }
        }

        return false; // Item wasn't part of the SellShop
    }

    private bool HasSpaceForItem(ItemSO itemSO)
    {
        foreach (var slot in inventoryManager.itemSlots)
        {
            if (slot.itemSO == itemSO && slot.quantity < itemSO.stackSize)
                return true;
            else if (slot.itemSO == null)
                return true;
        }
        return false;
    }
}

[System.Serializable]
public class ShopItems
{
    public ItemSO itemSO;
    public int price;
}
