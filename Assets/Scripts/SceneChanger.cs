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
    private bool objectsFoundInScene1;
    private bool objectsFoundInMenu;
    private string scene1Name = "Scene1";
    private string menuScene = "Menu";
    private GameObject playerInMenu;
    private GameObject menu;

    void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == scene1Name && !objectsFoundInScene1)
        {
            checkerManager = GameObject.Find("CheckerManager");
            checkerScript = checkerManager.GetComponent<CheckerManager>();
            objectsFoundInScene1 = true;
        }
        else if (currentSceneName == menuScene && !objectsFoundInMenu)
        {
            playerInMenu = GameObject.FindGameObjectWithTag("Player");
            menu = GameObject.Find("Menu");
            objectsFoundInMenu = true;
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

        if (currentSceneName == menuScene)
        {
            Vector3 player_position = playerInMenu.transform.position;
            Vector3 player_direction = playerInMenu.transform.forward;
            Quaternion player_rotation = playerInMenu.transform.rotation;

            menu.transform.position = player_position + player_direction;
            menu.transform.rotation = player_rotation;
        }
    }
}
