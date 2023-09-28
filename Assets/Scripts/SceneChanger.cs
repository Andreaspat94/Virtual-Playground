using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public Animator animator;
    private string sceneToLoad;
    private GameObject checkerManager;
    private CheckerManager checkerScript;
    private string currentSceneName;
    private bool objectsFound;
    private string scene1Name = "Scene1";

    void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == scene1Name && !objectsFound)
        {
            checkerManager = GameObject.Find("CheckerManager");
            checkerScript = checkerManager.GetComponent<CheckerManager>();
            objectsFound = true;
        }
    }

    public void FadeToLevel (string name)
    {
        if (currentSceneName == scene1Name)
        {
            checkerScript.fadeOut = true;
        }
        
        sceneToLoad = name;
        animator.SetTrigger("FadeOut");
    }
    
    public void OnFadeComplete()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        if (currentSceneName == scene1Name)
        {
            checkerScript.fadeOut = false;
        }   
    }
}
