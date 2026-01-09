using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackingBoxIntreactable : MonoBehaviour, IInteractable
{
    public string itemName = "PackingBox";
    public int orderID, counter_pos_index;
    public bool ready_to_pick, picked, placed_on_counter,dont_save;
    public Transform topPos;
    public Transform[] pos;


    public Animator anim;
    public BoxCollider collider_ref;
    public FoldingTableIntreactable table_ref;

    public GameObject canvas;
    public Text orderId_Text;

    bool orderdelivered = false;


    private void Start()
    {
        if (anim == null)
            anim = gameObject.GetComponent<Animator>();

        if(ready_to_pick && placed_on_counter && !orderdelivered)
        {
            orderId_Text.text = $"Ticket ID: {orderID}";
        }
        else
        {
            canvas.SetActive(false);
        }
    }
    public void Interact()
    {
        if(orderdelivered) return;

        if (ready_to_pick && placed_on_counter && !orderdelivered)
        {
            var frontcustomer = PickUpCounterIntreactable.instance.GetFrontCustomer();
            if (frontcustomer != null && frontcustomer.orderID == orderID)
            {
                frontcustomer.CanPickOrder = true;
                orderdelivered = true;
                Debug.Log("Order Delivered to Customer");
            }
            return;
        }

        if (!ready_to_pick && picked && placed_on_counter)
        {
            return;
        }

        ShootRay.instance.box = this;
        // fold = true;
        picked = true;
        ShootRay.instance.currentPrompt.SetActive(false);
        // ShootRay.instance.p_controller.enabled = false;

        canvas.SetActive(true);
        orderId_Text.text = $"Ticket ID: {orderID}";
        StartCoroutine(ShootRay.instance.MoveInHand(gameObject));
        ShootRay.instance.ActivateRigLayers("Basket");
        ShootRay.instance.currentOutline.enabled = false;
        table_ref.currentBasketId = 0;
        if (table_ref != null)
            StartCoroutine(ShootRay.instance.SpawnClothBox(table_ref.box_pos, table_ref));
    }

    public string GetInteractionText()
    {
        if (ready_to_pick && placed_on_counter && !orderdelivered)
        {
            //if (right_order())
            {
                return "Press E to deliver Order to Customer";
            }
            // else
            // {
            //     return "This is not the right order for the front customer.";
            // }
        }
        else
            return $"Press E to Pick box then place it on Recive Counter";
        //  return $"Press E to Pick Up {itemName}";
    }

    bool right_order()
    {
        var frontcustomer = PickUpCounterIntreactable.instance.GetFrontCustomer();
        if (frontcustomer != null && frontcustomer.orderID == orderID)
        {
            return true;
        }
        return false;
    }
}
