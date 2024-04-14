using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartupTutorial : MonoBehaviour
{
    [Serializable]
    public class Wavs
    {
        public string audioname;
        public float duration = 0;
        public float pause = 0;
        public bool waitForButtonClick = false;
        public bool chooseBetween = false;
        public bool openMainPanel = false;
        public Sprite image;
        public string text;
        public string topText;
        public string buttonA;
        public string buttonB;
        public bool activateGrab;
        public bool activateRay;
        public string correctAnswer;
        public OVRInput.Button keyToProceed = OVRInput.Button.None;
        public UnityEvent OnKeyEvent;
        public OVRInput.Button secondkeyToProceed = OVRInput.Button.None;
        public UnityEvent OnSecondKeyEvent;
        public string keyInstructionText = null;
        public string instructionText = null;
    }

    [Serializable]
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
    public List<Wavs> redWavList = new List<Wavs>();
    [HideInInspector]
    Dictionary<string, List<Wavs>> tutoringWavs = new Dictionary<string, List<Wavs>>();
    string colorToCheck;
    GameObject lastQuestionObjects;
    bool gotIt;
    string lastButtonClicked;

    // public List<TutoringImages> tutoringImages = new List<TutoringImages>();
    public Dictionary<string, int> idOKDictionary = new Dictionary<string, int> 
    {
        {"blue", 1},
        {"yellow", 2},
        {"brown", 3},
        {"green", 4},
        {"red", 5},
        {"grey", 6}
    };

    Dictionary<int, string> poolDictionary = new Dictionary<int, string>
    {
        {0, "cube_blue_pool_collider_static(Clone)"},
        {1, "cube_yellow_pool_collider_static(Clone)"},
        {2, "cube_orange_pool_collider_static(Clone)"},
        {3, "cube_green_pool_collider_static(Clone)"},
        {4, "cube_red_pool_collider_static(Clone)"},
        {5, "cube_grey_pool_collider_static(Clone)"}
    };

    [HideInInspector]
    public bool[] idOK = new bool[6] { false, false, false, false, false, false};

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
    [SerializeField]
    GameObject tutoringCanvas;
    [SerializeField]
    GameObject instructionsCanvas;
    [SerializeField]
    GameObject instructionsTextObj;
    GameObject mainPanel;
    GameObject gotItButton;
    GameObject whichColorPanel;
    [HideInInspector]
    public bool isTutorial;
    bool isPlayingSounds;
    [HideInInspector]
    public bool owlIsSpeaking;
    bool cubeReleasedTutorial;
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
    void Start()
    {
        tutoringWavs.Add("red",redWavList);
        whichColorPanel = tutoringCanvas.transform.GetChild(0).GetChild(0).gameObject;
        mainPanel = tutoringCanvas.transform.GetChild(0).GetChild(1).gameObject;
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
        ActivateGrabInteractors(false);
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
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene.Equals("Passive"))
        {
            yield return new WaitUntil(() => !isPlayingSounds);
            // put it inside CheckIfOk()
            AudioManager.Instance.playSound("magic");
        }
    }

    //Color values available:
    // Slide:1, MonkeyBars:2, CrawlTunnel:3, RoundAbout:4, Swings:5, SandPit:6
    public void SelectColorFromPanel(string color)
    {
        whichColorPanel.SetActive(false);
        colorToCheck = color;
        // checkIfOk() - that sets the bool 'colorIsOk'
        CheckerManager.Instance.CheckIfOK();
    }
    public void Tutoring()
    {      
        owlIsSpeaking = true;
        List<Wavs> wavList = tutoringWavs[colorToCheck];
        gotItButton = mainPanel.transform.GetChild(0).gameObject;
        StartCoroutine(PlayTutoringSequence(redWavList));
    }

    public void GotIt()
    {
        gotIt = true;
    }

    public void ChooseBetween(string buttonName)
    {
        lastButtonClicked = buttonName;
        Debug.Log("LastButtonClicked "+ lastButtonClicked);
    }

    IEnumerator PlayTutoringSequence(List<Wavs> wavList)
    {
        yield return new WaitForSeconds(2f);
        ActivateTalkingBirds(false);
        Image image = mainPanel.GetComponent<Image>();
        Text tutorText = mainPanel.transform.GetChild(1).GetComponent<Text>();
        lastQuestionObjects = mainPanel.transform.GetChild(2).gameObject;
        bool skipCorrectWa = false;
        foreach (Wavs wa in wavList)
        {            
            if (string.IsNullOrEmpty(wa.audioname))
                continue;
            if (skipCorrectWa)
                continue;

            // Debug.Log("wa name -->" + wa.audioname);
             
            // Check if ray or grab is needed
            ActivateGrabInteractors(wa.activateGrab);
            ActivateRayInteractors(wa.activateRay);

            gotIt = false;
            lastButtonClicked = string.Empty;
            image.sprite = null;
            tutorText.text = string.Empty;
            
            // open main panel
            tutoringCanvas.SetActive(wa.openMainPanel);
            mainPanel.SetActive(wa.openMainPanel);
            
            //Play explanation
            AudioManager.Instance.playSound(wa.audioname);
            
            if (wa.image != null)
                image.sprite = wa.image;
            
            if (!string.IsNullOrEmpty(wa.text))
                tutorText.text = wa.text;

            //Start owl morph
            if (owlAnimator)
                owlAnimator.PlayAnimation();

            //Wait duration of sound
            yield return new WaitForSeconds(wa.duration);

            //Stop owl morph
            if (owlAnimator)
                owlAnimator.PauseAnimation();

            if (wa.chooseBetween)
            {
                lastQuestionObjects.SetActive(true);
                //set main text and buttons text
                lastQuestionObjects.transform.GetChild(0).GetComponent<Text>().text = wa.topText;
                lastQuestionObjects.transform.GetChild(1).GetComponentInChildren<Text>().text = wa.buttonA;
                lastQuestionObjects.transform.GetChild(2).GetComponentInChildren<Text>().text = wa.buttonB;
                yield return new WaitUntil(() => gotIt);
                lastQuestionObjects.SetActive(false);
                
                // if wrong answer
                if (!lastButtonClicked.Equals(wa.correctAnswer))
                {
                    tutoringCanvas.SetActive(false);
                    skipCorrectWa = true;
                    /* This logic erases all cubes of the correct's answer color and replace it with the correct island*/
                    // CheckerManager.Instance.MakeIsland(idOKDictionary[colorToCheck]);
                }
            }

            // wait until 'got it' button clicked. single button appears on the panel now.
            if (wa.waitForButtonClick)
            {
                gotItButton.SetActive(true);
                // Check if ray or grab is needed
                // ActivateGrabInteractors(wa.activateGrab);
                // ActivateRayInteractors(wa.activateRay);
                yield return new WaitUntil(() => gotIt);
                gotItButton.SetActive(false);
                tutorText.text = string.Empty;
            }

            mainPanel.SetActive(false);
            // image.sprite = null;
            //pause a bit
            yield return new WaitForSeconds(wa.pause);
            
            // Stop Tutoring
            //Wait until key is pressed
            if (wa.keyToProceed != OVRInput.Button.None)
            {
                //Display text to inform about key press
                if (!string.IsNullOrEmpty(wa.keyInstructionText) && instructionText)
                {
                    instructionsCanvas.SetActive(true);
                    instructionsTextObj.SetActive(true);
                    instructionText.text = wa.keyInstructionText;
                    instructionText.gameObject.SetActive(true);
                }

                //wait unti key pressed
                while (!OVRInput.GetDown(wa.keyToProceed))
                    yield return null;

                //Issue event that key was pressed
                if (wa.OnKeyEvent != null)
                    wa.OnKeyEvent.Invoke();
                break;
            }

            if (wa.audioname.Equals("final_no"))
            {
                CheckerManager.Instance.MakeIsland(idOKDictionary[colorToCheck]);
                owlIsSpeaking = false;
            }
        }
        // owlIsSpeaking = false;
    }

    public void ExitInstructions()
    {
        StopAllCoroutines();
        ActivateTalkingBirds(true);
        instructionsCanvas.SetActive(false);
        owlIsSpeaking = false;
    }
    /**
    red: 4
    */
    void FinishTutoring(int id)
    {
        
         CheckerManager.Instance.MakeIsland(id);
         tutoringCanvas.SetActive(false);
         GrayOutWhichPanelButton(id);
         ActivateTalkingBirds(true);
         owlIsSpeaking = false;
    }

    void ActivateTalkingBirds(bool activate)
    {
        GameObject birds = CheckerManager.Instance.talkingBirds;
        Component[] birdColliders = birds.GetComponentsInChildren(typeof(Collider));
        foreach (Collider collider in birdColliders)
        {
            collider.enabled = activate;
        }
    }

    IEnumerator PlaySounds()
    {
        gotIt = false;
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
            // Debug.Log("PlaySounds-->" + wa.audioname);
            if (string.IsNullOrEmpty(wa.audioname))
                continue;

            //Play explanation
            AudioManager.Instance.playSound(wa.audioname);

            //Start owl morph
            if (owlAnimator)
            {
                ActivateTalkingBirds(false);
                owlAnimator.PlayAnimation();
                owlIsSpeaking = true;
            }
                
            //Display a info text if there is one
            if (!string.IsNullOrEmpty(wa.instructionText) && instructionText)
            {
                instructionText.text = wa.instructionText;
                instructionText.gameObject.SetActive(true);
                if (wa.audioname.Equals("owl5_2"))
                    ActivateGrabInteractors(true);
            }

            //Wait duration of sound
            yield return new WaitForSeconds(wa.duration);

            //Stop wol morph
            if (owlAnimator)
            {
                ActivateTalkingBirds(true);
                owlAnimator.PauseAnimation();
                owlIsSpeaking = false;
            }    
            
            if (wa.audioname.Equals("owl5_2"))
            {
                yield return new WaitUntil(() => cubeReleasedTutorial);
                ActivateGrabInteractors(false);
            }

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
            
            // Check if ray or grab is needed
            ActivateGrabInteractors(wa.activateGrab);
            ActivateRayInteractors(wa.activateRay);
                
            // show "which color" panel
            if (wa.audioname.Equals("which"))
            {
                tutoringCanvas.SetActive(true);
                whichColorPanel.SetActive(true);
                // precaution
                mainPanel.SetActive(false);
                // wait until CheckIfOk finishes.
                yield return new WaitUntil(() => gotIt);
                int id = idOKDictionary[colorToCheck] - 1;
                if (idOK[id])
                {
                    FinishTutoring(id);
                    //!!TODO: disable pool cube
                    // DisablePoolCube(4);
                }
                else
                {
                    AudioManager.Instance.playSound("magic");
                    tutoringCanvas.SetActive(false);
                    break;
                }
            }

            //pause a bit
            yield return new WaitForSeconds(wa.pause);
        }

        if (!idOK[idOKDictionary[colorToCheck]] == false)
            Tutoring();

        isPlayingSounds = false;
        //Activate the game manager
        if (isTutorial)
            StartGame();
    }

    void GrayOutWhichPanelButton(int id)
    {
        GameObject panelButton = whichColorPanel.transform.GetChild(id).gameObject;
        Button buttonComponent = panelButton.GetComponent<Button>();
        buttonComponent.interactable = false;
        ColorBlock cb = buttonComponent.colors;
        cb.normalColor = Color.gray;
        buttonComponent.colors = cb;
    }

    void DisablePoolCube(int id)
    {
        GameObject pool = CheckerManager.Instance.objectPool;
        string objName = string.Concat("/", poolDictionary[id]); 
        Debug.Log("OBJ name --> " + objName);
        GameObject poolCube = pool.transform.Find(objName).gameObject;
        Debug.Log("FOUND POOL CUBE --> " + poolCube);
        // poolCube.GetComponentInChildren<Grabbable>().enabled = false;
    }

    public void ReleaseEventTutorial()
    {
        cubeReleasedTutorial = true;
    }

    void StartGame()
    {
        ActivateTalkingBirds(true);
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
