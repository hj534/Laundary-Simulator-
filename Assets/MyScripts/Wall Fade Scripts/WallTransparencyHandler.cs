using UnityEngine;
using System.Collections.Generic;

public class ViewObstructionFader : MonoBehaviour
{
    public Transform player;
    public LayerMask obstacleLayer;
    public float fadeAlpha = 0.3f;
    public float fadeSpeed = 5f;
    public float sphereRadius = 0.3f;

    private class FadingObject
    {
        public Renderer renderer;
        public Material[] originalMaterials;
        public Material[] fadingMaterials;
    }

    private List<FadingObject> fadingObjects = new List<FadingObject>();
    private HashSet<Renderer> currentlyBlocking = new HashSet<Renderer>();

    void LateUpdate()
    {
        currentlyBlocking.Clear();

        Vector3 camPos = transform.position;
        Vector3 playerPos = player.position;
        float distance = Vector3.Distance(camPos, playerPos);

        Ray ray = new Ray(camPos, (playerPos - camPos).normalized);
        RaycastHit[] hits = Physics.SphereCastAll(ray, sphereRadius, distance, obstacleLayer);

        foreach (var hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
            {
                // ?? Extra check: is this object actually blocking the view?
                if (IsInFrontOfPlayer(rend))
                {
                    if (!IsAlreadyFading(rend))
                        StartFading(rend);

                    currentlyBlocking.Add(rend);
                }
            }
        }

        for (int i = fadingObjects.Count - 1; i >= 0; i--)
        {
            FadingObject obj = fadingObjects[i];
            if (currentlyBlocking.Contains(obj.renderer))
            {
                FadeToAlpha(obj, fadeAlpha);
            }
            else
            {
                FadeToAlpha(obj, 1f);
                if (IsFullyOpaque(obj))
                {
                    RestoreOriginal(obj);
                    fadingObjects.RemoveAt(i);
                }
            }
        }
    }

    bool IsInFrontOfPlayer(Renderer rend)
    {
        Bounds bounds = rend.bounds;
        Vector3 playerDir = (player.position - transform.position).normalized;
        Vector3 toObj = bounds.center - transform.position;

        return Vector3.Dot(toObj.normalized, playerDir) > 0.5f; // object between cam and player
    }

    bool IsAlreadyFading(Renderer rend)
    {
        foreach (var obj in fadingObjects)
        {
            if (obj.renderer == rend)
                return true;
        }
        return false;
    }

    void StartFading(Renderer rend)
    {
        Material[] original = rend.materials;
        Material[] fading = new Material[original.Length];

        for (int i = 0; i < original.Length; i++)
        {
            fading[i] = new Material(original[i]);
            SetMaterialTransparent(fading[i]);
        }

        rend.materials = fading;
        rend.sharedMaterials = fading;

        fadingObjects.Add(new FadingObject
        {
            renderer = rend,
            originalMaterials = original,
            fadingMaterials = fading
        });
    }

    void FadeToAlpha(FadingObject obj, float targetAlpha)
    {
        foreach (Material mat in obj.fadingMaterials)
        {
            Color col = mat.color;
            float alpha = Mathf.Lerp(col.a, targetAlpha, Time.deltaTime * fadeSpeed);
            mat.color = new Color(col.r, col.g, col.b, alpha);

            if (mat.IsKeywordEnabled("_EMISSION") && mat.HasProperty("_EmissionColor"))
            {
                Color emission = mat.GetColor("_EmissionColor");
                float intensity = emission.maxColorComponent;
                Color baseColor = intensity > 0 ? emission / intensity : Color.black;

                float newIntensity = Mathf.Lerp(intensity, targetAlpha, Time.deltaTime * fadeSpeed);
                Color fadedEmission = baseColor * newIntensity;
                mat.SetColor("_EmissionColor", fadedEmission);
            }
        }
    }

    bool IsFullyOpaque(FadingObject obj)
    {
        foreach (Material mat in obj.fadingMaterials)
        {
            if (mat.color.a < 0.98f)
                return false;
        }
        return true;
    }

    void RestoreOriginal(FadingObject obj)
    {
        obj.renderer.materials = obj.originalMaterials;
    }

    void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }
}
