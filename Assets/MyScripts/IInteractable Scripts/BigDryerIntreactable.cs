using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BigDryerIntreactable : MonoBehaviour, IInteractable
{
    public string itemName = "BigDryer";
    public int currentBasketId = 0;
    public bool bigDryer_in_use, pullout_clothes, usingByAi;
    public GameObject bigdryer_door;
    public BasketSize acceptableSize;
    public Transform aiStandPos;
    public Animator anim;
    public Transform front_pos, targetPos;
    public List<ClothScript> cloths_ref = new List<ClothScript>();
    private void Start()
    {
        if (anim == null)
            gameObject.GetComponent<Animator>();
    }

    public void Interact()
    {
        if (bigDryer_in_use && !pullout_clothes)
            return;
        if (!ShootRay.instance.has_basket)
            return;
        if (acceptableSize != ShootRay.instance.clothBasket.basketSize)
            return;
        if (currentBasketId != 0 && ShootRay.instance.clothBasket.id != currentBasketId)
        {
            SoundManager.instance.playsound("warning");
            return;
        }
        Debug.Log("Picked up: " + itemName);

        //  smallDryer_in_use = true;
        ShootRay.instance.currentPrompt.SetActive(false);
        ShootRay.instance.bigDryer_ref = this;
        if (!ShootRay.instance.CheckInHandBasketIsnotEmpty())
        {
            Debug.Log("Into Iffff");
            StartCoroutine(ShootRay.instance.PullOutClothFromBigDryer(this));
        }
        else
        {
            SoundManager.instance.playsound("StartMachine");
            StartCoroutine(ShootRay.instance.MoveClothInBigDryer());
        }


        ShootRay.instance.currentOutline.enabled = false;
    }

    public string GetInteractionText()
    {
        if (bigDryer_in_use && !pullout_clothes)
            return $"";
        else if (pullout_clothes && bigDryer_in_use)
            return $"Press E to pull out Clothes from {itemName}" + " and place in basket";
        else
            return $"Press E to Place Clothes in {itemName}";
    }


    public Image fillImage;
    public GameObject fill_img_bg;
    private float duration;
    private float timer;
    public bool running;
    public GameObject canvasBar;
    public Text orderId_Text;
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

        if (usingByAi)
        {
            canvasBar.SetActive(false);
        }
        else
        {
            fillImage.gameObject.SetActive(false);
            fill_img_bg.gameObject.SetActive(false);
        }

        anim.Play("Idle");
        bigdryer_door.transform.DOLocalRotate(new Vector3(0, 0, 90), .5f).SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(.5f);
        pullout_clothes = true;
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
            fillImage.gameObject.SetActive(true);
            fill_img_bg.gameObject.SetActive(true);
            orderId_Text.text = usingByAi ? "" : $"Ticket ID: {currentBasketId}";
            anim.Play("Spin");
            StartCoroutine(StartTimer());
        });

    }
}
