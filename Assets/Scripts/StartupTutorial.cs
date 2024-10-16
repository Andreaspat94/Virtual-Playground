using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartupTutorial : MonoBehaviour
{
    [SerializeField]
    GameObject cubeHierarchy;
    [Serializable]
    public class Wavs
    {
        public string audioname;
        public float duration = 0;
        public float pause = 0;
        public bool waitForButtonClick = false;
        public bool chooseBetween = false;
        public bool openMainPanel = false;
        public string[] startAnimation;
        public Sprite image;
        public string text;
        public string blueText;
        public string yellowText;
        public string topText;
        public string buttonA;
        public string buttonB;
        public bool activateGrab;
        public bool activateRay;
        public bool finishTutoring;
        public bool skipNextWa;
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
    [SerializeField]
    GameObject leftController;
    [SerializeField]
    GameObject rightController;
    [SerializeField]
    GameObject[] leftControllerText;

    [SerializeField]
    GameObject[] rightControllerText;   
    [SerializeField]
    GameObject[] particleSystems;
    Transform cameraTransform;
    public float lookThreshold = 83f;
    [SerializeField]
    private GameObject leftRay;
    [SerializeField]
    private GameObject rightRay;
    public List<Wavs> wayPointList = new List<Wavs>();
    public List<Wavs> confirmAudioList = new List<Wavs>();
    public List<Wavs> redWavList = new List<Wavs>();
    public List<Wavs> blueWavList = new List<Wavs>();
    public List<Wavs> greenWavList = new List<Wavs>();
    public List<Wavs> yellowWavList = new List<Wavs>();
    public List<Wavs> orangeWavList = new List<Wavs>();
    [HideInInspector]
    Dictionary<string, List<Wavs>> tutoringWavs = new Dictionary<string, List<Wavs>>();
    public List<GameObject> controllerButtons = new List<GameObject>();
    public Dictionary<string, GameObject> buttonDictionary;
    string colorToCheck;
    GameObject lastQuestionObjects;
    bool gotIt;
    string lastButtonClicked;
    
    public Dictionary<string, int> idOKDictionary = new Dictionary<string, int> 
    {
        {"blue", 1},
        {"yellow", 2},
        {"orange", 3},
        {"green", 4},
        {"red", 5},
        {"grey", 6}
    };
    Dictionary<int, int> properColorCount = new Dictionary<int, int> 
    {
        {1, 12},
        {2, 2},
        {3, 4},
        {4, 4},
        {5, 16},
        {6, 12}
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
    Dictionary<string, string> correct_audio = new Dictionary<string, string>
    {
        {"red", "correct_red"},
        {"blue", "correct_blue"},
        {"green", "correct_green"},
        {"yellow", "correct_yellow"},
        {"orange", "correct_orange"}
    };

    Dictionary<string, List<Wavs>> wavLists;
    [HideInInspector]
    public bool[] idOK = new bool[6] {false, false, false, false, false, false};

    [Header ("Things to Hide at startup")]
    public GameObject[] ListToHide;

    [Header("Things to show at startup")]
    public GameObject[] badPlayground;

    public bool isActive = true;
    public Text instructionText = null;
    public ModelSwitcher owlAnimator = null;
    public Animator leftBuildingAnimator;
    public Animator rightBuildingAnimator;
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
    public bool allOK;
    [HideInInspector]
    public bool isTutorial;
    bool isPlayingSounds;
    [HideInInspector]
    public bool owlIsSpeaking;
    bool cubeReleasedTutorial;
    //Mask set to "InteractionGeneralObject" layer mask and designete general interaction objects like the birds
    //They issue events when pointed at and clicked upon
    public LayerMask interactionObjectsMask;
    public String activeScene;

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
        activeScene = SceneManager.GetActiveScene().name;
        cameraTransform = Camera.main.transform;
        
        buttonDictionary = new Dictionary<string, GameObject> 
        {
            {"A", controllerButtons[0]},
            {"TriggerRight", controllerButtons[1]},
            {"X", controllerButtons[2]},
            {"TriggerLeft", controllerButtons[3]},
            {"SignLeft", controllerButtons[4]},
            {"SignRight", controllerButtons[5]}
        };

        wavLists = new Dictionary<string, List<Wavs>> 
        {
            {"blue", blueWavList},
            {"yellow", yellowWavList},
            {"orange", orangeWavList},
            {"green", greenWavList},
            {"red", redWavList}
        };
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
            leftRay.SetActive(true);
            rightRay.SetActive(true);
        }
    }
    
    public void ActivateRayInteractors(bool activate)
    {
        leftRay.SetActive(activate);
        rightRay.SetActive(activate);
    }

    public void ActivateGrabInteractors(bool activate)
    {
        // Debug.Log("CALLED: " + activate);
        // leftHandGrab.SetActive(activate);
        // rightHandGrab.SetActive(activate);
    }

    //This is called afer clicking on the owl, it starts the Tutorial sequence
    public void StartPlayingTheSounds()
    {
        owlIsSpeaking = true;
        GetComponent<Collider>().enabled = false;
        owlAnimator.GetComponent<Collider>().enabled = false;

        //Finds Owl_ModelSwitcher object and turns off red shader in order to solve a bug: the shader turns on and off while the owl is talking.
        owlOutline.SetActive(false);        

        ActivateRayInteractors(false);
        StartCoroutine(PlaySounds());
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!isTutorial)
        {
            CheckIfLookingAtController(leftController, leftControllerText);
            CheckIfLookingAtController(rightController, rightControllerText);
        }
        

        //Press 'Y' to skip intro
        if (OVRInput.GetDown(OVRInput.Button.Four) && isTutorial)
        {
            AudioManager.Instance.ResetSound();
            StopAllCoroutines();
            tutoringCanvas.SetActive(false);
            foreach (GameObject obj in particleSystems)
            {
                Debug.Log("PArticle --> " + obj.name);
                obj.SetActive(false);
            }
            StartGame();            
        }
    }

    void CheckIfLookingAtController(GameObject controller, GameObject[] controllerText)
    {
        Vector3 directionToController = (controller.transform.position - cameraTransform.position).normalized;
        Vector3 playerLookDirection = cameraTransform.forward;

        float angle = Vector3.Angle(directionToController, playerLookDirection);
        foreach (GameObject text in controllerText)
        {
            if (angle < lookThreshold)
            {  
                text.SetActive(true);
            }
            else
            {
                text.SetActive(false);
            }
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
        if (activeScene.Equals("Passive"))
        {
            yield return new WaitUntil(() => !isPlayingSounds);
            // put it inside CheckIfOk()
            AudioManager.Instance.playSound("magic");
            CheckerManager.Instance.readyToExitPresentationMode = true;
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
        // List<Wavs> wavList = tutoringWavs[colorToCheck];
        gotItButton = mainPanel.transform.GetChild(0).gameObject;
        StartCoroutine(PlayTutoringSequence(wavLists[colorToCheck]));
    }

    public void GotIt()
    {
        gotIt = true;
    }

    public void ChooseBetween(string buttonName)
    {
        lastButtonClicked = buttonName;
    }

    IEnumerator PlayTutoringSequence(List<Wavs> wavList)
    {
        yield return new WaitForSeconds(2f);
        ActivateTalkingBirds(false);
        
        Image image = mainPanel.GetComponent<Image>();
        Text tutorText = mainPanel.transform.GetChild(1).GetComponent<Text>();
        Text blueText = mainPanel.transform.GetChild(2).GetComponent<Text>();
        Text yellowText = mainPanel.transform.GetChild(3).GetComponent<Text>();
        lastQuestionObjects = mainPanel.transform.GetChild(4).gameObject;
        bool skipCorrectWa = false;
        
        foreach (Wavs wa in wavList)
        {            
            if (string.IsNullOrEmpty(wa.audioname))
                continue;

            if (skipCorrectWa) {
                skipCorrectWa = false;
                continue;
            }
        
            // Debug.Log("wa name -->" + wa.audioname);
            // Check if ray or grab is needed
            ActivateRayInteractors(wa.activateRay);

            gotIt = false;
            lastButtonClicked = string.Empty;
            image.sprite = null;
            tutorText.text = string.Empty;
            blueText.text = string.Empty;
            yellowText.text = string.Empty;
            
            // open main panel
            tutoringCanvas.SetActive(wa.openMainPanel);
            mainPanel.SetActive(wa.openMainPanel);
            
            //Play explanation
            if (!wa.audioname.StartsWith("noaudio"))
                AudioManager.Instance.playSound(wa.audioname);
            
            if (wa.image != null)
                image.sprite = wa.image;
            
            if (!string.IsNullOrEmpty(wa.text))
                tutorText.text = wa.text;
            if (!string.IsNullOrEmpty(wa.blueText))
                blueText.text = wa.blueText;
            if (!string.IsNullOrEmpty(wa.yellowText))
                yellowText.text = wa.yellowText;

            //Start owl morph
            if (owlAnimator && !wa.audioname.StartsWith("noaudio"))
                owlAnimator.PlayAnimation();

            //Wait duration of sound
            yield return new WaitForSeconds(wa.duration);

            //Stop owl morph
            if (owlAnimator && !wa.audioname.StartsWith("noaudio"))
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
                      
            }
            // if wrong answer
            if (!lastButtonClicked.Equals(wa.correctAnswer) || wa.finishTutoring || wa.skipNextWa)
                skipCorrectWa = true;

            // wait until 'got it' button clicked. single button appears on the panel now.
            if (wa.waitForButtonClick)
            {
                gotItButton.SetActive(true);
                yield return new WaitUntil(() => gotIt);
                gotItButton.SetActive(false);
                tutorText.text = string.Empty;
            }

            mainPanel.SetActive(false);
            //pause a bit
            yield return new WaitForSeconds(wa.pause);        

             // Stop Tutoring
            if (wa.finishTutoring)
                CheckerManager.Instance.readyToExitPresentationMode = wa.finishTutoring;
                
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
                break;
            }

            if (wa.audioname.StartsWith("final_no"))
            {
                CheckerManager.Instance.MakeIsland(idOKDictionary[colorToCheck]-1);
                owlIsSpeaking = false;
            }
        }
    }

    public void StopAllCoroutinesAfterCorrectAnswer()
    {
        StopAllCoroutines();
    }

    public void FinishTutoring()
    {
        tutoringCanvas.SetActive(false);
        instructionText.text = string.Empty;
        instructionText.gameObject.SetActive(true);
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

    // TUTORIAL SEQUENCE
    IEnumerator PlaySounds()
    {
        gotIt = false;
        Image image = mainPanel.GetComponent<Image>();
        bool skipCorrectWa = false;
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
            Debug.Log("Audioname --> " + wa.audioname);
            ActivateRayInteractors(wa.activateRay);
            // open main panel
            // image.sprite = null;
            // tutoringCanvas.SetActive(wa.openMainPanel);
            // mainPanel.SetActive(wa.openMainPanel); //owl3
            // if (wa.image != null)
            //    image.sprite = wa.image;

            // float rectScaleX = 1;
            // if (wa.audioname.Equals("owl3") || wa.audioname.Equals("owl6") || wa.audioname.Equals("owl10"))
                // rectScaleX = 0.5f;
            // RecalibrateMainPanel(rectScaleX);
            
            if (string.IsNullOrEmpty(wa.audioname))
                continue;
            if (skipCorrectWa)
            {
                skipCorrectWa = false;
                continue;
            }
            //Play explanation
            if (wa.audioname.Equals("correct"))
            {
                AudioManager.Instance.playSound(correct_audio[colorToCheck]);
            }
            else 
            {
                AudioManager.Instance.playSound(wa.audioname);
            }

            //Start owl morph
            if (owlAnimator)
            {
                ActivateTalkingBirds(false);
                owlAnimator.PlayAnimation();
                // owlIsSpeaking = true;
            }

            string anim = wa.startAnimation[0];
            if (buttonDictionary.ContainsKey(anim))
            {
                GameObject animateButton = buttonDictionary[anim];
                animateButton.SetActive(true);
                if (anim.Equals("signs"))
                {
                    leftBuildingAnimator.SetTrigger(anim);
                    rightBuildingAnimator.SetTrigger(anim);
                }
            }
            
            
                
            //Display a info text if there is one
            if (!string.IsNullOrEmpty(wa.instructionText) && instructionText)
            {
                instructionText.text = wa.instructionText;
                instructionText.gameObject.SetActive(true);
            }

            //Wait duration of sound
            yield return new WaitForSeconds(wa.duration);

            //Stop owl morph
            if (owlAnimator)
            {
                ActivateTalkingBirds(true);
                owlAnimator.PauseAnimation();
                // owlIsSpeaking = false;
            }    

            if (wa.audioname.Equals("owl5_b"))
            {
                // mainPanel.SetActive(false);
                // tutoringCanvas.SetActive(false);
                instructionText.text = wa.keyInstructionText;
                instructionText.gameObject.SetActive(true);
                yield return new WaitUntil(() => cubeReleasedTutorial);
            }  
        
            // if (wa.audioname.Equals("owl3") || wa.audioname.Equals("owl6") || wa.audioname.Equals("owl10"))
            // {
            //     mainPanel.SetActive(false);
            //     tutoringCanvas.SetActive(false);
            // }

            if (wa.audioname.Equals("check1"))
            {
                ActivateRayInteractors(true);
            }


            if (wa.finishTutoring)
                CheckerManager.Instance.readyToExitPresentationMode = wa.finishTutoring;
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
            if (wa.audioname.Equals("owl11"))
                {
                    // buttonDictionary[wa.startAnimation[0]].SetActive(false);
                    // if (wa.startAnimation.Length > 1)
                    // {
                    //     buttonDictionary[wa.startAnimation[1]].SetActive(false);
                    // }

                    // rightTouchAnimator.SetTrigger("stop");
                    leftBuildingAnimator.SetTrigger("stop");
                    rightBuildingAnimator.SetTrigger("stop");
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
            if (!wa.finishTutoring)
            {
                ActivateRayInteractors(wa.activateRay);
            }
                
            // show "which color" panel
            if (wa.audioname.Equals("which"))
            {
                tutoringCanvas.SetActive(true);
                whichColorPanel.SetActive(true);
                mainPanel.SetActive(false);
                // wait until CheckIfOk finishes.
                yield return new WaitUntil(() => gotIt);

                // Game finishes. Invoke game event and play last audio
                if (allOK)
                {
                    tutoringCanvas.SetActive(false);
                    break;
                }
                
                int id = idOKDictionary[colorToCheck];
                bool badNeighbors = CheckerManager.Instance.bad_neighbors_color[id];

                AudioManager.Instance.playSound("magic");
                
                //Count how many cubes of that color are in board
                GameObject colorParent = cubeHierarchy.transform.GetChild(id-1).gameObject;
                int childCount = colorParent.transform.childCount;
                if (badNeighbors && (properColorCount[id] == childCount))
                {
                    FinishTutoring();
                    skipCorrectWa = true;
                }
                else if (idOK[id-1])
                {
                    FinishTutoring();
                }
                else
                {
                    tutoringCanvas.SetActive(false);
                    break;
                }
                
            }

            //pause a bit
            yield return new WaitForSeconds(wa.pause);
        }
        foreach (GameObject obj in particleSystems)
        {
            obj.SetActive(false);
        }
        // IF CHECKIFOK is false, then starts TUTORING
        isPlayingSounds = false;
        //Activate the game manager
        if (isTutorial)
        {
            RecalibrateMainPanel(1);
            StartGame();
        }
        else if (activeScene.Equals("Active"))
        {
            int temp = idOKDictionary[colorToCheck] - 1;
            if (!idOK[temp])
                Tutoring();
        }
    }

    void RecalibrateMainPanel(float x)
    {
        RectTransform rect = mainPanel.GetComponent<RectTransform>();
        var scale = rect.localScale;
        scale.x = x;
        rect.localScale = scale;
    }

    public void ReleaseEventTutorial()
    {
        cubeReleasedTutorial = true;
    }

    void StartGame()
    {
        ActivateTalkingBirds(true);
        ActivateRayInteractors(true);
        isActive = false;
        isTutorial = false;
        CheckerManager.Instance.isActive = true;
        owlCollider.enabled = false;
        
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
