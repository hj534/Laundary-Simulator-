using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Animations.Rigging;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;


public class SelfServiceCustomer : MonoBehaviour
{

    /*  enum State { Idle, MovingToWasher, Washing, MovingToDryer, Drying, Exiting }
      State currentState;

      NavMeshAgent agent;
      SelfServiceManager manager;
      CustomerData myData;
      LaundryStation currentStation;

      public float smallLoadTime = 5f;
      public float largeLoadTime = 10f;

      public ProgressBar progressBar;

      private Coroutine currentRoutine;

      void Awake()
      {
          agent = GetComponent<NavMeshAgent>();
      }

      public void Initialize(CustomerData data, SelfServiceManager mgr)
      {
          myData = data;
          manager = mgr;

          // Reset state
          currentStation = null;
          currentState = State.MovingToWasher;

          // Stop old routine if reused
          if (currentRoutine != null)
              StopCoroutine(currentRoutine);

          currentRoutine = StartCoroutine(StateMachine());
      }

      IEnumerator StateMachine()
      {
          while (true)
          {
              switch (currentState)
              {
                  case State.MovingToWasher:
                      currentStation = manager.GetAvailableStation(manager.washers);
                      if (currentStation != null)
                      {
                          currentStation.isBusy = true;
                          agent.SetDestination(currentStation.position.position);
                          currentState = State.Washing;
                          yield return new WaitUntil(() => ReachedDestination());
                      }
                      else
                      {
                          yield return StartCoroutine(WaitUntilAvailable(manager.washers));
                      }
                      break;

                  case State.Washing:
                      yield return UseMachine();
                      currentStation.isBusy = false;
                      currentState = State.MovingToDryer;
                      break;

                  case State.MovingToDryer:
                      currentStation = manager.GetAvailableStation(manager.dryers);
                      if (currentStation != null)
                      {
                          currentStation.isBusy = true;
                          agent.SetDestination(currentStation.position.position);
                          currentState = State.Drying;
                          yield return new WaitUntil(() => ReachedDestination());
                      }
                      else
                      {
                          yield return StartCoroutine(WaitUntilAvailable(manager.dryers));
                      }
                      break;

                  case State.Drying:
                      yield return UseMachine();
                      currentStation.isBusy = false;
                      currentState = State.Exiting;
                      break;

                  case State.Exiting:
                      agent.SetDestination(manager.spawnPoint.position + Vector3.right * 5f);
                      yield return new WaitUntil(() => ReachedDestination());
                      manager.RemoveCustomer(gameObject); // Re-add to pool
                      yield break;

                  default:
                      yield return null;
                      break;
              }

              yield return null;
          }
      }

      IEnumerator UseMachine()
      {
          float duration = myData.isLargeLoad ? largeLoadTime : smallLoadTime;
          progressBar.StartProgress(duration);
          yield return new WaitForSeconds(duration);
      }

      IEnumerator WaitUntilAvailable(System.Collections.Generic.List<LaundryStation> stations)
      {
          Vector3 idlePos = transform.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
          agent.SetDestination(idlePos);

          while (true)
          {
              LaundryStation available = manager.GetAvailableStation(stations);
              if (available != null)
              {
                  currentStation = available;
                  currentStation.isBusy = true;
                  agent.SetDestination(currentStation.position.position);

                  if (stations == manager.washers)
                      currentState = State.Washing;
                  else
                      currentState = State.Drying;

                  yield return new WaitUntil(() => ReachedDestination());
                  yield break;
              }

              yield return new WaitForSeconds(1f);
          }
      }

      bool ReachedDestination()
      {
          return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
      }*/
    enum State { Idle, MovingToWasher, Washing, MovingToDryer, Drying, Exiting }
    [SerializeField] State currentState;
    [SerializeField] float smallloudOutCash, bigloudOutCash;
    NavMeshAgent agent;
    SelfServiceManager manager;
    [SerializeField] CustomerData myData;
    LaundryStation currentStation;
    [SerializeField] NavMeshObstacle navMeshObsticale;
    public float smallLoadTime = 5f;
    public float largeLoadTime = 10f;
    [SerializeField] Animator animator;
    [Header("Progress UI")]
    public GameObject progressBarObj;
    public Image progressFillImage;
    public Transform cameraTransform;
    [Header("Basket")]
    public GameObject basketPrefab, basketprefeblarge; // Prefab for customer's basket
    private GameObject carriedBasket; // Basket in customer's hand
    private ClothBasket carriedBasket_cloth; // Basket in customer's hand
    public Transform basketHoldPoint;   // Where customer holds basket
    public bool movingCloths;
    [SerializeField] Rig[] handRigs;
    private Coroutine currentRoutine;
    [SerializeField] float timer;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (progressBarObj != null)
            progressBarObj.SetActive(false);

        canvasGroup.alpha = 0f;
        pop_up_bg.localScale = Vector3.zero;
    }

    public void Initialize(CustomerData data, SelfServiceManager mgr)
    {
        myData = data;
        manager = mgr;

        // Reset state
        currentStation = null;
        currentState = State.MovingToWasher;

        // Stop old routine if reused
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(StateMachine());

        // Reset progress bar
        if (progressBarObj != null)
        {
            progressBarObj.SetActive(false);
            progressFillImage.fillAmount = 0f;
        }
        StartCoroutine(UpdateBlendTree());
        SpawnWithBasket();
    }
    IEnumerator UpdateBlendTree()
    {
        while (gameObject.activeInHierarchy)
        {
            // Calculate speed of agent relative to max speed
            float speedPercent = agent.velocity.magnitude / agent.speed;


            // Set Speed parameter for blend tree
            animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);
            yield return null;
            if (animator.GetFloat("Speed") <= .1)
                navMeshObsticale.enabled = false;
            else
                navMeshObsticale.enabled = true;
        }
    }


    IEnumerator StateMachine()
    {
        while (true)
        {
            Debug.Log("Running  " + transform.name);
            switch (currentState)
            {
                case State.MovingToWasher:
                    currentStation = manager.GetAvailableStation(manager.washers);
                    if (currentStation != null)
                    {
                        currentStation.isBusy = true;
                        Transform targetPos = currentStation.position.gameObject.GetComponent<MachineIntreactable>().AiStandPos;
                        currentStation.position.gameObject.GetComponent<MachineIntreactable>().usingByAI = true;
                        agent.SetDestination(targetPos.position/*currentStation.position.position*/);
                        currentState = State.Washing;
                        yield return new WaitUntil(() => ReachedDestination());
                        agent.transform.DORotate(targetPos.eulerAngles, .3f).SetEase(Ease.InOutSine);
                    }
                    else
                    {
                        yield return StartCoroutine(WaitUntilAvailable(manager.washers));
                    }
                    break;

                case State.Washing:
                    yield return UseMachine();
                    // currentStation.isBusy = false;
                    currentState = State.MovingToDryer;
                    break;

                case State.MovingToDryer:
                    if (!myData.isLargeLoad)
                        currentStation = manager.GetAvailableStation(manager.dryers);
                    else
                        currentStation = manager.GetAvailableStation(manager.Bigdryers);
                    if (currentStation != null)
                    {
                        currentStation.isBusy = true;
                        if (!myData.isLargeLoad)
                        {
                            Transform
                                targetPos = currentStation.position.transform.GetChild(0).GetComponent<SmallDryerIntreactable>().AIstandPos;
                            currentStation.position.transform.GetChild(0).GetComponent<SmallDryerIntreactable>().usingByAI = true;
                            agent.SetDestination(targetPos.position/*currentStation.position.position*/);
                        }
                        else
                        {
                            Transform
                                targetPos = currentStation.position.gameObject.GetComponent<BigDryerIntreactable>().aiStandPos;
                            currentStation.position.gameObject.GetComponent<BigDryerIntreactable>().usingByAi = true;
                            agent.SetDestination(targetPos.position/*currentStation.position.position*/);
                        }
                        currentState = State.Drying;
                        yield return new WaitUntil(() => ReachedDestination());
                        //agent.transform.DORotate(targetPos.eulerAngles, .3f).SetEase(Ease.InOutSine);
                    }
                    else
                    {
                        if (!myData.isLargeLoad)
                            yield return StartCoroutine(WaitUntilAvailable(manager.dryers));
                        else
                            yield return StartCoroutine(WaitUntilAvailable(manager.Bigdryers));
                    }
                    break;

                case State.Drying:
                    yield return UseMachine();
                    // currentStation.isBusy = false;
                    currentState = State.Exiting;
                    break;

                case State.Exiting:
                    agent.SetDestination(manager.spawnPoint.position + Vector3.right * 5f);
                    yield return new WaitForSeconds(1f);

                    PlayerPrefs.SetInt(PlayerPrefsHolder.SS_DailyOrders, PlayerPrefs.GetInt(PlayerPrefsHolder.SS_DailyOrders) + 1);
                    PlayerPrefs.SetInt(PlayerPrefsHolder.SS_DailyEarnings, PlayerPrefs.GetInt(PlayerPrefsHolder.SS_DailyEarnings) + (int)(myData.isLargeLoad ? bigloudOutCash : smallloudOutCash));
                    ShopManager.instance.AddingCoinsEffectUI(myData.isLargeLoad ? bigloudOutCash : smallloudOutCash);

                    yield return new WaitUntil(() => ReachedDestination());
                    manager.RemoveCustomer(gameObject); // Return to pool
                    yield break;

                default:
                    yield return null;
                    break;
            }

            yield return null;
        }
    }

    IEnumerator UseMachine()
    {
        float duration = myData.isLargeLoad ? largeLoadTime : smallLoadTime;

        if (progressBarObj != null)
        {
            progressBarObj.SetActive(true);
            progressFillImage.fillAmount = 0f;
        }
        if (currentState != State.Drying)
        {
            // Using Machine
            StartCoroutine(MoveClothInMachine());
            yield return new WaitForSeconds(2f);
            yield return new WaitUntil(() => !movingCloths);
            //  currentStation.isBusy = false;
        }
        else
        {
            if (myData.isLargeLoad)
            {
                // using big dryer
                StartCoroutine(MoveClothInBigDryer());
                yield return new WaitForSeconds(2f);
                yield return new WaitUntil(() => !movingCloths);
            }
            else
            {
                // Using Small Dryer
                StartCoroutine(MoveClothInSmallDryer());
                yield return new WaitForSeconds(2f);
                yield return new WaitUntil(() => !movingCloths);
            }


            /* yield return new WaitForSeconds(2f);
             yield return new WaitUntil(() => !movingCloths);*/
            // currentStation.isBusy = false;
        }

        /*  float timer = 0f;
          while (timer < duration)
          {
              timer += Time.deltaTime;
              float progress = timer / duration;

              if (progressFillImage != null)
                  progressFillImage.fillAmount = Mathf.Clamp01(progress);

              if (progressBarObj != null && cameraTransform != null)
                  progressBarObj.transform.LookAt(cameraTransform);

              yield return null;
          }

          if (progressBarObj != null)
              progressBarObj.SetActive(false);*/
    }

    IEnumerator WaitUntilAvailable(System.Collections.Generic.List<LaundryStation> stations)
    {
        //  Vector3 idlePos = transform.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        //  Vector3 idlePos = transform.position + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
        Vector3 idlePos = SelfServiceManager.instance.selfService_waitPoint.position + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
        agent.SetDestination(idlePos);


        while (true)
        {
            LaundryStation available = manager.GetAvailableStation(stations);
            if (available != null)
            {
                animator.applyRootMotion = false;
                currentStation = available;
                currentStation.isBusy = true;
                //     agent.SetDestination(currentStation.position.position);

                Transform targetPos = currentStation.position.gameObject.GetComponent<MachineIntreactable>().AiStandPos;
                currentStation.position.gameObject.GetComponent<MachineIntreactable>().usingByAI = true;
                agent.SetDestination(targetPos.position/*currentStation.position.position*/);



                if (stations == manager.washers)
                    currentState = State.Washing;
                else
                    currentState = State.Drying;

                yield return new WaitUntil(() => ReachedDestination());
                agent.transform.DORotate(targetPos.eulerAngles, .3f).SetEase(Ease.InOutSine);
                yield break;
            }
            if (timer > 0)
            {
                timer -= Time.deltaTime;

                if (timer < .02f)
                {
                    ShowPopup("Why it is taking so long !!!! ");
                    timer = .1f;
                }
            }
            yield return new WaitForSeconds(1f);

        }
    }

    bool ReachedDestination()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    private void SpawnWithBasket()
    {
        if (carriedBasket != null)
        {
            Destroy(carriedBasket); // Clean up any old basket
        }
        // Create a basket in customer's hand
        if (myData.isLargeLoad)
            carriedBasket = Instantiate(basketprefeblarge, basketHoldPoint.position, basketHoldPoint.rotation, basketHoldPoint);
        else
            carriedBasket = Instantiate(basketPrefab, basketHoldPoint.position, basketHoldPoint.rotation, basketHoldPoint);

        carriedBasket.transform.SetParent(basketHoldPoint);
        carriedBasket_cloth = carriedBasket.GetComponent<ClothBasket>();
        animator.SetBool("Hold", true);
        EnableDisableRig(1);
    }
    void EnableDisableRig(int weightValue)
    {
        for (int i = 0; i < handRigs.Length; i++)
        {
            handRigs[i].weight = weightValue;
        }
    }

    #region - Moving Clothes in Machine - 
    public IEnumerator MoveClothInMachine()
    {
        if (carriedBasket_cloth != null && carriedBasket_cloth.cloths.Count != 0)
        {
            movingCloths = true;
            MachineIntreactable machine_ref = currentStation.position.gameObject.GetComponent<MachineIntreactable>();
            //Reset Machine Anim
            machine_ref.anim.StopPlayback();
            machine_ref.anim.Play("Open", 0, 0);
            machine_ref.usingByAI = true;

            yield return new WaitForSeconds(.1f);

            //Loop To put Cloths in Machine
            for (int i = 1; carriedBasket_cloth.cloths.Count - i > -1; i++)
            {
                Vector3 pos = carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.position;
                pos.y += .5f;
                if (carriedBasket_cloth.basketSize == BasketSize.Bigbasket)
                {
                    if (i >= 3)
                    {
                        machine_ref.anim.speed = 0;
                        Debug.Log("Animator Speed   " + animator.speed);
                    }
                }
                carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.DOMove(pos, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.parent = null;
                    carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].anim.Play("ShirtClose");
                    carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.DOMove(machine_ref.top_pos.position, .5f).SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.DOMove(machine_ref.targetPos.position, .5f).SetEase(Ease.InOutSine);
                    });
                });
                yield return new WaitForSeconds(1.3f);
                carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.parent = machine_ref.targetPos;
                machine_ref.cloths_ref.Add(carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i]);

            }
            machine_ref.anim.speed = 2;
            Debug.Log("Animator Speed   " + animator.speed);
            machine_ref.StartProgress(Random.Range(30, 40));
            yield return new WaitForSeconds(.5f);
            carriedBasket_cloth.cloths = null;
            yield return new WaitUntil(() => !machine_ref.running);
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(PullOutClothFromMachine(machine_ref));
            yield return new WaitUntil(() => !movingCloths);
        }
        yield return null;
        movingCloths = false;
        //  currentStation.isBusy = false;
    }


    public IEnumerator PullOutClothFromMachine(MachineIntreactable machine)
    {
        yield return null;
        if (carriedBasket_cloth != null)
        {
            movingCloths = true;
            // carriedBasket_cloth.step = ClothStep.Dryer;
            carriedBasket_cloth.cloths = new List<ClothScript>();
            Debug.Log("Closth basket is not null"); int i2 = 0;
            for (int i = 0; i < machine.cloths_ref.Count; i++)
            {
                Debug.Log("Closth basket is not null  Loop");
                machine.cloths_ref[i].transform.DOMove(machine.top_pos.position, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    Debug.Log("Reached MAchine Top ");
                    Vector3 pos = carriedBasket_cloth.cloths_pos[i2/*clothBasket.cloths_pos.Length - i*/].transform.position;
                    pos.y += .5f;

                    machine.cloths_ref[i].transform.parent = null;
                    machine.cloths_ref[i].anim.Play("ShirtOpen");
                    machine.cloths_ref[i].transform.DOMove(pos, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                    {
                        Debug.Log("Reached Basket Top ");
                        machine.cloths_ref[i].transform.DOMove(carriedBasket_cloth.cloths_pos[i2/*clothBasket.cloths_pos.Length - i*/].position, .5f).
                        SetEase(Ease.InOutSine);
                    });
                });
                yield return new WaitForSeconds(1.3f);

                Debug.Log("Reached Basket Top " + i2/*(clothBasket.cloths_pos.Length - i)*/);
                machine.cloths_ref[i].transform.parent = carriedBasket_cloth.cloths_pos[i2/*clothBasket.cloths_pos.Length - i*/];
                carriedBasket_cloth.cloths.Add(machine.cloths_ref[i]);
                i2++;
            }
            yield return new WaitForSeconds(.3f);
            machine.anim.speed = 1;
            machine.cloths_ref = null;
            machine.cloths_ref = new List<ClothScript>();
            machine.machine_in_use = false;
            machine.pullout_clothes = false;
            machine.usingByAI = false;
            machine.currentBasketId = 0;
            yield return new WaitForSeconds(.3f);
            movingCloths = false;
            currentStation.isBusy = false;
        }
    }
    #endregion

    #region - Move Clothes in/out the Small Dryer -
    public IEnumerator MoveClothInSmallDryer()
    {
        if (carriedBasket_cloth != null && carriedBasket_cloth.cloths.Count != 0
            )
        {
            //    smallDryer_ref.smallDryer_in_use = true;
            movingCloths = true;
            //Anim         
            SmallDryerIntreactable smallDryer_ref = currentStation.position.transform.GetChild(0).GetComponent<SmallDryerIntreactable>();
            smallDryer_ref.smalldryer_door.transform.DOLocalRotate(new Vector3(0, 0, 90), .5f).SetEase(Ease.InOutSine);
            smallDryer_ref.usingByAI = true;

            yield return new WaitForSeconds(.3f);

            //Loop To put Cloths in Dryer
            for (int i = 1; carriedBasket_cloth.cloths.Count - i > -1; i++)
            {
                Vector3 pos = carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.position;
                pos.y += .5f;
                carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.DOMove(pos, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.parent = null;
                    carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].anim.Play("ShirtClose");
                    carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.DOMove(smallDryer_ref.front_pos.position, .5f).SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.DOMove(smallDryer_ref.targetPos.position, .5f).SetEase(Ease.InOutSine);
                        carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.DOScale(new Vector3(.7f, .7f, .7f), .5f).SetEase(Ease.InOutSine);
                    });
                });
                yield return new WaitForSeconds(1.3f);
                carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.parent = smallDryer_ref.targetPos;
                smallDryer_ref.cloths_ref.Add(carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i]);
            }
            smallDryer_ref.StartProgress(Random.Range(20, 25));
            yield return new WaitForSeconds(.5f);
            yield return new WaitUntil(() => !smallDryer_ref.running);
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(PullOutClothFromSmallDryer(smallDryer_ref));
            yield return new WaitUntil(() => !movingCloths);
            carriedBasket_cloth.cloths = null;
        }
        yield return new WaitForSeconds(1.5f);
        movingCloths = false;
        currentStation.isBusy = false;
    }
    public IEnumerator PullOutClothFromSmallDryer(SmallDryerIntreactable smallDryer)
    {
        yield return null;
        if (carriedBasket_cloth != null)
        {
            //   carriedBasket_cloth.step = ClothStep.Folding;
            carriedBasket_cloth.cloths = new List<ClothScript>();
            Debug.Log("Closth basket is not null"); int i2 = 0;
            for (int i = 0; i < smallDryer.cloths_ref.Count; i++)
            {
                Debug.Log("Closth basket is not null  Loop");
                smallDryer.cloths_ref[i].transform.DOMove(smallDryer.front_pos.position, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    Debug.Log("Reached MAchine Top ");
                    Vector3 pos = carriedBasket_cloth.cloths_pos[i2 - i].transform.position;
                    pos.y += .5f;

                    smallDryer.cloths_ref[i].transform.parent = null;
                    smallDryer.cloths_ref[i].anim.Play("ShirtOpen");
                    smallDryer.cloths_ref[i].transform.DOMove(pos, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                    {
                        Debug.Log("Reached Basket Top ");
                        smallDryer.cloths_ref[i].transform.DOMove(carriedBasket_cloth.cloths_pos[i2 - i].position, .5f).
                        SetEase(Ease.InOutSine);
                        smallDryer.cloths_ref[i].transform.DOScale(Vector3.one, .5f).SetEase(Ease.InOutSine);
                    });
                });
                yield return new WaitForSeconds(1.3f);

                //   Debug.Log("Reached Basket Top " + i2(clothBasket.cloths_pos.Length - i));
                smallDryer.cloths_ref[i].transform.parent = carriedBasket_cloth.cloths_pos[i2 - i];
                carriedBasket_cloth.cloths.Add(smallDryer.cloths_ref[i]);
                i2++;
            }
            smallDryer.smalldryer_door.transform.DOLocalRotate(Vector3.zero, .3f).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(.4f);
            smallDryer.cloths_ref = null;
            smallDryer.cloths_ref = new List<ClothScript>();
            smallDryer.smallDryer_in_use = false;
            smallDryer.pullout_clothes = false;
            smallDryer.usingByAI = false;
            smallDryer.smallDryer_in_use = false;
            smallDryer.currentBasketId = 0;
            movingCloths = false;
        }
    }
    #endregion

    #region - Move Clothes in/out the Big Dryer -
    public IEnumerator MoveClothInBigDryer()
    {
        if (carriedBasket_cloth != null && carriedBasket_cloth.cloths.Count != 0)
        {
            movingCloths = true;
            BigDryerIntreactable bigDryer_ref = currentStation.position.GetComponent<BigDryerIntreactable>();
            //Anim         
            bigDryer_ref.bigDryer_in_use = true;
            bigDryer_ref.bigdryer_door.transform.DOLocalRotate(new Vector3(0, 0, 90), .5f).SetEase(Ease.InOutSine);


            yield return new WaitForSeconds(.3f);

            //Loop To put Cloths in Dryer
            for (int i = 1; carriedBasket_cloth.cloths.Count - i > -1; i++)
            {
                Vector3 pos = carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.position;
                pos.y += .5f;
                carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.DOMove(pos, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.parent = null;
                    carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].anim.Play("ShirtClose");
                    carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.DOMove(bigDryer_ref.front_pos.position, .5f).SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.DOMove(bigDryer_ref.targetPos.position, .5f).SetEase(Ease.InOutSine);
                        carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.DOScale(new Vector3(.7f, .7f, .7f), .5f).SetEase(Ease.InOutSine);
                    });
                });
                yield return new WaitForSeconds(1.3f);
                carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i].transform.parent = bigDryer_ref.targetPos;
                bigDryer_ref.cloths_ref.Add(carriedBasket_cloth.cloths[carriedBasket_cloth.cloths.Count - i]);
            }
            bigDryer_ref.StartProgress(Random.Range(30, 40));
            yield return new WaitForSeconds(.5f);
            carriedBasket_cloth.cloths = null;
            yield return new WaitUntil(() => !bigDryer_ref.running);
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(PullOutClothFromBigDryer(bigDryer_ref));
            yield return new WaitUntil(() => !movingCloths);
        }
        yield return null;
        movingCloths = false;
        currentStation.isBusy = false;
    }
    public IEnumerator PullOutClothFromBigDryer(BigDryerIntreactable bigDryer)
    {
        yield return null;
        if (carriedBasket_cloth != null)
        {
            //  carriedBasket_cloth.step = ClothStep.Folding;
            carriedBasket_cloth.cloths = new List<ClothScript>();
            Debug.Log("Closth basket is not null"); int i2 = 0;
            for (int i = 0; i < bigDryer.cloths_ref.Count; i++)
            {
                Debug.Log("Closth basket is not null  Loop");
                bigDryer.cloths_ref[i].transform.DOMove(bigDryer.front_pos.position, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    Debug.Log("Reached MAchine Top ");
                    Vector3 pos = carriedBasket_cloth.cloths_pos[i2].transform.position;
                    pos.y += .5f;

                    bigDryer.cloths_ref[i].transform.parent = null;
                    bigDryer.cloths_ref[i].anim.Play("ShirtOpen");
                    bigDryer.cloths_ref[i].transform.DOMove(pos, .5f).SetEase(Ease.InOutSine).OnComplete(() =>
                    {
                        Debug.Log("Reached Basket Top ");
                        bigDryer.cloths_ref[i].transform.DOMove(carriedBasket_cloth.cloths_pos[i2].position, .5f).
                        SetEase(Ease.InOutSine);
                        bigDryer.cloths_ref[i].transform.DOScale(Vector3.one, .5f).SetEase(Ease.InOutSine);
                    });
                });
                yield return new WaitForSeconds(1.3f);


                bigDryer.cloths_ref[i].transform.parent = carriedBasket_cloth.cloths_pos[i2];
                carriedBasket_cloth.cloths.Add(bigDryer.cloths_ref[i]);
                i2++;
            }
            bigDryer.bigdryer_door.transform.DOLocalRotate(Vector3.zero, .3f).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(.4f);
            bigDryer.cloths_ref = null;
            bigDryer.cloths_ref = new List<ClothScript>();
            bigDryer.bigDryer_in_use = false;
            bigDryer.pullout_clothes = false;
            bigDryer.currentBasketId = 0;
            bigDryer.usingByAi = false;
            movingCloths = false;
        }
    }
    #endregion

    #region - Pop Up Message - 
    [Space]
    [Header(" --- Pop Ref ---- ")]
    public CanvasGroup canvasGroup; // Add CanvasGroup on root for fading
    public TextMeshProUGUI popupText;
    [SerializeField] Transform pop_up_bg;
    private Sequence popupSequence;

    public void ShowPopup(string message, float duration = 8f)
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        pop_up_bg.localScale = Vector3.zero;

        if (popupSequence != null) popupSequence.Kill();

        popupText.text = message;
        canvasGroup.alpha = 0f;
        pop_up_bg.localScale = Vector3.zero;
        animator.Play(ReturnRandomWaitingAanim());
        animator.applyRootMotion = true;
        popupSequence = DOTween.Sequence();
        popupSequence.Append(pop_up_bg.DOScale(1f, 0.25f).SetEase(Ease.OutBack)); // scale pop
        popupSequence.Join(canvasGroup.DOFade(1f, 0.25f)); // fade in
        popupSequence.AppendInterval(duration);
        popupSequence.Append(canvasGroup.DOFade(0f, 0.2f)); // fade out
        popupSequence.Join(pop_up_bg.DOScale(0f, 0.2f).SetEase(Ease.InBack)); // scale down
    }

    string ReturnRandomWaitingAanim()
    {
        float val = Random.Range(0f, 3f);
        if (val < 1)
            return "wait1";
        else if (val > 1 && val < 2)
            return "wait2";
        else
            return "wait3";
    }
    #endregion
}

