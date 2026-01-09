using UnityEngine;

public class PromptFollowTarget : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, .5f, .5f); // how high above object

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;
        transform.forward = cam.transform.forward;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
