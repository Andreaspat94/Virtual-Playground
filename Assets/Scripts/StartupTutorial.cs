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

    [System.Serializable]
    public class Wavs2
    {
        public string audioname;
        public float duration = 0;
        public float pause = 0;
        public Button[] colorButtons;
        public UnityEvent OnEvent;
    }

    private GameObject player;
    private GameObject leftRay;
    private GameObject rightRay;
    private String pathToLeftRay = "OVRInteraction/OVRControllerHands/LeftControllerHand/ControllerHandInteractors/HandRayInteractorLeft";
    private String pathToRightRay = "OVRInteraction/OVRControllerHands/RightControllerHand/ControllerHandInteractors/HandRayInteractorRight";

    private GameObject leftHandGrab;
    private GameObject rightHandGrab;
    private String pathToLeftHandGrab = "OVRInteraction/OVRControllerHands/LeftControllerHand/ControllerHandInteractors/DistanceHandGrabInteractorLeft";
    private String pathToRightHandGrab = "OVRInteraction/OVRControllerHands/RightControllerHand/ControllerHandInteractors/DistanceHandGrabInteractorRight";

    public List<Wavs> wayPointList = new List<Wavs>();
    public List<Wavs> confirmAudioList = new List<Wavs>();

    [Header ("Things to Hide at startup")]
    public GameObject[] ListToHide;

    [Header("Things to show at startup")]
    public GameObject[] badPlayground;

    public bool isActive = true;
    public Text instructionText = null;
    public ModelSwitcher owlAnimator = null;
    string displayText;
    Collider owlCollider;
    [SerializeField]
    GameObject owlOutline;
    public bool isTutorial;
    bool isPlayingSounds;
    [HideInInspector]
    public bool owlIsSpeaking;
    bool cubeReleasedEventTutorial;
    //Mask set to "InteractionGeneralObject" layer mask and designete general interaction objects like the birds
    //They issue events when pointed at and clicked upon
    public LayerMask interactionObjectsMask;

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
        TrackInteractors();
        owlCollider = owlAnimator.GetComponent<Collider>();
        isTutorial = true;
    }

    // Use this for initialization
    void Start ()
    {
        //if not active proceed to play immediately
        if (isActive)
        {
            displayText = "The Playground is a mess.\nAsk the Owl how to fix it.";
            StartCoroutine(AskTheOwl());
        }
        else
        {
            StartGame();
        }
	}

    void TrackInteractors()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            leftRay = player.transform.Find(pathToLeftRay).gameObject;
            rightRay = player.transform.Find(pathToRightRay).gameObject;

            leftRay.SetActive(false);
            rightRay.SetActive(false);

            leftHandGrab = player.transform.Find(pathToLeftHandGrab).gameObject;
            rightHandGrab = player.transform.Find(pathToRightHandGrab).gameObject;

            leftHandGrab.SetActive(false);
            rightHandGrab.SetActive(false);
        }
    }
    
    public void ActivateRayInteractors(bool activate)
    {
        leftRay.SetActive(activate);
        rightRay.SetActive(activate);
    }

    public void ActivateGrabInteractors(bool activate)
    {
        leftHandGrab.SetActive(activate);
        rightHandGrab.SetActive(activate);
    }

    //This is called afer clicking on the owl, it starts the Tutorial sequence
    public void StartPlayingTheSounds()
    {
        GetComponent<Collider>().enabled = false;
        owlAnimator.GetComponent<Collider>().enabled = false;

        //Finds Owl_ModelSwitcher object and turns off red shader in order to solve a bug: the shader turns on and off while the owl is talking.
        owlOutline.SetActive(false);        

        ActivateRayInteractors(false);
        // Debug.Log("Activate from StartPlayingTheSounds()-- " + false);
        StartCoroutine(PlaySounds());
    }
	
	// Update is called once per frame
	void Update ()
    {
            //Press 'Y' to skip intro
        if (OVRInput.GetDown(OVRInput.Button.Four) && isTutorial)
        {
            AudioManager.Instance.ResetSound();
            StopAllCoroutines();
            StartGame();            
        }
    }
    
    //Show instruction text to aske the owl and fade out
    IEnumerator AskTheOwl()
    {
        if (instructionText)
        {
            instructionText.text = displayText;
            instructionText.gameObject.SetActive(true);
            yield return new WaitForSeconds(5);
        }

        CanvasGroup gr = instructionText.GetComponent<CanvasGroup>();
        while (gr.alpha > 0)
        {
            gr.alpha -= Time.deltaTime*0.4f;
            yield return null;
        }

        instructionText.gameObject.SetActive(false);
        gr.alpha = 1;
        yield return null;
    }
    void ResetTheOwl()
    {
        GetComponent<Collider>().enabled = true;
        owlAnimator.GetComponent<Collider>().enabled = true;
        owlOutline.SetActive(true);

    }
    // Here the confirmation sequence for given answer starts 
    // [1] -- This method is called when 'X' touch controller button is pressed.
    public void ConfirmAnswer()
    {
        isPlayingSounds = true;
        StartCoroutine(StartConfirmSequence());
    }

    IEnumerator StartConfirmSequence()
    {
        GetComponent<Collider>().enabled = true;
        owlCollider.enabled = true;
        displayText = "Do you want to check your answer?\nLet's ask the Owl!";
        StartCoroutine(AskTheOwl());
        yield return new WaitUntil(() => !isPlayingSounds);
        // AudioManager.Instance.playSound("magic");
    }

    //This coroutine plays all the pre-game text
    // public void SequenceIsPlaying(bool over)
    // {
    //     isPlayingSounds = over;
    //     CheckerManager.Instance.inSequence = over;
    // }
    IEnumerator PlaySounds()
    {
        List<Wavs> wavList = new List<Wavs>();
        if (isTutorial)
        {
            wavList = wayPointList;
        }
        else
        {
            wavList = confirmAudioList;
        }
        //for each entry in the text, each entry if a text file
        foreach (Wavs wa in wavList)
        {
            // if (!wa.audioname.Equals("owl5_2") || !wa.audioname.Equals("owl6"))
            // {
            //     continue;
            // }
            if (string.IsNullOrEmpty(wa.audioname))
                continue;

            //Play explanation
            AudioManager.Instance.playSound(wa.audioname);

            //Start owl morph
            if (owlAnimator)
            {
                owlAnimator.PlayAnimation();
                owlIsSpeaking = true;
            }
                
            //Display a info text if there is one
            if (!string.IsNullOrEmpty(wa.instructionText) && instructionText)
            {
                instructionText.text = wa.instructionText;
                instructionText.gameObject.SetActive(true);
            }

            //Wait duration of sound
            yield return new WaitForSeconds(wa.duration);

            //Stop wol morph
            if (owlAnimator)
            {
                owlAnimator.PauseAnimation();
                owlIsSpeaking = false;
            }    

            // This is for passive
            PassiveTutorCheck(wa.audioname);
            
            //Wait until key is pressed
            if (wa.keyToProceed != OVRInput.Button.None)
            {
                //Display text to inform about key press
                if (!string.IsNullOrEmpty(wa.keyInstructionText) && instructionText)
                {
                    instructionText.text = wa.keyInstructionText;
                    instructionText.gameObject.SetActive(true);
                }

                //wait unti key pressed
                while (!OVRInput.GetDown(wa.keyToProceed))
                    yield return null;

                //Issue event that key was pressed
                if (wa.OnKeyEvent != null)
                    wa.OnKeyEvent.Invoke();
            }
            else
            {
                if (wa.audioname.Equals("owl5_2"))
                {
                    //Display text to inform about key press
                    if (!string.IsNullOrEmpty(wa.keyInstructionText) && instructionText)
                    {
                        instructionText.text = wa.keyInstructionText;
                        instructionText.gameObject.SetActive(true);
                    }
                    
                    //WAIT UNTIL CUBE RELEASED !!! 
                    while (!cubeReleasedEventTutorial)
                        yield return null;
                }
            }

            //Wait for second keypress
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
            if (instructionText)
                instructionText.gameObject.SetActive(false);

            //pause a bit
            yield return new WaitForSeconds(wa.pause);
        }

        isPlayingSounds = false;
        //Activate the game manager
        if (isTutorial)
            StartGame();
    }

    public void ReleaseEventTutorial()
    {
        cubeReleasedEventTutorial = true;
    }
    void PassiveTutorCheck(string wa)
    {
        if (wa.Equals("check1"))
            {
                ActivateGrabInteractors(true);
                ActivateRayInteractors(true);
            }
    }
    void StartGame()
    {
        ActivateRayInteractors(true);
        ActivateGrabInteractors(true);
        isActive = false;
        isTutorial = false;
        CheckerManager.Instance.isActive = true;
        owlCollider.enabled = false;
        //gameObject.SetActive(false);
        
        foreach (GameObject gs in ListToHide)
            gs.SetActive(true);

        foreach (GameObject gs in badPlayground)
            gs.SetActive(false);

        if (owlAnimator)
            owlAnimator.Reset();

        CheckerManager.Instance.countCoveredBlocksUI();

        instructionText.gameObject.SetActive(false);
        CanvasGroup gr = instructionText.GetComponent<CanvasGroup>();
        gr.alpha = 1;
        // the owl model switcher was turned off when tutorial ends for some reason. This line of code solves this issue.
        owlAnimator.gameObject.SetActive(true);
    }
}
