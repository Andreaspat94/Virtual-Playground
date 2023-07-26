using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeHighlight : MonoBehaviour
{
    //Positioned on update whena cube is pointed at. And hidden again when not.
    //There are two kinds of highlights because the cude have different sizes 
    //The pool cubes as the carry cubes are half size
    private GameObject pool;
    private GameObject poolCubeHighlight;
    private GameObject interactionCubeHighlight;
    void Start()
    {
        pool = GameObject.Find("CubeHighlightsPool");
        poolCubeHighlight = pool.transform.Find("cube_pool_highlight").gameObject;
        interactionCubeHighlight = pool.transform.Find("cube_interaction_highlight").gameObject;
    }
    
    public void InteractionCubeHighlight(Transform parent)
    {
        if (parent.tag == "InteractionCube" && interactionCubeHighlight != null)
        {
            interactionCubeHighlight.transform.position = parent.position;
            interactionCubeHighlight.transform.rotation = parent.rotation;
            interactionCubeHighlight.SetActive(true);
        }
    }
    public void PoolCubeHighlight(Transform parent)
    {
        if (parent.tag == "PoolCube" && poolCubeHighlight != null)
        {
            Debug.Log("HIGHLIGHT--> " + poolCubeHighlight);
            poolCubeHighlight.transform.position = parent.position;
            poolCubeHighlight.transform.rotation = parent.rotation;
            poolCubeHighlight.SetActive(true);
        }
    }

      //Switch off wireframe cubes that highlight which cube can be selected
    public void DisableHighlightOfCubes()
    {
        if (interactionCubeHighlight != null)
            interactionCubeHighlight.SetActive(false);
        if (poolCubeHighlight != null)
            poolCubeHighlight.SetActive(false);
    }
}
