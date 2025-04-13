using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public ItemSO itemSO;
    public int quantity;

    public Image itemImage;
    public TMP_Text quantityText;

    private InventoryManager inventoryManager;
    private static ShopManager activeShop;

    private void Start()
    {
        inventoryManager = GetComponentInParent<InventoryManager>();
    }

    private void OnEnable()
    {
        ShopKeeper.OnShopStateChanged += HandleShopStateChanged;
    }

    private void OnDisable()
    {
        ShopKeeper.OnShopStateChanged -= HandleShopStateChanged;
    }

    private void HandleShopStateChanged(ShopManager shopManager, bool isOpen)
    {
        activeShop = isOpen ? shopManager : null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (quantity > 0)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (activeShop != null)
                {
                    if (activeShop.currentMode == ShopMode.Sell)
                    {
                        bool sold = activeShop.SellItem(itemSO);
                        if (sold)
                        {
                            quantity--;
                            UpdateUI();
                        }
                        else
                        {
                            Debug.Log("Item not in SellShop, cannot be sold.");
                        }
                    }
                    else
                    {
                        Debug.Log("Cannot sell items in Buy mode.");
                    }
                }
                else
                {
                    inventoryManager.UseItem(this);
                }
            }
        }
    }


    public void UpdateUI(){
        if(quantity <= 0)
            itemSO = null;

        if(itemSO != null){
            itemImage.sprite = itemSO.icon;
            itemImage.gameObject.SetActive(true);
            quantityText.text = quantity.ToString();
        }
        else{
            itemImage.gameObject.SetActive(false);
            quantityText.text = "";
        }
    }
}
