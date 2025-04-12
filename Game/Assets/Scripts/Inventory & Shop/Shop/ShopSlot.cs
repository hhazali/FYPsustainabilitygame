using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    public ItemSO itemSO;
    public TMP_Text itemNameText;
    public TMP_Text priceText;
    public Image itemImage;
    public TMP_Text itemDescription;

    [SerializeField] private ShopManager shopManager;

    public int price;

    public void Initialize(ItemSO newItemSO, int price)
    {
        itemSO = newItemSO;
        itemImage.sprite = itemSO.icon;
        itemNameText.text = itemSO.itemName;
        itemDescription.text = itemSO.itemDescription;
        this.price = price;
        priceText.text = price.ToString();
    }

    public void OnBuyButtonClicked()
    {
        shopManager.TryBuyItem(itemSO, price);
    }
}
