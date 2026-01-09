using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Cart : MonoBehaviour
{
    public Text num_of_items_txt, price_txt, item_name;
    [SerializeField] int numOf_items, old_val, range_to_add_item;
    [SerializeField] MachineName machineName;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        range_to_add_item = ShopManager.instance.LimitToAddItems(machineName);
    }


    public void AddItem()
    {
        SoundManager.instance.playsound("click");
        if (numOf_items < range_to_add_item)
        {
            numOf_items++;
            num_of_items_txt.text = numOf_items.ToString();
        }
    }

    public void RemoveItem()
    {
        SoundManager.instance.playsound("click");
        if (numOf_items > 0)
        {
            numOf_items--;
            num_of_items_txt.text = numOf_items.ToString();
        }
    }
    public void AddToCart()
    {
        if (numOf_items > 0)
        {
            SoundManager.instance.playsound("click");
            CartBar bar = ShopManager.instance.ReturnCartBar(item_name.text);
            bar.item_name_txt.text = item_name.text;
            bar.units_txt.text = num_of_items_txt.text;
            bar.machineName = machineName;

            for (int i = 0; i < ShopManager.instance.cartBarRef.Length; i++)
            {
                if (ShopManager.instance.cartBarRef[i].item_name_txt.text == item_name.text && ShopManager.instance.cartBarRef[i].gameObject.activeInHierarchy)
                {
                    float total = float.Parse(ShopManager.instance.totalBill_txt.text) - ReturnOldPrice();
                    ShopManager.instance.totalBill_txt.text = total.ToString("F2");
                    break;
                }
            }

            bar.total_price.text = ReturnPrice().ToString("F2");


            float total_bill = float.Parse(ShopManager.instance.totalBill_txt.text) + float.Parse(bar.total_price.text);
            Debug.Log("Total Bill   " + total_bill);
            ShopManager.instance.totalBill_txt.text = total_bill.ToString("F2");
            bar.gameObject.SetActive(true);
            old_val = numOf_items;
            numOf_items = 0;
            num_of_items_txt.text = numOf_items.ToString();
            if (!ShopManager.instance.purchase_btn.interactable)
                ShopManager.instance.purchase_btn.interactable = true;

        }
    }
    float ReturnPrice()
    {
        float totalVal = 0;

        for (int i = 0; i < numOf_items; i++)
        {
            totalVal += float.Parse(price_txt.text);
        }
        return totalVal;
    }

    float ReturnOldPrice()
    {
        float totalVal = 0;
        for (int i = 0; i < old_val; i++)
        {
            totalVal += float.Parse(price_txt.text);
        }
        return totalVal;
    }
}
