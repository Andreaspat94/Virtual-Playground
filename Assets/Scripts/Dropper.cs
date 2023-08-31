using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dropper : MonoBehaviour
{
    public Rigidbody rg = null;
    public float timeToWait = 2;
    public bool isPhysicsOn = false;
    string cubeID;
    //coords on checkerbord grid we fall
    int x, y, z;

    /// <summary> 
    /// Start physics by disabling Kinematic and store info and show dropCube
    /// Called form CheckerManager in Update() when player has an active cube and presses button to place
    /// </summary>
    public void StartFalling(string name, int xpos, int ypos, int zpos)
    {
        //Store X,y checkker position where the final static interaction cube will be places
        x = xpos;
        y = ypos;    
        z = zpos;
        //Store id of cube (red,yellow etc)
        cubeID = name;

        //Compute checkker world position as we do from corner, just add an Y offset since the dropper has the pivot on top
        Vector3 positionNewCube = CheckerManager.Instance.cornerCheckerboard.position + CheckerManager.Instance.cornerCheckerboard.right * (x + 0.5f) 
                                + CheckerManager.Instance.cornerCheckerboard.forward * (y + 0.5f) + (Vector3.up * 1.0f);
        
        //Position the dropCube represantation in the cube carry hook positions
        transform.position = positionNewCube;

        //Enable physics
        rg.isKinematic = false;
        isPhysicsOn = true;

        //Show
        gameObject.SetActive(true);
    }

    //On Collision with the floor bounce and disable after tiemToWait
    void OnCollisionEnter(Collision collision)
    {
        if (isPhysicsOn)
            StartCoroutine(StartStop());
    }

    //Wait and disable physics again and hide dropCube
    //After that alert Manager to create a static cube passing the aquired info
    IEnumerator StartStop()
    {
        isPhysicsOn = false;
        yield return new WaitForSeconds(timeToWait);
        rg.isKinematic = true;
        gameObject.SetActive(false);
        CheckerManager.Instance.CreateStaticCube(cubeID, x, y);
    }
}
