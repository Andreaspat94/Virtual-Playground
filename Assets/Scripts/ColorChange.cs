using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChange : MonoBehaviour
{
    Color originalColor;
    Color hoverColor = Color.green;
    Renderer renderer;
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponentInChildren<Renderer>();
        originalColor = renderer.material.color;
    }

   public void OnHoverEnter()
   {
       renderer.material.color = hoverColor;
   }

   public void OnHoverExit()
   {
       renderer.material.color = originalColor;
   }
}