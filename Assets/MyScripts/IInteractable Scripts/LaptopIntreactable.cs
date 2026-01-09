using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaptopIntreactable : MonoBehaviour , IInteractable
{
    public string itemName = "Elliott";
    public void Interact()
    {
        if (UIManager.instance.shopPanel.activeInHierarchy)
            return;
        Debug.Log("Picked up: " + itemName);
        //  Destroy(gameObject);
        ShootRay.instance.currentPrompt.SetActive(false);
        ShootRay.instance.p_controller.enabled = false;
        ShootRay.instance.currentOutline.enabled = false;
        UIManager.instance.OpenShopPanel();
    }

    public string GetInteractionText()
    {
        return $"Press E to Open {itemName}";
    }

}
