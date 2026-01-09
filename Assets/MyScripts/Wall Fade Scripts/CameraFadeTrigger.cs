using UnityEngine;
using System.Collections.Generic;

public class CameraFadeTrigger : MonoBehaviour
{
    /* public Transform player;
     public float fadeAlpha = 0.3f;
     public float fadeSpeed = 5f;

     private HashSet<Renderer> fadingRenderers = new HashSet<Renderer>();
     private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
     public LayerMask fadeableLayer;
     *//* private void OnTriggerStay(Collider other)
      {
          if (((1 << other.gameObject.layer) & fadeableLayer) == 0) return;

          Renderer rend = other.GetComponentInChildren<Renderer>();
          if (rend == null) return;

          Vector3 toObject = rend.bounds.center - transform.position;
          Vector3 toPlayer = player.position - transform.position;

          // New: Only fade if object is between camera and player
          if (Vector3.Dot(toObject.normalized, toPlayer.normalized) < 0.1f)
          {
              if (!originalMaterials.ContainsKey(rend))
              {
                  Material[] original = rend.materials;
                  Material[] fading = new Material[original.Length];

                  for (int i = 0; i < original.Length; i++)
                  {
                      fading[i] = new Material(original[i]);
                      SetMaterialTransparent(fading[i]);
                  }

                  rend.materials = fading;
                  originalMaterials[rend] = original;
              }

              fadingRenderers.Add(rend);
              FadeToAlpha(rend, fadeAlpha);
          }
      }*//*
     private void OnTriggerStay(Collider other)
     {
         if (((1 << other.gameObject.layer) & fadeableLayer) == 0) return;

         Renderer rend = other.GetComponentInChildren<Renderer>();
         if (rend == null) return;

         Vector3 cameraToPlayer = player.position - transform.position;
         Vector3 cameraToObject = rend.bounds.center - transform.position;

         float playerDistance = cameraToPlayer.magnitude;
         float objectDistance = cameraToObject.magnitude;

         // Player must be behind object from camera’s point of view
         if (objectDistance < playerDistance - 0.2f) // -0.2f buffer to avoid flicker
         {
             if (!originalMaterials.ContainsKey(rend))
             {
                 Material[] original = rend.materials;
                 Material[] fading = new Material[original.Length];

                 for (int i = 0; i < original.Length; i++)
                 {
                     fading[i] = new Material(original[i]);
                     SetMaterialTransparent(fading[i]);
                 }

                 rend.materials = fading;
                 originalMaterials[rend] = original;
             }

             fadingRenderers.Add(rend);
             FadeToAlpha(rend, fadeAlpha);
         }
     }



     private void OnTriggerExit(Collider other)
     {
         Renderer rend = other.GetComponentInChildren<Renderer>();
         if (rend == null) return;

         if (originalMaterials.ContainsKey(rend))
         {
             rend.materials = originalMaterials[rend];
             originalMaterials.Remove(rend);
             fadingRenderers.Remove(rend);
         }
     }

     private void Update()
     {
         List<Renderer> toRestore = new List<Renderer>();

         foreach (Renderer rend in fadingRenderers)
         {
             if (rend == null) continue;

             FadeToAlpha(rend, fadeAlpha);

             if (!IsRendererInsideTrigger(rend))
             {
                 toRestore.Add(rend);
             }
         }

         foreach (Renderer rend in toRestore)
         {
             if (originalMaterials.ContainsKey(rend))
             {
                 rend.materials = originalMaterials[rend];
                 originalMaterials.Remove(rend);
                 fadingRenderers.Remove(rend);
             }
         }
     }

     bool IsRendererInsideTrigger(Renderer rend)
     {
         Collider trigger = GetComponentInChildren<BoxCollider>();
         if (!trigger || !trigger.isTrigger) return false;

         return trigger.bounds.Intersects(rend.bounds);
     }

     void FadeToAlpha(Renderer rend, float targetAlpha)
     {
         foreach (Material mat in rend.materials)
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
     }*/
    //Sahi wall code ha ye bss light ky opacity low ni kr rha 
    /*  public Transform player;
      public LayerMask fadeableLayer;
      public float fadeAlpha = 0.3f;
      public float fadeSpeed = 5f;

      private Dictionary<Renderer, Material[]> originalMaterials = new();
      private List<Renderer> currentHits = new();

      void Update()
      {
          Vector3 dir = player.position - transform.position;
          float distance = dir.magnitude;

          RaycastHit[] hits = Physics.RaycastAll(transform.position, dir.normalized, distance, fadeableLayer);

          HashSet<Renderer> newHits = new();

          foreach (var hit in hits)
          {
              Renderer rend = hit.collider.GetComponentInChildren<Renderer>();
              if (rend == null) continue;

              newHits.Add(rend);

              if (!originalMaterials.ContainsKey(rend))
              {
                  Material[] original = rend.materials;
                  Material[] fading = new Material[original.Length];

                  for (int i = 0; i < original.Length; i++)
                  {
                      fading[i] = new Material(original[i]);
                      SetTransparent(fading[i]);
                  }

                  rend.materials = fading;
                  originalMaterials[rend] = original;
              }

              Fade(rend, fadeAlpha);
          }

          // Restore objects no longer between player and camera
          foreach (Renderer rend in originalMaterials.Keys)
          {
              if (!newHits.Contains(rend))
                  Fade(rend, 1f);
          }

          currentHits.Clear();
          currentHits.AddRange(newHits);
      }

      void Fade(Renderer rend, float targetAlpha)
      {
          foreach (Material mat in rend.materials)
          {
              Color c = mat.color;
              c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
              mat.color = c;

              if (mat.IsKeywordEnabled("_EMISSION") && mat.HasProperty("_EmissionColor"))
              {
                  Color e = mat.GetColor("_EmissionColor");
                  float i = e.maxColorComponent;
                  Color baseColor = i > 0 ? e / i : Color.black;
                  float newI = Mathf.Lerp(i, targetAlpha, Time.deltaTime * fadeSpeed);
                  mat.SetColor("_EmissionColor", baseColor * newI);
              }
          }
      }

      void SetTransparent(Material mat)
      {
          mat.SetFloat("_Mode", 3);
          mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
          mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
          mat.SetInt("_ZWrite", 0);
          mat.DisableKeyword("_ALPHATEST_ON");
          mat.EnableKeyword("_ALPHABLEND_ON");
          mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
          mat.renderQueue = 3000;
      }*/
    // Sahi ha ye code b 
    /*   public Transform player;
       public LayerMask fadeableLayer;
       public float fadeAlpha = 0.3f;
       public float fadeSpeed = 5f;

       private Dictionary<Renderer, Material[]> originalMaterials = new();
       private List<Renderer> currentHits = new();

       void Update()
       {
           Vector3 dir = player.position - transform.position;
           float distance = dir.magnitude;

           RaycastHit[] hits = Physics.RaycastAll(transform.position, dir.normalized, distance, fadeableLayer);
           HashSet<Renderer> newHits = new();

           foreach (var hit in hits)
           {
               Transform root = hit.collider.GetComponentInParent<Transform>();
               if (root == null) continue;

               Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
               foreach (Renderer rend in renderers)
               {
                   newHits.Add(rend);

                   if (!originalMaterials.ContainsKey(rend))
                   {
                       Material[] original = rend.materials;
                       Material[] fading = new Material[original.Length];

                       for (int i = 0; i < original.Length; i++)
                       {
                           fading[i] = new Material(original[i]);
                           SetTransparent(fading[i]);
                       }

                       rend.materials = fading;
                       originalMaterials[rend] = original;
                   }

                   Fade(rend, fadeAlpha);
               }
           }

           // Restore materials that are no longer obstructing view
           foreach (var kvp in originalMaterials)
           {
               Renderer rend = kvp.Key;
               if (!newHits.Contains(rend))
               {
                   Fade(rend, 1f);
               }
           }

           currentHits.Clear();
           currentHits.AddRange(newHits);
       }

       void Fade(Renderer rend, float targetAlpha)
       {
           foreach (Material mat in rend.materials)
           {
               Color c = mat.color;
               c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
               mat.color = c;

               if (mat.IsKeywordEnabled("_EMISSION") && mat.HasProperty("_EmissionColor"))
               {
                   Color e = mat.GetColor("_EmissionColor");
                   float i = e.maxColorComponent;
                   Color baseColor = i > 0 ? e / i : Color.black;
                   float newI = Mathf.Lerp(i, targetAlpha, Time.deltaTime * fadeSpeed);
                   mat.SetColor("_EmissionColor", baseColor * newI);
               }
           }
       }

       void SetTransparent(Material mat)
       {
           mat.SetFloat("_Mode", 3);
           mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
           mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
           mat.SetInt("_ZWrite", 0);
           mat.DisableKeyword("_ALPHATEST_ON");
           mat.EnableKeyword("_ALPHABLEND_ON");
           mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
           mat.renderQueue = 3000;
       }*/

    public Transform player;
    public LayerMask fadeableLayer;
    public float fadeAlpha = 0.3f;
    public float fadeSpeed = 5f;

    private Dictionary<Renderer, Material[]> originalMaterials = new();
    private HashSet<Renderer> fadingRenderers = new();

    void Update()
    {
        Vector3 dir = player.position - transform.position;
        float distance = dir.magnitude;

        RaycastHit[] hits = Physics.RaycastAll(transform.position, dir.normalized, distance, fadeableLayer);
        HashSet<Renderer> newHits = new();

        foreach (var hit in hits)
        {
            Transform root = hit.collider.GetComponentInParent<Transform>();
            if (root == null) continue;

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                newHits.Add(rend);

                if (!originalMaterials.ContainsKey(rend))
                {
                    // Save the original material references
                    originalMaterials[rend] = rend.materials;

                    // Clone materials for fading
                    Material[] fading = new Material[rend.materials.Length];
                    for (int i = 0; i < fading.Length; i++)
                    {
                        fading[i] = new Material(rend.materials[i]);
                        SetTransparent(fading[i]);
                    }

                    rend.materials = fading;
                }

                Fade(rend, fadeAlpha);
                fadingRenderers.Add(rend);
            }
        }

        // Restore all renderers that are no longer in the hit list
        List<Renderer> toRestore = new();

        foreach (Renderer rend in fadingRenderers)
        {
            if (!newHits.Contains(rend))
            {
                // Fully fade in
                bool fullyVisible = Fade(rend, 1f);

                // When fade complete, restore original materials
                if (fullyVisible)
                {
                    if (originalMaterials.ContainsKey(rend))
                    {
                        rend.materials = originalMaterials[rend];
                        originalMaterials.Remove(rend);
                    }

                    toRestore.Add(rend);
                }
            }
        }

        // Clean up
        foreach (Renderer r in toRestore)
        {
            fadingRenderers.Remove(r);
        }
    }

    /// <summary>
    /// Fades material to target alpha. Returns true if it has reached targetAlpha.
    /// </summary>
    bool Fade(Renderer rend, float targetAlpha)
    {
        bool fullyReached = true;

        foreach (Material mat in rend.materials)
        {
            if (mat.HasProperty("_Color"))
            {
                Color c = mat.color;
                float newAlpha = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
                if (Mathf.Abs(newAlpha - targetAlpha) > 0.01f)
                    fullyReached = false;

                c.a = newAlpha;
                mat.color = c;
            }

            if (mat.IsKeywordEnabled("_EMISSION") && mat.HasProperty("_EmissionColor"))
            {
                Color e = mat.GetColor("_EmissionColor");
                float i = e.maxColorComponent;
                Color baseColor = i > 0 ? e / i : Color.black;
                float newI = Mathf.Lerp(i, targetAlpha, Time.deltaTime * fadeSpeed);
                mat.SetColor("_EmissionColor", baseColor * newI);

                if (Mathf.Abs(newI - targetAlpha) > 0.01f)
                    fullyReached = false;
            }
        }

        return fullyReached;
    }

    void SetTransparent(Material mat)
    {
        if (!mat.HasProperty("_Color")) return;

        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }



    public Transform mainCamera; // Assign Cinemachine-controlled camera

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.position = mainCamera.position;
            transform.rotation = mainCamera.rotation;
        }
    }

}
