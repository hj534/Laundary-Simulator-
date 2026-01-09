using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

using Random = UnityEngine.Random;  
public class TimeHandler : MonoBehaviour
{
    public static TimeHandler Instance;
    public float timeLeft, WatchTimeLeft;
    public bool dayCompleted;
    [Space]
    [Header(" ----- New Timmer Code ----- ")]
    [SerializeField] Text time_text, day_text;
    public float gametimer = 0;
    [SerializeField] float day_night_timer = 0;
    [SerializeField] GameObject dayEnd_obj, dayEnd_txt;
    [SerializeField] string[] daynames;
    public int dayindex;
    [SerializeField] float time_multiplier; // 300
                                            //  [SerializeField] LightManager light_manager;
    private const float inverseDayLength = 1f / 1440f;
    bool setting_Data = false;

    [SerializeField] Material[] skyboxes;
    #region - For Hot Time In a Day -
    [Header(" ----- Hot Time System ----- ")]
    [SerializeField] float hotTimeDurationInHours = 1f; // e.g. 1 = 1 hour hot time
    [SerializeField] int hotTimeStartMinHour = 9;       // earliest hot time can start
    [SerializeField] int hotTimeEndMaxHour = 16;        // latest hot time can start

    [SerializeField] float hotTimeStartSeconds; // in seconds
    [SerializeField] float hotTimeEndSeconds;
    [SerializeField] bool isHotTimeActive = false;

    public event Action OnTimeStoppedEvent;


    #endregion
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    private void Start()
    {
        if (PlayerPrefs.GetString(PlayerPrefsHolder.days_Time_pm, "False") == "True")
            pm_time = true;
        else
            pm_time = false;

        gametimer = PlayerPrefs.GetFloat(PlayerPrefsHolder.day_time, 28800f);
        day_night_timer = PlayerPrefs.GetFloat(PlayerPrefsHolder.day_night_lighting_time, 28800f);
        int _minutes = (int)(gametimer / 60) % 60;
        int _hours = (int)(gametimer / 3600) % 24;

        string timestring = string.Format("{0:0}:{1:00}", _hours, _minutes);
        time_text.text = timestring;

        //  ChangeSkybox(_hours);

        day_text.text = PlayerPrefs.GetString(PlayerPrefsHolder.days_name, "Monday");
        dayindex = PlayerPrefs.GetInt(PlayerPrefsHolder.days_index, 0);
        setting_Data = true;

        // For Hot Time
        // Load or Generate Hot Time
        string today = PlayerPrefs.GetString(PlayerPrefsHolder.days_name, "Monday");//System.DateTime.Now.Date.ToString("yyyyMMdd");

        if (PlayerPrefs.HasKey(PlayerPrefsHolder.HOT_TIME_DATE_KEY) && PlayerPrefs.GetString(PlayerPrefsHolder.HOT_TIME_DATE_KEY) == today)
        {
            hotTimeStartSeconds = PlayerPrefs.GetFloat(PlayerPrefsHolder.HOT_TIME_START_KEY);
            hotTimeEndSeconds = PlayerPrefs.GetFloat(PlayerPrefsHolder.HOT_TIME_END_KEY);
        }
        else
        {
            float minHotTime = 28800 + (hotTimeStartMinHour - 8) * 3600;
            float maxHotTime = Mathf.Min(79200 - (hotTimeDurationInHours * 3600), 28800 + (hotTimeEndMaxHour - 8) * 3600);

            hotTimeStartSeconds = Random.Range(minHotTime, maxHotTime);
            hotTimeEndSeconds = hotTimeStartSeconds + (hotTimeDurationInHours * 3600);

            PlayerPrefs.SetFloat(PlayerPrefsHolder.HOT_TIME_START_KEY, hotTimeStartSeconds);
            PlayerPrefs.SetFloat(PlayerPrefsHolder.HOT_TIME_END_KEY, hotTimeEndSeconds);
            PlayerPrefs.SetString(PlayerPrefsHolder.HOT_TIME_DATE_KEY, today);
        }
    }
    public bool pm_time = false, stop_timer = false;
    [SerializeField] float test_timer = 0;
    // Update is called once per frame
    void Update()
    {
        if (!stop_timer && setting_Data)
        {
            gametimer += Time.deltaTime * time_multiplier;
            day_night_timer += Time.deltaTime * time_multiplier * 1.1f;

            //  int _seconds = (int)(gametimer % 60);
            int _minutes = (int)(gametimer / 60) % 60;
            int _hours = (int)(gametimer / 3600) % 24;

            //   string timestring = string.Format("{0:0}:{1:00}:{2:00}", _hours, _minutes, _seconds);
            string timestring = string.Format("{0:0}:{1:00}", _hours, _minutes);
            time_text.text = timestring;
            if (_hours == 12)
            {
                time_text.text += "PM";
            }
            else if (_hours >= 7 && _hours <= 12 && !pm_time)
            {
                time_text.text += "AM";
                PlayerPrefs.SetString(PlayerPrefsHolder.days_Time_pm, "False");
            }
            else if (pm_time)
            {
                time_text.text += "PM";
                PlayerPrefs.SetString(PlayerPrefsHolder.days_Time_pm, "True");
            }

            if (_hours == 13)
            {
                gametimer = 3600;
                pm_time = true;
            }

            if (gametimer >= 35000/*28500*/ && pm_time)
            {
                //  UIManager.Instance.enday_btn.SetActive(true);
                stop_timer = true;
                time_text.text = "Shift Completed";
                dayCompleted = true;
                dayEnd_obj.SetActive(true);
                dayEnd_txt.SetActive(true);

                //OnTimeStoppedEvent?.Invoke();
                //  GamePlayHandler.Instance.CloseShop();
            }

            /* if (_minutes == 59)
                 ChangeSkybox(_hours);*/

            test_timer = (day_night_timer / 60) % 1440f;
            //  light_manager.UpdateLighting(test_timer * inverseDayLength);
            //For Hot Time

            // Use day_night_timer instead of gametimer
            if (!isHotTimeActive && day_night_timer >= hotTimeStartSeconds && day_night_timer < hotTimeEndSeconds)
            {
                isHotTimeActive = true;
                Debug.Log(" HOT TIME STARTED!");

                MasterCustomerSpawner.instance.SetSpawnInterval(13f); // Faster spawn
            }
            else if (isHotTimeActive && day_night_timer >= hotTimeEndSeconds)
            {
                isHotTimeActive = false;
                Debug.Log(" HOT TIME ENDED!");

                MasterCustomerSpawner.instance.SetSpawnInterval(25f); // Restore default
            }
            if (isHotTimeActive)
                time_text.color = Color.red;
            else
                time_text.color = Color.white;
        }
        if (Input.GetKeyDown(KeyCode.C) && stop_timer && dayCompleted)
        {
            Enday();
            dayEnd_obj.SetActive(false);
            dayEnd_txt.SetActive(false);
        }
    }
    void ChangeSkybox(int hours)
    {
        if (hours > 7 && hours < 11 && !pm_time && RenderSettings.skybox != skyboxes[0])
        {
            RenderSettings.skybox = skyboxes[0];
            return;
        }
        else if (hours == 11 || hours > 1 && hours < 4 && pm_time && RenderSettings.skybox != skyboxes[1])
        {
            RenderSettings.skybox = skyboxes[1];
            return;
        }
        else if (hours > 4 && pm_time && RenderSettings.skybox != skyboxes[2])
        {
            RenderSettings.skybox = skyboxes[2];
            return;
        }
        else
            return;
    }

    public bool CheckIfDayisEnding()
    {
        if (gametimer >= 34001 && pm_time)
        {
            return true;
        }
        return false;
    }
    public void Enday()
    {
        UIManager.instance.OpenDailySummaryPanel();

        

    }

    public void ResetDay()
    {
        gametimer = 28500f;
        day_night_timer = 28500f;
        PlayerPrefs.SetFloat(PlayerPrefsHolder.day_time, 28800f);
        int _minutes = (int)(gametimer / 60) % 60;
        int _hours = (int)(gametimer / 3600) % 24;
        // RenderSettings.skybox = skyboxes[0];

        //   string timestring = string.Format("{0:0}:{1:00}:{2:00}", _hours, _minutes, _seconds);
        string timestring = string.Format("{0:0}:{1:00}", _hours, _minutes);
        time_text.text = timestring;

        PlayerPrefs.SetString(PlayerPrefsHolder.days_name, SetNextDay(dayindex));
        PlayerPrefs.SetInt(PlayerPrefsHolder.days_index, dayindex);
        day_text.text = PlayerPrefs.GetString(PlayerPrefsHolder.days_name, "Monday");
        dayindex = PlayerPrefs.GetInt(PlayerPrefsHolder.days_index, 0);

        PlayerPrefs.SetString(PlayerPrefsHolder.days_Time_pm, "False");
        pm_time = false;
        stop_timer = false;
        dayCompleted = false;

        // Reset hot time for new day
        float minHotTime = 28800 + (hotTimeStartMinHour - 8) * 3600;
        float maxHotTime = Mathf.Min(79200 - (hotTimeDurationInHours * 3600), 28800 + (hotTimeEndMaxHour - 8) * 3600);

        hotTimeStartSeconds = Random.Range(minHotTime, maxHotTime);
        hotTimeEndSeconds = hotTimeStartSeconds + (hotTimeDurationInHours * 3600);
        isHotTimeActive = false;

        // Save new hot time with today's date
        string today = PlayerPrefs.GetString(PlayerPrefsHolder.days_name, "Monday");/*System.DateTime.Now.Date.ToString("yyyyMMdd");*/
        PlayerPrefs.SetFloat(PlayerPrefsHolder.HOT_TIME_START_KEY, hotTimeStartSeconds);
        PlayerPrefs.SetFloat(PlayerPrefsHolder.HOT_TIME_END_KEY, hotTimeEndSeconds);
        PlayerPrefs.SetString(PlayerPrefsHolder.HOT_TIME_DATE_KEY, today);

        //Reset Daily Count 
        MachineBreakdownManager.Instance?.ResetDailyCount();

        UIManager.instance.ResetDailySummary();
        UIManager.instance.CloseDailySummaryPanel();
    }
    string SetNextDay(int index)
    {
        string name = string.Empty;
        index = index + 1;
        if (index < 7)
            name = daynames[index];
        else
        {
            index = 0;
            name = daynames[index];
        }

        dayindex = index;

        return name;
    }


    private void OnApplicationQuit()
    {
        if (setting_Data)
        {
            Debug.Log("Disable --------------------");
            PlayerPrefs.SetFloat(PlayerPrefsHolder.day_time, gametimer);
            PlayerPrefs.SetFloat(PlayerPrefsHolder.day_night_lighting_time, day_night_timer);

            // Save hot time
            PlayerPrefs.SetFloat(PlayerPrefsHolder.HOT_TIME_START_KEY, hotTimeStartSeconds);
            PlayerPrefs.SetFloat(PlayerPrefsHolder.HOT_TIME_END_KEY, hotTimeEndSeconds);
        }
    }
    private void OnApplicationFocus(bool focus)
    {
        if (!focus && setting_Data)
        {
            Debug.Log("Focuss  --------------------");
            PlayerPrefs.SetFloat(PlayerPrefsHolder.day_time, gametimer);
            PlayerPrefs.SetFloat(PlayerPrefsHolder.day_night_lighting_time, day_night_timer);

            //  Save hot time
            PlayerPrefs.SetFloat(PlayerPrefsHolder.HOT_TIME_START_KEY, hotTimeStartSeconds);
            PlayerPrefs.SetFloat(PlayerPrefsHolder.HOT_TIME_END_KEY, hotTimeEndSeconds);
        }
    }
}
