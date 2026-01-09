using UnityEngine;

public class CounterTrigger : MonoBehaviour
{
    public static bool isPlayerAtCounter = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerAtCounter = true;
            Debug.Log("Player arrived at counter.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerAtCounter = false;
            Debug.Log(" Player left counter.");
        }
    }
}
