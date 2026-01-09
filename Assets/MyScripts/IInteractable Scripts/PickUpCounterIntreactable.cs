using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
public class PickUpCounterIntreactable : MonoBehaviour, IInteractable
{
    public static PickUpCounterIntreactable instance;
    public GameObject box_prefeb;
    public string itemName = "PickUpCounter";

    public bool space_full;
    public PickUpCounterData[] pos;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(instance);

        savePath = Application.persistentDataPath + "/ready_boxes.json";
        LoadBoxesData();
    }
    private void Start()
    {
        // LaundryOrderManager.Instance.MarkOrderComplete(1);
        SpawnAllBoxes();
    }
    public void Interact()
    {
        if (space_full)
            return;

        // fold = true;
        ShootRay.instance.pickUpCounter_ref = this;
        ShootRay.instance.currentPrompt.SetActive(false);
        // ShootRay.instance.p_controller.enabled = false;
        StartCoroutine(ShootRay.instance.PlaceBoxOnPickUpCounter());

        ShootRay.instance.currentOutline.enabled = false;
    }

    public string GetInteractionText()
    {
        if (space_full)
            return $"Space is full cannot place on Pick Up Counter";
        else
            return $"Press E to place box on Pick Up Counter";
        //  return $"Press E to Pick Up {itemName}";
    }

    public PickUpCounterData GiveEmptySpace()
    {
        for (int i = 0; i < pos.Length; i++)
        {
            if (pos[i].isfree)
            {
                pos[i].box_ref = ShootRay.instance.box.gameObject;
                ShootRay.instance.box.counter_pos_index = i;
                return pos[i];
            }
        }
        return null;
    }

    [System.Serializable]
    public class StandingSpot
    {
        public Transform position;
        public bool isOccupied;
        public FullServiceCustomer currentCustomer;
    }

    public List<StandingSpot> standingSpots = new List<StandingSpot>();

    // Get a free spot for a customer to stand
    public Transform GetFreeSpot()
    {
        foreach (StandingSpot spot in standingSpots)
        {
            if (!spot.isOccupied)
            {
                spot.isOccupied = true; // Mark as occupied
                return spot.position;
            }
        }
        return null; // No free spots
    }

    // Free up a spot when customer leaves
    public void FreeSpot(Transform spotTransform)
    {
        if (spotTransform == null)
        {
            Debug.LogWarning("?? Attempted to free a null spotTransform!");
            return;
        }
        foreach (StandingSpot spot in standingSpots)
        {
            if (spot.position == spotTransform)
            {
                spot.isOccupied = false;
                spot.currentCustomer = null;
                break;
            }
        }
    }

    // Check if all spots are full
    public bool IsCounterFull()
    {
        foreach (StandingSpot spot in standingSpots)
        {
            if (!spot.isOccupied)
                return false; // At least one free spot
        }
        return true; // All full
    }
    public void ShiftQueueForward()
    {
        for (int i = 0; i < standingSpots.Count - 1; i++)
        {
            if (standingSpots[i].isOccupied == false && standingSpots[i + 1].isOccupied)
            {
                // Check if the next spot actually has a customer
                FullServiceCustomer nextCustomer = standingSpots[i + 1].currentCustomer;
                if (nextCustomer != null)
                {
                    // Move customer forward
                    nextCustomer.MoveToPickupSpot(standingSpots[i].position);

                    // Update spot data
                    standingSpots[i].currentCustomer = nextCustomer;
                    standingSpots[i].isOccupied = true;

                    standingSpots[i + 1].currentCustomer = null;
                    standingSpots[i + 1].isOccupied = false;
                }
                else
                {
                    Debug.LogWarning("? standingSpots[i+1] had no customer assigned!");
                }
            }
        }
    }

    public FullServiceCustomer GetFrontCustomer()
    {
        if (standingSpots[0].isOccupied)
            return standingSpots[0].currentCustomer;
        else
            return null;
    }
    #region - Save/Load Data - 
    public List<ReadyToDeliverBoxData> boxData = new List<ReadyToDeliverBoxData>();
    // public string filename = "";
    // string folderPath = "/DocumentsAssets/JSONData/";
    // [HideInInspector] public string saveFileName = "ReadyBoxes.json";
    string savePath;
    public void SaveBoxesData()
    {
        // string json = JsonUtility.ToJson(this);

        // // Check if folder exists, if not, create it
        // if (!Directory.Exists(Application.persistentDataPath + folderPath))
        // {
        //     Directory.CreateDirectory(Application.persistentDataPath + folderPath);
        // }
        // File.WriteAllText(Application.persistentDataPath + folderPath + saveFileName, json);

        PickUpCounterSaveWrapper wrapper = new PickUpCounterSaveWrapper();
        wrapper.boxData = boxData;
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Saved Ready Box Count: " + boxData.Count);
    }
    private void OnApplicationQuit()
    {
        GetAllReadyBoxToSaveData();// SaveBoxesData();
    }
    // Load shelf data from a JSON file
    public void LoadBoxesData()
    {
        if (File.Exists(savePath))
        {
            Debug.Log("Save file found   " + savePath);
            string json = File.ReadAllText(savePath);
            // PickUpCounterIntreactable s = new PickUpCounterIntreactable();
            // JsonUtility.FromJsonOverwrite(json, s);
            // filename = s.filename;
            // folderPath = s.folderPath;
            // saveFileName = s.saveFileName;
            PickUpCounterSaveWrapper wrapper = JsonUtility.FromJson<PickUpCounterSaveWrapper>(json);

            if (wrapper != null && wrapper.boxData != null)
            {
                boxData = wrapper.boxData;
                Debug.Log("Loaded Ready Box Count: " + boxData.Count);
            }
            else
            {
                Debug.LogWarning("Save file loaded but boxData was null");
            }

            Debug.Log(savePath + " ==> " + json);
        }
        else
        {
            Debug.LogWarning("Save file not found!");
        }
    }
    void GetAllReadyBoxToSaveData()
    {
        PackingBoxIntreactable[] box = FindObjectsOfType<PackingBoxIntreactable>();
        for (int i = 0; i < box.Length; i++)
        {
            if (box[i].ready_to_pick && !box[i].dont_save)
            {
                ReadyToDeliverBoxData data = new ReadyToDeliverBoxData();
                data.orderID = box[i].orderID;
                data.place_counter_index = box[i].counter_pos_index;
                data.place_on_counter = box[i].placed_on_counter;
                data.ready_to_pick = box[i].ready_to_pick;
                data.pos = box[i].transform.position;
                data.rot = box[i].transform.eulerAngles;
                boxData.Add(data);
            }
        }
        SaveBoxesData();
    }
    void SpawnAllBoxes()
    {
        for (int i = 0; i < boxData.Count; i++)
        {
            GameObject box = Instantiate(box_prefeb);
            box.transform.position = boxData[i].pos;
            box.transform.eulerAngles = boxData[i].rot;
            PackingBoxIntreactable box_ref = box.GetComponent<PackingBoxIntreactable>();
            box_ref.orderID = boxData[i].orderID;
            if (boxData[i].place_on_counter)
            {
                box_ref.placed_on_counter = true;
                box_ref.ready_to_pick = boxData[i].ready_to_pick;
                box_ref.counter_pos_index = boxData[i].place_counter_index;
                pos[boxData[i].place_counter_index].box_ref = box;
                pos[boxData[i].place_counter_index].isfree = false;
                LaundryOrderManager.Instance.CreateAndMarkAsCompleteOrder(box_ref.orderID, box);

                Debug.Log("Registaring Order Complete    :   " + box_ref.orderID);
            }
        }

        boxData.Clear();
    }

    [System.Serializable]
    class PickUpCounterSaveWrapper
    {
        public List<ReadyToDeliverBoxData> boxData;
    }
    #endregion
}

[System.Serializable]
public class PickUpCounterData
{
    public bool isfree;
    public Transform pos;
    public GameObject box_ref;
}
[System.Serializable]
public class ReadyToDeliverBoxData
{
    public int orderID, place_counter_index;
    public bool place_on_counter;
    public bool ready_to_pick;
    public Vector3 pos, rot;
}