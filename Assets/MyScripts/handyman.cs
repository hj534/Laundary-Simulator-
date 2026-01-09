using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class handyman : MonoBehaviour, IInteractable
{
    public string itemName = "Elliott";
    [SerializeField]
    string[] dialogues;
    [SerializeField] TextMeshPro dialog_txt;

    public Animator animator;

    [Header("Idle Animations")]
    public string[] idleAnimations = { "Idle1", "Idle2", "Idle3" };

    [Header("Talking Animations")]
    public string[] talkingAnimations = { "Talking1", "Talking2" };

    public float minIdleDelay = 0f;
    public float maxIdleDelay = 1f;

    private bool isTalking = false;
    private bool oneTime = false;
    public void Interact()
    {
        if (oneTime)
            return;
        Debug.Log("Picked up: " + itemName);
        //  Destroy(gameObject);
        oneTime = true;
        ShootRay.instance.currentPrompt.SetActive(false);
        ShootRay.instance.p_controller.enabled = false;
        ShootRay.instance.currentOutline.enabled = false;
        ShowDialogOpenShoppingPanel();
    }

    public string GetInteractionText()
    {
        return $"Press E to Talk To {itemName}";
    }

    void ShowDialogOpenShoppingPanel()
    {
        int i = Random.Range(0, dialogues.Length);

        StopCoroutine(IdleLoop());
        StartCoroutine(PlayTalkingAnimation());
        
        dialog_txt.transform.DOScale(new Vector3(.05f, .05f, .05f), .3f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            StartCoroutine(TypeSentence(dialogues[i] /*"Welcome What are ypu looking for today" +
                " I have various machines, to help you grow your shop" +
                " Would you  like to buy somthing?"*/));
        });

    }

    IEnumerator TypeSentence(string sentence)
    {
        dialog_txt.text = "";
        for (int i = 0; i < sentence.Length; i++)
        {
            dialog_txt.text += sentence[i];
            yield return new WaitForSeconds(.05f);
        }
        yield return new WaitForSeconds(1.5f);
        dialog_txt.text = "";
        yield return new WaitForSeconds(.5f);
       // ShootRay.instance.ShowWorldPrompt("Press S to open Shop or Press Z to cancel", transform);
        string Dialog = "Press S to open Shop or Press Z to cancel";
        for (int i = 0; i < Dialog.Length; i++)
        {
            dialog_txt.text += Dialog[i];
            yield return new WaitForSeconds(.05f);
        }

        yield return new WaitForSeconds(.2f);
        bool CHECK = false;
        while (!CHECK)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("Into While Loop  S pressed");
                ShootRay.instance.currentPrompt.SetActive(false);
                UIManager.instance.OpenShopPanel();
                dialog_txt.text = "";
                CHECK = true;
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                Debug.Log("Into While Loop  Z preseed ");
                ShootRay.instance.currentPrompt.SetActive(false);
                CHECK = true;
                ShootRay.instance.p_controller.enabled = true;
                dialog_txt.text = "";
            }
            Debug.Log("Into While Loop  ");
            yield return null;
        }
        oneTime = false;
    }

    void Start()
    {
        PlayNextIdle();
    }
    void PlayNextIdle()
    {
        StartCoroutine(IdleLoop());
    }

    IEnumerator IdleLoop()
    {
        while (!isTalking)
        {
            string idleAnim = idleAnimations[Random.Range(0, idleAnimations.Length)];
            animator.Play(idleAnim);

            // Wait for animation length + random buffer
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            float waitTime = state.length + Random.Range(minIdleDelay, maxIdleDelay);

            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator PlayTalkingAnimation()
    {
        isTalking = true;

        string talkAnim = talkingAnimations[Random.Range(0, talkingAnimations.Length)];
        animator.Play(talkAnim);

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(state.length);

        isTalking = false;
        PlayNextIdle(); // resume idle loop
    }
}
