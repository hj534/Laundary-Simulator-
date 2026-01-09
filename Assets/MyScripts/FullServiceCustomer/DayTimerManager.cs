using UnityEngine;
using System;

public class DayTimerManager : MonoBehaviour
{
    public static DayTimerManager Instance;

    [Header("Day Cycle Settings")]
    public float dayDurationInSeconds = 300f; // Full in-game day length (e.g., 5 minutes)
    private float currentTime = 0f;

    public enum TimeOfDay { Morning, Afternoon, Evening, Night }
    public TimeOfDay currentTimeOfDay;

    [Header("Hot Time Settings")]
    public bool isHotTime = false;
    public float hotTimeStart = 120f; // Hot Time starts at 2 minutes
    public float hotTimeEnd = 180f;   // Hot Time ends at 3 minutes

    public event Action OnHotTimeStart;
    public event Action OnHotTimeEnd;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        // Advance time
        currentTime += Time.deltaTime;

        if (currentTime >= dayDurationInSeconds)
        {
            currentTime = 0f; // Reset to next day
            Debug.Log("A new day begins!");
        }

        UpdateTimeOfDay();
        CheckHotTime();
    }

    private void UpdateTimeOfDay()
    {
        float quarterDay = dayDurationInSeconds / 4f;

        if (currentTime < quarterDay)
            currentTimeOfDay = TimeOfDay.Morning;
        else if (currentTime < quarterDay * 2)
            currentTimeOfDay = TimeOfDay.Afternoon;
        else if (currentTime < quarterDay * 3)
            currentTimeOfDay = TimeOfDay.Evening;
        else
            currentTimeOfDay = TimeOfDay.Night;
    }

    private void CheckHotTime()
    {
        if (currentTime >= hotTimeStart && currentTime <= hotTimeEnd)
        {
            if (!isHotTime)
            {
                isHotTime = true;
                Debug.Log("?? Hot Time started!");
                OnHotTimeStart?.Invoke();
            }
        }
        else
        {
            if (isHotTime)
            {
                isHotTime = false;
                Debug.Log("? Hot Time ended.");
                OnHotTimeEnd?.Invoke();
            }
        }
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public string GetFormattedTime()
    {
        float normalizedTime = (currentTime / dayDurationInSeconds) * 24f;
        int hours = Mathf.FloorToInt(normalizedTime);
        int minutes = Mathf.FloorToInt((normalizedTime - hours) * 60f);
        return $"{hours:00}:{minutes:00}";
    }
}
