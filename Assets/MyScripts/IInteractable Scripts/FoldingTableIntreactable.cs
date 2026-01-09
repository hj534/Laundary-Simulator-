using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FoldingTableIntreactable : MonoBehaviour ,IInteractable
{
    public string itemName = "FoldingTable";
    public int currentBasketId = 0;
    public bool foldingtable_in_use , folding_complete;

    public Transform basketPos, cloth_pos , box_pos;
    public PackingBoxIntreactable box;
    public GameObject basket_ref;


    private void OnEnable()
    {
        if (box == null)
            return;
        if (!box.gameObject.activeInHierarchy)
            box.gameObject.SetActive(true);
    }
    public void Interact()
    {
        if (foldingtable_in_use)
            return;
        if (!ShootRay.instance.has_basket)
            return;
        if (currentBasketId != 0 && ShootRay.instance.clothBasket.id != currentBasketId)
            return;
        Debug.Log("Picked up: " + itemName);


        ShootRay.instance.currentPrompt.SetActive(false);
        ShootRay.instance.foldingTable_ref = this;

        StartCoroutine(ShootRay.instance.MoveBasketOnFoldingTable(this));

        ShootRay.instance.currentOutline.enabled = false;
    }

    public string GetInteractionText()
    {
        if (folding_complete)
            return $"";
        else 
            return $"Press E to put Clothes Basket on {itemName}";
    }


   /* public Image fillImage;
    private float duration;
    private float timer;
    private bool running;
    public GameObject canvasBar;
    IEnumerator StartTimer()
    {
        yield return null;
        // if (!running) return;
        while (running)
        {
            timer += Time.deltaTime;
            fillImage.fillAmount = 1 - (timer / duration);

            if (timer >= duration)
                running = false;

            yield return null;
        }

        canvasBar.SetActive(false);
        yield return new WaitForSeconds(.5f);
    }

    public void StartProgress(float time)
    {
        bigdryer_door.transform.DOLocalRotate(new Vector3(0, 0, 0), .5f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            duration = time;
            timer = 0f;
            running = true;
            fillImage.fillAmount = 1;
            canvasBar.SetActive(true);
            anim.Play("Spin");
            StartCoroutine(StartTimer());
        });

    }*/
}
