using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class LaundryOrder
{
    public int orderID;
    public GameObject basket;
    public bool isCompleted;

    public LaundryOrder(int id, GameObject basketObj)
    {
        orderID = id;
        basket = basketObj;
        isCompleted = false;
    }
}
public class LaundryOrderManager : MonoBehaviour
{
    public static LaundryOrderManager Instance;

    public GameObject basketPrefeb, box_prefeb;
    [SerializeField] List<LaundryOrder> activeOrders = new List<LaundryOrder>();
    private Dictionary<int, bool> orderStatus = new Dictionary<int, bool>();
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        //savePath = Application.persistentDataPath + "/order_status.json";
        LoadOrderData();
    }
    // Create a new order for customer
    public void CreateOrder(int orderID, GameObject basket)
    {
        if (!orderStatus.ContainsKey(orderID))
        {
            orderStatus.Add(orderID, false); // false means order not yet complete
            Debug.Log($"?? Order {orderID} created.");
        }
    }

    // Mark the order as completed
    public void MarkOrderComplete(int orderID)
    {
        if (orderStatus.ContainsKey(orderID))
        {
            orderStatus[orderID] = true;
            Debug.Log($"? Order {orderID} marked as complete.");
        }
    }
    // Only TO Call When Spawn Old Completed order data 
    public void CreateAndMarkAsCompleteOrder(int orderID, GameObject basket)
    {
        if (!orderStatus.ContainsKey(orderID))
        {
            orderStatus.Add(orderID, true); // false means order not yet complete
            Debug.Log($"?? Order {orderID} created.");
        }
    }
    // Check if order is complete
    public bool IsOrderComplete(int orderID)
    {
        return orderStatus.ContainsKey(orderID) && orderStatus[orderID];
    }

    // Remove order after customer pickup
    public void RemoveOrder(int orderID, int counter_index)
    {
        if (orderStatus.ContainsKey(orderID))
        {
            orderStatus.Remove(orderID);
            Debug.Log($"?? Order {orderID} removed from system.");
        }
        PickUpCounterIntreactable.instance.pos[counter_index].isfree = true;//PickUpCounterIntreactable.instance.pos[orderID].isfree = true;
        PickUpCounterIntreactable.instance.pos[counter_index].box_ref = null;//PickUpCounterIntreactable.instance.pos[orderID].box_ref = null;
        Debug.Log("Removing Order from Pickup Counter   " + orderID);


    }
    /// <summary>
    /// Create and register a new laundry order.
    /// </summary>
  /*  public LaundryOrder CreateOrder(int orderID, GameObject basket)
    {
        LaundryOrder newOrder = new LaundryOrder(orderID, basket);
        activeOrders.Add(newOrder);
        Debug.Log($"?? Order {orderID} created.");
        return newOrder;
    }*/

    /// <summary>
    /// Find an order by its unique ID.
    /// </summary>
    public LaundryOrder FindOrderByID(int orderID)
    {
        foreach (var order in activeOrders)
        {
            if (order.orderID == orderID)
                return order;
        }
        return null;
    }

    /// <summary>
    /// Mark an order as completed.
    /// </summary>
    public void CompleteOrder(int orderID)
    {
        LaundryOrder order = FindOrderByID(orderID);
        if (order != null)
        {
            order.isCompleted = true;
            Debug.Log($"? Order {orderID} marked as completed.");
        }
        else
        {
            Debug.LogWarning($"? Tried to complete unknown order ID: {orderID}");
        }
    }

    /// <summary>
    /// Remove an order after the customer has collected it.
    /// </summary>
    /*  public void RemoveOrder(int orderID)
      {
          LaundryOrder order = FindOrderByID(orderID);
          if (order != null)
          {
              activeOrders.Remove(order);
              Debug.Log($"?? Order {orderID} removed from active orders.");
          }
      }*/

    /// <summary>
    /// Check if a basket is ready for pickup.
    /// </summary>
    /* public bool IsOrderCompleted(int orderID)
     {
         LaundryOrder order = FindOrderByID(orderID);
         return order != null && order.isCompleted;
     }*/
    /*  public bool IsOrderComplete(int orderID)
      {
          return orderStatus.ContainsKey(orderID) && orderStatus[orderID];
      }*/
    /// <summary>
    /// Returns all active orders.
    /// </summary>
    public List<LaundryOrder> GetActiveOrders()
    {
        return activeOrders;
    }
    /* public GameObject FindBasketByOrderID(int orderID)
     {
         foreach (RackManager.RackSpot spot in RackManager.Instance.rackSpots)
         {
             if (spot.isOccupied)
             {
                 ClothBasket basketOrder = spot.clothbasket.GetComponent<ClothBasket>();
                 if (basketOrder != null && basketOrder.id == orderID)
                 {
                     return spot.clothbasket.gameObject;
                 }
             }
         }
         return null; // No basket found
     }*/

    public GameObject FindBasketByOrderID(int orderID)
    {
        foreach (PickUpCounterData spot in PickUpCounterIntreactable.instance.pos)
        {
            if (!spot.isfree && spot.box_ref != null)
            {
                PackingBoxIntreactable box = spot.box_ref.GetComponent<PackingBoxIntreactable>();
                if (box.orderID == orderID)
                {
                    Debug.Log("Find Box Returning box   inded   " + box.orderID + "   " + orderID);
                    return box.gameObject;
                    //return spot;
                }
            }
        }
        return null; // No basket found
    }


    #region - Save/Load Generated Order - 
    public string filename = "";
    string folderPath = "/DocumentsAssets/JSONData/";
    [HideInInspector] public string saveFileName = "shopmanager.json";

    string savePath;
    public void SaveOrdersData()
    {
        string json = JsonUtility.ToJson(this);

        // Check if folder exists, if not, create it
        if (!Directory.Exists(Application.persistentDataPath + folderPath))
        {
            Directory.CreateDirectory(Application.persistentDataPath + folderPath);
        }
        File.WriteAllText(Application.persistentDataPath + folderPath + saveFileName, json);

        // LaundryOrderSaveData data = new LaundryOrderSaveData();

        // foreach (var kvp in orderStatus)
        // {
        //     data.orderIDs.Add(kvp.Key);
        //     data.orderCompleted.Add(kvp.Value);
        // }

        // string json = JsonUtility.ToJson(data, true);
        // File.WriteAllText(savePath, json);
    }
    private void OnApplicationQuit()
    {
        SaveOrdersData();
    }
    // Load shelf data from a JSON file
    public void LoadOrderData()
    {
        if (File.Exists(Application.persistentDataPath + folderPath + saveFileName))
        {
            Debug.LogWarning("Save file found Shelf");
            Debug.Log("Save file found   " + Application.persistentDataPath + folderPath + saveFileName);
            string json = File.ReadAllText(Application.persistentDataPath + folderPath + saveFileName);
            LaundryOrderManager s = new LaundryOrderManager();
            JsonUtility.FromJsonOverwrite(json, s);
            filename = s.filename;
            folderPath = s.folderPath;
            saveFileName = s.saveFileName;

            orderStatus = s.orderStatus;
            Debug.Log("Json Values   " + json);
        }
        else
        {
            transform.name = saveFileName;
            Debug.LogWarning("Save file Shelf not found!");
        }

        // if (File.Exists(savePath))
        // {
        //     string json = File.ReadAllText(savePath);
        //     LaundryOrderSaveData data = JsonUtility.FromJson<LaundryOrderSaveData>(json);

        //     orderStatus.Clear();

        //     for (int i = 0; i < data.orderIDs.Count; i++)
        //     {
        //         orderStatus[data.orderIDs[i]] = data.orderCompleted[i];
        //     }

        //     Debug.Log("Orders loaded successfully " + orderStatus.Count);
        // }
        // else
        // {
        //     orderStatus = new Dictionary<int, bool>();
        // }


    }

    [System.Serializable]
    public class LaundryOrderSaveData
    {
        public List<int> orderIDs = new List<int>();
        public List<bool> orderCompleted = new List<bool>();
    }

    #endregion


}


