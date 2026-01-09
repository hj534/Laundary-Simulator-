using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.IO;
using UnityEngine.UI;
public class ClothBasket : MonoBehaviour, IInteractable
{
    public string itemName = "Basket";
    public bool firstTimePicked, picked, picked_by_AI = true, placedOnRack, place_onCounter;
    public Transform rackPos;
    public ClothStep step;
    public BasketSize basketSize;
    public int id, rack_place_index = -1;
    public int cloths_weight;
    public Rigidbody rb;
    public BoxCollider col;
    [Header(" ----   Cloths Pos in Basket -----  ")]
    public Transform[] cloths_pos;
    public List<ClothScript> cloths = new List<ClothScript>();

    [Header("Canvas")]
    public GameObject canvas;
    public Text orderId_Text;

    void Start()
    {
        if (!picked_by_AI) 
        {
            canvas.SetActive(true);
            orderId_Text.text = $"Ticket ID: {id}";
        }
    }
    public void Interact()
    {
        if (!firstTimePicked)
        {
            firstTimePicked = true;
            UIManager.instance.OpenOrderDetailsPanel(id, cloths_weight, cloths_weight * PlayerPrefsHolder.PricePerPound);
        }


        if (picked)
            return;
        Debug.Log("Picked up: " + itemName);
        if (picked_by_AI) // it only returned before if picked by AI
        {
            return;
        }
        else
        {
            canvas.SetActive(true);
            orderId_Text.text = $"Ticket ID: {id}";
        }
        if (ShootRay.instance.has_basket)
            return;
        picked = true;
        place_onCounter = false;
        ShootRay.instance.has_basket = true;
        ShootRay.instance.clothBasket = this;
        if (placedOnRack)
        {
            RackManager.Instance.FreeSpot(rackPos);
            placedOnRack = false;
        }
        // Inform customer to leave
        FullServiceCustomer customer = FullServiceCustomerManager.Instance.FindCustomerByID(id);
        if (customer != null)
        {
            customer.OnBasketPickedUp();
        }
        //    rb.useGravity = false;
        rb.isKinematic = true;
        col.enabled = false;

        StartCoroutine(ShootRay.instance.MoveInHand(transform.gameObject));
        ShootRay.instance.ActivateRigLayers(itemName);
        ShootRay.instance.currentPrompt.SetActive(false);
        //  ShootRay.instance.p_controller.enabled = false;
        ShootRay.instance.currentOutline.enabled = false;
    }

    public string GetInteractionText()
    {
        //   return $"Press E to Pick Up";
        return picked_by_AI ? "": $"Press E to Pick Up {itemName}";
    }

    #region - Save/Load Data - 
    // Save basket data
    public ClothBasketData GetData()
    {
        return new ClothBasketData
        {
            itemName = this.itemName,
            firstTimePicked = this.firstTimePicked,
            picked = this.picked,
            picked_by_AI = this.picked_by_AI,
            placedOnRack = this.placedOnRack,
            step = this.step.ToString(),
            basketSize = this.basketSize.ToString(),
            id = this.id,
            cloths_weight = this.cloths_weight,
            rack_place_index = this.rack_place_index,
            position = new float[] { transform.position.x, transform.position.y, transform.position.z },
            rotation = new float[] { transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z }
        };
    }

    // Load basket data
    public void InitializeFromData(ClothBasketData data)
    {
        this.itemName = data.itemName;
        //  this.picked = data.picked;
        this.firstTimePicked = data.firstTimePicked;
        this.picked_by_AI = data.picked_by_AI;
        this.placedOnRack = data.placedOnRack;
        this.step = (ClothStep)System.Enum.Parse(typeof(ClothStep), data.step);
        this.basketSize = (BasketSize)System.Enum.Parse(typeof(BasketSize), data.basketSize);
        this.id = data.id;
        this.cloths_weight = data.cloths_weight;
        this.rack_place_index = data.rack_place_index;

        if (rack_place_index != -1 && placedOnRack)
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            RackManager.Instance.rackSpots[rack_place_index].isOccupied = true;
            RackManager.Instance.rackSpots[rack_place_index].clothbasket = this;
            rackPos = RackManager.Instance.rackSpots[rack_place_index].spotTransform;
        }
        if (!placedOnRack)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }
        /*        if (picked)
                {
                }*/
        col.enabled = true;
        transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);
        transform.rotation = Quaternion.Euler(data.rotation[0], data.rotation[1], data.rotation[2]);

    }
    #endregion
}
