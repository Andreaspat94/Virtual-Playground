using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    public Animator animator;
    private string sceneToLoad;

    public void FadeToLevel (string name)
    {
        Debug.Log("FadeToLevel called... name --> " + name + " /sceneToLoad --> " + sceneToLoad);
        sceneToLoad = name;
        animator.SetTrigger("FadeOut");
    }
    
    public void OnFadeComplete()
    {
        Debug.Log("OnFadeComplete called... name --> " + name + " /sceneToLoad --> " + sceneToLoad);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
    }
}
