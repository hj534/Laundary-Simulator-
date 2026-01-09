using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MachineIntreactable : MonoBehaviour, IInteractable
{
    public string itemName = "Machine";
    public int currentBasketId = 0;
    public bool machine_in_use, pullout_clothes, usingByAI;
    public Transform AiStandPos;
    public Animator anim;
    public Transform top_pos, targetPos;
    public List<ClothScript> cloths_ref = new List<ClothScript>();

    [Header("Breakdown System")]
    public BreakdownType breakdownType = BreakdownType.None;
    public bool isRepairing = false;
    public int daysRemainingForLargeRepair = 0;

    [Header("Breakdown Visuals")]
    public GameObject yellowExclamationIcon;
    public GameObject smokeEffect;
    public GameObject leakEffect;
    public TextMeshProUGUI repairStatusText;

    [Header(" ---  Water Effect Particales ----- ")]
    public ParticleSystem[] waterBubble_particale;

    private void Start()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        UpdateBreakdownVisuals();
    }

    public void Interact()
    {
        if (breakdownType == BreakdownType.Small)
        {
            if (!isRepairing)
                StartCoroutine(StartSmallRepair());
            return;
        }
        if (breakdownType == BreakdownType.Large)
        {
            Debug.Log("Machine under large repair!");
            SoundManager.instance.playsound("warning");
            return;
        }

        if (machine_in_use && !pullout_clothes) return;
        if (!ShootRay.instance.has_basket) return;
        if (currentBasketId != 0 && ShootRay.instance.clothBasket.id != currentBasketId)
        {
            SoundManager.instance.playsound("warning");
            return;
        }
        if (usingByAI)
        {
            SoundManager.instance.playsound("warning");
            return;
        }
        Debug.Log("Picked up: " + itemName);
        ShootRay.instance.machine_ref = this;
        ShootRay.instance.currentPrompt.SetActive(false);

        if (!ShootRay.instance.CheckInHandBasketIsnotEmpty())
        {
            machine_in_use = true;
            StartCoroutine(ShootRay.instance.PullOutClothFromMachine(this));
        }
        else
        {
            machine_in_use = true;
            SoundManager.instance.playsound("StartMachine");
            StartCoroutine(ShootRay.instance.MoveClothInMachine());
        }

        ShootRay.instance.currentOutline.enabled = false;
    }

    public string GetInteractionText()
    {
        if (breakdownType == BreakdownType.Small)
            return "Hold E to Repair Machine";

        if (breakdownType == BreakdownType.Large)
            return "Machine is under maintenance";

        if (machine_in_use && !pullout_clothes)
            return $"";

        if (pullout_clothes && machine_in_use)
            return $"Press E to pull out Clothes from {itemName} and place in basket";

        return $"Press E to Place Clothes in {itemName}";
    }

    public Image fillImage;
    public float duration;
    public float timer;
    public bool running;
    public GameObject canvasBar , fill_img_bg;
    public Text oderId_Text;

    IEnumerator StartTimer()
    {
        yield return null;
        while (running)
        {
            timer += Time.deltaTime;
            fillImage.fillAmount = 1 - (timer / duration);

            if (timer >= duration)
                running = false;

            yield return null;
        }

        fillImage.gameObject.SetActive(false);
        fill_img_bg.gameObject.SetActive(false);
        anim.Play("Open");
        yield return new WaitForSeconds(1.8f);
        anim.speed = 0;
        pullout_clothes = true;
       // yield return new WaitForSeconds(1.5f);
       for(int i =0;i< waterBubble_particale.Length; i++)
        {
            waterBubble_particale[i].Play();
        }
    }

    public void StartProgress(float time)
    {
        duration = time;
        timer = 0f;
        running = true;
        fillImage.fillAmount = 1;
        fillImage.gameObject.SetActive(true);
        fill_img_bg.gameObject.SetActive(true);
        canvasBar.SetActive(true);
        oderId_Text.text = usingByAI ? "" : $"Ticket ID: {currentBasketId}";
        StartCoroutine(StartTimer());
    }

    IEnumerator StartSmallRepair()
    {
        isRepairing = true;
        repairStatusText.gameObject.SetActive(true);
        repairStatusText.text = "Repairing...";

        float repairTime = 3f;
        duration = repairTime;
        timer = 0f;
        fillImage.fillAmount = 1;
        fillImage.gameObject.SetActive(true);
        canvasBar.SetActive(true);

        while (timer < repairTime)
        {
            timer += Time.deltaTime;
            fillImage.fillAmount = 1 - (timer / duration);
            yield return null;
        }

        fillImage.gameObject.SetActive(false);
        canvasBar.SetActive(false);
        repairStatusText.gameObject.SetActive(false);
        yellowExclamationIcon.SetActive(false);
        breakdownType = BreakdownType.None;
        isRepairing = false;
        UpdateBreakdownVisuals();
    }

    public void UpdateBreakdownVisuals()
    {
        if (breakdownType == BreakdownType.Small)
            yellowExclamationIcon?.SetActive(breakdownType == BreakdownType.Small);
        if (breakdownType == BreakdownType.Large)
            yellowExclamationIcon?.SetActive(breakdownType == BreakdownType.Large);
        smokeEffect?.SetActive(breakdownType == BreakdownType.Large);
        leakEffect?.SetActive(breakdownType == BreakdownType.Large);
    }
}
