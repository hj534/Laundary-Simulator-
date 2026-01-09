using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineBreakdownManager : MonoBehaviour
{
    public static MachineBreakdownManager Instance;

    public float smallBreakChance = 0.1f;
    public float largeBreakChance = 0.05f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(.3f);
        LoadMachineStates();
    }
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("Manually triggering breakdowns...");
            ResetDailyCount(); // Force trigger
        }
#endif
    }
    public void ResetDailyCount()
    {
        MachineIntreactable[] machines = FindObjectsOfType<MachineIntreactable>();

        foreach (var machine in machines)
        {
            if (machine.gameObject.activeInHierarchy && machine.breakdownType == BreakdownType.None)
            {
                float roll = Random.value;
                if (roll < largeBreakChance)
                {
                    machine.breakdownType = BreakdownType.Large;
                    machine.daysRemainingForLargeRepair = Random.Range(1, 3);
                    machine.UpdateBreakdownVisuals();
                    Debug.Log("BreakDown Debug   Large " + roll + "   ___  " + largeBreakChance);
                }
                else if (roll < smallBreakChance + largeBreakChance)
                {
                    machine.breakdownType = BreakdownType.Small;
                    machine.UpdateBreakdownVisuals();
                    Debug.Log("BreakDown Debug   Small " + roll + "   ___  " + smallBreakChance + largeBreakChance);
                }
            }
            else if (machine.breakdownType == BreakdownType.Large)
            {
                machine.daysRemainingForLargeRepair--;
                if (machine.daysRemainingForLargeRepair <= 0)
                {
                    machine.breakdownType = BreakdownType.None;
                    machine.UpdateBreakdownVisuals();
                    Debug.Log("BreakDown Debug   Large " + "  __ Name   "+machine.name + " decrease a day  " +  machine.daysRemainingForLargeRepair);
                }
            }
        }

        SaveMachineStates();
    }

    public void SaveMachineStates()
    {
        MachineIntreactable[] machines = FindObjectsOfType<MachineIntreactable>();
        for (int i = 0; i < machines.Length; i++)
        {
            PlayerPrefs.SetInt($"Machine_{i}_Breakdown", (int)machines[i].breakdownType);
            PlayerPrefs.SetInt($"Machine_{i}_DaysLeft", machines[i].daysRemainingForLargeRepair);
        }
    }

    public void LoadMachineStates()
    {
        MachineIntreactable[] machines = FindObjectsOfType<MachineIntreactable>();
        for (int i = 0; i < machines.Length; i++)
        {
            machines[i].breakdownType = (BreakdownType)PlayerPrefs.GetInt($"Machine_{i}_Breakdown", 0);
            machines[i].daysRemainingForLargeRepair = PlayerPrefs.GetInt($"Machine_{i}_DaysLeft", 0);
            machines[i].UpdateBreakdownVisuals();
        }
    }
    private void OnApplicationQuit()
    {
        Debug.Log("Quit ");
        SaveMachineStates();
    }
}
[System.Serializable]
public enum BreakdownType
{
    None = 0,
    Small = 1,
    Large = 2
}