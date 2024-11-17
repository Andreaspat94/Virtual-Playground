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
    public List<Wavs> wavNoSpace = new List<Wavs>();
    public List<Wavs> confirmAudioList = new List<Wavs>();
    public List<Wavs> selfReflectionWavList = new List<Wavs>();
    public List<Wavs> redWavList = new List<Wavs>();
    public List<Wavs> blueWavList = new List<Wavs>();
    public List<Wavs> greenWavList = new List<Wavs>();
    public List<Wavs> yellowWavList = new List<Wavs>();
    public List<Wavs> orangeWavList = new List<Wavs>();

    public GameObject[] listWithObjectsForTutorial;
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
        {"Blue", 1},
        {"Yellow", 2},
        {"Orange", 3},
        {"Green", 4},
        {"Red", 5},
        {"Grey", 6}
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
        {"Red", "correct_red"},
        {"Blue", "correct_blue"},
        {"Green", "correct_green"},
        {"Yellow", "correct_yellow"},
        {"Orange", "correct_orange"}
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
    string displayText;
    public Collider owlCollider;
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
            {"TriggerLeft", controllerButtons[2]},
            {"Y", controllerButtons[3]}
        };

        wavLists = new Dictionary<string, List<Wavs>> 
        {
            {"Blue", blueWavList},
            {"Yellow", yellowWavList},
            {"Orange", orangeWavList},
            {"Green", greenWavList},
            {"Red", redWavList}
        };
        whichColorPanel = tutoringCanvas.transform.GetChild(0).GetChild(0).gameObject;
        mainPanel = tutoringCanvas.transform.GetChild(0).GetChild(0).gameObject;
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

    //This is called afer clicking on the owl, it starts the Tutorial sequence
    public void StartPlayingTheSounds()
    {
        owlIsSpeaking = true;
        GetComponent<Collider>().enabled = false;
        owlCollider.enabled = false;

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
        
        //Press 'Y' to skip intro & other tutorials.
        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            AudioManager.Instance.ResetSound();
            StopAllCoroutines();
            owlAnimator.PauseAnimation();
            FinishTutoring();
            CheckerManager.Instance.readyToExitPresentationMode = true;
            tutoringCanvas.SetActive(false);
            foreach (GameObject obj in particleSystems)
            {
                obj.SetActive(false);
            }
            if (isTutorial)
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

    public void ResetTheOwl()
    {
        GetComponent<Collider>().enabled = true;
        owlAnimator.GetComponent<Collider>().enabled = true;
        owlOutline.SetActive(true);
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
        List<Wavs> wavList = new List<Wavs>();
        if (activeScene.Equals("Active"))
        {
            wavList = wavLists[colorToCheck];
        }
        else if (activeScene.Equals("Passive"))
        {
            wavList = selfReflectionWavList;
        }

        StartCoroutine(PlayTutoringSequence(wavList));
    }

    public void GotIt()
    {
        gotIt = true;
    }

    public void NoSpace() 
    {
        StartCoroutine(PlayTutoringSequence(wavNoSpace));
    }

    public void ChooseBetween(string buttonName)
    {
        lastButtonClicked = buttonName;
    }

    // This is ACTIVE SCENE FEEDBACK sequence
    IEnumerator PlayTutoringSequence(List<Wavs> wavList)
    {
        if (activeScene.Equals("Active"))
            yield return new WaitForSeconds(2f);
        ActivateTalkingBirds(false);
        
        Image image = mainPanel.GetComponent<Image>();
        Text tutorText = mainPanel.transform.GetChild(1).GetComponent<Text>();
        Text blueText = mainPanel.transform.GetChild(2).GetComponent<Text>();
        Text yellowText = mainPanel.transform.GetChild(3).GetComponent<Text>();
        lastQuestionObjects = mainPanel.transform.GetChild(4).gameObject;
        bool skipCorrectWa = false;
        bool skipBadNeighbors = false;
        
        foreach (Wavs wa in wavList)
        {    
            if (string.IsNullOrEmpty(wa.audioname))
                continue;

            if (skipCorrectWa) 
            {
                skipCorrectWa = false;
                continue;
            }

            if (wa.audioname.Equals("bad_neighbors") && !skipBadNeighbors)
            {
                continue;
            }

            Debug.Log("Audiowav --> " + wa.audioname);
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
            
            ActivateAnimation(wa.startAnimation, true);
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
            if ((!lastButtonClicked.Equals(wa.correctAnswer) && activeScene.Equals("Active"))|| wa.finishTutoring || wa.skipNextWa)
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

            if (wa.audioname.StartsWith("final_no"))
                // || (wa.audioname.Equals("general1") && lastButtonClicked.Equals(wa.correctAnswer)))
            {
                CheckerManager.Instance.MakeIsland(idOKDictionary[colorToCheck]-1);
                CheckerManager.Instance.readyToExitPresentationMode = true;
                FinishTutoring();
                yield break;
            }
            
            //this is for passive scene
            if (wa.audioname.StartsWith("noaudio") && activeScene.Equals("Passive"))
            {
                if(lastButtonClicked.Equals(wa.correctAnswer))
                {
                    CheckerManager.Instance.readyToExitPresentationMode = true;
                    FinishTutoring();
                    CheckerManager.Instance.MakeAllCubesInteractable(true, "all");
                    yield break;
                }
                else
                {
                    // skipCorrectWa = true;
                    CheckerManager.Instance.CheckIfOK();
                    yield return new WaitForSeconds(1f);
                    if (allOK)
                        yield break;

                    int id = idOKDictionary[colorToCheck];
                    bool badNeighbors = CheckerManager.Instance.bad_neighbors_color[id];
                    GameObject colorParent = cubeHierarchy.transform.GetChild(id-1).gameObject;
                    int childCount = colorParent.transform.childCount;
                    
                    if (!(properColorCount[id] == childCount))
                    {
                        FinishTutoring();
                        skipCorrectWa = true;
                    }
                    else if (badNeighbors)
                    {
                        FinishTutoring();
                        skipCorrectWa = true;
                        skipBadNeighbors = true;
                    }
                    else if (idOK[id-1])
                    {
                        FinishTutoring();
                    }
                }
            }
        
            ActivateAnimation(wa.startAnimation, false);
            
            //pause a bit
            yield return new WaitForSeconds(wa.pause);        

             // Stop Tutoring
            if (wa.finishTutoring)
            {
                CheckerManager.Instance.readyToExitPresentationMode = wa.finishTutoring;
                FinishTutoring();
                yield break;
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
                break;
            }
        }
    }

    void ActivateAnimation(string[] animationArray, bool hasAnimations) 
    {
        if (animationArray.Length != 0)
        {
            for (int i=0; i < animationArray.Length; i++)
            {
                string anim = animationArray[i];
                // Debug.Log("buttonDictionary.ContainsKey --> " + buttonDictionary.ContainsKey(anim));
                if (buttonDictionary.ContainsKey(anim))
                    buttonDictionary[anim].SetActive(hasAnimations);           
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
        owlCollider.enabled = true;
        GetComponent<Collider>().enabled = true;
        CheckerManager.Instance.inSequence = false;
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
        bool skipBadNeighbors = false;
        string lastCubeGrabbed;
        List<Wavs> wavList = new List<Wavs>();
        if (isTutorial)
        {
            wavList = wayPointList;
        }
        else if (activeScene.Equals("Passive"))
        {
            wavList = confirmAudioList;
            lastCubeGrabbed = CheckerManager.Instance.lastCubeGrabbed;
            colorToCheck = lastCubeGrabbed.Remove(lastCubeGrabbed.Length-4);
            int id = idOKDictionary[colorToCheck];
        }
        else
        {
            wavList = confirmAudioList;
            //checkIfOK
            lastCubeGrabbed = CheckerManager.Instance.lastCubeGrabbed;
            colorToCheck = lastCubeGrabbed.Remove(lastCubeGrabbed.Length-4);
            CheckerManager.Instance.CheckIfOK();
            yield return new WaitForSeconds(1f);
            if (allOK)
            {
                mainPanel.SetActive(false);
                yield break;
            }
                
            int id = idOKDictionary[colorToCheck];
            bool badNeighbors = CheckerManager.Instance.bad_neighbors_color[id];

            GameObject colorParent = cubeHierarchy.transform.GetChild(id-1).gameObject;
            int childCount = colorParent.transform.childCount;
            if (!(properColorCount[id] == childCount))
            {
                FinishTutoring();
                skipCorrectWa = true;
            }
            else if (badNeighbors)
            {
                FinishTutoring();
                skipCorrectWa = true;
                skipBadNeighbors = true;
            }
            else if (idOK[id-1])
            {
                FinishTutoring();
            }
            else
            {
                tutoringCanvas.SetActive(false);
                //break;
            }
        }
        
        //for each entry in the text, each entry if a text file
        foreach (Wavs wa in wavList)
        {
            ActivateRayInteractors(wa.activateRay);
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
            else if (wa.audioname.Equals("bad_neighbors") && !skipBadNeighbors)
            {
                continue;
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
            }

            ActivateAnimation(wa.startAnimation, true);
                
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
            }    

            if (wa.audioname.Equals("owl5_b"))
            {
                instructionText.text = wa.keyInstructionText;
                instructionText.gameObject.SetActive(true);
                CheckerManager.Instance.MakeAllCubesInteractable(true, "onlyPool");
                yield return new WaitUntil(() => cubeReleasedTutorial);
            }
            
            if (wa.audioname.Equals("owl1"))
                CheckerManager.Instance.tutorialClickOwl = false;
            
            if (wa.audioname.Equals("owl3"))
            {
                instructionText.text = wa.keyInstructionText;
                instructionText.gameObject.SetActive(true);
                owlCollider.enabled = true;
                CheckerManager.Instance.timeToChangeModeForTutorial = true;
                yield return new WaitUntil(() => CheckerManager.Instance.tutorialClickOwl);
            }

            if (wa.audioname.Equals("owl4"))
                CheckerManager.Instance.MakeAllCubesInteractable(false, "all");    
            
            if(wa.audioname.Equals("owl11"))
                CheckerManager.Instance.MakeAllCubesInteractable(true, "all");

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

            ActivateAnimation(wa.startAnimation, false);

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
            
            if (wa.audioname.StartsWith("bad_neighbors"))
            {
                FinishTutoring();
                yield break;
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
        if (isTutorial)
        {
            RecalibrateMainPanel(1);
            StartGame();
        }
        else
        {
            int temp = idOKDictionary[colorToCheck] - 1;
            if (activeScene.Equals("Active"))
            {
                if (!idOK[temp])
                    Tutoring();
            }
            else
            {
                Tutoring();
            }
        }
    }

    // TUTORIAL - switch to construction mode
    public void ListWithObjectsForTutorial()
    {
        listWithObjectsForTutorial[0].SetActive(true);
        listWithObjectsForTutorial[1].SetActive(false);
        listWithObjectsForTutorial[2].SetActive(true);
        listWithObjectsForTutorial[3].SetActive(true);
        CheckerManager.Instance.countCoveredBlocksUI();
        CheckerManager.Instance.constructionModeOnOff.text = "on";
        owlCollider.enabled = false;
        AudioManager.Instance.playSound("magic");
        CheckerManager.Instance.MakeAllCubesInteractable(false, "onlyPool");
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
        CheckerManager.Instance.constructionModeOnOff.text = "on";
        CheckerManager.Instance.timeToChangeModeForTutorial = true;
        CheckerManager.Instance.readyToExitPresentationMode = false;
        CheckerManager.Instance.inSequence = false;
        ActivateTalkingBirds(true);
        ActivateRayInteractors(true);
        isActive = false;
        isTutorial = false;
        CheckerManager.Instance.isActive = true;
        owlCollider.enabled = true;

        // CheckerManager.Instance.constructionModeOnOff.text = "on";
        
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
