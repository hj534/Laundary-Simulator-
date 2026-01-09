using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestRoomDoor : MonoBehaviour
{
    public Vector3 openRotation = new Vector3(0, 90, 0); // How much to rotate when opening
    public float openSpeed = 2f;
    public bool autoClose = true;
    public float autoCloseDelay = 3f;
    public GameObject door;

    private Quaternion closedRotation;
    private Quaternion targetRotation;
    private bool isOpen = false;
    private bool playerInside = false;
    private float closeTimer;

    void Start()
    {
        closedRotation = door.transform.rotation;
        targetRotation = closedRotation;
    }

    void Update()
    {
        door.transform.rotation = Quaternion.Lerp(door.transform.rotation, targetRotation, Time.deltaTime * openSpeed);

        if (autoClose && isOpen && !playerInside)
        {
            closeTimer -= Time.deltaTime;
            if (closeTimer <= 0f)
                CloseDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            OpenDoor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            closeTimer = autoCloseDelay;
        }
    }

    void OpenDoor()
    {
        targetRotation = Quaternion.Euler(closedRotation.eulerAngles + openRotation);
        isOpen = true;
    }

    void CloseDoor()
    {
        targetRotation = closedRotation;
        isOpen = false;
    }
}
