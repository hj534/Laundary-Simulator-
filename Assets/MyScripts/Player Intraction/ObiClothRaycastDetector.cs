using UnityEngine;
using Obi;
using System.Collections.Generic;
using System.Collections;

public class ObiClothRaycastDetector : MonoBehaviour
{
    public Camera cam;
    public float detectionRadius = 0.15f;
    public Transform rightHandAttachPoint;
    public float pickupMoveSpeed = 5f;
    public float dropMoveSpeed = 5f;
    public Transform targetDropPoint; // Dynamic drop target

    private ObiCloth pickedCloth = null;
    private ObiClothRenderer pickedRenderer = null;
    private ObiSolver pickedSolver = null;
    private bool isPickingUp = false;

    private List<ObiCloth> allCloths = new List<ObiCloth>();

    void Start()
    {
        ObiCloth[] clothsInScene = FindObjectsOfType<ObiCloth>();
        allCloths.AddRange(clothsInScene);
        Debug.Log("Found " + allCloths.Count + " ObiCloth objects.");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click to pick/drop
        {
            if (pickedCloth == null && !isPickingUp)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                ObiCloth cloth = DetectCloth(ray);

                if (cloth != null)
                    StartCoroutine(PickUpClothSmooth(cloth));
            }
            else if (pickedCloth != null && !isPickingUp)
            {
                StartCoroutine(DropClothSmooth());
            }
        }
    }

    ObiCloth DetectCloth(Ray ray)
    {
        foreach (ObiCloth cloth in allCloths)
        {
            ObiSolver solver = cloth.solver;

            for (int i = 0; i < solver.activeParticleCount; i++)
            {
                Vector3 solverSpacePos = solver.positions[i];
                Vector3 worldPos = solver.transform.TransformPoint(solverSpacePos);

                float distanceToRay = Vector3.Cross(ray.direction, worldPos - ray.origin).magnitude;

                if (distanceToRay <= detectionRadius)
                {
                    Debug.Log("Detected cloth: " + cloth.name);
                    return cloth;
                }
            }
        }
        return null;
    }

    IEnumerator PickUpClothSmooth(ObiCloth cloth)
    {
        isPickingUp = true;

        pickedCloth = cloth;
        pickedRenderer = cloth.GetComponent<ObiClothRenderer>();
        pickedSolver = cloth.solver;

        // ?? Disable cloth simulation temporarily
        pickedCloth.enabled = false;
        yield return null;

        // Smooth move to hand
        yield return MoveSolver(pickedSolver.transform, rightHandAttachPoint.position, rightHandAttachPoint.rotation, pickupMoveSpeed);

        pickedSolver.transform.SetParent(rightHandAttachPoint, true);
        pickedSolver.transform.localPosition = Vector3.zero;
        pickedSolver.transform.localRotation = Quaternion.identity;

        // ? Reactivate cloth simulation
        pickedCloth.enabled = true;

        Debug.Log("Picked up cloth: " + pickedCloth.name);

        if (targetDropPoint != null)
            yield return DropToTarget(targetDropPoint);

        isPickingUp = false;
    }

    IEnumerator DropClothSmooth()
    {
        isPickingUp = true;

        if (targetDropPoint != null)
            yield return DropToTarget(targetDropPoint);

        isPickingUp = false;
    }

    IEnumerator DropToTarget(Transform dropTarget)
    {
        // ?? Disable simulation during movement
        pickedCloth.enabled = false;
        yield return null;

        // Smooth move to drop target
        yield return MoveSolver(pickedSolver.transform, dropTarget.position, dropTarget.rotation, dropMoveSpeed);

        pickedSolver.transform.SetParent(dropTarget, true);
        pickedSolver.transform.localPosition = Vector3.zero;
        pickedSolver.transform.localRotation = Quaternion.identity;

        // ? Reactivate simulation
        pickedCloth.enabled = true;

        Debug.Log("Dropped cloth at: " + dropTarget.name);

        pickedCloth = null;
        pickedSolver = null;
        pickedRenderer = null;
    }

    IEnumerator MoveSolver(Transform solverTransform, Vector3 targetPos, Quaternion targetRot, float speed)
    {
        float t = 0;
        Vector3 startPos = solverTransform.position;
        Quaternion startRot = solverTransform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            solverTransform.position = Vector3.Lerp(startPos, targetPos, t);
            solverTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
    }
}
