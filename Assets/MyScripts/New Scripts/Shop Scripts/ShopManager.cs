using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using DG.Tweening;
using System.Runtime.CompilerServices;
public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [Header(" ---  Data Extra of Machines --- ")]
    public MachinesData[] wahing_machines_data;
    public MachinesData[] dryer_machines_data;
    public MachinesData[] bigDryer_machines_data;
    public MachinesData[] folding_table_data;

    [Space]
    [Header(" ---  Cart Bar ---  ")]
    public CartBar[] cartBarRef;
    public Text totalBill_txt;

    [SerializeField] Text cash_txt, adding_coins_effect_txt;
    public Button purchase_btn;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(instance);

        LoadShopData();
    }
    private void Start()
    {
        purchase_btn.interactable = false;
        UpdateCashUI();
        UnlockPurchaseItems();
    }

    public int LimitToAddItems(MachineName name)
    {
        int num = 0;
        if (name == MachineName.WashingMachine)
        {
            for (int i = 0; i < wahing_machines_data.Length; i++)
            {
                if (!wahing_machines_data[i].is_unlocked)
                {
                    num++;
                }
            }
            return num;
        }//Big Dryer
        if (name == MachineName.BigDryer)
        {
            for (int i = 0; i < bigDryer_machines_data.Length; i++)
            {
                if (!bigDryer_machines_data[i].is_unlocked)
                {
                    num++;
                }
            }
            return num;
        }//Small Dryer
        if (name == MachineName.SmallDryer)
        {
            for (int i = 0; i < dryer_machines_data.Length; i++)
            {
                if (!dryer_machines_data[i].is_unlocked)
                {
                    num++;
                }
            }
            return num;
        }//Folding Tabel
        if (name == MachineName.FoldingTable)
        {
            for (int i = 0; i < folding_table_data.Length; i++)
            {
                if (!folding_table_data[i].is_unlocked)
                {
                    num++;
                }
            }
            return num;
        }
        return 0;
    }


    #region - Save/Load Data - 
    public string filename = "";
    string folderPath = "/DocumentsAssets/JSONData/";
    [HideInInspector] public string saveFileName = "shopmanager.json";
    public void SaveShopData()
    {
        string json = JsonUtility.ToJson(this);

        // Check if folder exists, if not, create it
        if (!Directory.Exists(Application.persistentDataPath + folderPath))
        {
            Directory.CreateDirectory(Application.persistentDataPath + folderPath);
        }
        File.WriteAllText(Application.persistentDataPath + folderPath + saveFileName, json);
    }
    private void OnApplicationQuit()
    {
        SaveShopData();
    }
    // Load shelf data from a JSON file
    public void LoadShopData()
    {
        if (File.Exists(Application.persistentDataPath + folderPath + saveFileName))
        {
            Debug.LogWarning("Save file found Shelf");
            Debug.Log("Save file found   " + Application.persistentDataPath + folderPath + saveFileName);
            string json = File.ReadAllText(Application.persistentDataPath + folderPath + saveFileName);
            ShopManager s = new ShopManager();
            JsonUtility.FromJsonOverwrite(json, s);
            filename = s.filename;
            folderPath = s.folderPath;
            saveFileName = s.saveFileName;
            // wahing_machines_data = s.wahing_machines_data;
            SetBoolValue(wahing_machines_data, s.wahing_machines_data);

            //  dryer_machines_data = s.dryer_machines_data;
            SetBoolValue(dryer_machines_data, s.dryer_machines_data);

            //  bigDryer_machines_data = s.bigDryer_machines_data;
            SetBoolValue(bigDryer_machines_data, s.bigDryer_machines_data);

            //  folding_table_data = s.folding_table_data;
            SetBoolValue(folding_table_data, s.folding_table_data);
            Debug.Log("Json Values   " + json);
        }
        else
        {
            transform.name = saveFileName;
            Debug.LogWarning("Save file Shelf not found!");
        }
    }

    void SetBoolValue(MachinesData[] current, MachinesData[] newData)
    {
        for (int i = 0; i < current.Length; i++)
        {
            current[i].is_unlocked = newData[i].is_unlocked;
        }
    }

    #endregion


    public CartBar ReturnCartBar(string itemName)
    {
        for (int i = 0; i < cartBarRef.Length; i++)
        {
            if (cartBarRef[i].item_name_txt.text == itemName && cartBarRef[i].gameObject.activeInHierarchy)
            {
                return cartBarRef[i];
            }
        }
        for (int i = 0; i < cartBarRef.Length; i++)
        {
            if (!cartBarRef[i].gameObject.activeInHierarchy)
            {
                return cartBarRef[i];
            }
        }
        return null;
    }

    public void FindSameBar(string itemName)
    {
        for (int i = 0; i < cartBarRef.Length; i++)
        {
            if (cartBarRef[i].item_name_txt.text == itemName)
            {
                float amount = float.Parse(cartBarRef[i].total_price.text) - float.Parse(totalBill_txt.text);
                if (amount < 0)
                    amount *= -1;
                totalBill_txt.text = amount.ToString("F2");
                cartBarRef[i].item_name_txt.text = "";
                cartBarRef[i].total_price.text = "0.00";
                cartBarRef[i].gameObject.SetActive(false);

                if (float.Parse(totalBill_txt.text) <= 0)
                    purchase_btn.interactable = false;


                return;
            }
        }
    }
    public void UpdateCashUI()
    {
        SoundManager.instance.playsound("CashSound");
        cash_txt.text = PlayerPrefs.GetFloat(PlayerPrefsHolder.Cash, PlayerPrefsHolder.defaultCash_value).ToString("F2");
    }
    public void AddingCoinsEffectUI(float amount)
    {
        Vector3 start_pos = adding_coins_effect_txt.transform.position;
        adding_coins_effect_txt.text = "+" + amount.ToString("F2");

        PlayerPrefs.SetInt(PlayerPrefsHolder.Total_DailyOrders, PlayerPrefs.GetInt(PlayerPrefsHolder.Total_DailyOrders) + 1);
        PlayerPrefs.SetInt(PlayerPrefsHolder.Total_DailyEarnings, PlayerPrefs.GetInt(PlayerPrefsHolder.Total_DailyEarnings) + (int)amount);
        PlayerPrefs.SetFloat(PlayerPrefsHolder.Cash, PlayerPrefs.GetFloat(PlayerPrefsHolder.Cash, PlayerPrefsHolder.defaultCash_value) + amount);
        adding_coins_effect_txt.gameObject.SetActive(true);
        adding_coins_effect_txt.transform.DOMoveY(adding_coins_effect_txt.transform.position.y + 100, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            UpdateCashUI();
            adding_coins_effect_txt.gameObject.SetActive(false);
            adding_coins_effect_txt.transform.position = start_pos;
        });
    }
    public void PurchaseBtn()
    {
        if (PlayerPrefs.GetFloat(PlayerPrefsHolder.Cash, PlayerPrefsHolder.defaultCash_value) >= float.Parse(totalBill_txt.text))
        {
            //Purchased
            PlayerPrefs.SetFloat(PlayerPrefsHolder.Cash, PlayerPrefs.GetFloat(PlayerPrefsHolder.Cash,
                PlayerPrefsHolder.defaultCash_value) - float.Parse(totalBill_txt.text));
            UpdateCashUI();
            SetPurchasedItemsValue();
            ResetCartsBars();
            LimitToAddItems(MachineName.WashingMachine);
            LimitToAddItems(MachineName.BigDryer);
            LimitToAddItems(MachineName.SmallDryer);
            LimitToAddItems(MachineName.FoldingTable);
            totalBill_txt.text = "0.00";
        }
    }
    void ResetCartsBars()
    {
        for (int i = 0; i < cartBarRef.Length; i++)
        {
            if (cartBarRef[i].gameObject.activeInHierarchy)
            {
                cartBarRef[i].item_name_txt.text = "";
                cartBarRef[i].units_txt.text = "";
                cartBarRef[i].total_price.text = "";
                cartBarRef[i].gameObject.SetActive(false);
            }
        }
    }
    //Purchase Btn
    void SetPurchasedItemsValue()
    {
        for (int i = 0; i < cartBarRef.Length; i++)
        {
            if (cartBarRef[i].gameObject.activeInHierarchy)
            {
                if (cartBarRef[i].machineName == MachineName.WashingMachine)
                {
                    PlayerPrefs.SetInt(PlayerPrefsHolder.Unlocked_Machines,
                        PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_Machines, PlayerPrefsHolder.defaultUnlockedMacine_value) + int.Parse(cartBarRef[i].units_txt.text));

                    wahing_machines_data[PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_Machines, 1)].is_unlocked = true;

                    Debug.Log("Washing Machine   " + int.Parse(cartBarRef[i].units_txt.text) +
                        "  ___ PlayerPref   __ " + PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_Machines, PlayerPrefsHolder.defaultUnlockedMacine_value));
                }
                else if (cartBarRef[i].machineName == MachineName.BigDryer)
                {
                    PlayerPrefs.SetInt(PlayerPrefsHolder.Unlocked_Big_Dryer,
                        PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_Big_Dryer, PlayerPrefsHolder.defaultUnlockedBigDryer_value) + int.Parse(cartBarRef[i].units_txt.text));

                    bigDryer_machines_data[PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_Big_Dryer, 1)].is_unlocked = true;

                    Debug.Log("Big Dryer   " + int.Parse(cartBarRef[i].units_txt.text) +
                      "  ___ PlayerPref   __ " + PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_Big_Dryer, PlayerPrefsHolder.defaultUnlockedBigDryer_value));
                }
                else if (cartBarRef[i].machineName == MachineName.SmallDryer)
                {
                    PlayerPrefs.SetInt(PlayerPrefsHolder.Unlocked_Small_Dryer,
                        PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_Small_Dryer, PlayerPrefsHolder.defaultSmallDryer_value) + int.Parse(cartBarRef[i].units_txt.text));

                    dryer_machines_data[PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_Small_Dryer, 1)].is_unlocked = true;

                    Debug.Log("Small Dryer   " + int.Parse(cartBarRef[i].units_txt.text) +
                      "  ___ PlayerPref   __ " + PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_Small_Dryer, PlayerPrefsHolder.defaultSmallDryer_value));
                }
                else if (cartBarRef[i].machineName == MachineName.FoldingTable)
                {
                    PlayerPrefs.SetInt(PlayerPrefsHolder.Unlocked_FoldingTable,
                        PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_FoldingTable, PlayerPrefsHolder.defaultFoldingTable_value) + int.Parse(cartBarRef[i].units_txt.text));

                    Debug.Log("Folding Table    " + PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_FoldingTable, 1));
                    folding_table_data[PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_FoldingTable, 1)].is_unlocked = true;
                }
            }
        }
        UnlockPurchaseItems();
    }

    void UnlockPurchaseItems()
    {
        for (int i = 0; i <= PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_Machines, PlayerPrefsHolder.defaultUnlockedMacine_value); i++)
        {
            wahing_machines_data[i].is_unlocked = true;
            wahing_machines_data[i].machine_ref.SetActive(true);
            SelfServiceManager.instance.UpdateMachinesList(wahing_machines_data[i].machine_ref.transform, i);

        }
        //Big Dryer
        for (int i = 0; i <= PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_Big_Dryer, PlayerPrefsHolder.defaultUnlockedBigDryer_value); i++)
        {

            bigDryer_machines_data[i].is_unlocked = true;
            bigDryer_machines_data[i].machine_ref.SetActive(true);
            SelfServiceManager.instance.UpdateBigDryerList(bigDryer_machines_data[i].machine_ref.transform, i);

        }

        //Small Dryer 
        for (int i = 0; i <= PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_Small_Dryer, PlayerPrefsHolder.defaultSmallDryer_value); i++)
        {
            dryer_machines_data[i].is_unlocked = true;
            dryer_machines_data[i].machine_ref.SetActive(true);
            SelfServiceManager.instance.UpdateSmallDryerList(dryer_machines_data[i].machine_ref.transform, i);
        }

        //Folding Table 
        for (int i = 0; i <= PlayerPrefs.GetInt(PlayerPrefsHolder.Unlocked_FoldingTable, PlayerPrefsHolder.defaultFoldingTable_value); i++)
        {

            folding_table_data[i].is_unlocked = true;
            folding_table_data[i].machine_ref.SetActive(true);
        }
    }
}


[System.Serializable]
public class MachinesData
{
    public bool is_unlocked;
    public GameObject machine_ref;
}
