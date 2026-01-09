using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SelfServiceManager : MonoBehaviour
{
    public static SelfServiceManager instance;
    [Header("Prefabs & Points")]
    public GameObject customerPrefab;
    public Transform spawnPoint;
    public Transform selfService_waitPoint;
    [Header("Stations")]
    public List<LaundryStation> washers = new();
    public List<LaundryStation> dryers = new();
    public List<LaundryStation> Bigdryers = new();

    [Header("Pooling")]
    public int poolSize = 10;
    private Queue<GameObject> customerPool = new();

    [Header("Spawning")]
    public float spawnInterval = 5f;

    public List<CustomerData> activeCustomers = new();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(instance);
    }
    void Start()
    {
        InitializePool();
        //  StartCoroutine(SpawnRoutine());
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(customerPrefab);
            obj.SetActive(false);
            customerPool.Enqueue(obj);
        }
    }

    public void UpdateMachinesList(Transform obj, int index)
    {
        LaundryStation h = new LaundryStation();
        if (index >= washers.Count && h.position != obj)
        {
            h.position = obj;
            h.isBusy = false;
            washers.Add(h);
        }
    }
    public void UpdateSmallDryerList(Transform obj, int index)
    {
        LaundryStation h = new LaundryStation();
        if (index >= dryers.Count && h.position != obj.transform.parent)
        {
            h.position = obj.transform.parent;
            h.isBusy = false;
            dryers.Add(h);
        }
    }
    public void UpdateBigDryerList(Transform obj, int index)
    {
        LaundryStation h = new LaundryStation();
        if (index >= Bigdryers.Count && h.position != obj)
        {
            h.position = obj;
            h.isBusy = false;
            Bigdryers.Add(h);
        }
    }
    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            bool washerAvailable = GetAvailableStation(washers) != null;
            bool dryerAvailable = GetAvailableStation(dryers) != null;

            if (!washerAvailable &&/*||*/ !dryerAvailable)
                continue;


            if (customerPool.Count == 0)
                continue;

            SpawnCustomer();
        }
    }

    public void SpawnCustomer()
    {
        GameObject obj = customerPool.Dequeue();
        obj.transform.position = spawnPoint.position;
        obj.SetActive(true);

        SelfServiceCustomer customer = obj.GetComponent<SelfServiceCustomer>();
        bool isLargeLoad = Random.value <= 0.25f;

        CustomerData data = new(obj, isLargeLoad);
        activeCustomers.Add(data);

        customer.Initialize(data, this);
    }

    public void RemoveCustomer(GameObject obj)
    {
        activeCustomers.RemoveAll(c => c.customerObject == obj);

        obj.SetActive(false);
        customerPool.Enqueue(obj);
    }

    public LaundryStation GetAvailableStation(List<LaundryStation> stations)
    {
        MachineIntreactable machine = new MachineIntreactable();
        SmallDryerIntreactable smalldryer = new SmallDryerIntreactable();
        BigDryerIntreactable bigdryer = new BigDryerIntreactable();
        foreach (var station in stations)
        {
            if (station.position.gameObject.GetComponent<MachineIntreactable>())
            {
                if (!station.position.gameObject.GetComponent<MachineIntreactable>().machine_in_use
                    && !station.position.gameObject.GetComponent<MachineIntreactable>().usingByAI && !station.isBusy
                    && station.position.gameObject.GetComponent<MachineIntreactable>().breakdownType == BreakdownType.None)
                {
                    machine = station.position.gameObject.GetComponent<MachineIntreactable>();
                    return station;
                }

            }
            if (station.position.GetComponent<BigDryerIntreactable>())
            {
                if (!station.position.GetComponent<BigDryerIntreactable>().bigDryer_in_use &&
                    !station.position.GetComponent<BigDryerIntreactable>().usingByAi && !station.isBusy)
                {
                    bigdryer = station.position.GetComponent<BigDryerIntreactable>();
                    return station;
                }
            }
            // else
            // {
            if (station.position.transform.GetChild(0).GetComponent<SmallDryerIntreactable>())
            {
                if (!station.position.transform.GetChild(0).GetComponent<SmallDryerIntreactable>().smallDryer_in_use &&
                    !station.position.transform.GetChild(0).GetComponent<SmallDryerIntreactable>().usingByAI && !station.isBusy)
                {
                    smalldryer = station.position.transform.GetChild(0).GetComponent<SmallDryerIntreactable>();
                return station;
                }
            }

            //}
            /*            if (!station.isBusy)
                            return station;*/
              //  return station;
           /* if (machine != null)
                return station;
            if (smalldryer != null)
                return station;
            if (bigdryer != null)
                return station;*/
        }
        return null;
    }
    public LaundryStation GetAvailableBigDryerStation(List<LaundryStation> stations)
    {
        BigDryerIntreactable bigdryer = null;
        foreach (var station in stations)
        {

            if (!station.position.transform.GetComponent<BigDryerIntreactable>().bigDryer_in_use &&
                !station.position.transform.GetComponent<BigDryerIntreactable>().usingByAi && !station.isBusy)
            {
                bigdryer = station.position.transform.GetChild(0).GetComponent<BigDryerIntreactable>();
            }

            if (bigdryer != null)
                return station;
        }
        return null;
    }
    public void OverridePool(Queue<GameObject> injectedPool)
    {
        customerPool = injectedPool;
    }



}
[System.Serializable]
public class LaundryStation
{
    public Transform position;
    public bool isBusy = false;
}

[System.Serializable]
public class CustomerData
{
    public GameObject customerObject;
    public bool isLargeLoad;

    public CustomerData(GameObject obj, bool isLarge)
    {
        customerObject = obj;
        isLargeLoad = isLarge;
    }
}