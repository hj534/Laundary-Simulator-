using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject shopPanel;

    [Space(50)]
    [Header("Order Details Panel")]
    public GameObject orderDetailsPanel;
    public Text orderID_Text;
    public Text orderWeight_Text;
    public Text price_Text;

    [Space(50)]
    [Header("Daily Summary Panel")]
    public GameObject dailySummaryPanel;
    public Text ssDailyOrders_Text;
    public Text ssDailyEarnings_Text;
    public Text fsDailyOrders_Text;
    public Text fsDailyEarnings_Text;
    public Text totalDailyOrders_Text;
    public Text totalDailyEarnings_Text;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(instance);
    }
    private void Start()
    {
        SoundManager.instance.playsound("BG");
    }


    #region Shop Panel
    public void OpenShopPanel()
    {
        Cursor.lockState = CursorLockMode.None;
        SoundManager.instance.playsound("click");
        shopPanel.SetActive(true);
        shopPanel.transform.DOScale(Vector3.one, .8f).SetEase(Ease.InOutBounce);
    }
    public void CloseShopPanel()
    {
        Cursor.lockState = CursorLockMode.Locked;
        SoundManager.instance.playsound("click");
        shopPanel.transform.DOScale(Vector3.zero, .8f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            shopPanel.SetActive(false);
            ShootRay.instance.p_controller.enabled = true;
        });
    }
    #endregion

    #region Order Details Panel
    public void OpenOrderDetailsPanel(int orderID, int orderWeight, int price)
    {
        orderID_Text.text = orderID.ToString();
        orderWeight_Text.text = orderWeight.ToString() + " lb";
        price_Text.text = price.ToString() + " $";


        Cursor.lockState = CursorLockMode.None;
        SoundManager.instance.playsound("click");
        orderDetailsPanel.SetActive(true);
        orderDetailsPanel.transform.DOScale(Vector3.one, .8f).SetEase(Ease.InOutBounce);
    }
    public void CloseOrderDetailsPanel()
    {
        Cursor.lockState = CursorLockMode.Locked;
        SoundManager.instance.playsound("click");
        orderDetailsPanel.transform.DOScale(Vector3.zero, .8f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            orderID_Text.text = "";
            orderWeight_Text.text = "";
            price_Text.text = "";

            orderDetailsPanel.SetActive(false);
            ShootRay.instance.p_controller.enabled = true;
        });
    }
    #endregion
    #region Daily Summary Panel
    public void OpenDailySummaryPanel()
    {
        ssDailyOrders_Text.text = PlayerPrefs.GetInt(PlayerPrefsHolder.SS_DailyOrders).ToString();
        ssDailyEarnings_Text.text = PlayerPrefs.GetInt(PlayerPrefsHolder.SS_DailyEarnings).ToString() + " $";
        fsDailyOrders_Text.text = PlayerPrefs.GetInt(PlayerPrefsHolder.FS_DailyOrders).ToString();
        fsDailyEarnings_Text.text = PlayerPrefs.GetInt(PlayerPrefsHolder.FS_DailyEarnings).ToString() + " $";
        totalDailyOrders_Text.text = PlayerPrefs.GetInt(PlayerPrefsHolder.Total_DailyOrders).ToString();
        totalDailyEarnings_Text.text = PlayerPrefs.GetInt(PlayerPrefsHolder.Total_DailyEarnings).ToString() + " $";



        Cursor.lockState = CursorLockMode.None;
        SoundManager.instance.playsound("click");
        dailySummaryPanel.SetActive(true);
        dailySummaryPanel.transform.DOScale(Vector3.one, .8f).SetEase(Ease.InOutBounce);
    }
    public void CloseDailySummaryPanel()
    {
        Cursor.lockState = CursorLockMode.Locked;
        SoundManager.instance.playsound("click");
        dailySummaryPanel.transform.DOScale(Vector3.zero, .8f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            ssDailyOrders_Text.text = "";
            ssDailyEarnings_Text.text = "";
            fsDailyOrders_Text.text = "";
            fsDailyEarnings_Text.text = "";
            totalDailyOrders_Text.text = "";
            totalDailyEarnings_Text.text = "";

            dailySummaryPanel.SetActive(false);
            ShootRay.instance.p_controller.enabled = true;
        });
    }

    public void ResetDailySummary()
    {
        PlayerPrefs.SetInt(PlayerPrefsHolder.SS_DailyOrders, 0);
        PlayerPrefs.SetInt(PlayerPrefsHolder.SS_DailyEarnings, 0);
        PlayerPrefs.SetInt(PlayerPrefsHolder.FS_DailyOrders, 0);
        PlayerPrefs.SetInt(PlayerPrefsHolder.FS_DailyEarnings, 0);
        PlayerPrefs.SetInt(PlayerPrefsHolder.Total_DailyOrders, 0);
        PlayerPrefs.SetInt(PlayerPrefsHolder.Total_DailyEarnings, 0);
    }
    #endregion
}
