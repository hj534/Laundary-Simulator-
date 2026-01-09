using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsHolder : MonoBehaviour
{
    public static string Cash = "Cash";
    public static int defaultCash_value = 2000; //Change it for giving customer cash at start 

    public static int PricePerPound = 20;
    
    
    public static string Unlocked_Machines = "Unlocked_Machines";
    public static int defaultUnlockedMacine_value = 15;
    public static string Unlocked_Big_Dryer  = "Unlocked_Big_Dryer";
    public static int defaultUnlockedBigDryer_value = 7;
    public static string Unlocked_Small_Dryer = "Unlocked_Small_Dryer";
    public static int defaultSmallDryer_value = 13;
    public static string Unlocked_FoldingTable = "Unlocked_FoldingTable";
    public static int defaultFoldingTable_value = 1;

    public static string days_Time_pm = "DayTimePm";
    public static string days_name = "DayName";
    public static string day_night_lighting_time = "DayNightLightingTime";
    public static string day_time = "Day_Time";
    public static string days_index = "days_index";


    public static string HOT_TIME_DATE_KEY = "HotTimeDate";
    public static string HOT_TIME_START_KEY = "HotTimeStart";
    public static string HOT_TIME_END_KEY = "HotTimeEnd";

    // --- Machine Breakdown System ---
    public const string MACHINE_BREAKDOWN_PREFIX = "Machine_";
    public const string MACHINE_BREAKDOWN_TYPE_SUFFIX = "_Breakdown";
    public const string MACHINE_BREAKDOWN_DAYS_SUFFIX = "_DaysLeft";

    //Daily Summary Keys
    public static string SS_DailyOrders = "SS_DailyOrders";
    public static string SS_DailyEarnings = "SS_DailyEarnings";
    public static string FS_DailyOrders = "FS_DailyOrders";
    public static string FS_DailyEarnings = "FS_DailyEarnings";
    public static string Total_DailyOrders = "Total_DailyOrders";
    public static string Total_DailyEarnings = "Total_DailyEarnings";
}
