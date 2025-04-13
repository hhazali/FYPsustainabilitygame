using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopKeeper : MonoBehaviour
{
    public Animator anim;
    public CanvasGroup shopCanvasGroup;
    public ShopManager shopManager;

    [SerializeField] private List<ShopItems> shopSell;
    [SerializeField] private List<ShopItems> shopBuy;

    public static event Action<ShopManager, bool> OnShopStateChanged;

    private bool isShopOpen;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            anim.SetBool("playerInRange", true);

            if (!isShopOpen)
            {
                Time.timeScale = 0;
                isShopOpen = true;
                OnShopStateChanged?.Invoke(shopManager, true);
                shopCanvasGroup.alpha = 1;
                shopCanvasGroup.blocksRaycasts = true;
                shopCanvasGroup.interactable = true;
                OpenSellShop(); // or OpenBuyShop() based on your design
            }
        }
    }

    public void OpenSellShop()
    {
        shopManager.PopulateShopItems(shopSell, ShopMode.Sell);
    }

    public void OpenBuyShop()
    {
        shopManager.PopulateShopItems(shopBuy, ShopMode.Buy);
    }

    public void ExitShop()
    {
        if (isShopOpen)
        {
            Time.timeScale = 1;
            isShopOpen = false;
            OnShopStateChanged?.Invoke(shopManager, false);
            shopCanvasGroup.alpha = 0;
            shopCanvasGroup.blocksRaycasts = false;
            shopCanvasGroup.interactable = false;
        }
    }
}