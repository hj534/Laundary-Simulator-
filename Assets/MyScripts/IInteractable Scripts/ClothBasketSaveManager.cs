using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;

public class ClothBasketSaveManager : MonoBehaviour
{
    public static ClothBasketSaveManager instance;

    public GameObject clothBasketPrefab;
    private string savePath;

   /* private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            savePath = Application.persistentDataPath + "/cloth_baskets.json";
            Debug.Log("Cloth Basket Save Path   " + savePath);
            LoadClothBaskets();
        }
        else
        {
            Destroy(gameObject);
        }
    }*/
    private void Start()
    {

        if (instance == null)
        {
            instance = this;
            savePath = Application.persistentDataPath + "/cloth_baskets.json";
            Debug.Log("Cloth Basket Save Path   " + savePath);
            LoadClothBaskets();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnApplicationQuit()
    {
        SaveClothBaskets();
    }

    public void SaveClothBaskets()
    {
        ClothBasket[] baskets = FindObjectsOfType<ClothBasket>();
        List<ClothBasketData> dataList = new List<ClothBasketData>();

        foreach (var basket in baskets)
        {
            if (!basket.picked_by_AI && !basket.place_onCounter)
                dataList.Add(basket.GetData());
        }

        string json = JsonUtility.ToJson(new ClothBasketWrapper { baskets = dataList }, true);
        File.WriteAllText(savePath, json);
    }

    public void LoadClothBaskets()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        ClothBasketWrapper wrapper = JsonUtility.FromJson<ClothBasketWrapper>(json);

        foreach (var data in wrapper.baskets)
        {
            GameObject obj = Instantiate(clothBasketPrefab);
            ClothBasket basket = obj.GetComponent<ClothBasket>();
            basket.InitializeFromData(data);
        }
    }

    [System.Serializable]
    private class ClothBasketWrapper
    {
        public List<ClothBasketData> baskets;
    }
}
[System.Serializable]
public class ClothBasketData
{
    public string itemName;
    public bool picked;
    public bool firstTimePicked;
    public bool picked_by_AI;
    public bool placedOnRack;
    public string step;
    public string basketSize;
    public int id;
    public int cloths_weight;
    public int rack_place_index;
    public float[] position;
    public float[] rotation;
}