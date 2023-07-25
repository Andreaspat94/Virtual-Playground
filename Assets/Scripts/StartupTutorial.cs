using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StartupTutorial : MonoBehaviour
{
    [System.Serializable]
    public class Wavs
    {
        public string audioname;
        public float duration = 0;
        public float pause = 0;
        public OVRInput.Button keyToProceed = OVRInput.Button.None;
        public UnityEvent OnKeyEvent;
        public OVRInput.Button secondkeyToProceed = OVRInput.Button.None;
        public UnityEvent OnSecondKeyEvent;
        public string keyInstructionText = null;
        public string instructionText = null;
    }

    private GameObject player;
    private GameObject leftRay;
    private GameObject rightRay;
    private String pathToLeftRay = "OVRInteraction/OVRControllerHands/LeftControllerHand/ControllerHandInteractors/HandRayInteractorLeft";
    private String pathToRightRay = "OVRInteraction/OVRControllerHands/RightControllerHand/ControllerHandInteractors/HandRayInteractorRight";
    public List<Wavs> wayPointList = new List<Wavs>();

    [Header ("Things to Hide at startup")]
    public GameObject[] ListToHide;

    [Header("Things to show at startup")]
    public GameObject[] badPlayground;

    public bool isActive = true;
    bool isSayingInfo = false;
    public Text instructionTextUI = null;
    public ModelSwitcher owlAnimator = null;

    //Mask set to "InteractionGeneralObject" layer mask and designete general interaction objects like the birds
    //They issue events when pointed at and clicked upon
    public LayerMask interactionObjectsMask;


    //Raycasting is only active if we are int this trigger bbox
    private void OnTriggerEnter(Collider other)
    {
         if (other.tag == "Player")
            SwitchOnOffRayInteractors();
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
            SwitchOnOffRayInteractors();
    }

    void Awake()
    {
        //Set visibility of objects for instructions
        //is this object is isActive=false then interaction is possible right at startup
        if (isActive)
        {
            foreach (GameObject gs in ListToHide)
                gs.SetActive(false);

            foreach (GameObject gs in badPlayground)
                gs.SetActive(true);
        }
        // track ray interactors and switch them off
        TrackRayInteractors();
    }

    // Use this for initialization
    void Start ()
    {
        //if not active proceed to play immediately
        if (isActive)
        {
            StartCoroutine(AskTheOwl());
        }
        else
        {
            StartGame();
        }
	}

    void StartGame()
    {
        SwitchOnOffRayInteractors();
        isActive = false;
        CheckerManager.Instance.isActive = true;
        owlAnimator.GetComponent<Collider>().enabled = false;
        gameObject.SetActive(false);

        foreach (GameObject gs in ListToHide)
            gs.SetActive(true);

        foreach (GameObject gs in badPlayground)
            gs.SetActive(false);

        isSayingInfo = false;

        if (owlAnimator)
            owlAnimator.Reset();

        CheckerManager.Instance.countCoveredBlocksUI();

        instructionTextUI.gameObject.SetActive(false);
        CanvasGroup gr = instructionTextUI.GetComponent<CanvasGroup>();
        gr.alpha = 1;
        // the owl model switcher was turned off when tutorial ends for some reason. This line of code solves this issue.
        owlAnimator.gameObject.SetActive(true);
    }

    void TrackRayInteractors()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            leftRay = player.transform.Find(pathToLeftRay).gameObject;
            rightRay = player.transform.Find(pathToRightRay).gameObject;

            leftRay.SetActive(false);
            rightRay.SetActive(false);
        }
    }

    void SwitchOnOffRayInteractors()
    {
        leftRay.SetActive(!leftRay.activeSelf);
        rightRay.SetActive(!rightRay.activeSelf);
    }

    //This is called afer clicking on the owl, it starts the Tutorial sequence
    public void StartPlayingTheSounds()
    {
        isSayingInfo = true;
        GetComponent<Collider>().enabled = false;
        owlAnimator.GetComponent<Collider>().enabled = false;

        //Finds Owl_ModelSwitcher object and turns off red shader in order to solve a bug: the shader turns on and off while the owl is talking.
        GameObject.Find("OwlOutline").SetActive(false);        
    
        SwitchOnOffRayInteractors();
        // CheckerManager.Instance.IssueInterationEvents(null);
        StartCoroutine(PlaySounds());
        
    }
	
	// Update is called once per frame
	void Update ()
    {
        //Press 'Y' to skip intro
        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            AudioManager.Instance.ResetSound();
            StopAllCoroutines();
            StartGame();            
        }
    }

    //Show instruction text to aske the owl and fade out
    IEnumerator AskTheOwl()
    {
        if (instructionTextUI)
        {
            instructionTextUI.text = "The Playground is a mess.\nAsk the Owl how to fix it.";
            instructionTextUI.gameObject.SetActive(true);
            yield return new WaitForSeconds(5);
        }

        CanvasGroup gr = instructionTextUI.GetComponent<CanvasGroup>();
        while (gr.alpha > 0)
        {
            gr.alpha -= Time.deltaTime*0.4f;
            yield return null;
        }

        instructionTextUI.gameObject.SetActive(false);
        gr.alpha = 1;
        yield return null;
    }

    //This coroutine plays all the pre-game text
    IEnumerator PlaySounds()
    {
        //for each entry in the text, each entry if a text file
        foreach (Wavs wa in wayPointList)
        {
            if (string.IsNullOrEmpty(wa.audioname))
                continue;

            //Play explanation
            AudioManager.Instance.playSound(wa.audioname);

            //Start owl morph
            if (owlAnimator)
                owlAnimator.PlayAnimation();

            //Display a info text if there is one
            if (!string.IsNullOrEmpty(wa.instructionText) && instructionTextUI)
            {
                instructionTextUI.text = wa.instructionText;
                instructionTextUI.gameObject.SetActive(true);
            }

            //Wait duration of sound
            yield return new WaitForSeconds(wa.duration);

            //Stop wol morph
            if (owlAnimator)
                owlAnimator.PauseAnimation();

            //Wait until key is pressed
            if (wa.keyToProceed != OVRInput.Button.None)
            {
                //Display text to inform about key press
                if (!string.IsNullOrEmpty(wa.keyInstructionText) && instructionTextUI)
                {
                    instructionTextUI.text = wa.keyInstructionText;
                    instructionTextUI.gameObject.SetActive(true);
                }

                //wait unti key pressed
                while (!OVRInput.GetDown(wa.keyToProceed))
                    yield return null;

                //Issue event that key was pressed
                if (wa.OnKeyEvent != null)
                    wa.OnKeyEvent.Invoke();
            }

            //Wait for econd keypress
            if (wa.secondkeyToProceed != OVRInput.Button.None)
            {
                yield return new WaitForSeconds(2);
                while (!OVRInput.GetDown(wa.secondkeyToProceed))
                    yield return null;

                //Issue the event for second key event
                if (wa.OnSecondKeyEvent != null)
                    wa.OnSecondKeyEvent.Invoke();
            }

            //Hide ui
            if (instructionTextUI)
                instructionTextUI.gameObject.SetActive(false);

            //pause a bit
            yield return new WaitForSeconds(wa.pause);
        }

        //Activate the game manager
        StartGame();
    }
}
