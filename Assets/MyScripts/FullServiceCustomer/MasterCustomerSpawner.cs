using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MasterCustomerSpawner : MonoBehaviour
{
    public static MasterCustomerSpawner instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(instance);
    }
    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    private float spawnTimer;

    [Header("Self Service Setup")]
    public GameObject[] selfServicePrefabs;
    public int selfServicePoolSize = 10;
    private Queue<GameObject> selfServicePool = new Queue<GameObject>();
    public SelfServiceManager selfServiceManager;

    [Header("Full Service Setup")]
    public GameObject[] fullServicePrefabs;
    public int fullServicePoolSize = 10;
    private List<GameObject> fullServicePool = new List<GameObject>();
    public FullServiceCustomerManager fullServiceManager;

    private void Start()
    {
        InitializeSelfServicePool();
        InitializeFullServicePool();
        spawnTimer = spawnInterval;
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && !TimeHandler.Instance.CheckIfDayisEnding())
        {
            TrySpawnCustomer();
            spawnTimer = spawnInterval;
        }
    }
    public void SetSpawnInterval(float master_interval)
    {
        spawnInterval = master_interval; // your spawn interval variable
    }
    void InitializeSelfServicePool()
    {
        for (int i = 0; i < selfServicePoolSize; i++)
        {
            GameObject prefab = selfServicePrefabs[Random.Range(0, selfServicePrefabs.Length)];
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            selfServicePool.Enqueue(obj);
        }

        // Inject the pool into SelfServiceManager
        selfServiceManager.OverridePool(selfServicePool);
    }

    void InitializeFullServicePool()
    {
        FullServiceCustomer[] customers = new FullServiceCustomer[fullServicePoolSize];

        for (int i = 0; i < fullServicePoolSize; i++)
        {
            GameObject prefab = fullServicePrefabs[Random.Range(0, fullServicePrefabs.Length)];
            GameObject obj = Instantiate(prefab, fullServiceManager.spawnPoint.position, Quaternion.identity);
            obj.SetActive(false);
            customers[i] = obj.GetComponent<FullServiceCustomer>();
            fullServicePool.Add(obj);
        }

        // Inject into FullServiceCustomerManager
        fullServiceManager.OverridePool(customers);
    }

    void TrySpawnCustomer()
    {
       
        float chance = Random.Range(0f, 100f); // Using 0–100 for readability

        if (chance <= 75f)
        {
            Debug.Log("Master Spawner: Spawning Self-Service (75%)");
            StartCoroutine(SelfServiceSpawnRoutine());
        }
        else
        {
            Debug.Log("Master Spawner: Spawning Full-Service (25%)");
            fullServiceManager.TrySpawnCustomer();
        }
    }

    IEnumerator SelfServiceSpawnRoutine()
    {
        bool washerAvailable = selfServiceManager.GetAvailableStation(selfServiceManager.washers) != null;
        bool dryerAvailable = selfServiceManager.GetAvailableStation(selfServiceManager.dryers) != null;
        bool bigdryerAvailable = selfServiceManager.GetAvailableStation(selfServiceManager.Bigdryers) != null;

        if (!washerAvailable && !dryerAvailable && !bigdryerAvailable)
            yield break;

        if (selfServicePool.Count == 0)
            yield break;

        GameObject obj = selfServicePool.Dequeue();
        obj.transform.position = selfServiceManager.spawnPoint.position;
        obj.SetActive(true);

        bool isLargeLoad = Random.value <= 0.25f;

        CustomerData data = new(obj, isLargeLoad);
        selfServiceManager.activeCustomers.Add(data);

        SelfServiceCustomer customer = obj.GetComponent<SelfServiceCustomer>();
        customer.Initialize(data, selfServiceManager);
    }
}
