using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FullServiceCustomerSaveManager : MonoBehaviour
{
    public static FullServiceCustomerSaveManager instance;

    public GameObject fullServiceCustomerPrefab;
    public FullCustomerPrefeb[] fullServiceCustomer_Prefab;
    private string savePath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            savePath = Application.persistentDataPath + "/full_customers.json";
            LoadCustomers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        SaveCustomers();
    }

    public void SaveCustomers()
    {
        FullServiceCustomer[] customers = FindObjectsOfType<FullServiceCustomer>();
        List<FullServiceCustomerData> dataList = new List<FullServiceCustomerData>();

        foreach (var customer in customers)
        {
            if (customer.placedBasket && customer.waiting)
                dataList.Add(customer.GetData());
        }

        string json = JsonUtility.ToJson(new CustomerWrapper { customers = dataList }, true);
        File.WriteAllText(savePath, json);
    }

    public void LoadCustomers()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        CustomerWrapper wrapper = JsonUtility.FromJson<CustomerWrapper>(json);

        foreach (var data in wrapper.customers)
        {
           // GameObject obj = Instantiate(fullServiceCustomerPrefab);
            GameObject obj = Instantiate(ReturnDullServicePrefeb(data.character_name));
            FullServiceCustomer customer = obj.GetComponent<FullServiceCustomer>();
            customer.InitializeFromData(data);
        }
    }
    GameObject ReturnDullServicePrefeb(string name)
    {
        for(int i = 0; i < fullServiceCustomer_Prefab.Length; i++)
        {
            if(name == fullServiceCustomer_Prefab[i].name)
            {
                return fullServiceCustomer_Prefab[i].dullcustomer_prefeb;
            }
        }
        return null;
    }


    [System.Serializable]
    private class CustomerWrapper
    {
        public List<FullServiceCustomerData> customers;
    }
}
[System.Serializable]
public class FullServiceCustomerData
{
    public string character_name;
    public int orderID;
    public int orderWeight;
    public float[] position;
    public bool basketPlaced;
    public bool orderPicked;
    public bool placedBasket;
    public bool waiting;
}

[System.Serializable]
public class FullCustomerPrefeb
{
    public string name;
    public GameObject dullcustomer_prefeb;
}
