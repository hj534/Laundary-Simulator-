using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Animations.Rigging;
using Unity.VisualScripting;
using UnityEngine.UI;

public class FullServiceCustomer : MonoBehaviour
{
    public string character_name;
    public NavMeshAgent agent;
    public Animator animator;
    //public int priceReward = 200; // not going to be used anymore
    [SerializeField]
    Rig[] handRigs;
    [Header("Assigned by Manager")]
    public Transform assignedSpot;
    public int orderID;
    public int orderWeight;

    [Header("Basket")]
    public GameObject basketPrefab; // Prefab for customer's basket
    private GameObject carriedBasket; // Basket in customer's hand

    public Transform basketHoldPoint, box_hold_pos;   // Where customer holds basket
    public Transform counterDropPoint;  // Where basket is placed on counter

    public bool basketPlaced = false, _orderPicked, placedBasket, waiting;
    public ParticleSystem angryReact;

    public bool IsRetrying { get; private set; } = false;
    public GameObject canvas;
    public Text orderId_Text;
    void OnEnable()
    {
        if (placedBasket)
            return;
        basketPlaced = false;
        StartCoroutine(SpawnWithBasket());
        StartCoroutine(WalkToAssignedSpot());
        Invoke(nameof(showorderid), 1f);
    }
    void showorderid()
    {
        orderId_Text.text = $"Ticket ID: {orderID}";
    }
    public void MoveToCounterSpot(Vector3 targetPosition)
    {
        agent.SetDestination(targetPosition);
        // animator.SetBool("isWalking", true);
    }

    private IEnumerator SpawnWithBasket()
    {
        yield return new WaitForSeconds(.8f);
        if (!placedBasket)
        {

            if (carriedBasket != null)
            {
                Destroy(carriedBasket); // Clean up any old basket
            }
            // Create a basket in customer's hand
            carriedBasket = Instantiate(basketPrefab, basketHoldPoint.position, basketHoldPoint.rotation, basketHoldPoint);
            carriedBasket.transform.SetParent(basketHoldPoint);

            carriedBasket.GetComponent<ClothBasket>().id = this.orderID;
            carriedBasket.GetComponent<ClothBasket>().cloths_weight = this.orderWeight;
            animator.SetBool("Hold", true);
            EnableDisableRig(1);
        }

    }

    private IEnumerator WalkToAssignedSpot()
    {
        yield return new WaitForSeconds(1.2f);
        if (assignedSpot != null)
            agent.SetDestination(assignedSpot.position);
        //  animator.SetBool("isWalking", true);
    }

    void Update()
    {
        if (agent.remainingDistance <= agent.stoppingDistance && !basketPlaced)
        {
            //   animator.SetBool("isWalking", false);

            // Check if this customer is at the FIRST spot in line
            if (CounterManager.Instance.IsCustomerFirstInLine(this)
                && Vector3.Distance(transform.position, CounterManager.Instance.standingSpots[0].position.position) < 1f)
            {
                StartCoroutine(PlaceBasketAndWait());
            }
        }
        // Calculate speed of agent relative to max speed
        float speedPercent = agent.velocity.magnitude / agent.speed;

        // Set Speed parameter for blend tree
        animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);
    }
    void EnableDisableRig(int weightValue)
    {
        for (int i = 0; i < handRigs.Length; i++)
        {
            handRigs[i].weight = weightValue;
        }
    }
    private IEnumerator PlaceBasketAndWait()
    {
        basketPlaced = true;

        Debug.Log($" Customer {orderID} placing basket on counter.");
        //  animator.SetTrigger("Greet");

        yield return new WaitForSeconds(1.5f); // Optional greeting animation
        carriedBasket.GetComponent<ClothBasket>().rb.useGravity = false;
        carriedBasket.GetComponent<ClothBasket>().rb.isKinematic = true;
        animator.SetBool("Hold", false);
        EnableDisableRig(0);
        // Place basket on counter
        carriedBasket.transform.SetParent(null);
        carriedBasket.transform.DOScale(Vector3.one, .5f).SetEase(Ease.InOutSine);
        carriedBasket.transform.position = CounterManager.Instance.basket_pos.position;//counterDropPoint.position;
        carriedBasket.transform.rotation = CounterManager.Instance.basket_pos.rotation;//counterDropPoint.rotation;

        // Tell LaundryOrderManager about new order
        LaundryOrderManager.Instance.CreateOrder(orderID, carriedBasket);

        Debug.Log($" Customer {orderID} placed basket. Waiting for player to pick it up.");
        yield return new WaitForSeconds(.3f);
        carriedBasket.GetComponent<ClothBasket>().id = orderID;
        carriedBasket.GetComponent<ClothBasket>().picked_by_AI = false;
        carriedBasket.GetComponent<ClothBasket>().place_onCounter = true;
        // Wait until player picks up basket
        yield return new WaitUntil(() => carriedBasket == null);

        Debug.Log($" Basket picked up. Customer {orderID} leaving.");

        LeaveCounter();


    }

    public void OnBasketPickedUp()
    {
        // Called when player picks basket
        carriedBasket = null;
    }

    private void LeaveCounter()
    {
        placedBasket = true;
        waiting = true;
        // Go to wait point outside
        Vector3 waitPoint = FullServiceCustomerManager.Instance.GetRandomWaitPoint();
        agent.SetDestination(waitPoint);
        //      animator.SetBool("isWalking", true);

        // Free counter spot
        CounterManager.Instance.FreeSpot(assignedSpot);
        assignedSpot = null;

        // Shift queue forward
        CounterManager.Instance.ShiftQueueForward();

        // Start waiting before coming back to check
        StartCoroutine(WaitBeforeReturnRoutine());
    }

    private IEnumerator WaitBeforeReturnRoutine()
    {
        // Go to wait point
        yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);
        //  yield return new WaitUntil(() => Vector3.Distance(transform.position , agent.destination) < .1f);
        //    animator.SetBool("isWalking", false);

        while (true && !_orderPicked) // Keep checking until order is picked up
        {
            // Wait random time before coming back
            float waitTime = Random.Range(50f, 110f);
            Debug.Log($"? Customer {orderID} will return in {waitTime} seconds to check.");
            yield return new WaitForSeconds(waitTime);
            Debug.Log("While True Running !!!!  ");
            TryReturnForPickup();
            // Wait until customer leaves or retries
            yield return new WaitUntil(() => isAtCounter == false);
        }
    }

    bool shouldKillCustomer()
    {
        bool matchingclothbaseket = false;
        ClothBasket[] clothbasket = FindObjectsOfType<ClothBasket>();
        for (int i = 0; i < clothbasket.Length; i++)
        {
            if (orderID == clothbasket[i].id)
            {
                matchingclothbaseket = true;
                break;
            }
        }
        GameObject basket = LaundryOrderManager.Instance.FindBasketByOrderID(orderID);

        return basket == null && !matchingclothbaseket;
    }
    IEnumerator GetOrder()
    {
        yield return new WaitForSeconds(2);

        if (shouldKillCustomer())
        {
            Debug.Log($"Killinmg Customer {orderID} as Basket is null");
            gameObject.SetActive(false);
            ResetAI();
            yield break;
        }

        if (waiting)
        {
            waitpoint = FullServiceCustomerManager.Instance.GetRandomWaitPoint();
            agent.SetDestination(waitpoint);
        }
        while (true && !_orderPicked) // Keep checking until order is picked up
        {
            // Wait random time before coming back
            float waitTime = Random.Range(50f, 110f);
            Debug.Log($"? Customer {orderID} will return in {waitTime} seconds to check.");
            yield return new WaitForSeconds(waitTime);
            Debug.Log("While True Running !!!!  ");
            TryReturnForPickup();

            // Wait until customer leaves or retries
            yield return new WaitUntil(() => isAtCounter == false);
        }
    }
    private void TryReturnForPickup()
    {
        assignedSpot = PickUpCounterIntreactable.instance.GetFreeSpot();
        if (assignedSpot != null)
        {
            Debug.Log("Assigned Spot is not equal to null ");
            // Mark this spot as occupied and assign customer
            foreach (var spot in PickUpCounterIntreactable.instance.standingSpots)
            {
                if (spot.position == assignedSpot)
                {
                    spot.isOccupied = true;
                    spot.currentCustomer = this;
                    break;
                }
            }
        }
        if (assignedSpot == null)
            TryReturnForPickup();
        agent.SetDestination(assignedSpot.position);
        //    animator.SetBool("isWalking", true);

        StartCoroutine(PickupBasketRoutine());
    }
    public void MoveToPickupSpot(Transform newSpot)
    {
        assignedSpot = newSpot;
        agent.SetDestination(newSpot.position);
        //   animator.SetBool("isWalking", true);

        StartCoroutine(CheckIfFirstInLine());
    }
    private IEnumerator CheckIfFirstInLine()
    {
        yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);
        //  animator.SetBool("isWalking", false);

        // Check if this is first customer in line
        bool isFirstInLine = PickUpCounterIntreactable.instance.standingSpots[0].currentCustomer == this;
        if (isFirstInLine)
        {
            StartCoroutine(PickupBasketRoutine());
        }
        else
        {
            Debug.Log($"? Customer {orderID} waiting for their turn.");
        }
    }
    [SerializeField] bool isAtCounter = false;

    public bool CanPickOrder;
    [SerializeField] float waitTimeToPickOrder = 15f;
    private IEnumerator PickupBasketRoutine() // alpha
    {
        //  isAtCounter = true;
        yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);
        //  yield return new WaitUntil(() => Vector3.Distance(transform.position, assignedSpot.position) < .1f);
        Debug.Log($"?? Customer {orderID} checking rack for their basket.");
        yield return null;
        GameObject basket = LaundryOrderManager.Instance.FindBasketByOrderID(orderID);

        if (basket != null && LaundryOrderManager.Instance.IsOrderComplete(orderID))
        {
            if (assignedSpot != null)
                yield return new WaitUntil(() => Vector3.Distance(transform.position, assignedSpot.position) < .5f);
            // ? Order ready
            agent.isStopped = true;
            float elapsedTime = 0f;
            while (!CanPickOrder)
            {
                yield return null;
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= waitTimeToPickOrder && !CanPickOrder)
                {
                    Debug.Log($"?? Customer {orderID} waited too long to pick up the order and is now leaving.");
                    agent.isStopped = false;
                    PickUpCounterIntreactable.instance.FreeSpot(assignedSpot);
                    assignedSpot = null;
                    PickUpCounterIntreactable.instance.ShiftQueueForward();
                    ExitLaundromat(false);
                    yield break;
                }
            }

            basket.GetComponent<PackingBoxIntreactable>().dont_save = true;
            _orderPicked = true;
            animator.SetBool("Hold", true);
            EnableDisableRig(1);
            basket.transform.SetParent(box_hold_pos);
            //  basket.transform.SetParent(box_hold_pos);
            basket.transform.DOLocalMove(Vector3.zero, .3f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                Debug.Log($"? Customer {orderID} picked up basket.");
                RackManager.Instance.FreeSpot(basket.transform);
                LaundryOrderManager.Instance.RemoveOrder(orderID, basket.GetComponent<PackingBoxIntreactable>().counter_pos_index);

                // Leave happily
                //  yield return new WaitForSeconds(.5f);

                agent.isStopped = false;
                PickUpCounterIntreactable.instance.FreeSpot(assignedSpot);
                assignedSpot = null;
                PickUpCounterIntreactable.instance.ShiftQueueForward();
                ExitLaundromat(true);
            });

        }
        else
        {
            if (gameObject.activeInHierarchy && assignedSpot != null)
                yield return new WaitUntil(() => Vector3.Distance(transform.position, assignedSpot.position) < .5f);
            yield return new WaitForSeconds(.5f);
            // ? Order not ready
            Debug.Log($"?? Customer {orderID} angry: order not ready.");
            agent.isStopped = true;
            animator.Play("Angry");
            // Wait until animator enters the state (important if you trigger transition)
            yield return new WaitForSeconds(1);

            // Get current animation clip length from Animator
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 = base layer
            float animationLength = stateInfo.length;
            angryReact.Play();
            Debug.Log("Animation Length: " + animationLength + " seconds");
            // Wait for animation to finish
            yield return new WaitForSeconds(animationLength);
            // Move back to wait point
            agent.isStopped = false;
            //  animator.SetTrigger("isAngry");

            agent.isStopped = false;
            PickUpCounterIntreactable.instance.FreeSpot(assignedSpot);
            assignedSpot = null;
            PickUpCounterIntreactable.instance.ShiftQueueForward();
            ExitLaundromat(false);
        }
        // isAtCounter = false;
    }

    [SerializeField] Vector3 waitpoint;

    bool paymentDone = false;
    private void ExitLaundromat(bool orderPicked)
    {
        // animator.SetBool("isWalking", true);
        if (_orderPicked)
        {
            waiting = false;
            agent.SetDestination(FullServiceCustomerManager.Instance.exitPoint.position);
            EnableDisableRig(1);

            if (!paymentDone)  // temporary fix will fix core logi later
            {
                paymentDone = true;
                Debug.Log($"daily full service updated-> Orders : {PlayerPrefs.GetInt(PlayerPrefsHolder.FS_DailyOrders) + 1} Earnings : {PlayerPrefs.GetInt(PlayerPrefsHolder.FS_DailyEarnings) + (orderWeight * PlayerPrefsHolder.PricePerPound)} ");
                PlayerPrefs.SetInt(PlayerPrefsHolder.FS_DailyOrders, PlayerPrefs.GetInt(PlayerPrefsHolder.FS_DailyOrders) + 1);
                PlayerPrefs.SetInt(PlayerPrefsHolder.FS_DailyEarnings, PlayerPrefs.GetInt(PlayerPrefsHolder.FS_DailyEarnings) + (orderWeight * PlayerPrefsHolder.PricePerPound));
                ShopManager.instance.AddingCoinsEffectUI(orderWeight * PlayerPrefsHolder.PricePerPound);
            }

        }
        else
        {
            waitpoint = FullServiceCustomerManager.Instance.GetRandomWaitPoint();
            agent.SetDestination(waitpoint);
            Debug.Log("Exit " + waitpoint);
        }
        StartCoroutine(DisableAfterExit(_orderPicked));
    }
    void ResetAI()
    {
        basketPlaced = false;
        // _orderPicked = false;
        isAtCounter = false;
        Destroy(basket_box);
        waitpoint = Vector3.zero;
        if (box_hold_pos.childCount > 0)
        {
            GameObject obj = box_hold_pos.transform.GetChild(0).gameObject;
            Destroy(obj);
        }
        _orderPicked = false;
        placedBasket = false;
        waiting = false;
        CanPickOrder = false;
    }
    private IEnumerator DisableAfterExit(bool orderPicked)
    {
        Debug.Log("Wait Point Before wait ");
        yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);
        if (orderPicked)
        {
            yield return new WaitUntil(() => Vector3.Distance(transform.position, FullServiceCustomerManager.Instance.exitPoint.position) < .5f);
            // ? Fully done: disable object
            gameObject.SetActive(false);
            ResetAI();

            // Free their counter pickup spot
            // PickUpCounterIntreactable.instance.FreeSpot(assignedSpot);
            // assignedSpot = null;
            // // Shift queue forward
            // PickUpCounterIntreactable.instance.ShiftQueueForward();
        }
        // else if(TimeHandler.Instance.stop_timer)
        // {
        //     yield return new WaitUntil(() => Vector3.Distance(transform.position, waitpoint) < .5f);
        //     // ? Order not picked: disable object
        //     // gameObject.SetActive(false);
        //     // ResetAI();
        // }
        else
        {
            Debug.Log("Wait Point Before wait " + waitpoint);
            //  yield return new WaitUntil(() => Vector3.Distance(transform.position, waitpoint) < .1f);
            // ? Order not picked: retry logic
            StartCoroutine(WaitBeforeRetrying());
        }


        // Free their spot in the line
        PickUpCounterIntreactable.instance.FreeSpot(assignedSpot);
        assignedSpot = null;
        yield return new WaitForSeconds(.3f);
        // Shift queue forward
        PickUpCounterIntreactable.instance.ShiftQueueForward();
    }

    private IEnumerator WaitForTurnAtPickupCounter()
    {
        // Customer waits at their pickup spot
        yield return new WaitUntil(() =>
            PickUpCounterIntreactable.instance.standingSpots[0].currentCustomer == this);

        // Move to counter when it's this customer's turn
        agent.SetDestination(PickUpCounterIntreactable.instance.standingSpots[0].position.position);
        //  animator.SetBool("isWalking", true);

        // Wait until they reach counter
        yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);
        //  animator.SetBool("isWalking", false);

        Debug.Log($"?? Customer {orderID} reached counter to check basket.");

        StartCoroutine(CheckForBasketAtCounter());
    }
    [SerializeField] GameObject basket_box;
    private IEnumerator CheckForBasketAtCounter() //beta
    {
        /*GameObject*/
        basket_box = LaundryOrderManager.Instance.FindBasketByOrderID(orderID);
        if (basket_box == null)
            Debug.Log("Basket is nullll    ");
        if (basket_box != null && LaundryOrderManager.Instance.IsOrderComplete(orderID))
        {
            yield return new WaitUntil(() => Vector3.Distance(transform.position,
                PickUpCounterIntreactable.instance.standingSpots[0].position.position) < .5f);

            /* agent.isStopped = true;
             animator.Play("Happy");
             yield return new WaitForSeconds(3);

             // Wait until animator enters the state (important if you trigger transition)
             yield return new WaitForSeconds(1);

             // Get current animation clip length from Animator
             AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 = base layer
             float animationLength = stateInfo.length;

             Debug.Log("Animation Length: " + animationLength + " seconds");
             // Wait for animation to finish
             yield return new WaitForSeconds(animationLength);
             // Move back to wait point
             agent.isStopped = false;*/



            // ? Basket ready

            agent.isStopped = true;
            float elapsedTime = 0f;
            while (!CanPickOrder)
            {
                yield return null;
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= waitTimeToPickOrder && !CanPickOrder)
                {
                    Debug.Log($"?? Customer {orderID} waited too long to pick up the order and is now leaving.");
                    //agent.isStopped = false;
                    agent.isStopped = false;
                    PickUpCounterIntreactable.instance.FreeSpot(assignedSpot);
                    assignedSpot = null;
                    PickUpCounterIntreactable.instance.ShiftQueueForward();
                    ExitLaundromat(false);
                    yield break;
                }
            }
            _orderPicked = true;
            basket_box.transform.SetParent(box_hold_pos);
            basket_box.transform.DOLocalMove(Vector3.zero, .3f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                Debug.Log($"? Customer {orderID} picked up basket.");
                RackManager.Instance.FreeSpot(basket_box.transform);
                LaundryOrderManager.Instance.RemoveOrder(orderID, basket_box.GetComponent<PackingBoxIntreactable>().counter_pos_index);

                agent.isStopped = false;
                PickUpCounterIntreactable.instance.FreeSpot(assignedSpot);
                assignedSpot = null;
                PickUpCounterIntreactable.instance.ShiftQueueForward();
                ExitLaundromat(true);
            });
            basket_box.transform.DOLocalRotate(Vector3.zero, .3f).SetEase(Ease.InOutSine);

        }
        else
        {
            // ? Basket not ready
            Debug.Log($"?? Customer {orderID} angry: order not ready.");
            //  animator.SetTrigger("isAngry");
            agent.isStopped = true;
            animator.Play("Angry");
            // Wait until animator enters the state (important if you trigger transition)
            yield return new WaitForSeconds(1);

            // Get current animation clip length from Animator
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 = base layer
            float animationLength = stateInfo.length;
            angryReact.Play();
            Debug.Log("Animation Length: " + animationLength + " seconds");
            // Wait for animation to finish
            yield return new WaitForSeconds(animationLength);
            // Move back to wait point
            agent.isStopped = false;
            StartCoroutine(ReturnToWaitPoint());
        }
    }
    private IEnumerator ReturnToWaitPoint()
    {
        Vector3 waitPoint = FullServiceCustomerManager.Instance.GetRandomWaitPoint();
        agent.SetDestination(waitPoint);
        //   animator.SetBool("isWalking", true);
        Debug.Log("Wait before Trying again  !!!");
        yield return new WaitUntil(() => Vector3.Distance(transform.position, waitPoint) < .2f);

        StartCoroutine(WaitBeforeRetrying());
    }

    private IEnumerator WaitBeforeRetrying()
    {
        yield return new WaitForSeconds(.1f);
        //   yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);
        //    animator.SetBool("isWalking", false);

        // Wait random time before retrying
        float waitTime = Random.Range(100f, 150f);
        Debug.Log($"? Customer {orderID} will retry in {waitTime} seconds.");
        yield return new WaitForSeconds(waitTime);

        // Try returning again
        StartCoroutine(WaitForTurnAtPickupCounter());
    }

    public void ForceLeaveLaundromat()
    {
        if (_orderPicked)
            return; // already done, do nothing

        Debug.Log($"⏰ Customer {orderID} forced to leave due to timer.");

        // Stop any pickup attempt
        StopAllCoroutines();

        // Free pickup spot if occupied
        if (assignedSpot != null)
        {
            PickUpCounterIntreactable.instance.FreeSpot(assignedSpot);
            assignedSpot = null;
            //PickUpCounterIntreactable.instance.ShiftQueueForward();
        }

        // Use EXISTING exit flow (important)
        ExitLaundromat(false);
    }

    public FullServiceCustomerData GetData()
    {
        return new FullServiceCustomerData
        {
            character_name = this.character_name,
            orderID = this.orderID,
            orderWeight = this.orderWeight,
            position = new float[] { transform.position.x, transform.position.y, transform.position.z },
            basketPlaced = this.basketPlaced,
            orderPicked = this._orderPicked,
            placedBasket = this.placedBasket,
            waiting = this.waiting
        };
    }

    public void InitializeFromData(FullServiceCustomerData data)
    {
        this.character_name = data.character_name;
        this.orderID = data.orderID;
        this.orderWeight = data.orderWeight;
        this.basketPlaced = data.basketPlaced;
        this._orderPicked = data.orderPicked;
        this.placedBasket = data.placedBasket;
        this.waiting = data.waiting;

        transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);
        /* GameObject basket = LaundryOrderManager.Instance.FindBasketByOrderID(orderID);
         if (basket != null)
         {
             // Re-link the basket if needed
             carriedBasket = basket;
         }*/

        // Optionally reassign nav destination or any other behavior
        agent.Warp(transform.position); // Snap position
        StartCoroutine(GetOrder());
    }

}
