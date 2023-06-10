using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelSwitcher : MonoBehaviour
{
    public bool Play = false;
    public Vector2 frameDelayMinMax = new Vector2(0.01f, 0.0175f);
    public float frameDelay = 0;
    public GameObject[] modelArray;
    public int activeIndex = 0;

    [Header("Billboard Owl when Play")]
    public HoloToolkit.Unity.Billboard owlBillboard = null;

    float currentTimer = 0;

    public void PlayAnimation()
    {
        Play = true;

        if (owlBillboard)
            owlBillboard.enabled = true;
    }
    public void PauseAnimation()
    {
        Reset();
    }

    public void Reset()
    {
        //if (owlBillboard)
        //    owlBillboard.enabled = false;

        Play = false;
        modelArray[activeIndex].SetActive(false);
        modelArray[0].SetActive(true);
        activeIndex = 0;
    }

    // Use this for initialization
    void Start ()
    {
        //amountOfKids is equal to 35 (owl_animate_xxx)
        int amountOfKids = transform.childCount;
        modelArray = new GameObject[amountOfKids];

        for (int i = 0; i < amountOfKids; i++)
        {
            modelArray[i] = transform.GetChild(i).gameObject;
            modelArray[i].SetActive(false);
        }
        
        modelArray[activeIndex].SetActive(true);
        frameDelay = Random.Range(frameDelayMinMax.x, frameDelayMinMax.y);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Play)
        {
            currentTimer += Time.deltaTime;
            if (currentTimer > frameDelay)
            {
                modelArray[activeIndex].SetActive(false);
                activeIndex = (activeIndex + 1) % modelArray.Length;
                modelArray[activeIndex].SetActive(true);
                currentTimer = 0;
                frameDelay = Random.Range(frameDelayMinMax.x, frameDelayMinMax.y);
            }
        }
	}
}
