using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CartBar : MonoBehaviour
{
    public Text item_name_txt, total_price,units_txt;
    public MachineName machineName;
    public void RemoveFromList()
    {
        ShopManager.instance.FindSameBar(item_name_txt.text);
    }
}
[System.Serializable]
public enum MachineName
{
    WashingMachine,
    BigDryer,
    SmallDryer,
    FoldingTable
}