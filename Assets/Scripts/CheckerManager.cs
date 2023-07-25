using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using UnityEngine.Events;

public class CheckerManager : Singleton<CheckerManager>
{
    [Header("Info needed to switch into cube interaction mode")]
    public GameObject staticObjects = null;
    public Transform vantagePoint = null;
    ViewModes previousViewMode;
    Vector3 previousFPSPos;
    bool isZoomOutViewMode = false;

    public enum ViewModes { CUBE_INTERACTION, PRESENTATION };
    [Header("The checker view mode")]
    public ViewModes view_mode_ = ViewModes.CUBE_INTERACTION;
    bool canChangeViewMode = true; //To  prevent  switch when cube drops through physics

    //To show how many cubes are placed
    public TextMesh[] totalCoveredTilesTextsUI;

    [Header("Reticle to show when in cube interaction mode")]
    public GameObject ui_reticle = null;
    public GameObject ui_escape = null;

    //Dimensions of Checker Grid
    const int XDim = 20;
    const int YDim = 10;

    [Header ("Red Highlight Tile for cube placement")]
    public Transform redTile = null;
    [Header("Position under camera where carried cube is placed")]
    public Transform cameraHookForCube = null;
    [Header("Corner of Checkerboard designating 0,0")]
    public Transform cornerCheckerboard = null;

    //Used to swich on off then entering the presentation mode
    [Header("Talking Birds Parent")]
    public GameObject talkingBirds = null;
    public GameObject owlBird = null;

    //The interactible cubes have a "CubeInteraction" layer mask and are the big cubes placed on the ckecker board.
    //Clicking on them enable the carry cube, destroys the currnt instance ans set the chekker arry to zero
    //They are generated from prefabs Prefabs/**bas_collider_static pointed to form the CubesArray[]
    [Header("RayMask for the dynamic interactible cubes")]
    public LayerMask cubeInteractionMask;

    //The cubes in the pool have a "CubePool" layer mask and are the cubes in the pool to be grabbed
    //They remain static in the pool and clicking on them just enable the carry cube
    [Header("RayMask for the interactible pool cubes")]
    public LayerMask cubePoolMask;

    //Mask set to "InteractionGeneralObject" layer mask and designete general interaction objects like the birds
    //They issue events when pointed at and clicked upon
    public LayerMask interactionObjectsMask;

    //Positioned on update whena cube is pointed at. And hidden again when not.
    //There are two kinds of highlights because the cude have different sizes 
    //The pool cubes as the carry cubes are half size
    // [Header("Objects to show for cube highlight")]
    // public GameObject poolCubeHighlight        = null;
    // public GameObject interactionCubeHighlight = null;
    OnInteraction currentInteractionObject = null;

    //this is set by the EnterPoolNotifier attached to cube_pool_fountain
    bool playerIsInPool = false;

    //Game is finished well done
    bool playGroundFinished = false;

    [Header("Is Manager Active")]
    public bool isActive = false;

    private GameObject fps_controller;
    private SimpleCapsuleWithStickMovement movementScript;
    private Rigidbody rb;
    private bool isExitViewModeOn;

    //In general when a cube is grabed the carycube representation is active (is under Camera).
    //Then when droped carycube becomes inactive and dropcube is activated.
    //When dropcube is colliding with checker it becomes inactive and a prefabStaticCube is created in the respective checker pos
    [System.Serializable]
    public class Cubes
    {
        public string name; //nameID of cube
        public Dropper dropCube; //Represenation that is used when droping a cube has physics
        // public GameObject caryCube; //Represenation that is used when carying a cube, is under the camera
        public GameObject prefabStaticCube; //Instance created when drop cube has hit the checkerboard
    }

    [System.Serializable]
    public class PlayGroundObjects
    {
        public string color; //color of cubes that identify the playground object
        public GameObject playgroundObject; //Represenation of the playground object
        public GameObject multiCubeRepresentation; //Parent of all cubes of the same color
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

    [Header("Index into array of active cube or -1")]
    public int activeCubeIndex = -1;

    [Header("Events executed when playground is ready")]
    //Is called when Playground is fixed
    public UnityEvent GameWonEvent;

    //Slide:1, MonkeyBars:2, CrawlTunnel:3, RoundAbout:4, Swings:5, SandPit:6, Pavement:7, Bench:8, Fence:9, 10:is cube pool
    //array x is Z (unity), array y is X Unity
    public int[,] checkkerArray = new int[YDim, XDim] {
        {9, 9, 9, 9, 9, 9, 9, 9, 9,   8,  8, 9, 9, 9, 9, 9, 9, 9, 9, 9},
        {9, 0, 1, 1, 1, 1, 1, 0, 7,   7,  7, 7, 0, 0, 6, 6, 6, 6, 0, 9},
        {9, 0, 1, 1, 1, 1, 1, 7, 7,   7,  7, 7, 7, 0, 6, 6, 6, 6, 0, 8},
        {9, 0, 2, 0, 0, 0, 0, 7, 10, 10, 10, 10, 7, 0, 6, 6, 6, 6, 0, 8},
        {9, 0, 2, 0, 0, 0, 0, 7, 10, 10, 10, 10, 7, 0, 0, 0, 0, 0, 0, 9},
        {9, 0, 2, 4, 4, 4, 4, 7, 10, 10, 10, 10, 7, 0, 0, 0, 5, 5, 5, 9},
        {9, 0, 2, 4, 4, 4, 4, 7, 7,   7,  7, 7, 7, 0, 0, 0, 5, 5, 5, 9},
        {8, 0, 2, 4, 4, 4, 4, 7, 7,   7,  7, 7, 7, 0, 0, 0, 5, 5, 5, 9},
        {8, 0, 2, 4, 4, 4, 4, 0, 0,   7,  7, 0, 0, 0, 0, 0, 5, 5, 5, 9},
        {9, 9, 9, 9, 9, 0, 9, 9, 9,   7,  7, 9, 9, 9, 9, 9, 9, 9, 9, 9}};

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

        //CreateStatic cubes from initial Chekker array
        for (int x = 0; x < YDim; x++)
        {
            for (int y = 0; y < XDim; y++)
            {
                // If there is a colour stored in the matrix
                if (checkkerArray[x, y] != 0 && checkkerArray[x, y] <= 6)
                {
                    CreateStaticCube(GetCubeName(checkkerArray[x, y]), x, y);
                }
            }
        }

        ResetToCubeRepresentation();
    }


    //Swtich off wireframe cubes that highlight which cube can be selected
    // void DisableHighlightOfCubes()
    // {
    //     if (interactionCubeHighlight != null)
    //         interactionCubeHighlight.SetActive(false);
    //     if (poolCubeHighlight != null)
    //         poolCubeHighlight.SetActive(false);
    // }

    //Activate highlight on that cube depending on what cube it is.
    // void CubeHighlight(Transform parent)
    // {
    //     if (parent.tag == "InteractionCube" && interactionCubeHighlight != null)
    //     {
    //         interactionCubeHighlight.transform.position = parent.position;
    //         interactionCubeHighlight.transform.rotation = parent.rotation;
    //         interactionCubeHighlight.SetActive(true);
    //     }
    //     else if (parent.tag == "PoolCube" && poolCubeHighlight != null)
    //     {
    //         poolCubeHighlight.transform.position = parent.position;
    //         poolCubeHighlight.transform.rotation = parent.rotation;
    //         poolCubeHighlight.SetActive(true);
    //     }
    // }

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
                // cubesArray[i].caryCube.SetActive(true);

                //Show model of hands carying the cubes on the FPS controller
                // HandSwitcher.Instance.ShowHands();
            }
        }
    }


    //Add a static cube to the checker board.
    //Creates an instance and adds the entry to the checkker array
    //Is called form Dropper after collision or from Start() to creat initial cubes
    public void CreateStaticCube(string cubeId, int x, int y)
    {
        //DropCube finished now we can enable presentation switch
        canChangeViewMode = true;

        foreach (Cubes cb in cubesArray)
        {
            if (cb.name == cubeId && cb.prefabStaticCube != null)
            {
                //Compute world position of new cube offseted by (0.5,0.5) since cube is 1x1 and pivot is base center
                Vector3 positionNewCube = cornerCheckerboard.position + cornerCheckerboard.right * (x + 0.5f) + cornerCheckerboard.forward * (y + 0.5f);

                //Add cube to world using the prefab for the static intersection cubes
                GameObject clone;
                clone = Instantiate(cb.prefabStaticCube, positionNewCube, Quaternion.identity);

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
                        checkkerArray[x, y] = GetCubeId(cubeId);

                        //Search through list to find under which hierarchy to place the cube
                        foreach (PlayGroundObjects pg in playGroundObjectsArray)
                        {
                            if (pg.color == cubeId && pg.multiCubeRepresentation != null)
                            {
                                clone.transform.parent = pg.multiCubeRepresentation.transform;
                            }
                        }
                    }
                    break;
                }
            }
        } //foreach
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

                            // Test are rotate 90 Degrees if normal test does not suceed
                            if (!okNorm)
                                okRotate = testArea(x, y, colorSizes[whichColor - 1,1], colorSizes[whichColor - 1,0], whichColor, localMatrix);

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
                                    Vector3 offset = cornerCheckerboard.right * (x + colorSizes[whichColor - 1, 0]*0.5f) + cornerCheckerboard.forward * (y + colorSizes[whichColor - 1, 1] * 0.5f);
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
                            } //okNorm || okRotate

                            //we already processed an island of this color
                            idCheck[whichColor - 1] = true;
                        }
                        else
                        {
                            //This color is not ready we have processed it before there must be multiple islands
                            idOK[whichColor - 1] = false;
                        }
                    }
                } // if a colour stored in matrix

            }//y //Traverse all checkker array
        }//x //Traverse all checkker array


        //Now do final check and show playground objects if ok or leave as cubes
        bool allOK = true;

        //Set to default value and see if there is any cube island that is correct, 
        //if no island correct do not swith to PResentation mode 
        view_mode_ = ViewModes.CUBE_INTERACTION;

        // Check trough all the colors and switch geometry on/off.
        for (int i = 0; i < playGroundObjectsArray.Length; i++)
        {
            if (idOK[i])
            {
                if (playGroundObjectsArray[i].multiCubeRepresentation != null)
                    playGroundObjectsArray[i].multiCubeRepresentation.SetActive(false);

                if (playGroundObjectsArray[i].playgroundObject != null)
                    playGroundObjectsArray[i].playgroundObject.SetActive(true);

                if (talkingBirds) talkingBirds.SetActive(false);
                //if (owlBird) owlBird.SetActive(false);

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
                //if (owlBird) owlBird.SetActive(false);

                allOK = false;
            }
        }

        if (allOK)
        {
            if (talkingBirds)
                talkingBirds.SetActive(false);

            if (ui_reticle)
                ui_reticle.SetActive(false);

            if (GameWonEvent != null)
                GameWonEvent.Invoke();

            playGroundFinished = true;
        }
    }

    //Test continuity of a specific color objects
    bool testArea(int x, int y, int sizex, int sizey, int ID, int[,] localMatrix )
    {
        int areax = x + sizex;
        int areay = y + sizey;

        if (((x + sizex) > YDim) || ((y + sizey) > XDim))
            return false;

        for (int iy = y; iy < areay; iy++)
        {
            for (int jx = x; jx < areax; jx++)
            {
                localMatrix[jx, iy] = 1;

                //If this field has a different ID or if not a color cube or has bad neighbours Not OK
                if ((checkkerArray[jx, iy] != ID) || (ID > 6) || adjacentCheck(jx,iy, ID))
                {
                    return false;
                }
            }
        }

        return true;
    }

    //Returns true if there are adjacent cubes with different color and the cube can not be placed
    bool adjacentCheck(int x, int y, int CubeID)
    {
        int xlocal, ylocal;
        int[,] offsetArray = new int[8,2] {{1, 0}, {1, 1}, {0, 1}, {-1, 1}, {-1, 0}, {-1, -1}, {0, -1}, {1, -1}};
    
        for (int i = 0; i<8; i++)
        {
	        xlocal = x + offsetArray[i,0];
	        ylocal = y + offsetArray[i,1];
	
	        if (xlocal<YDim && xlocal>=0 && ylocal<XDim && ylocal>=0)
            {
                //Check if adjacent ID is different and not 0:empty,7:pavemtn, 9:fence
	            if ((checkkerArray[xlocal, ylocal] != CubeID) && 
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
        int[,] offsetArray = new int[8, 2] { { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 }, { -1, 0 }, { -1, -1 }, { 0, -1 }, { 1, -1 } };

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

    public void ZoomIn()
    {
        fps_controller.transform.position = previousFPSPos;
        movementScript.enabled = true; //fps_controller.RestoreGroundForce();
        rb.useGravity = true;
        isZoomOutViewMode = false;
        if (staticObjects) staticObjects.SetActive(true);
        if (talkingBirds) talkingBirds.SetActive(true);
        if (owlBird) owlBird.SetActive(true);
        AudioManager.Instance.playSound("goGround");
        fps_controller.transform.rotation = Quaternion.identity;
        Camera.main.transform.localRotation = Quaternion.identity;
    }

    public void ZoomOut()
    {
        //disable player movement
        movementScript.enabled = false; 
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

    //Count total tile that are covered and update UI
    public void countCoveredBlocksUI()
    {
        //Count total tile that are covered and update UI
        foreach (TextMesh tx in totalCoveredTilesTextsUI)
        {
            string totalCoveredTilesStr = CountTotalCoveredTilesInCheckkerArray();
            if (tx)
                tx.text = totalCoveredTilesStr;
        }
    }
    
    // Update is called once per frame
    void Update ()
    {
        if (!isActive)
            return;
        
        //ON esc show ui to select to go to menu
        if (OVRInput.GetDown(OVRInput.Button.Start))
        {
            if (ui_escape && !isExitViewModeOn)
            {
                ui_escape.SetActive(true);
                isExitViewModeOn = true;
                movementScript.enabled = false;
            }
            else
            {
                ui_escape.SetActive(false);
                isExitViewModeOn = false;
                movementScript.enabled = true;
            }
        }

        if (playGroundFinished)
            return;

        //Count total tile that are covered and update UI
        countCoveredBlocksUI();
        
        //Switch to and from zoom mode
        if (OVRInput.GetDown(OVRInput.Button.One) && !isExitViewModeOn)
        {
            if (isZoomOutViewMode)
            {
                ZoomIn();
            }
            else if (canChangeViewMode)
            {
                ZoomOut();
            }
        }

        //We can only change to presentation mode when not carrying a cube and there is no dropper active
        if (OVRInput.GetDown(OVRInput.Button.Three) && 
            canChangeViewMode && !isZoomOutViewMode &&
            activeCubeIndex == -1 && !isExitViewModeOn)
        {
            AudioManager.Instance.playSound("magic");
            if (view_mode_ != ViewModes.PRESENTATION)
            {
                CheckIfOK();
            }
            else if (view_mode_ == ViewModes.PRESENTATION)
            {
                ResetToCubeRepresentation();
            }
        } 

        //Show hide reticle depending on state
        if (activeCubeIndex == -1 && view_mode_ == ViewModes.CUBE_INTERACTION && !isZoomOutViewMode)
        {
            if (ui_reticle)
                ui_reticle.SetActive(true);
        }
        else
        {
            if (ui_reticle)
                ui_reticle.SetActive(false);
        }

        //Hide cube highlights 
        // DisableHighlightOfCubes();

        //Hide Red Tile
        if (redTile != null)
            redTile.gameObject.SetActive(false);

        //if we are in presentation mode no interaction
        if (view_mode_ == ViewModes.PRESENTATION || isZoomOutViewMode)
        {
            return;
        }

        //If we are not carrying an active cube
        if (activeCubeIndex == -1)
        {
            //Ray to middle of screen
            Vector3 middleScreen = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
            Ray middleRay = Camera.main.ScreenPointToRay(middleScreen);
            RaycastHit hit;
            
            if (Physics.Raycast(middleRay, out hit, 1, cubeInteractionMask|cubePoolMask|interactionObjectsMask, QueryTriggerInteraction.Ignore))
            {
                //if we got a hit and its a interaction cube or pool cube
            CubeNameID idOfCube = hit.collider.GetComponent<CubeNameID>();

            //GREY CUBES ARE NON SELECTABLE FOR NOW
            if (idOfCube != null && idOfCube.cubeID != "GreyCube")
            {
                //Issue a EventLost event on current Interaction Object
                // IssueInterationEvents(null);

                //Highlight the pointat cube
                //Because cubes have hierarchy, highlight from parent coords
                // CubeHighlight(idOfCube.transform.parent);

                //If mouse press grab cube and enable carry cube, destroy interactible cube, set matrix to 0
                
                // if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) >= 0.5
                //     || OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger, OVRInput.Controller.Touch) >= 0.5)
                // {
                    Debug.Log("--> SKANDALI PRESSED");
                    // DisableHighlightOfCubes();

                    //Show carry cube of same name, and set the active cube index
                    EnableCube(idOfCube.cubeID);

                    //If the cube is an interaction cube destroy and set chekker array to 0
                    //else its a pool cube in which case the enable above is enough
                    if (idOfCube.tag == "InteractionCube")
                    {
                        //Set matrix entry to 0
                        checkkerArray[idOfCube.xCheckerArrayCoord, idOfCube.yCheckerArrayCoord] = 0;

                        //Because static interaction cubes have hierarchy, delete from parent
                        Destroy(idOfCube.transform.parent.gameObject);
                    }
                // }
            }
        } 
        else if (activeCubeIndex >= 0 && activeCubeIndex < cubesArray.Length) //If we carry a cube 
        {
            //Sanity check of references
            if (redTile != null && cornerCheckerboard != null)
            {
                /****** compute the x,y coords of the player (cameraHook) inside the checker *****/
                Vector3 offset;
                int x, y;

                //The corner is the (0,0) of the Checker. Compute the int coords of where the carried cube is now
                //Compute relative position of FPS cameraHook form corner by substracting the Hook form the corner.
                offset = cameraHookForCube.position - cornerCheckerboard.position;
                
                //Clamp to size of array
                //Because the (0,0) is top left z and x can be negative so use Abs
                //Also remove decimals and floor values
                offset.x = Mathf.Floor(Mathf.Clamp(Mathf.Abs(offset.x), 0, XDim - 1));
                offset.z = Mathf.Floor(Mathf.Clamp(Mathf.Abs(offset.z), 0, YDim - 1));

                //compute int checkker array index
                x = (int)offset.z;
                y = (int)offset.x;

                //Compute position of red highlight tile, 
                //assume that cube is 1x1x0.5, therefore add offset, the pivot of the cube is in the bottom center
                offset = cornerCheckerboard.right * (x + 0.5f) + cornerCheckerboard.forward * (y + 0.5f) + new Vector3(0, 0.01f, 0);
                
                //Place highlight tile and hide it
                redTile.position = cornerCheckerboard.position + offset;

                //Check if cube can be place in this x,y. return value has several values
                //0:OK, -1:Occupied with color cube, 7:Occupied with Pavement, 8:Occupied with Bench 
                //9:Next to fence, 1-6:Next to cube with different color, 10: cube pool
                int adjacencyError = forbiddenCheck(x,y, GetCubeId(cubesArray[activeCubeIndex].name));
                //print(adjacencyError);

                //Draw a red tile only if field empty and not in pool
                if (adjacencyError >= 0 && adjacencyError != 10)
                    redTile.gameObject.SetActive(true);

                //Player stands in pool
                if (adjacencyError == 10)
                    playerIsInPool = true;
                else
                    playerIsInPool = false;

                /****** Place a cube when input, the sequence is CarryCube->DropCube->(Instantiated) InteractionCube *****/
                /****** CarryCube is the activeCubeIndex we are carying now                                          *****/
                //In case button is pressed drop cube
                if (Input.GetMouseButtonDown(0))
                {
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

                    if (adjacencyError == 0 || playerIsInPool)
                    {
                        //Place new cube only of matrix is empty and player not in pool
                        if (checkkerArray[x, y] == 0 && !playerIsInPool)
                        {
                            //Issue a start physics command with the cubeID name and its computed coord array positions
                            //Also enables the dropCube and positions it correctly to where the hook is now
                            //This will trigger a sequence of a physics dropCube falling and then
                            //a static interaction cube will appear Instatiated (CreateStaticCube()).
                            cubesArray[activeCubeIndex].dropCube.StartFalling(cubesArray[activeCubeIndex].name, x, y);

                            //Hide model of hands carying the cubes on the FPS controller
                            HandSwitcher.Instance.HideHands();

                            //Disable switch to presentation mode until drop sequence finishes
                            canChangeViewMode = false;
                        }

                        //Reset Carying cube only if matrix empty and we can place cube 
                        //or if player is in pool trying to return object
                        if (checkkerArray[x, y] == 0 || playerIsInPool)
                        {
                            if (playerIsInPool)
                            {
                                HandSwitcher.Instance.HideHands();
                                AudioManager.Instance.playSound("dropBlock");
                            }

                            //Hide Carry cube
                            // cubesArray[activeCubeIndex].caryCube.SetActive(false);

                            //Inidicate that we dont carry anything anymore and hide redTile
                            activeCubeIndex = -1;
                            redTile.gameObject.SetActive(false);
                        }
                    }
                }
            }
        } //carry cube section

	}//Update

    //Issue events on Interaction Objects
    // public void IssueInterationEvents(OnInteraction interactionObj)
    // {
    //     //If interaction object changed issue events 
    //     if (currentInteractionObject != interactionObj)
    //     {
    //         if (currentInteractionObject != null)
    //             currentInteractionObject.OnFocusExit();

    //         if (interactionObj != null)
    //             interactionObj.OnFocusEnter();

    //         //store current object or null if none
    //         currentInteractionObject = interactionObj;
    //     }

    //     //Get click info
    //     if (interactionObj != null)
    //     {
    //         if (Input.GetMouseButtonDown(0))
    //         {
    //             interactionObj.OnClicked();
    //         }
    //     }
    // }
    }
    public void LoadScene(string name)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(name);
    }
}
