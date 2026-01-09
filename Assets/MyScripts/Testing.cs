using UnityEngine;

public class Testing : MonoBehaviour
{
    public Transform[] clothBones; // Add your rig bones here
    public float springForce = 5f;
    public float damping = 2f;
    public Vector3 gravity = new Vector3(0, -9.8f, 0);

    private Vector3[] velocities;

    void Start()
    {
        velocities = new Vector3[clothBones.Length];
    }

    void LateUpdate()
    {
        for (int i = 0; i < clothBones.Length; i++)
        {
            Transform bone = clothBones[i];

            Vector3 targetPos = bone.position + bone.forward * 0.1f; // small offset
            Vector3 force = gravity - velocities[i] * damping;

            velocities[i] += force * Time.deltaTime;
            bone.position += velocities[i] * Time.deltaTime;

            // optional: constrain bone rotation to avoid too much twisting
            bone.localRotation = Quaternion.Lerp(bone.localRotation, Quaternion.identity, 0.1f);
        }
    }
}
