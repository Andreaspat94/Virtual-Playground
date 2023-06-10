using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Set the outline shader value
public class ShowOutline : MonoBehaviour
{
    Renderer localRenderer = null;
    public int[] whichMaterialsToChange = null;

    // Use this for initialization
    void Start ()
    {
        localRenderer = GetComponent<Renderer>();
    }
	
	public void OutlineMaterials(float val)
    {
        if (whichMaterialsToChange != null)
        {
            foreach (int i in whichMaterialsToChange)
            {
                if (i < localRenderer.materials.Length)
                {
                    localRenderer.materials[i].SetFloat("_Outline", val);
                }
            }
        }
    }
}
