using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothScript : MonoBehaviour ,IInteractable
{
    public string itemName = "Cloth";
    public bool fold ,allowintreaction;


    public Cloth cloth_ref;
    public Animator anim;
    public BoxCollider collider_ref;
    public Rigidbody rb_ref;

  
    private void Start()
    {
        if (anim == null)
            anim = gameObject.GetComponent<Animator>();

     //   cloth_mat.mainTexture = RandomTexture();
        SetRandomTexture();
    }
    public Texture[] cloth_textures; // Assign in inspector
    public Renderer clothRenderer; // Assign the cloth's MeshRenderer or SkinnedMeshRenderer

    [Tooltip("Name of the texture property in the material (usually _MainTex)")]
    public string textureProperty = "_MainTex";

    void SetRandomTexture()
    {
        if (cloth_textures == null || cloth_textures.Length == 0 || clothRenderer == null)
        {
            Debug.LogWarning("Missing texture array or renderer.");
            return;
        }

        // 1. Get a random texture
        Texture chosenTexture = cloth_textures[Random.Range(0, cloth_textures.Length)];

        // 2. Clone the material so it doesn't affect others
        Material newMat = new Material(clothRenderer.sharedMaterial);

        // 3. Assign the random texture
        newMat.SetTexture(textureProperty, chosenTexture);

        // 4. Apply the cloned material
        clothRenderer.material = newMat;
    }


    Texture RandomTexture()
    {
        int i = Random.Range(0, cloth_textures.Length);
        return cloth_textures[i];
    }
    public void Interact()
    {
        if (!allowintreaction)
            return;
        if (fold)
            return;
      
       // fold = true;

        ShootRay.instance.currentPrompt.SetActive(false);
        // ShootRay.instance.p_controller.enabled = false;
        StartCoroutine(ShootRay.instance.MoveClothIntoBox(this));
        ShootRay.instance.currentOutline.enabled = false;
    }

    public string GetInteractionText()
    {
        return $"Press E to Fold and Place in box";
      //  return $"Press E to Pick Up {itemName}";
    }
}

[System.Serializable]
public enum ClothStep
{
    WashingMachine,
    Dryer,
    Folding
}
[System.Serializable]
public enum BasketSize
{
    SmallBasket,
    Bigbasket
}