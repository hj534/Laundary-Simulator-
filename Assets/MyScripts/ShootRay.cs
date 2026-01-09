using DG.Tweening;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class ShootRay : MonoBehaviour
{
    public static ShootRay instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(instance);
    }
    [Header("Player Ref")]
    public ThirdPersonController p_controller;
    public Animator p_anim;
    public Transform holding_pos;
    public Text clockText;

    [Header("References")]
    public RectTransform crosshairUI;
    public LayerMask interactableLayer;
    public float rayDistance = 100f;

    [Header(" ----  Rig Layer Data ----  ")]
    public RigDataAccordingTobjects[] rigData;

    [Space]
    [Header("  ----  Cloth Basket data Ref  --- ")]
    public bool has_basket;
    public bool moving_cloth;
    public ClothBasket clothBasket;
    public MachineIntreactable machine_ref;
    public SmallDryerIntreactable smallDryer_ref;
    public BigDryerIntreactable bigDryer_ref;
    public FoldingTableIntreactable foldingTable_ref;
    public PickUpCounterIntreactable pickUpCounter_ref;
    public PackingBoxIntreactable box;

    [Header("World Prompt")]
    public GameObject interactionPromptPrefab; // Assign prefab in inspector

    public GameObject currentPrompt; // only one prompt
    private PromptFollowTarget promptFollow;
    private Text promptText;

    private GameObject currentTarget;
    public Outline currentOutline;
    [SerializeField] IInteractable currentInteractable;
    private Camera cam;

    // NEW: reference to currently held object
    private GameObject heldObject;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Spawn the prompt at start and keep disabled
        if (interactionPromptPrefab != null)
        {
            currentPrompt = Instantiate(interactionPromptPrefab);
            promptFollow = currentPrompt.GetComponent<PromptFollowTarget>();
            promptText = currentPrompt.GetComponentInChildren<Text>();
            currentPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (cam == null || crosshairUI == null || UIManager.instance.shopPanel.activeInHierarchy) return;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, crosshairUI.position);
        Ray ray = cam.ScreenPointToRay(screenPoint);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactableLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject != currentTarget)
            {
                ClearPreviousOutline();

                Outline outline = hitObject.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = true;
                    currentTarget = hitObject;
                    currentOutline = outline;

                    currentInteractable = hitObject.GetComponent<IInteractable>();

                    string message = currentInteractable != null ? currentInteractable.GetInteractionText() : "Press E to Interact";
                    ShowWorldPrompt(message, hitObject.transform);
                }
            }
        }
        else
        {
            ClearPreviousOutline();
        }
        if (currentInteractable == null)
        {
            Debug.Log("Intreactable is null");
        }
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null && !moving_cloth)
        {
            Debug.Log("Ifff");
            currentInteractable.Interact();
        }
        //  NEW: Drop logic
        if (Input.GetKeyDown(KeyCode.T) && heldObject != null && !moving_cloth)
        {
            SoundManager.instance.playsound("Drop");
            DropHeldObject();
            ClearPreviousOutline();
            clothBasket.picked = false;
            machine_ref = null;
            clothBasket = null;
            has_basket = false;
        }
     //   clockText.text = DayTimerManager.Instance.GetFormattedTime();
    }

    public void ClearPreviousOutline()
    {
        if (currentOutline != null)
            currentOutline.enabled = false;

        currentTarget = null;
        currentOutline = null;
        currentInteractable = null;

        if (currentPrompt != null)
            currentPrompt.SetActive(false);
    }

    public void ShowWorldPrompt(string message, Transform target)
    {
        // Safety checks
        if (currentPrompt == null)
        {
            Debug.LogWarning("World prompt prefab not initialized.");
            return;
        }

        if (promptFollow == null)
        {
            promptFollow = currentPrompt.GetComponent<PromptFollowTarget>();
            if (promptFollow == null)
            {
                Debug.LogWarning("PromptFollowTarget component missing.");
                return;
            }
        }

        if (promptText == null)
        {
            promptText = currentPrompt.GetComponentInChildren<Text>();
            if (promptText == null)
            {
                Debug.LogWarning("Text component missing in prompt prefab.");
                return;
            }
        }

        if (target == null)
        {
            Debug.LogWarning("Target transform is null. Cannot show prompt.");
            return;
        }

        // Set text and target
        promptFollow.SetTarget(target);
        promptText.text = message;
        currentPrompt.SetActive(true);
    }

    public IEnumerator MoveInHand(GameObject obj)
    {
        SoundManager.instance.playsound("Pick");
        p_anim.SetBool("Holding", true);
        //  NEW: Set reference to held object
        heldObject = obj;
        /*   while (Vector3.Distance(obj.transform.position, holding_pos.position) >= .01f)
           {
               obj.transform.position = Vector3.Lerp(obj.transform.position, holding_pos.position, .02f);
               obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, holding_pos.rotation, .02f);
               yield return null;
           }
           obj.transform.position = holding_pos.position;
           obj.transform.rotation = holding_pos.rotation;
           obj.transform.parent = holding_pos;
           yield return null;*/
        obj.transform.DOMove(holding_pos.position, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            obj.transform.position = holding_pos.position;
            obj.transform.rotation = holding_pos.rotation;

        });
        obj.transform.DORotate(holding_pos.eulerAngles, .4f).SetEase(Ease.InOutSine);
        obj.transform.parent = holding_pos;
        yield return null;
    }
    public void AfterPlaceOnRack()
    {
        // Clear animation state
        p_anim.SetBool("Holding", false);
        DisableRigEffect();
        // Clear reference
        heldObject = null;
    }
    //  NEW: DropHeldObject Method
    public void DropHeldObject()
    {
        if (heldObject != null)
        {
            // Detach from hand
            heldObject.transform.parent = null;

            if (heldObject.gameObject.GetComponent<Collider>())
                heldObject.gameObject.GetComponent<Collider>().enabled = true;

            // Enable physics
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            // Clear animation state
            p_anim.SetBool("Holding", false);
            DisableRigEffect();
            // Clear reference
            heldObject = null;
        }
    }
    #region - Enable/Disbale RigLayers -
    public void ActivateRigLayers(string itemname)
    {
        for (int i = 0; i < rigData.Length; i++)
        {
            if (itemname == rigData[i].itemName)
            {
                for (int k = 0; k < rigData[i].rigLayer_to_activate.Length; k++)
                {
                    rigData[i].rigLayer_to_activate[k].weight = 1;
                }
                return;
            }
        }
    }

    void DisableRigEffect()
    {
        for (int i = 0; i < rigData.Length; i++)
        {
            for (int k = 0; k < rigData[i].rigLayer_to_activate.Length; k++)
            {
                rigData[i].rigLayer_to_activate[k].weight = 0;
            }
        }
    }

    #endregion

    #region - Move Cloth in Machine -

    public bool CheckInHandBasketIsnotEmpty()
    {
        if (clothBasket == null)
            return false;

        if (clothBasket.cloths != null && clothBasket.cloths.Count != 0)
            return true;
        else
            return false;
    }
    void EnabeleDisablePlayerController(bool active)
    {
        p_controller.enabled = active;
    }

    public IEnumerator MoveClothInMachine()
    {
        if (clothBasket != null && has_basket && machine_ref != null && clothBasket.cloths.Count != 0
            && clothBasket.step == ClothStep.WashingMachine)
        {
            moving_cloth = true;
            //Reset Machine Anim
            machine_ref.anim.StopPlayback();
            machine_ref.anim.Play("Open",0,0);
            machine_ref.currentBasketId = clothBasket.id;
            EnabeleDisablePlayerController(false);
            yield return new WaitForSeconds(.1f);

            //Loop To put Cloths in Machine
            for (int i = 1; clothBasket.cloths.Count - i > -1; i++)
            {
                Vector3 pos = clothBasket.cloths[clothBasket.cloths.Count - i].transform.position;
                pos.y += .5f;
                clothBasket.cloths[clothBasket.cloths.Count - i].transform.DOMove(pos, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    clothBasket.cloths[clothBasket.cloths.Count - i].transform.parent = null;
                    clothBasket.cloths[clothBasket.cloths.Count - i].anim.Play("ShirtClose");
                    clothBasket.cloths[clothBasket.cloths.Count - i].transform.DOMove(machine_ref.top_pos.position, .5f).SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        clothBasket.cloths[clothBasket.cloths.Count - i].transform.DOMove(machine_ref.targetPos.position, .5f).SetEase(Ease.InOutSine);
                    });
                });
                yield return new WaitForSeconds(1.3f);
                clothBasket.cloths[clothBasket.cloths.Count - i].transform.parent = machine_ref.targetPos;
                machine_ref.cloths_ref.Add(clothBasket.cloths[clothBasket.cloths.Count - i]);
            }
            machine_ref.StartProgress(Random.Range(30, 40));
            EnabeleDisablePlayerController(true);
            yield return new WaitForSeconds(.5f);
            clothBasket.cloths = null;
            moving_cloth = false;
        }
        yield return null;
    }

    public IEnumerator PullOutClothFromMachine(MachineIntreactable machine)
    {
        yield return null;
        if (clothBasket != null)
        {
            machine.canvasBar.SetActive(false);
            EnabeleDisablePlayerController(false);
            clothBasket.step = ClothStep.Dryer;
            clothBasket.cloths = new List<ClothScript>();
            Debug.Log("Closth basket is not null"); int i2 = 0;
            for (int i = 0; i < machine.cloths_ref.Count; i++)
            {
                Debug.Log("Closth basket is not null  Loop");
                machine.cloths_ref[i].transform.DOMove(machine.top_pos.position, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    Debug.Log("Reached MAchine Top ");
                    Vector3 pos = clothBasket.cloths_pos[i2/*clothBasket.cloths_pos.Length - i*/].transform.position;
                    pos.y += .5f;

                    machine.cloths_ref[i].transform.parent = null;
                    machine.cloths_ref[i].anim.Play("ShirtOpen");
                    machine.cloths_ref[i].transform.DOMove(pos, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                    {
                        Debug.Log("Reached Basket Top ");
                        machine.cloths_ref[i].transform.DOMove(clothBasket.cloths_pos[i2/*clothBasket.cloths_pos.Length - i*/].position, .5f).
                        SetEase(Ease.InOutSine);
                    });
                });
                yield return new WaitForSeconds(1.3f);

                Debug.Log("Reached Basket Top " + i2/*(clothBasket.cloths_pos.Length - i)*/);
                machine.cloths_ref[i].transform.parent = clothBasket.cloths_pos[i2/*clothBasket.cloths_pos.Length - i*/];
                clothBasket.cloths.Add(machine.cloths_ref[i]);
                i2++;
            }
            EnabeleDisablePlayerController(true);
            yield return new WaitForSeconds(.3f);
            machine.anim.speed = 2;
            machine.cloths_ref = null;
            machine.cloths_ref = new List<ClothScript>();
            machine.machine_in_use = false;
            machine.pullout_clothes = false;
            machine.currentBasketId = 0;
        }
    }
    #endregion


    #region - Move Clothes in/out the Small Dryer -
    public IEnumerator MoveClothInSmallDryer()
    {
        if (clothBasket != null && has_basket && smallDryer_ref != null && clothBasket.cloths.Count != 0
            && clothBasket.step == ClothStep.Dryer && smallDryer_ref.itemName == "SmallDryer")
        {
            smallDryer_ref.smallDryer_in_use = true;
            moving_cloth = true;
            //Anim         
            smallDryer_ref.smalldryer_door.transform.DOLocalRotate(new Vector3(0, 0, 90), .5f).SetEase(Ease.InOutSine);
            smallDryer_ref.currentBasketId = clothBasket.id;
            EnabeleDisablePlayerController(false);
            yield return new WaitForSeconds(.3f);

            //Loop To put Cloths in Dryer
            for (int i = 1; clothBasket.cloths.Count - i > -1; i++)
            {
                Vector3 pos = clothBasket.cloths[clothBasket.cloths.Count - i].transform.position;
                pos.y += .5f;
                clothBasket.cloths[clothBasket.cloths.Count - i].transform.DOMove(pos, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    clothBasket.cloths[clothBasket.cloths.Count - i].transform.parent = null;
                    clothBasket.cloths[clothBasket.cloths.Count - i].anim.Play("ShirtClose");
                    clothBasket.cloths[clothBasket.cloths.Count - i].transform.DOMove(smallDryer_ref.front_pos.position, .5f).SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        clothBasket.cloths[clothBasket.cloths.Count - i].transform.DOMove(smallDryer_ref.targetPos.position, .5f).SetEase(Ease.InOutSine);
                        clothBasket.cloths[clothBasket.cloths.Count - i].transform.DOScale(new Vector3(.7f, .7f, .7f), .5f).SetEase(Ease.InOutSine);
                    });
                });
                yield return new WaitForSeconds(1.3f);
                clothBasket.cloths[clothBasket.cloths.Count - i].transform.parent = smallDryer_ref.targetPos;
                smallDryer_ref.cloths_ref.Add(clothBasket.cloths[clothBasket.cloths.Count - i]);
            }
            smallDryer_ref.StartProgress(Random.Range(20, 25));
            EnabeleDisablePlayerController(true);
            yield return new WaitForSeconds(.5f);
            clothBasket.cloths = null;
            moving_cloth = false;
        }
        yield return null;
    }
    public IEnumerator PullOutClothFromSmallDryer(SmallDryerIntreactable smallDryer)
    {
        yield return null;
        if (clothBasket != null)
        {
            smallDryer.canvasBar.SetActive(false);
            EnabeleDisablePlayerController(false);
            clothBasket.step = ClothStep.Folding;
            clothBasket.cloths = new List<ClothScript>();
            Debug.Log("Closth basket is not null"); int i2 = 0;
            for (int i = 0; i < smallDryer.cloths_ref.Count; i++)
            {
                Debug.Log("Closth basket is not null  Loop");
                smallDryer.cloths_ref[i].transform.DOMove(smallDryer.front_pos.position, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    Debug.Log("Reached MAchine Top ");
                    Vector3 pos = clothBasket.cloths_pos[i2/*clothBasket.cloths_pos.Length - i*/].transform.position;
                    pos.y += .5f;

                    smallDryer.cloths_ref[i].transform.parent = null;
                    smallDryer.cloths_ref[i].anim.Play("ShirtOpen");
                    smallDryer.cloths_ref[i].transform.DOMove(pos, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                    {
                        Debug.Log("Reached Basket Top ");
                        smallDryer.cloths_ref[i].transform.DOMove(clothBasket.cloths_pos[i2/*clothBasket.cloths_pos.Length - i*/].position, .5f).
                        SetEase(Ease.InOutSine);
                        smallDryer.cloths_ref[i].transform.DOScale(Vector3.one, .5f).SetEase(Ease.InOutSine);
                    });
                });
                yield return new WaitForSeconds(1.3f);

                Debug.Log("Reached Basket Top " + i2/*(clothBasket.cloths_pos.Length - i)*/);
                smallDryer.cloths_ref[i].transform.parent = clothBasket.cloths_pos[i2/*clothBasket.cloths_pos.Length - i*/];
                clothBasket.cloths.Add(smallDryer.cloths_ref[i]);
                i2++;
            }
            smallDryer.smalldryer_door.transform.DOLocalRotate(Vector3.zero, .3f).SetEase(Ease.InOutSine);
            EnabeleDisablePlayerController(true);
            yield return new WaitForSeconds(.4f);
            smallDryer.cloths_ref = null;
            smallDryer.cloths_ref = new List<ClothScript>();
            smallDryer.smallDryer_in_use = false;
            smallDryer.pullout_clothes = false;
            smallDryer.currentBasketId = 0;
        }
    }
    #endregion


    #region - Move Clothes in/out the Big Dryer -
    public IEnumerator MoveClothInBigDryer()
    {
        if (clothBasket != null && has_basket && bigDryer_ref != null && clothBasket.cloths.Count != 0
            && clothBasket.step == ClothStep.Dryer && bigDryer_ref.itemName == "BigDryer")
        {
            bigDryer_ref.bigDryer_in_use = true;
            moving_cloth = true;
            //Anim         
            bigDryer_ref.bigdryer_door.transform.DOLocalRotate(new Vector3(0, 0, 90), .5f).SetEase(Ease.InOutSine);

            bigDryer_ref.currentBasketId = clothBasket.id;
            EnabeleDisablePlayerController(false);
            yield return new WaitForSeconds(.3f);

            //Loop To put Cloths in Dryer
            for (int i = 1; clothBasket.cloths.Count - i > -1; i++)
            {
                Vector3 pos = clothBasket.cloths[clothBasket.cloths.Count - i].transform.position;
                pos.y += .5f;
                clothBasket.cloths[clothBasket.cloths.Count - i].transform.DOMove(pos, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    clothBasket.cloths[clothBasket.cloths.Count - i].transform.parent = null;
                    clothBasket.cloths[clothBasket.cloths.Count - i].anim.Play("ShirtClose");
                    clothBasket.cloths[clothBasket.cloths.Count - i].transform.DOMove(bigDryer_ref.front_pos.position, .5f).SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        clothBasket.cloths[clothBasket.cloths.Count - i].transform.DOMove(bigDryer_ref.targetPos.position, .5f).SetEase(Ease.InOutSine);
                        clothBasket.cloths[clothBasket.cloths.Count - i].transform.DOScale(new Vector3(.7f, .7f, .7f), .5f).SetEase(Ease.InOutSine);
                    });
                });
                yield return new WaitForSeconds(1.3f);
                clothBasket.cloths[clothBasket.cloths.Count - i].transform.parent = bigDryer_ref.targetPos;
                bigDryer_ref.cloths_ref.Add(clothBasket.cloths[clothBasket.cloths.Count - i]);
            }
            bigDryer_ref.StartProgress(Random.Range(30, 40));
            EnabeleDisablePlayerController(true);
            yield return new WaitForSeconds(.5f);
            clothBasket.cloths = null;
            moving_cloth = false;
        }
        yield return null;
    }
    public IEnumerator PullOutClothFromBigDryer(BigDryerIntreactable bigDryer)
    {
        yield return null;
        if (clothBasket != null)
        {
            bigDryer.canvasBar.SetActive(false);
            EnabeleDisablePlayerController(false);
            clothBasket.step = ClothStep.Folding;
            clothBasket.cloths = new List<ClothScript>();
            Debug.Log("Closth basket is not null"); int i2 = 0;
            for (int i = 0; i < bigDryer.cloths_ref.Count; i++)
            {
                Debug.Log("Closth basket is not null  Loop");
                bigDryer.cloths_ref[i].transform.DOMove(bigDryer.front_pos.position, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    Debug.Log("Reached MAchine Top ");
                    Vector3 pos = clothBasket.cloths_pos[i2/*clothBasket.cloths_pos.Length - i*/].transform.position;
                    pos.y += .5f;

                    bigDryer.cloths_ref[i].transform.parent = null;
                    bigDryer.cloths_ref[i].anim.Play("ShirtOpen");
                    bigDryer.cloths_ref[i].transform.DOMove(pos, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                    {
                        Debug.Log("Reached Basket Top ");
                        bigDryer.cloths_ref[i].transform.DOMove(clothBasket.cloths_pos[i2/*clothBasket.cloths_pos.Length - i*/].position, .5f).
                        SetEase(Ease.InOutSine);
                        bigDryer.cloths_ref[i].transform.DOScale(Vector3.one, .5f).SetEase(Ease.InOutSine);
                    });
                });
                yield return new WaitForSeconds(1.3f);

                Debug.Log("Reached Basket Top " + i2/*(clothBasket.cloths_pos.Length - i)*/);
                bigDryer.cloths_ref[i].transform.parent = clothBasket.cloths_pos[i2/*clothBasket.cloths_pos.Length - i*/];
                clothBasket.cloths.Add(bigDryer.cloths_ref[i]);
                i2++;
            }
            bigDryer.bigdryer_door.transform.DOLocalRotate(Vector3.zero, .3f).SetEase(Ease.InOutSine);
            EnabeleDisablePlayerController(true);
            yield return new WaitForSeconds(.4f);
            bigDryer.cloths_ref = null;
            bigDryer.cloths_ref = new List<ClothScript>();
            bigDryer.bigDryer_in_use = false;
            bigDryer.pullout_clothes = false;
            bigDryer.currentBasketId = 0;
        }
    }
    #endregion


    #region - Move Cloth Basket OnFolding Table -
    public IEnumerator MoveBasketOnFoldingTable(FoldingTableIntreactable foldingTable)
    {
        if (clothBasket != null && has_basket && clothBasket.step == ClothStep.Folding && clothBasket.cloths.Count != 0)
        {
            foldingTable_ref = foldingTable;
            foldingTable.foldingtable_in_use = true;
            clothBasket.transform.parent = null;
            clothBasket.transform.parent = foldingTable.basketPos;
            foldingTable.currentBasketId = clothBasket.id;
            has_basket = false;
            clothBasket.transform.DOMove(foldingTable.basketPos.position, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                clothBasket.col.enabled = false;

                ActivateClothComponentOnTable();
            });
            clothBasket.transform.DORotate(foldingTable.basketPos.eulerAngles, .5f).SetEase(Ease.InOutSine);
            if (heldObject != null)
            {
                // Detach from hand
                heldObject.transform.parent = null;

                if (heldObject.gameObject.GetComponent<Collider>())
                    heldObject.gameObject.GetComponent<Collider>().enabled = true;


                // Clear animation state
                foldingTable.box.anim.Play("Open");
                p_anim.SetBool("Holding", false);
                DisableRigEffect();
                // Clear reference
                heldObject = null;
            }
            yield return null;
        }
    }
    public void ActivateClothComponentOnTable()
    {
        if (clothBasket.cloths.Count == 0)
        {
            if (!foldingTable_ref.box.ready_to_pick)
            {
                foldingTable_ref.box.ready_to_pick = true;
                Debug.Log("Box Ready To pick and Place  ");
                foldingTable_ref.box.anim.Play("Close");
                foldingTable_ref.box.collider_ref.enabled = true;
                foldingTable_ref.box.orderID = clothBasket.id;
                foldingTable_ref.box.table_ref = foldingTable_ref;
                foldingTable_ref.basket_ref= clothBasket.gameObject;    
                
            }
            return;
        }
        int k = clothBasket.cloths.Count - 1;
        clothBasket.cloths[k].collider_ref.enabled = true;

        Vector3 pos = clothBasket.cloths[k].transform.position;
        pos.y += .2f;

        clothBasket.cloths[k].transform.DOMove(pos, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            clothBasket.cloths[k].anim.Play("ShirtClose");
            clothBasket.cloths[k].transform.DOMove(foldingTable_ref.cloth_pos.position, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                clothBasket.cloths[k].allowintreaction = true;
                clothBasket.cloths.RemoveAt(k);
            });
            clothBasket.cloths[k].transform.DORotate(foldingTable_ref.cloth_pos.eulerAngles, .5f).SetEase(Ease.InOutSine);
        });
    }
    public IEnumerator MoveClothIntoBox(ClothScript obj)
    {
        obj.collider_ref.enabled = false;
        Vector3 pos = obj.transform.position;
        pos.y += .2f;
        obj.gameObject.transform.DOMove(pos, .6f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            obj.cloth_ref.enabled = false;
            obj.anim.Play("ShirtOpen");
            obj.gameObject.transform.DOMove(foldingTable_ref.box.topPos.position, .6f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                obj.gameObject.transform.DOMove(foldingTable_ref.box.pos[clothBasket.cloths.Count].position, .6f).SetEase(Ease.InOutSine)
                /*.SetDelay(.4f)*/;
                obj.transform.parent = null;
                obj.transform.parent = foldingTable_ref.box.transform;

                ActivateClothComponentOnTable();
            });
        });
        yield return null;
    }
    #endregion
    #region - Move Box on pick Up Counter -
    public IEnumerator PlaceBoxOnPickUpCounter()
    {
        if (pickUpCounter_ref != null && box != null)
        {
            PickUpCounterData counter_data = pickUpCounter_ref.GiveEmptySpace();
            if(counter_data != null)
            {
                counter_data.isfree = false;
                has_basket = false;

                box.gameObject.transform.DOMove(counter_data.pos.position, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    LaundryOrderManager.Instance.MarkOrderComplete(ShootRay.instance.box.orderID);
                    if (heldObject != null)
                    {
                        // Detach from hand
                        heldObject.transform.parent = null;

                        if (heldObject.gameObject.GetComponent<Collider>())
                            heldObject.gameObject.GetComponent<Collider>().enabled = true;


                        // Clear animation state
                        p_anim.SetBool("Holding", false);
                        DisableRigEffect();
                        // Clear reference
                        heldObject = null;
                        box.transform.parent = null;
                        box.transform.parent = pickUpCounter_ref.transform;


                        box.placed_on_counter = true;
                        //box.collider_ref.enabled = false;
                        box.picked = false;
                        box = null;
                        pickUpCounter_ref = null;
                    }
                });
                box.gameObject.transform.DORotate(counter_data.pos.eulerAngles, .5f).SetEase(Ease.InOutSine);
            }
        }

        yield return null;
    }
    #endregion

    #region - Spawn Box At Folding Table / Remove Basket 
    public IEnumerator SpawnClothBox(Transform boxSpawnPos , FoldingTableIntreactable table)
    {
        yield return new WaitForSeconds(2f);
        GameObject box = Instantiate(LaundryOrderManager.Instance.box_prefeb);
        box.transform.position = boxSpawnPos.position;
        table.box = box.GetComponent<PackingBoxIntreactable>();
        table.box.collider_ref.enabled = false;
        table.box.table_ref = table;
        GameObject obj = table.basket_ref;
        Destroy(obj);
        table.foldingtable_in_use = false;
        table.folding_complete = false;
    }
    #endregion

}


[System.Serializable]
public class RigDataAccordingTobjects
{
    public string itemName;
    public Rig[] rigLayer_to_activate;
}