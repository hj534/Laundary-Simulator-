using UnityEngine;
using System.Collections.Generic;

public class CounterManager : MonoBehaviour
{
    public static CounterManager Instance;

    [System.Serializable]
    public class StandingSpot
    {
        public Transform position;
        public bool isOccupied;
    }
    public Transform basket_pos;
    public List<StandingSpot> standingSpots = new List<StandingSpot>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

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
        foreach (StandingSpot spot in standingSpots)
        {
            if (spot.position == spotTransform)
            {
                spot.isOccupied = false;
                break;
            }
        }
    }

    // ? NEW: Manually occupy a spot
    public void OccupySpot(Transform spotTransform)
    {
        foreach (StandingSpot spot in standingSpots)
        {
            if (spot.position == spotTransform)
            {
                spot.isOccupied = true;
                break;
            }
        }
    }

    // Check if counter is full
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
            if (standingSpots[i + 1].isOccupied && !standingSpots[i].isOccupied)
            {
                // Move customer from spot i+1 to spot i
                FullServiceCustomer customerToShift = FindCustomerAtSpot(standingSpots[i + 1].position);
                if (customerToShift != null)
                {
                    customerToShift.assignedSpot = standingSpots[i].position;
                    customerToShift.agent.SetDestination(standingSpots[i].position.position);
                    standingSpots[i].isOccupied = true;
                    standingSpots[i + 1].isOccupied = false;
                }
            }
        }
    }

    private FullServiceCustomer FindCustomerAtSpot(Transform spot)
    {
        foreach (FullServiceCustomer customer in FullServiceCustomerManager.Instance.customerPool)
        {
            if (customer.gameObject.activeInHierarchy && customer.assignedSpot == spot)
                return customer;
        }
        return null;
    }

    public bool IsCustomerFirstInLine(FullServiceCustomer customer)
    {
        return standingSpots[0].position == customer.assignedSpot;
    }
}
