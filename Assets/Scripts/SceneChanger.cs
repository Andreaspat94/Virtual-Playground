using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    public Animator animator;
    private string sceneToLoad;

    public void FadeToLevel (string name)
    {
        sceneToLoad = name;
        animator.SetTrigger("FadeOut");
        Debug.Log("--> " + sceneToLoad);
    }
    
    public void OnFadeComplete()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
    }
}
