using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class RackManager : MonoBehaviour, IInteractable
{
    public static RackManager Instance;
    public string itemName = "Rack";
    [System.Serializable]
    public class RackSpot
    {
        public Transform spotTransform;
        public bool isOccupied;
        public ClothBasket clothbasket;
    }

    public List<RackSpot> rackSpots = new List<RackSpot>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
       // else
          //  Destroy(gameObject);
    }

    public Transform GetFreeSpot()
    {
        foreach (RackSpot spot in rackSpots)
        {
            if (!spot.isOccupied)
            {
                return spot.spotTransform;
            }
        }
        return null;
    }
    int ReturnIndex(Transform targetPos)
    {
        for(int i=0;i< rackSpots.Count; i++)
        {
            if(targetPos == rackSpots[i].spotTransform)
            {
                return i;
            }
        }
        return -1;
    }
    public void OccupySpot(Transform spotTransform , ClothBasket basket)
    {
        foreach (RackSpot spot in rackSpots)
        {
            if (spot.spotTransform == spotTransform)
            {
                spot.isOccupied = true;
                spot.clothbasket = basket;
                return;
            }
        }
    }

    public void FreeSpot(Transform spotTransform)
    {
        
        foreach (RackSpot spot in rackSpots)
        {
            if (spot.spotTransform == spotTransform)
            {
                spot.isOccupied = false;
                return;
            }
        }
    }
    public void Interact()
    {
        if (!ShootRay.instance.has_basket || ShootRay.instance.clothBasket == null)
        {
            Debug.Log(" No basket in hand to place.");
            return;
        }
        // Get first free rack spot
        Transform freeSpot = GetFreeSpot();
        if (freeSpot == null)
        {
            Debug.Log(" Rack is full! No free spots.");
            return;
        }

        // Place the basket on the rack
        GameObject basket = ShootRay.instance.clothBasket.gameObject;
        ShootRay.instance.clothBasket.placedOnRack = true;
        ShootRay.instance.clothBasket.rackPos = freeSpot;
        ShootRay.instance.clothBasket.rack_place_index = ReturnIndex(freeSpot);

        basket.transform.SetParent(null);
        basket.transform.DOLocalRotate(freeSpot.eulerAngles, .3f).SetEase(Ease.InOutSine);
        basket.transform.DOMove(freeSpot.position, .3f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            // Mark the rack spot as occupied
            OccupySpot(freeSpot, ShootRay.instance.clothBasket);

            ShootRay.instance.clothBasket.col.enabled = true;
            // Clear player's hand
            ShootRay.instance.has_basket = false;
            ShootRay.instance.clothBasket.picked = false;
            ShootRay.instance.clothBasket = null;
            ShootRay.instance.currentPrompt.SetActive(false);
            ShootRay.instance.currentOutline.enabled = false;

            ShootRay.instance.AfterPlaceOnRack();
            ShootRay.instance.ClearPreviousOutline();

        
        });


        /* if (ShootRay.instance.clothBasket == null)
             return;
         if (!ShootRay.instance.has_basket)
             return;


         ShootRay.instance.currentPrompt.SetActive(false);
         //  ShootRay.instance.p_controller.enabled = false;
         ShootRay.instance.currentOutline.enabled = false;*/
    }

    public string GetInteractionText()
    {
        //   return $"Press E to Pick Up";
        return $"Press E to Place Box in {itemName}";
    }
}
