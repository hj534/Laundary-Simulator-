using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FullServiceCustomerManager : MonoBehaviour
{
    public static FullServiceCustomerManager Instance;

    [Header("Customer Pool")]
    public Transform spawnPoint;
    public Transform exitPoint;

    public int poolSize = 10;
    public GameObject customerPrefab;
    public FullServiceCustomer[] customerPool;

    [Header("Spawn Timing")]
    public float normalMinSpawnDelay = 10f;
    public float normalMaxSpawnDelay = 20f;

    public float hotMinSpawnDelay = 3f;
    public float hotMaxSpawnDelay = 7f;

    private float spawnTimer;
    private bool isHotTime = false;

    private int nextOrderID = 1; // Track unique order IDs

    [SerializeField] int minOrderWeight = 10;
    [SerializeField] int maxOrderWeight = 20;

    void Awake()
    {
        Instance = this;

        // Create customer pool
        customerPool = new FullServiceCustomer[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
            obj.SetActive(false);
            customerPool[i] = obj.GetComponent<FullServiceCustomer>();
        }
    }

    void Start()
    {
        ScheduleNextSpawn();
        //TimeHandler.Instance.OnTimeStoppedEvent += SendBackAllCustomers;

        // Subscribe to Hot Time events
        /*   DayTimerManager.Instance.OnHotTimeStart += () => isHotTime = true;
           DayTimerManager.Instance.OnHotTimeEnd += () => isHotTime = false;*/
    }
    void OnDestroy()
    {
        // if (TimeHandler.Instance != null)
        // {
        //     TimeHandler.Instance.OnTimeStoppedEvent -= SendBackAllCustomers;
        // }
    }

    /* void Update()
     {
         spawnTimer -= Time.deltaTime;
         if (spawnTimer <= 0)
         {
             TrySpawnCustomer();
             ScheduleNextSpawn();
         }
     }*/
    IEnumerator SetORderId(int nextIndex, FullServiceCustomer customer)
    {
        int val = nextIndex;
        int g = ReturnID(val++);
        while (g == -1)
        {
            g = ReturnID(val++);
            if (g == -1)
                val++;
            yield return null;
        Debug.Log("Customer ID setting while  " + g);
        }
        customer.orderID = g;/*nextOrderID++;*/ // Assign unique order ID
        Debug.Log("Customer ID   " + g);
    }
    int ReturnID(int index)
    {
        ClothBasket[] clothbasket = FindObjectsOfType<ClothBasket>();
        for (int i = 0; i < clothbasket.Length; i++)
        {
            if (index == clothbasket[i].id)
            {
                return -1;
            }
        }

        PackingBoxIntreactable[] boxes = FindObjectsOfType<PackingBoxIntreactable>();  // added so even if box id same orderid be unique
        for (int i = 0; i < boxes.Length; i++)
        {
            if (index == boxes[i].orderID)
            {
                return -1;
            }
        }
        return index;
    }
    void ScheduleNextSpawn()
    {
        if (isHotTime)
        {
            spawnTimer = Random.Range(hotMinSpawnDelay, hotMaxSpawnDelay);
        }
        else
        {
            spawnTimer = Random.Range(normalMinSpawnDelay, normalMaxSpawnDelay);
        }
    }
    public void TrySpawnCustomer()
    {
        // Check if counter has space
        if (CounterManager.Instance.IsCounterFull())
        {
            Debug.Log("Counter full. Waiting to spawn.");
            return;
        }

        // Find an inactive customer in the pool
        foreach (FullServiceCustomer customer in customerPool)
        {
            if (!customer.gameObject.activeInHierarchy && !customer.IsRetrying)
            {
                // Assign a free counter spot first
                Transform freeSpot = CounterManager.Instance.GetFreeSpot();
                if (freeSpot == null)
                {
                    Debug.LogWarning("No free spot at counter.");
                    return;
                }

                customer.assignedSpot = freeSpot; // Assign counter spot

                StartCoroutine(SetORderId(nextOrderID++, customer));
                customer.orderWeight = Random.Range(minOrderWeight, maxOrderWeight + 1);
                //customer.orderID = g;/*nextOrderID++;*/ // Assign unique order ID

                customer.transform.position = spawnPoint.position;
                customer.gameObject.SetActive(true); // Activate customer

                // Set NavMeshAgent destination to counter
                customer.MoveToCounterSpot(freeSpot.position);

                Debug.Log($"? Spawned Customer with Order ID: {customer.orderID}");
                break;
            }
        }
    }
    /*  void TrySpawnCustomer()
      {
          // Check if counter has space
          if (CounterManager.Instance.IsCounterFull())
          {
              Debug.Log("Counter full. Waiting to spawn.");
              return;
          }

          // Find an inactive customer in the pool
          foreach (FullServiceCustomer customer in customerPool)
          {
              if (!customer.gameObject.activeInHierarchy)
              {
                  // Assign a free counter spot first
                  Transform freeSpot = CounterManager.Instance.GetFreeSpot();
                  if (freeSpot == null)
                  {
                      Debug.LogWarning("No free spot at counter.");
                      return;
                  }

                  customer.assignedSpot = freeSpot; // Assign counter spot
                  customer.orderID = nextOrderID++; // Assign unique order ID
                  customer.transform.position = spawnPoint.position;

                  customer.gameObject.SetActive(true); // Activate customer
                  Debug.Log($"Spawned Customer with Order ID: {customer.orderID}");

                  break;
              }
          }
      }*/
    public FullServiceCustomer FindCustomerByID(int orderID)
    {
        foreach (FullServiceCustomer customer in customerPool)
        {
            if (customer.gameObject.activeInHierarchy && customer.orderID == orderID)
            {
                return customer;
            }
        }
        return null; // No customer found
    }


    public List<Transform> waitPointsOutside = new List<Transform>();

    public Vector3 GetRandomWaitPoint()
    {
        if (waitPointsOutside.Count > 0)
        {
            int index = Random.Range(0, waitPointsOutside.Count);
            return waitPointsOutside[index].position;
        }
        return exitPoint.position; // fallback
    }
    public void OverridePool(FullServiceCustomer[] newPool)
    {
        customerPool = newPool;
    }

    public void SendBackAllCustomers()
    {
        FullServiceCustomer[] customers = FindObjectsOfType<FullServiceCustomer>();
        List<FullServiceCustomerData> dataList = new List<FullServiceCustomerData>();

        foreach (var customer in customers)
        {
            customer.ForceLeaveLaundromat();
        }
    }

}
