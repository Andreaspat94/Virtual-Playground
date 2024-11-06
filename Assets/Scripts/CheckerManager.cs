using UnityEngine;
using HoloToolkit.Unity;
using UnityEngine.Events;
using System.Collections;
using Oculus.Interaction;
using System.Collections.Generic;
using System;
using TMPro;

public class CheckerManager : Singleton<CheckerManager>
{
    // List of object that are OK
    [SerializeField]
    GameObject cubeHierarchy;
    public string lastCubeGrabbed = "";
    [Header("Info needed to switch into cube interaction mode")]
    public GameObject staticObjects = null;
    public Transform vantagePoint = null;
    Vector3 previousFPSPos;
    bool isZoomOutViewMode = false;

    public enum ViewModes { CUBE_INTERACTION, PRESENTATION };
    [Header("The checker view mode")]
    public ViewModes view_mode_ = ViewModes.CUBE_INTERACTION;
    bool canChangeViewMode = true; //To  prevent  switch when cube drops through physics

    //To show how many cubes are placed
    public TextMeshProUGUI[] totalCoveredTilesTextsUI;

    [Header("Reticle to show when in cube interaction mode")]
    public GameObject ui_canvas = null;
    public GameObject helpPanel = null;
    public GameObject ui_collider = null;

    //Dimensions of Checker Grid
    const int XDim = 20;
    const int YDim = 10;

    [Header ("Red Highlight Tile for cube placement")]
    public Transform redTile = null;
    [Header("Corner of Checkerboard designating 0,0")]
    public Transform cornerCheckerboard = null;

    //Used to swich on off then entering the presentation mode
    [Header("Talking Birds Parent")]
    public GameObject talkingBirds = null;
    public GameObject owlBird = null;

    //The interactible cubes have a "CubeInteraction" layer mask and are the big cubes placed on the checker board.
    //Clicking on them enable the carry cube, destroys the current instance and set the chekker array to zero
    //They are generated from prefabs Prefabs/**base_collider_static pointed to form the CubesArray[]
    [Header("RayMask for the dynamic interactible cubes")]
    public LayerMask cubeInteractionMask;
    
    private string interactionCubeTag = "InteractionCube";
    private string poolCubeTag = "PoolCube";
    //The cubes in the pool have a "CubePool" layer mask and are the cubes in the pool to be grabbed
    //They remain static in the pool and clicking on them just enable the carry cube
    [Header("RayMask for the interactible pool cubes")]
    public LayerMask cubePoolMask;
    // 11 means "CubePool" layer
    private int poolCubeLayerInteger = 11;
    
    //Mask set to "InteractionGeneralObject" layer mask and designete general interaction objects like the birds
    //They issue events when pointed at and clicked upon
    public LayerMask interactionObjectsMask;
    
    // This is for 'CreateStaticCube' method
    GameObject clone;
    OnInteraction currentInteractionObject = null;

    //Game is finished well done
    bool playGroundFinished = false;

    [Header("Is Manager Active")]
    public bool isActive = false;

    private GameObject fps_controller;
    private SimpleCapsuleWithStickMovement movementScript;
    private Rigidbody rb;
    private bool isExitViewModeOn;
    public StartupTutorial startupTutorial;
    private Canvas canvas;
    private float delayTime;
    [HideInInspector]
    public bool fadeOut;
    private bool pickedUpCounter = true;
    public bool inSequence;
    [HideInInspector]
    public bool readyToExitPresentationMode = false;

    //In general when a cube is grabed the carycube representation is active (is under Camera).
    //Then when droped carycube becomes inactive and dropcube is activated.
    //When dropcube is colliding with checker it becomes inactive and a prefabStaticCube is created in the respective checker pos
    [System.Serializable]
    public class Cubes
    {
        public string name; //nameID of cube
        public Dropper dropCube; //Represenation that is used when droping a cube has physics
        public GameObject prefabStaticCube; //Instance created when drop cube has hit the checkerboard
        public GameObject prefabPoolCube; // Instance created when we pick up cube from pool and replace the existed one
    }

    [System.Serializable]
    public class PlayGroundObjects
    {
        public string color; //color of cubes that identify the playground object
        public GameObject playgroundObject; //Represenation of the playground object
        public GameObject multiCubeRepresentation; //Parent of all interactioncubes of the same color
    }
    
    //This array has all the possible cubes we need and pointers to them.
    //the activeCubeIndex indexes into this array to indicate which cube is currently active carried
    [Header("Array holding all possible cubes")]
    public Cubes[] cubesArray = null;

    //This array points to the real object representation of a cube (say slide) and to the parent
    //holding all cubes of the same color. Its indexes and color order are the same as the indexes in the chekker array.
    //Its used to switch between presentation modes and to place new cubes into the right hierarchy
    [Header("Array holding all Playground objects")]
    public PlayGroundObjects[] playGroundObjectsArray = null;
    [Header("Object pool hierarchy")]
    public GameObject objectPool;
    [Header("Index into array of active cube or -1")]
    public int activeCubeIndex = -1;

    [Header("Events executed when playground is ready")]
    //Is called when Playground is fixed
    public UnityEvent GameWonEvent;

    [HideInInspector]
    public OneGrabFreeTransformer pickedCubeTransformer;

    public Dictionary<int, bool> bad_neighbors_color = new Dictionary<int, bool>
    {
        {1, false},
        {2, false},
        {3, false},
        {4, false},
        {5, false},
        {6, true}
    };
    private CubeNameID idOfCube;
    private GameObject cubePickedUp;
    private int adjacencyError = -1;
    
    private Vector3 offset;
    private int x, y, X, Y;
    //Slide:1, MonkeyBars:2, CrawlTunnel:3, RoundAbout:4, Swings:5, SandPit:6, 
    //         Pavement:7, Bench:8, Fence:9, Cube pool:10, Flower bed: 11
    //array x is Z (unity), array y is X Unity
    public int[,] checkkerArray = new int[YDim, XDim] {
        {9, 9, 9, 9, 9, 9, 9, 9, 9,   8,  8, 9, 9, 9, 9, 9, 9, 9, 9, 9},
        {9, 0, 1, 1, 1, 1, 1, 0, 7,   7,  7, 7, 0, 0, 6, 6, 6, 6, 0, 9},
        {9, 0, 1, 1, 1, 1, 1, 7, 7,   7,  7, 7, 7, 0, 6, 6, 6, 6, 0, 8},
        {9, 0, 0, 0, 0, 0, 0, 7, 10, 10, 10, 10,7, 0, 6, 6, 6, 6, 0, 8},
        {9, 0, 2, 0, 0, 0, 0, 7, 10, 10, 10, 10,7, 0, 0, 0, 0, 0, 0, 9},
        {9, 0, 2, 4, 4, 4, 4, 7, 10, 10, 10, 10,7, 0, 0, 0, 5, 5, 5, 9},
        {9, 0, 2, 4, 4, 4, 4, 7, 7,   7,  7, 7, 7, 0, 0, 0, 5, 5, 5, 9},
        {8, 0, 2, 4, 4, 4, 4, 7, 7,   7,  7, 7, 7, 0, 0, 0, 5, 5, 5, 9},
        {8, 0, 2, 4, 4, 4, 4, 0, 0,   7,  7, 0, 0, 0, 0, 0, 5, 5, 5, 9},
        {9, 9, 9, 9, 9, 9, 9, 9, 9,   7,  7, 9, 9, 9, 9, 9, 9, 9, 9, 9}};

    //Returns the int id for a cube name for the checkkerArray
    int GetCubeId(string name)
    {
        int cubeIdFound = 0;

        switch (name)
        {
            case "BlueCube": cubeIdFound=1; break; //Slide cube
            case "YellowCube": cubeIdFound=2; break; //monkey bars cube
            case "OrangeCube": cubeIdFound=3; break; //crawl tunnel cube
            case "GreenCube": cubeIdFound=4; break;  //round about cube
            case "RedCube": cubeIdFound = 5; break;  //swings cube
            case "GreyCube": cubeIdFound = 6; break; //sandpit cube
        }

        return cubeIdFound;
    }
    string GetCubeName(int ID)
    {
        string cubeNameFound = "None";

        switch (ID)
        {
            case 1: cubeNameFound = "BlueCube";  break; //Slide cube
            case 2: cubeNameFound = "YellowCube";  break; //monkey bars cube
            case 3: cubeNameFound = "OrangeCube"; break; //crawl tunnel cube
            case 4: cubeNameFound = "GreenCube";  break;  //round about cube
            case 5: cubeNameFound = "RedCube"; break;  //swings cube
            case 6: cubeNameFound = "GreyCube";  break; //sandpit cube
        }

        return cubeNameFound;
    }

    // Use this for initialization
    void Start()
    {
        activeCubeIndex = -1;
        fps_controller = GameObject.FindGameObjectWithTag("Player");
        movementScript = fps_controller.GetComponent<SimpleCapsuleWithStickMovement>();
        rb = fps_controller.GetComponent<Rigidbody>();
        canvas = ui_canvas.GetComponent<Canvas>();

        //CreateStatic cubes from initial Chekker array
        for (int x = 0; x < YDim; x++)
        {
            for (int y = 0; y < XDim; y++)
            {
                // If there is a colour stored in the matrix
                if (checkkerArray[x, y] != 0 && checkkerArray[x, y] <= 6)
                {
                    CreateStaticCube(GetCubeName(checkkerArray[x, y]), x, y, false);
                }
            }
        }

        ResetToCubeRepresentation();
    }

    /// <summary> 
    /// Call this to enable a carrying cube and set the activeCubeIndex
    /// </summary>
    public void EnableCube(string cubeId)
    {
        if (activeCubeIndex != -1) return;

        for (int i=0; i<cubesArray.Length; i++)
        {
            if (cubesArray[i].name == cubeId)
            {
                AudioManager.Instance.playSound("pickBlock");
                
                //Set active index and enable cube gameobject under the Controller hierarchy
                activeCubeIndex = i;
            }
        }
    }

    //Add a static cube to the checker board.
    //Creates an instance and adds the entry to the checkker array
    //Is called form Dropper after collision or from Start() to creat initial cubes
    public void CreateStaticCube(string cubeId, int x, int y, bool isPoolCube)
    {
        //DropCube finished now we can enable presentation switch
        canChangeViewMode = true;
        GameObject prefabCube;
        foreach (Cubes cb in cubesArray)
        {
            if (isPoolCube)
            {
                prefabCube = cb.prefabPoolCube;
            }
            else
            {
                prefabCube = cb.prefabStaticCube;
            }
            // if (isPoolCube) ? prefabCube = cb.prefabPoolCube : prefabCube = cb.prefabStaticCube;
        
            if (cb.name == cubeId && prefabCube != null)
            {
                //Compute world position of new cube offseted by (0.5,0.5) since cube is 1x1 and pivot is base center
                Vector3 positionNewCube = cornerCheckerboard.position + cornerCheckerboard.right * (x + 0.5f) + cornerCheckerboard.forward * (y + 0.5f);

                //Add cube to world using the prefab for the static intersection cubes
                clone = Instantiate(prefabCube, positionNewCube, Quaternion.identity);

                //Add x,y checker position info to instatiated cube and add to matrix
                if (clone != null)
                {
                    //Get script class of interation cube 
                    CubeNameID cubeScript = clone.GetComponentInChildren<CubeNameID>();
                    if (cubeScript)
                    {
                        //store the x,y coords usefull for pickup again
                        cubeScript.xCheckerArrayCoord = x;
                        cubeScript.yCheckerArrayCoord = y;

                        //enter entry to checker array
                        if (!isPoolCube)
                        {
                            checkkerArray[x, y] = GetCubeId(cubeId);
                        }

                        //Search through list to find under which hierarchy to place the cube
                        foreach (PlayGroundObjects pg in playGroundObjectsArray)
                        {
                            if (pg.color == cubeId && pg.multiCubeRepresentation != null)
                            {
                                if (clone.tag == poolCubeTag)
                                {
                                    RegulateCubePosition();
                                }
                                else if (clone.tag == interactionCubeTag)
                                {
                                    clone.transform.parent = pg.multiCubeRepresentation.transform;
                                }
                            }
                        }
                    }
                    break;
                }
            }
        } //foreach
    }

    void RegulateCubePosition()
    {
        clone.tag = poolCubeTag;
        clone.layer = 11;  // 11 means "CubePool" layer
        clone.transform.parent = objectPool.transform;
        clone.transform.GetChild(0).gameObject.layer = 11;
        clone.transform.GetChild(0).gameObject.tag = poolCubeTag;
        clone.transform.position = cubePickedUp.transform.position;
    }

    //Show interaction cubes again and hide playground objects for continuiing interaction
    public void ResetToCubeRepresentation()
    {
        view_mode_ = ViewModes.CUBE_INTERACTION;
        if (talkingBirds) talkingBirds.SetActive(true);
        if (owlBird) owlBird.SetActive(true);

        foreach (PlayGroundObjects pg in playGroundObjectsArray)
        {
            if (pg.multiCubeRepresentation != null)
                pg.multiCubeRepresentation.SetActive(true);

            if (pg.playgroundObject != null)
                pg.playgroundObject.SetActive(false);
        }
    }

    //Count how many tiles are covered with colored cubes
    string CountTotalCoveredTilesInCheckkerArray()
    {
        int totalTiles = 0;
        for (int x = 0; x < YDim; x++)
            for (int y = 0; y < XDim; y++)
                if (checkkerArray[x,y] != 0 && checkkerArray[x,y] <= 6)
                    totalTiles++;

        return totalTiles.ToString();
    }

    //Here we test out the chekker for correctness
    //We create arrays and assume the following indexes represent the folowing cubes (same color indexes as chekker array)
    //Slide-Blue: 0 (value in chekker array 1) 
    //MonkeyBars-Yellow:1 (value in chekker array 2)
    //CrawlTunnel-OrangeBrown:2 (value in chekker array 3), 
    //RoundAbout-Green:3 (value in chekker array 4), 
    //Swings-Red:4 (value in chekker array 5), 
    //SandPit-Grey:5 (value in chekker array 6)
    public void CheckIfOK()
    {
        //designamte if this checker was already tested
        int[,] localMatrix = new int[YDim, XDim];
        // Only six types of cubes in matrix supported
        // Has already been checked for
        bool[] idCheck = new bool[6] { false, false, false, false, false, false };

        // Is OK
        bool[] idOK = new bool[6] { false, false, false, false, false, false };
        for(int i=0; i < bad_neighbors_color.Count-1; i++)
        {
            bad_neighbors_color[i] = false;
        }
        
        // How each type of cube should be aligned in matrix
        int[,] colorSizes = new int[6, 2]
                 {
                   {2, 6}, // blue-slide
                   {2, 1}, // yellow-monkey-bars
                   {2, 2}, // orange-crawl tunnel
                   {2, 2}, // green-roind about
			       {4, 4}, // red-swings
                   {3, 4}, // grey-sandpit
			     };

        AudioManager.Instance.playSound("magic");
   
        //Color values available Slide:1, MonkeyBars:2, CrawlTunnel:3, RoundAbout:4, Swings:5, SandPit:6
        //Traverse all checkker array
        for (int x = 0; x < YDim; x++)
        {
            for (int y = 0; y < XDim; y++)
            {
                int whichColor;

                // If there is a colour stored in the matrix
                if (checkkerArray[x, y] != 0 && checkkerArray[x, y] <= 6)
                {
                    whichColor = checkkerArray[x, y];
                    // If this entry has not been checked before skip
                    if (localMatrix[x, y] == 0)
                    {
                        // If this colour has already been tested for
                        if (!idCheck[whichColor - 1])
                        {                          
                            bool okNorm = false, okRotate = false;
                            
                            // Test normal area    
                            okNorm = testArea(x, y, colorSizes[whichColor - 1,0], colorSizes[whichColor - 1,1], whichColor, localMatrix);
                            // Test are rotate 90 Degrees if normal test does not succeed
                            if (!okNorm)
                                okRotate = testArea(x, y, colorSizes[whichColor - 1,1], colorSizes[whichColor - 1,0], whichColor, localMatrix);
                            // if (whichColor == 4)
                                // Debug.Log("okNorm--> "  + okNorm + " ...OkRotate--> " + okRotate + "... Color --> " + whichColor);
                            if (okNorm || okRotate)
                            {
                                // Set as thing OK for display
                                idOK[whichColor - 1] = true;

                                // Position things into place
                                // Compute center postion in Grid Quad.
                                Vector3 pos;
                                Quaternion rot = Quaternion.identity;

                                if (okNorm)
                                {
                                    Vector3 offset = cornerCheckerboard.right * (x + colorSizes[whichColor - 1, 0] * 0.5f) + cornerCheckerboard.forward * (y + colorSizes[whichColor - 1, 1] * 0.5f);
                                    pos = cornerCheckerboard.position + offset;
                                }
                                else
                                {
                                    Vector3 offset = cornerCheckerboard.right * (x + colorSizes[whichColor - 1, 1] * 0.5f) + cornerCheckerboard.forward * (y + colorSizes[whichColor - 1, 0] * 0.5f);
                                    pos = cornerCheckerboard.position + offset;
                                    rot = Quaternion.Euler(0, 90, 0);
                                }

                                //Now position real geometry
                                string colorName = GetCubeName(whichColor);
                                foreach (PlayGroundObjects pg in playGroundObjectsArray)
                                {
                                    if (pg.color == colorName && pg.playgroundObject != null)
                                    {
                                        pg.playgroundObject.transform.position = pos;
                                        pg.playgroundObject.transform.rotation = rot;
                                        break;
                                    }
                                }
                            }
                            else
                            {  
                                startupTutorial.idOK[whichColor-1] = false;
                                idOK[whichColor-1] = false;
                            }//okNorm || okRotate

                            //we already processed an island of this color
                            idCheck[whichColor - 1] = true;
                        }
                        else
                        {
                            //This color is not ready we have processed it before there must be multiple islands
                            // if comment this out, yellow object will appear with 5 cubes
                            idOK[whichColor - 1] = false;
                        }
                    }
                } // if a colour stored in matrix

            }//y //Traverse all checkker array
        }//x //Traverse all checkker array

        //Now do final check and show playground objects if ok or leave as cubes
        bool allOK = true;

        //Set to default value and see if there is any cube island that is correct, 
        //if no island correct do not swith to Presentation mode 
        view_mode_ = ViewModes.CUBE_INTERACTION;

        // Check trough all the colors and switch geometry on/off.
        for (int i = 0; i < playGroundObjectsArray.Length; i++)
        {
            if (idOK[i])
            {
                // update startuptutorial dictionary
                startupTutorial.idOK[i] = true;
                // Debug.Log("idOK[i] " + i + "--> " + idOK[i]);
                // startupTutorial.GotIt();
                if (playGroundObjectsArray[i].multiCubeRepresentation != null)
                    playGroundObjectsArray[i].multiCubeRepresentation.SetActive(false);

                if (playGroundObjectsArray[i].playgroundObject != null)
                    playGroundObjectsArray[i].playgroundObject.SetActive(true);

                if (talkingBirds) talkingBirds.SetActive(false);

                //There is at least one cube island ok goto presentation mode
                view_mode_ = ViewModes.PRESENTATION;
            }
            else
            {
                if (playGroundObjectsArray[i].multiCubeRepresentation != null)
                    playGroundObjectsArray[i].multiCubeRepresentation.SetActive(true);

                if (playGroundObjectsArray[i].playgroundObject != null)
                    playGroundObjectsArray[i].playgroundObject.SetActive(false);

                if (talkingBirds) talkingBirds.SetActive(false);
                allOK = false;
            }
        }
        
        startupTutorial.GotIt();
        
        if (allOK)
        {
            startupTutorial.allOK = true;
            if (talkingBirds)
                talkingBirds.SetActive(false);

            if (GameWonEvent != null)
            {
                GameWonEvent.Invoke();
                // StartCoroutine(PlayLastAudio());        
            }
            playGroundFinished = true;
        }
    }
    
    IEnumerator PlayLastAudio()
    {
        yield return new WaitForSeconds(5.5f);
        //!!TODO change it to correct audio
        AudioManager.Instance.playSound("finish");
    }

    // This logic erases all cubes of the correct's answer color and replace it with the correct island
    public void MakeIsland(int id)
    {  
        ResetTheCubeNumberOfColor(id);
        CheckIfOK();
    }

    void MakeAllCubesInteractable(bool areInteractable)
    {
        RayInteractable[] scripts = FindObjectsOfType<RayInteractable>();
        foreach(RayInteractable scr in scripts)
        {
            if (scr.gameObject.name.StartsWith("HandGrabInter"))
            {
                scr.GetComponent<RayInteractable>().enabled = areInteractable;
            }
            
        }
    }

    public void ResetTheCubeNumberOfColor(int id)
    {
        //the real color id
        // id--;
        // 1) hide all cubes of that color 
        EraseCubesOfColor(id);
        // 2) CreateStaticCubes 
        CreateIsland(id);
    }
    
    public void EraseCubesOfColor(int id)
    {
        GameObject colorParent = cubeHierarchy.transform.GetChild(id).gameObject;
        int childCount = colorParent.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject childCube = colorParent.transform.GetChild(i).gameObject;
            if (childCube != null)
                Destroy(childCube);
        } 

        // int id2 = ++id;
        EraseColorFromChekkerArray(++id);
    }

    private void EraseColorFromChekkerArray(int id)
    {
        for (int x = 0; x < YDim; x++)
        {
            for (int y = 0; y < XDim; y++)
            {
                if (checkkerArray[x, y] == id)
                    checkkerArray[x, y] = 0;
            }
        }
    }

    void CreateIsland(int id)
    {
        if (id == 4)
        {
            // Debug.Log("Create an island of REDS-->");
            CreateStaticCube("RedCube", 5, 15, false);
            CreateStaticCube("RedCube", 5, 16, false);
            CreateStaticCube("RedCube", 5, 17, false);
            CreateStaticCube("RedCube", 5, 18, false);
            CreateStaticCube("RedCube", 6, 15, false);
            CreateStaticCube("RedCube", 6, 16, false);
            CreateStaticCube("RedCube", 6, 17, false);
            CreateStaticCube("RedCube", 6, 18, false);
            CreateStaticCube("RedCube", 7, 15, false);
            CreateStaticCube("RedCube", 7, 16, false);
            CreateStaticCube("RedCube", 7, 17, false);
            CreateStaticCube("RedCube", 7, 18, false);
            CreateStaticCube("RedCube", 8, 15, false);
            CreateStaticCube("RedCube", 8, 16, false);
            CreateStaticCube("RedCube", 8, 17, false);
            CreateStaticCube("RedCube", 8, 18, false);
        }
        else if (id == 0)
        {
            // Debug.Log("Create an island of BLUES-->");
            CreateStaticCube("BlueCube", 1, 1, false);
            CreateStaticCube("BlueCube", 1, 2, false);
            CreateStaticCube("BlueCube", 1, 3, false);
            CreateStaticCube("BlueCube", 1, 4, false);
            CreateStaticCube("BlueCube", 1, 5, false);
            CreateStaticCube("BlueCube", 1, 6, false);
            CreateStaticCube("BlueCube", 2, 1, false);
            CreateStaticCube("BlueCube", 2, 2, false);
            CreateStaticCube("BlueCube", 2, 3, false);
            CreateStaticCube("BlueCube", 2, 4, false);
            CreateStaticCube("BlueCube", 2, 5, false);
            CreateStaticCube("BlueCube", 2, 6, false);
        } else if (id == 3)
        {
            // Debug.Log("Create an island of GREENS-->");
            CreateStaticCube("GreenCube", 8, 4, false);
            CreateStaticCube("GreenCube", 8, 5, false);
            CreateStaticCube("GreenCube", 7, 4, false);
            CreateStaticCube("GreenCube", 7, 5, false);
        } else if (id == 1)
        {
            Debug.Log("Create an island of YELLOWS-->");
            CreateStaticCube("YellowCube", 6, 2, false);
            CreateStaticCube("YellowCube", 7, 2, false);
        } else if (id == 2)
        {
            Debug.Log("Create an island of ORANGES-->");
            CreateStaticCube("OrangeCube", 4, 6, false);
            CreateStaticCube("OrangeCube", 4, 5, false);
            CreateStaticCube("OrangeCube", 5, 6, false);
            CreateStaticCube("OrangeCube", 5, 5, false);
        }
    }

    //Test continuity of a specific color objects
    bool testArea(int x, int y, int sizex, int sizey, int ID, int[,] localMatrix )
    {   
        bool localBadNeighbors = false;
        int areax = x + sizex;
        int areay = y + sizey;

        if (((x + sizex) > YDim) || ((y + sizey) > XDim))
            return false;

        for (int iy = y; iy < areay; iy++)
        {
            for (int jx = x; jx < areax; jx++)
            {
                localMatrix[jx, iy] = 1;   
                localBadNeighbors = adjacentCheck(jx,iy, ID); 
                if (!bad_neighbors_color[ID] && localBadNeighbors)
                {
                    bad_neighbors_color[ID] = true;
                }
                
                //If this field has a different ID or if not a color cube or has bad neighbours Not OK
                if ((checkkerArray[jx, iy] != ID) || (ID > 6) || localBadNeighbors)
                {
                    return false;
                }
                
                // This is to prevent: 
                // 1) place cubes in final_no of different colors on top of each other
                // 2) inform in any case of bad neighbors with audio 
               
            }
        }
        return true;
    }

    //Returns true if there are adjacent cubes with different color and the cube can not be placed
    bool adjacentCheck(int x, int y, int ID)
    {
        int xlocal, ylocal;
        // this represents the 8 spots surround the placed cube.
        int[,] offsetArray = new int[8,2] {{1, 0}, {1, 1}, {0, 1}, {-1, 1}, {-1, 0}, {-1, -1}, {0, -1}, {1, -1}};
    
        for (int i = 0; i<8; i++)
        {
	        xlocal = x + offsetArray[i,0];
	        ylocal = y + offsetArray[i,1];
	
	        if (xlocal<YDim && xlocal>=0 && ylocal<XDim && ylocal>=0)
            {
                //Check if adjacent ID is different and not 0:empty,7:pavement, 9:fence
	            if ((checkkerArray[xlocal, ylocal] != ID) && 
                    (checkkerArray[xlocal, ylocal] != 0) &&
                    (checkkerArray[xlocal, ylocal] != 7) &&
                    (checkkerArray[xlocal, ylocal] != 9))
                    return true;
	        }
        }
        return false;
    }

    int forbiddenCheck(int x, int y, int CubeID)
    {
        int xlocal, ylocal;
        // this represents the 8 spots surround the placed cube.
        int[,] offsetArray = new int[8, 2] {{1, 0}, {1, 1}, {0, 1}, {-1, 1}, {-1, 0}, {-1, -1}, {0, -1}, {1, -1}};

        //Check if already occupied with color cube
        if (checkkerArray[x, y] != 0 && checkkerArray[x, y] <= 6)
        {
            return -1;
        }
        //Occupied with special cases
        if (checkkerArray[x, y] >= 7 && checkkerArray[x, y] <= 10)
        {
            return checkkerArray[x, y];
        }

        //Check adjacent fields
        for (int i = 0; i < 8; i++)
        {
            xlocal = x + offsetArray[i, 0];
            ylocal = y + offsetArray[i, 1];

            if (xlocal < YDim && xlocal >= 0 && ylocal < XDim && ylocal >= 0)
            {
                //Check if adjacent ID is different and not 0:empty, 7:pavemnt, 9:fence
                if ((checkkerArray[xlocal, ylocal] != CubeID) &&
                    (checkkerArray[xlocal, ylocal] != 0) &&
                    (checkkerArray[xlocal, ylocal] != 7) &&
                    (checkkerArray[xlocal, ylocal] != 9))
                    return (checkkerArray[xlocal, ylocal]);
            }
        }
        return 0;
    }

    //Count total tile that are covered and update UI
    public void countCoveredBlocksUI()
    {
        //Count total tile that are covered and update UI
        foreach (TextMeshProUGUI tx in totalCoveredTilesTextsUI)
        {
            string totalCoveredTilesStr = CountTotalCoveredTilesInCheckkerArray();
            if (tx)
                tx.text = totalCoveredTilesStr;
        }
    }
    
    public void CubeGrabbed()
    {
        //This is for storing cube location when picked up, in case it has to return there.
        clone = null;
        cubePickedUp = Grabbable.cubeGrabbed;
        var cubeTransform = cubePickedUp.transform;

        idOfCube = cubePickedUp.GetComponentInChildren<CubeNameID>();
        if (idOfCube != null && idOfCube.cubeID != "GreyCube")
        {
            // store cube color.
            lastCubeGrabbed = idOfCube.cubeID;
           //Issue a EventLost event on current Interaction Object
           IssueInterationEvents(null); 

           //Show carry cube of same name, and set the active cube index
           EnableCube(idOfCube.cubeID);
            
           if (idOfCube.tag == "InteractionCube" || idOfCube.tag == poolCubeTag)
           {
               //Set matrix entry to 0
               checkkerArray[idOfCube.xCheckerArrayCoord, idOfCube.yCheckerArrayCoord] = 0;
           }
        }

        // This section is for pool cubes
        if (cubePickedUp.layer == poolCubeLayerInteger)
        {
            PickUpFromPool();
        }
    }

    
    void PickUpFromPool()
    {
        //array x is Z (unity), array y is X Unity
        string cubeId = idOfCube.cubeID;
        float x = 0;
        float y = 0;
        float z = 0;
       
        // 1. Create Cube on the same position. 
        CreateStaticCube(cubeId, 0, 0, true);
        // 2. Change picked cube layer to "CubeInteraction" and Tag to "InteractionCube" 
    }

    
    public void CubeReleased()
    {
        StartCoroutine(LetTimeForCubeToDrop());
        
        AudioManager.Instance.playSound("dropBlock");
        //Test for errors and play sounds
        if (adjacencyError > 0 && adjacencyError <= 6)
        {
            AudioManager.Instance.playSound("forbiddenAdjacentBlock");
        } 
        else if (adjacencyError == 9)
        {
            AudioManager.Instance.playSound("forbiddenNext2Fence");
        }
        else if (adjacencyError == 8)
        {
            AudioManager.Instance.playSound("forbiddenOnBench");
        }
        else if (adjacencyError == 7)
        {
            AudioManager.Instance.playSound("forbiddenOnPath");
        }
        
        delayTime = 2.0f;
        //Place new cube only if matrix is empty
        if (adjacencyError == 0  && checkkerArray[x, y] == 0)
        {
            //Issue a start physics command with the cubeID name and its computed coord array positions
            //Also enables the dropCube and positions it correctly to where the hook is now
            //This will trigger a sequence of a physics dropCube falling and then
            //a static interaction cube will appear Instatiated (CreateStaticCube()).
            cubesArray[activeCubeIndex].dropCube.StartFalling(cubesArray[activeCubeIndex].name, x, y);

            //Disable switch to presentation mode until drop sequence finishes
            canChangeViewMode = false;

            delayTime = 0.0f;
        }
        //after releasing cube in forbidden place, this section returns it back to its previous location
        else if (cubePickedUp.tag != poolCubeTag && adjacencyError != 10 && checkkerArray[x,y] != 10)
        {   
            canChangeViewMode = false;
            string name = cubesArray[activeCubeIndex].name;
            StartCoroutine(CreateCubeForForbiddenDrop(name, delayTime));
        }
        
        if(adjacencyError == 10 || checkkerArray[x,y] == 10)
        {
            delayTime = 0.0f;
        }
        
        InteractableColorVisual colorScript = cubePickedUp.GetComponentInChildren<InteractableColorVisual>();
        colorScript.enabled = false;

        //Destroy released cube
        Destroy(cubePickedUp, delayTime);
        
        cubePickedUp = null;
        idOfCube = null;
        pickedUpCounter = true;
        
        //Inidicate that we dont carry anything anymore and hide redTile
        activeCubeIndex = -1;
        redTile.gameObject.SetActive(false);
    }

    IEnumerator LetTimeForCubeToDrop()
    {
        startupTutorial.ActivateRayInteractors(false);
        yield return new WaitForSeconds(2f);
        //activate hand grabs
        startupTutorial.ActivateRayInteractors(true);
    }

    IEnumerator CreateCubeForForbiddenDrop(string name, float timeToWait)
    {
        yield return new WaitForSecondsRealtime(timeToWait);
        CreateStaticCube(name, X, Y, false);
    }

    public void ZoomIn()
    {
        fps_controller.transform.position = previousFPSPos;
        movementScript.enabled = true; //fps_controller.RestoreGroundForce();
        rb.useGravity = true;
        if (view_mode_ == ViewModes.CUBE_INTERACTION)
        {
            startupTutorial.ActivateRayInteractors(true);
        }
    
        isZoomOutViewMode = false;
        if (staticObjects) staticObjects.SetActive(true);
        
        if (owlBird) owlBird.SetActive(true);
        AudioManager.Instance.playSound("goGround");
        fps_controller.transform.rotation = Quaternion.identity;
        Camera.main.transform.localRotation = Quaternion.identity;
        
        if (view_mode_ == ViewModes.PRESENTATION && talkingBirds)
        {
            talkingBirds.SetActive(false);
        }
        else
        {
            talkingBirds.SetActive(true);
        }
    }

    public void ZoomOut()
    {
        //disable player movement
        movementScript.enabled = false; 
        startupTutorial.ActivateRayInteractors(false);

        rb.useGravity = false;
        isZoomOutViewMode = true;
        if (staticObjects) staticObjects.SetActive(false);
        if (talkingBirds) talkingBirds.SetActive(false);
        if (owlBird) owlBird.SetActive(false);
        
        previousFPSPos = fps_controller.transform.position;
        fps_controller.transform.position = vantagePoint.position;
        fps_controller.transform.rotation = Quaternion.Euler(90, 0, 0);
        Camera.main.transform.localRotation = Quaternion.identity;
        AudioManager.Instance.playSound("goHigh");
    }

    public void ReturnFromEscapeUI()
    {
       helpPanel.SetActive(false);
       isExitViewModeOn = false;
       movementScript.enabled = true;
       if (view_mode_ != ViewModes.PRESENTATION && !startupTutorial.owlIsSpeaking)
       {
            // StartCoroutine(ActivateRayWithDelay());
       }
    }

    public void ReadyToExitPresentationMode()
    {
        readyToExitPresentationMode = true;
    }

    public void GoToEscapeUI()
    {
        //if not fade out has started..
        if (!fadeOut)
        {
            startupTutorial.ActivateRayInteractors(true);
            helpPanel.SetActive(true);
            isExitViewModeOn = true;
            movementScript.enabled = false;
        }
    }

    public void AskTheOwl()
    {
        if (lastCubeGrabbed.Equals("") && !startupTutorial.isTutorial)
        {
            Debug.Log("IS null -->");
            return;
        }
            
        Debug.Log("inSequence --> " + inSequence + " || readyToExitPresentationMode --> " + readyToExitPresentationMode);
        if (!inSequence && view_mode_ == ViewModes.CUBE_INTERACTION)
        {
            // start confirm answer sequence that leads to presentation mode.
            inSequence = true;
            // owlBird.GetComponent<Collider>().enabled = true;
            // startupTutorial.ConfirmAnswer();
            startupTutorial.StartPlayingTheSounds();
            MakeAllCubesInteractable(false);
        }
    
        // this block is for active game scene only
        if (view_mode_ == ViewModes.PRESENTATION && readyToExitPresentationMode)
        {
            startupTutorial.ActivateRayInteractors(true);
            ResetToCubeRepresentation();
            startupTutorial.ResetTheOwl();
            inSequence = false;
            readyToExitPresentationMode = false;
            AudioManager.Instance.playSound("magic");
            MakeAllCubesInteractable(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive)
            return;

        if (playGroundFinished)
            return;

        //Count total tile that are covered and update UI
        countCoveredBlocksUI();
        
        //Switch to and from zoom mode
        if (OVRInput.GetDown(OVRInput.Button.One) && !isExitViewModeOn
             && activeCubeIndex == -1)
        {
            if (isZoomOutViewMode)
            {
                ZoomIn();
            }
            else if (canChangeViewMode && !inSequence)
            {
                ZoomOut();
            }
        }
        
        // Enable owl collider if possible.
        // if (canChangeViewMode && !isZoomOutViewMode &&
        //     activeCubeIndex == -1 && !isExitViewModeOn )
        // {
        //    startupTutorial.owlCollider.enabled = true;
        // } 
        
        //if we are in presentation mode no interaction
        if (view_mode_ == ViewModes.PRESENTATION || isZoomOutViewMode)
            return;
             
        if (activeCubeIndex >= 0 && activeCubeIndex < cubesArray.Length) //If we carry a cube 
        {
            //Sanity check of references
            if (redTile != null && cornerCheckerboard != null && cubePickedUp != null)
                CalculateRedTilePosition();
        } //carry cube section
	}//Update

    //Issue events on Interaction Objects
    public void IssueInterationEvents(OnInteraction interactionObj)
    {
        //If interaction object changed issue events 
        if (currentInteractionObject != interactionObj)
        {
            if (currentInteractionObject != null)
                currentInteractionObject.OnFocusExit();

            if (interactionObj != null)
                interactionObj.OnFocusEnter();

            //store current object or null if none
            currentInteractionObject = interactionObj;
        }
    }

    void CalculateRedTilePosition()
    {
        //The corner is the (0,0) of the Checker. Compute the int coords of where the carried cube is now
        //Compute relative position of picked cube form corner by substracting the Hook form the corner.
        pickedCubeTransformer = cubePickedUp.GetComponentInChildren<OneGrabFreeTransformer>();
        offset = pickedCubeTransformer.targetTransformer - cornerCheckerboard.position;
        
        //Clamp to size of array
        //Because the (0,0) is top left z and x can be negative so use Abs
        //Also remove decimals and floor values
        offset.x = Mathf.Floor(Mathf.Clamp(Mathf.Abs(offset.x), 0, XDim - 1));
        offset.z = Mathf.Floor(Mathf.Clamp(Mathf.Abs(offset.z), 0, YDim - 1));

        //compute int checkker array index
        x = (int)offset.z;
        y = (int)offset.x;

        // We want to store location in grid, in case cube is realesed in forbidden place and has to go back
        // Enter if statement just once (when cube is grabbed).
        if (pickedUpCounter)
        {
            X = x;
            Y = y;
            pickedUpCounter = false;
        }
        
        //Compute position of red highlight tile, 
        //assume that cube is 1x1x0.5, therefore add offset, the pivot of the cube is in the bottom center
        offset = cornerCheckerboard.right * (x + 0.5f) + cornerCheckerboard.forward * (y + 0.5f) + new Vector3(0, 0.01f, 0);
        //Place highlight tile and hide it
        redTile.position = cornerCheckerboard.position + offset;

        //Check if cube can be place in this x,y. return value has several values
        //0:OK, -1:Occupied with color cube, 7:Occupied with Pavement, 8:Occupied with Bench 
        //9:Next to fence, 1-6:Next to cube with different color, 10: cube pool
        adjacencyError = forbiddenCheck(x,y, GetCubeId(cubesArray[activeCubeIndex].name));
    
        //Draw a red tile only if field empty and not in pool
        if (adjacencyError >= 0 && adjacencyError != 10)
            redTile.gameObject.SetActive(true);
    }
}
