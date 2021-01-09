using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using MathNet;
//using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.IO;
using UnityEngine.SceneManagement;


public class SPAAM : MonoBehaviour
{

    [Header("Toggle to calibrate Right Eye")]
    public bool calibrateRightEye;
    public float disparity;

    [Header("Toggle to enable Debugging Log generation")]
    public bool enableLogging;
    public bool enableDebugLog;
    
    [Header("RANSAC settings")]
    public bool enableRANSAC;
    public double inlierDistanceThreshold;
    int ransacMaxIterations;
    public int ransacPointsPerBatch;
    public float maxError;
    double reprojectionError = 0;
    double meanReprojectionError = 0;

    [Header("Toggle to apply intrinsic parameters")]
    public bool useIntrinsics;

    [Header("Toggle to use Custom input parameters (debugging)")]
    public bool useMyInputParameters;
    public List<float> myTrackerX = new List<float>();
    public List<float> myTrackerY = new List<float>();
    public List<float> myTrackerZ = new List<float>();
    public List<float> myU = new List<float>();
    public List<float> myV = new List<float>();

  
    [Header("Scale factor and rotation correction")]
    public float adjustmentShift = 0;
    public float adjustmentAngle = 0;
    /*
     public bool enableRigidTransformation;
     public float xMultiplier, yMultiplier, zMultiplier;
     public float xAngle, yAngle, zAngle;
     */

    [Header("Rendering resolution")]
    // Resolution of the headset
    public int resolutionWidth;
    public int resolutionHeight;
    [Tooltip("Dimensione del pixel proiettato lungo l'asse orizzontale.")]
    public float pixelSizeX= 124/2560/1000;
    [Tooltip("Dimensione del pixel proiettato lungo l'asse verticale.")]
    public float pixelSizeY= 85.5f/1440/1000;

    [Header("Crosshair positions during calibration")]
    public List<float> xSpawnPositionsUserDefined = new List<float>();
    public List<float> ySpawnPositionsUserDefined = new List<float>();

    [Header("Alignments requested for SPAAM")]
    [Tooltip("Number of matches required for the SPAAM procedure for each eye. The minimum should be at least 6, but since RANSAC is being implemented, more is advised.")]
    // Number of matches requested during the calibration procedure
    public int numberOfMatches;
    int originalNumberOfMatches;

    [Header("Crosshair size")]
    [Tooltip("Size of the sprite used as crosshair during the alignment task. The sprite is assumed to be square. e.g. 64 means a 64x64pixel wide sprite.")]
    // Size of the alignment cross sprite in pixel
    public int spriteSize;

   // [Header("The canvas where the crosshair is rendered upon")]
    // The parent canvas where the cross is instanced
   // public GameObject canvas;
   // [Header("Crosshair prefab")]
  //  [Tooltip("Prefab of the rawImage GameObject with the alignment sprite applied")]
    // Prefab of the rawImage GameObject with the cross sprite applied
   // public GameObject cross;


    // Generated positions where the cross will be displayed
    private float spawn_x, spawn_y;
    // Handle to the canvas RectTransform in order to move the sprite prefab to the generated Coordinates
    private RectTransform rt;
    // Counter to stop the calibration procedure when it has collected the wanted amount of matches (defined with the public variable numberOfMatches) 
    private int matchesCounter = 0;
    // Safety flag to ensure the correct data is being saved on keypress
    private bool ready = false;

    // Lists to store the position of the tracker
    List<float> tracker_x = new List<float>();
    List<float> tracker_y = new List<float>();
    List<float> tracker_z = new List<float>();
    // Lists to store the position of the tracker while using the RANSAC procedure. 
    // tracker_x/y/z will contain the positions of the subset of points used during a single RANSAC iteration
    List<float> ransac_tracker_x = new List<float>();
    List<float> ransac_tracker_y = new List<float>();
    List<float> ransac_tracker_z = new List<float>();
    // GameObject of the vive tracker
    [Header("Object tracked by vive tracker")]
    [Tooltip("GameObject which must be set as child of the HMD camera  and copy the 3D tracked world point coordinates for each update step")]
    public GameObject viveTracker;

    // Lists to store the pixel coordinates of the cross shown during calibration
    List<float> displayed_x = new List<float>();
    List<float> displayed_y = new List<float>();
    // Lists to store the pixel coordinates of the cross shown during calibration while using the RANSAC procedure. 
    // displayed_x/y/z will contain the cross positions of the subset of points used during a single RANSAC iteration
    List<float> ransac_displayed_x = new List<float>();
    List<float> ransac_displayed_y = new List<float>();

    // Lists to store the head pose
    List<float> head_x = new List<float>();
    List<float> head_y = new List<float>();
    List<float> head_z = new List<float>();
    List<float> head_rot_x = new List<float>();
    List<float> head_rot_y = new List<float>();
    List<float> head_rot_z = new List<float>();
    // prefab of the headset
    [Header("HMD camera")]
    [Tooltip("The GameObject which contains the calibrating camera")]
    public GameObject viveCamera;


    //list of indexes selected randomly during RANSAC
    List<int> ransac_indexes = new List<int>();

    // output variables for SVD
    double[] w;
    double[,] u;
    double[,] vt;
    double[,] a;
    int m;
    int n;
    double[] G;
    public Matrix4x4 originalProjection;

    [Header("Camera - Image plane reference frame alignment")]
    [Tooltip("Check If the image x-axis and camera x-axis point in opposite directions")]
    public bool flipX;
    [Tooltip("Check If the image y-axis and camera y-axis point in opposite directions")]
    public bool flipY;
    [Tooltip("Check If the camera looks down the negative-z axis")]
    public bool flipZ;

    [Header("Pixel Coordinates Transformations - Choose one")]
    [Tooltip("Normalizes the coordinates of the shown crosshair between -1 and 1.")]
    public bool Normalization;
    [Tooltip("Converts the coordinates of the shown crosshair in metric form, using Pixel Size (x,y) as parameters.")]
    public bool metricConversion;


    [Header("Remove crosshair sprite size shift")]
    [Tooltip("Toggle to perform the alignment with respect to the bottom left corner rather than the crosshair centre.")]
    public bool removeCrosshairShift;


    [Header("Use pixel dimensions during computations")]
    [Tooltip("Toggle to convert u,v image plane coordinates into metric units.")]
    public bool usePixelSizes;
    

    int logNumber = 0;
    StreamWriter sr, savedParams;
    double t1, t2, t3;
    double x, y, z;
    Matrix<double> Q;
    Matrix<double> R;
    Matrix<double> G_resized;
    bool calibrationSuccess = false;
    bool fileWritten = false;
    bool modifyTranslations = true;

    // Start is called before the first frame update
    void Start()
    {
        originalNumberOfMatches = numberOfMatches;
        // decommentare la prima condizione finito il debugging per permettere la calibrazione dell'occhio destro

        /*  if (calibrateRightEye) //GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().leftEyeCalibrated
          {
              calibrateRightEye = true;
              GameObject.Find("LeftCamera").GetComponent<smooth>().enabled = false;
              GameObject.Find("RightCamera").tag = "MainCamera";
              GameObject.Find("LeftCamera").tag = "Untagged";
              GameObject.Find("rightEye").tag = "TrackerEye";
              GameObject.Find("leftEye").tag = "Untagged";


          }
          */
        //  else
        //  {
        GameObject.Find("RightCamera").GetComponent<smooth>().enabled = false;
     //   }
        if (enableLogging)
        {
            while (File.Exists("Logs/Log" + logNumber + ".txt"))
                logNumber++;
            sr = File.CreateText("Logs/Log" + logNumber + ".txt");
        }
        if(enableDebugLog)
        Debug.Log("match number " + (matchesCounter+1));
        spawnCross();
        
    }

     void Update()
     {
        if (!calibrationSuccess)
        {
            if (useMyInputParameters && ready)
            {
                ready = false;
                useCustomData();
                Calibrate();
            }
            else
            {
                if (Input.GetKeyDown("space") && ready && matchesCounter < numberOfMatches - 1)
                {
                    ready = false;
                    saveData();
                    matchesCounter++;
                    if (enableDebugLog)
                        Debug.Log("match number " + (matchesCounter + 1));
                    spawnCross();

                }
                else if (Input.GetKeyDown("space") && ready && matchesCounter == numberOfMatches - 1)
                {
                    ready = false;
                    saveData();
                    Calibrate();
                }
            }
        }
        else
        {
           
            if (Input.GetKeyDown("up") && modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.localPosition = new Vector3(GameObject.FindWithTag("MainCamera").transform.localPosition.x, GameObject.FindWithTag("MainCamera").transform.localPosition.y, GameObject.FindWithTag("MainCamera").transform.localPosition.z - adjustmentShift);
            }
            if (Input.GetKeyDown("down") && modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.localPosition = new Vector3(GameObject.FindWithTag("MainCamera").transform.localPosition.x, GameObject.FindWithTag("MainCamera").transform.localPosition.y, GameObject.FindWithTag("MainCamera").transform.localPosition.z + adjustmentShift);
            }
            if (Input.GetKeyDown("left") && modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.localPosition = new Vector3(GameObject.FindWithTag("MainCamera").transform.localPosition.x + adjustmentShift, GameObject.FindWithTag("MainCamera").transform.localPosition.y, GameObject.FindWithTag("MainCamera").transform.localPosition.z);
            }
            if (Input.GetKeyDown("right") && modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.localPosition = new Vector3(GameObject.FindWithTag("MainCamera").transform.localPosition.x - adjustmentShift, GameObject.FindWithTag("MainCamera").transform.localPosition.y, GameObject.FindWithTag("MainCamera").transform.localPosition.z);
            }
            if (Input.GetKeyDown("space") && modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.localPosition = new Vector3(GameObject.FindWithTag("MainCamera").transform.localPosition.x, GameObject.FindWithTag("MainCamera").transform.localPosition.y + adjustmentShift, GameObject.FindWithTag("MainCamera").transform.localPosition.z);
            }
            if (Input.GetKeyDown(KeyCode.LeftControl) && modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.localPosition = new Vector3(GameObject.FindWithTag("MainCamera").transform.localPosition.x, GameObject.FindWithTag("MainCamera").transform.localPosition.y - adjustmentShift, GameObject.FindWithTag("MainCamera").transform.localPosition.z);
            }


            if (Input.GetKeyDown("up") && !modifyTranslations)
            {
                // GameObject.FindWithTag("MainCamera").transform.eulerAngles=new Vector3(0, 10, 0);
                // GameObject.FindWithTag("MainCamera").transform.localRotation *= Quaternion.Euler(0, 10, 0);
                GameObject.FindWithTag("MainCamera").transform.RotateAround(GameObject.FindWithTag("MainCamera").transform.position, GameObject.FindWithTag("HMD").transform.up, adjustmentAngle);
            }
            if (Input.GetKeyDown("down") && !modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.RotateAround(GameObject.FindWithTag("MainCamera").transform.position, GameObject.FindWithTag("HMD").transform.up, -adjustmentAngle);

            }
            if (Input.GetKeyDown("space") && !modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.RotateAround(GameObject.FindWithTag("MainCamera").transform.position, GameObject.FindWithTag("HMD").transform.right, adjustmentAngle);
            }
            if (Input.GetKeyDown(KeyCode.LeftControl) && !modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.RotateAround(GameObject.FindWithTag("MainCamera").transform.position, GameObject.FindWithTag("HMD").transform.right, -adjustmentAngle);

            }
            if (Input.GetKeyDown("left") && !modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.RotateAround(GameObject.FindWithTag("MainCamera").transform.position, GameObject.FindWithTag("HMD").transform.forward, adjustmentAngle);
            }

            if (Input.GetKeyDown("right") && !modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.RotateAround(GameObject.FindWithTag("MainCamera").transform.position, GameObject.FindWithTag("HMD").transform.forward, -adjustmentAngle);

            }



            if (Input.GetKeyDown("p"))
            {
                adjustmentShift *= 2;
                adjustmentAngle *= 2;
            }
            if (Input.GetKeyDown("m"))
            {
                adjustmentShift /= 2;
                adjustmentAngle /= 2;
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                modifyTranslations = !modifyTranslations;
            }
            if (Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Return))
            {
                changeScene();
            }

        }
    }
     

    // Starts to spawn crosses in random points. After the user align the cross with the real world (tracked) cross, the user head pose (and the real world cross pose) is saved upon pressing a button
    // Then, a new cross is generated.
    void spawnCross()
    {

        //spawn_x = Random.Range(0, resolutionWidth-spriteSize+1);
        //spawn_y = Random.Range(0, resolutionHeight - spriteSize+1);
        spawn_x = xSpawnPositionsUserDefined[matchesCounter];
        //spawn_y = resolutionHeight-ySpawnPositionsUserDefined[matchesCounter];
        spawn_y = ySpawnPositionsUserDefined[matchesCounter];
        float metric_spawn_x = (float)(- ((float) spawn_x * 16f / (float) resolutionWidth));
        float metric_spawn_y = (float)(- ((float) (resolutionHeight - spawn_y) * 18f / (float) resolutionHeight));
        string debug = "";

        debug = debug + "u coordinate: " + (spawn_x) + " v coordinate: " + ySpawnPositionsUserDefined[matchesCounter];
        if(enableDebugLog)
        Debug.Log(debug);
        GameObject.FindWithTag("MainCamera").GetComponent<smooth>().x = metric_spawn_x;
        GameObject.FindWithTag("MainCamera").GetComponent<smooth>().y = metric_spawn_y;

            ready = true;
    
    }


    // The SPAAM procedure requires, for each alignment, the following parameters:
    // - Vive tracker 3d position
    // - Head pose
    // - Pixel coordinates where the alignment cross was displayed on the headset
    void saveData()
    {
        // saving tracker position. 
        // attenzione, errato: questo non prende le coordinate locali
        //  tracker_x.Add(viveTracker.transform.localPosition.x);
        //  tracker_y.Add(viveTracker.transform.localPosition.y);
        //  tracker_z.Add(viveTracker.transform.localPosition.z);
        Vector3 cameraRelative = viveCamera.transform.InverseTransformPoint(viveTracker.transform.position);
        tracker_x.Add((float)Math.Round(cameraRelative.x,5));
        tracker_y.Add((float)Math.Round(cameraRelative.y,5));
        tracker_z.Add((float)Math.Round(cameraRelative.z,5));
        //Debug.Log("x relative " + cameraRelative.x);

        // saving the pixel coordinates of the cross
        // (not normalized version)
        //displayed_x.Add(spawn_x); 
        //displayed_y.Add(spawn_y);
        displayed_x.Add((float)Math.Round(pixelNormalization(spawn_x,"x"),5));
        displayed_y.Add((float)Math.Round(pixelNormalization(spawn_y, "y"),5));

        
        string debug = "";
        debug = debug + "x coordinate: " + pixelNormalization(spawn_x, "x") + " y coordinate: " + pixelNormalization(spawn_y, "y");
        if(enableDebugLog)
        Debug.Log(debug);

        // saving the head pose
        head_x.Add((float)Math.Round(viveCamera.transform.position.x,5));
        head_y.Add((float)Math.Round(viveCamera.transform.position.y,5));
        head_z.Add((float)Math.Round(viveCamera.transform.position.z,5));
        head_rot_x.Add((float)Math.Round(viveCamera.transform.eulerAngles.x,5));
        head_rot_y.Add((float)Math.Round(viveCamera.transform.eulerAngles.y,5));
        head_rot_z.Add((float)Math.Round(viveCamera.transform.eulerAngles.z,5));

        if (enableLogging)
        {
          
            sr.WriteLine(string.Format("________________________________________________________________\n######################### Alignment {0} #########################\n", (matchesCounter + 1)));
            sr.WriteLine(string.Format("Target absoulute: X= {0:F2} Y= {1:F2} Z= {2:F2};\n", viveTracker.transform.localPosition.x, viveTracker.transform.localPosition.y, viveTracker.transform.localPosition.z));
            sr.WriteLine(string.Format("Target absoulute rot: X= {0:F2} Y= {1:F2} Z= {2:F2};\n", viveTracker.transform.eulerAngles.x, viveTracker.transform.eulerAngles.y, viveTracker.transform.eulerAngles.z));
            sr.WriteLine(string.Format("Target relative: X= {0:F2} Y= {1:F2} Z= {2:F2};\n", cameraRelative.x, cameraRelative.y, cameraRelative.z));

            sr.WriteLine("Cross: X= {0:F2} Y= {1:F2};\n", spawn_x, spawn_y);
            sr.WriteLine("Head pos: X= {0:F2} Y= {1:F2} Z= {2:F2};\n", viveCamera.transform.position.x, viveCamera.transform.position.y, viveCamera.transform.position.z);
            sr.WriteLine("Head rot: X= {0:F2} Y= {1:F2} Z= {2:F2};\n\n", viveCamera.transform.eulerAngles.x, viveCamera.transform.eulerAngles.y, viveCamera.transform.eulerAngles.z);
        }

        }

    void useCustomData()
    {
       
        for (int i = 0; i < numberOfMatches; i++)
        {
           
            tracker_x.Add(myTrackerX[i]);
            tracker_y.Add(myTrackerY[i]);
            tracker_z.Add(myTrackerZ[i]);
           
        
          //  myU[i] = 0.1f * tracker_x[i] / tracker_z[i] /pixelSizeX;
          //  myV[i] = 0.1f * tracker_y[i] / tracker_z[i] /pixelSizeY;

            //myU[i] =  tracker_x[i] / tracker_z[i] ;
           // myV[i] =  tracker_y[i] / tracker_z[i] ;


            spawn_x = myU[i];
            spawn_y = myV[i];
            //spawn_x = spawn_x - (resolutionWidth / 2);
           // spawn_y = (resolutionHeight) / 2 - spawn_y;
            displayed_x.Add(spawn_x);
            displayed_y.Add(spawn_y);

            if (enableLogging)
            {
                sr.WriteLine(string.Format("________________________________________________________________\n######################### Alignment {0} #########################\n", i + 1));
                sr.WriteLine(string.Format("Target relative: X= {0:F2} Y= {1:F2} Z= {2:F2};\n", tracker_x[i], tracker_y[i], tracker_z[i]));
                sr.WriteLine("Cross: X= {0:F2} Y= {1:F2};\n", spawn_x, spawn_y);
            }

            string debug = "";
            debug = debug + "x coordinate: " + pixelNormalization(spawn_x, "x") + " y coordinate: " + pixelNormalization(spawn_y, "y");
            if(enableDebugLog)
            Debug.Log(debug);
        }
    }
    
    void Calibrate()
    {
        Debug.Log("calibrating");
        
        RANSAC();
       // inverseNormalization();
    }

    float pixelNormalization(float pixelCoordinates, string pixel)
    {
        // missing code to normalize pixel data
        if (pixel == "x")
        {
            // calcolo u tenendo presente della dimensione dello sprite
            if (!removeCrosshairShift)
            pixelCoordinates = pixelCoordinates + spriteSize/2;
            // calcolo u' esprimendo le coordinate rispetto al principal point
            pixelCoordinates = pixelCoordinates - (resolutionWidth / 2);
            if (Normalization)
            {
                pixelCoordinates = (pixelCoordinates * 2 / resolutionWidth) - 1;
            }
            else if (metricConversion)
            {
                pixelCoordinates = pixelCoordinates * pixelSizeX;
                if (GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().leftEyeCalibrated == true) 
                    pixelCoordinates += disparity;
            }
            if(usePixelSizes)
            pixelCoordinates = pixelCoordinates * pixelSizeX;
        }
        else if (pixel == "y")
        {
            // calcolo v tenendo presente della dimensione dello sprite, traslando l'origine degli assi in alto a sinistra
            //pixelCoordinates = resolutionHeight - pixelCoordinates - spriteSize/2;
            if (!removeCrosshairShift)
                pixelCoordinates = pixelCoordinates - spriteSize / 2;
            // calcolo v' esprimendo le coordinate rispetto al principal point
            pixelCoordinates = (resolutionHeight) / 2 - pixelCoordinates;

            if (Normalization)
            {
                pixelCoordinates = (pixelCoordinates * 2 / resolutionHeight) - 1;
            }
            else if (metricConversion)
            {
                pixelCoordinates = pixelCoordinates * pixelSizeY;
            }
            if (usePixelSizes)
                pixelCoordinates = pixelCoordinates * pixelSizeY;
        }

        return pixelCoordinates;
    }

    // revert the pixel normalization to obtain the correct G matrix
    void inverseNormalization()
    {
        // missing code to revert normalization
    }

    //builds the B matrix using readings taken by ransac_indexes
    void buildMatrix()
    {

        /*
         a = new double[4, 5] {
            {1, 0, 0, 0, 2} ,   //  initializers for row indexed by 0 
            {0, 0, 3, 0, 0} ,   //  initializers for row indexed by 1 
            {0, 0, 0, 0, 0},   //  initializers for row indexed by 2 
            {0, 2, 0, 0, 0},   //  initializers for row indexed by 3 
        };
        */


        if (enableLogging)
        {
            sr.WriteLine(string.Format("________________________________________________________________\n________________________________________________________________\n####################### Calibration Data #######################\n"));

            string debug2 = "P_I = [\n";
             for (int i = 0; i < numberOfMatches; i++)
                {
                    debug2 = debug2 + displayed_x[i].ToString("N6").Replace(".", "") + " " + displayed_y[i].ToString("N6").Replace(".", "") + ";\n";
                }
            debug2 = debug2.Remove(debug2.Length - 2);
            debug2 = debug2 + "\n];";


            sr.WriteLine(debug2);
            debug2 = "\nP_M = [\n";
            for (int i = 0; i < numberOfMatches; i++)
            {
                debug2 = debug2 + tracker_x[i].ToString("N9").Replace(",", ".") + " " + tracker_y[i].ToString("N9").Replace(",", ".") + " " + tracker_z[i].ToString("N9").Replace(",", ".") + ";\n";
            }
            debug2 = debug2.Remove(debug2.Length - 2);
            debug2 = debug2 + "\n];";
            sr.WriteLine(debug2);
            sr.WriteLine("\n\nB = [");
        }

        a = new double[numberOfMatches * 2, 12] ;
        for (int i = 0; i < numberOfMatches * 2; i += 2)
        {
            a[i, 0] = tracker_x[i / 2]; //xmi
            a[i, 1] = tracker_y[i / 2]; //ymi
            a[i, 2] = tracker_z[i / 2]; //zmi
            a[i, 3] = 1;
            a[i, 4] = 0;
            a[i, 5] = 0;
            a[i, 6] = 0;
            a[i, 7] = 0;
            a[i, 8] = -displayed_x[i / 2] * tracker_x[i / 2]; //-xi xmi
            a[i, 9] = -displayed_x[i / 2] * tracker_y[i / 2]; //-xi ymi
            a[i, 10] = -displayed_x[i / 2] * tracker_z[i / 2]; //-xi zmi
            a[i, 11] = -displayed_x[i / 2]; // -xi
            a[i + 1, 0] = 0;
            a[i + 1, 1] = 0;
            a[i + 1, 2] = 0;
            a[i + 1, 3] = 0;
            a[i + 1, 4] = tracker_x[i / 2]; //xmi
            a[i + 1, 5] = tracker_y[i / 2]; ; //ymi
            a[i + 1, 6] = tracker_z[i / 2]; //zmi
            a[i + 1, 7] = 1;
            a[i + 1, 8] = -displayed_y[i / 2] * tracker_x[i / 2]; // -yi xmi
            a[i + 1, 9] = -displayed_y[i / 2] * tracker_y[i / 2]; // -yi ymi
            a[i + 1, 10] = -displayed_y[i / 2] * tracker_z[i / 2]; // -yi zmi
            a[i + 1, 11] = -displayed_y[i / 2]; // -yi 

            if (enableLogging)
            {
                sr.WriteLine("{0:F2} {1:F2} {2:F2} {3:F2} {4:F2} {5:F2} {6:F2} {7:F2} {8:F2} {9:F2} {10:F2} {11:F2}; \n {12:F2} {13:F2} {14:F2} {15:F2} {16:F2} {17:F2} {18:F2} {19:F2} {20:F2} {21:F2} {22:F2} {23:F2};", a[i, 0].ToString("0.00").Replace(",", "."), a[i, 1].ToString("0.00").Replace(",", "."), a[i, 2].ToString("0.00").Replace(",", "."), a[i, 3].ToString("0.00").Replace(",", "."), a[i, 4].ToString("0.00").Replace(",", "."), a[i, 5].ToString("0.00").Replace(",", "."), a[i, 6].ToString("0.00").Replace(",", "."), a[i, 7].ToString("0.00").Replace(",", "."), a[i, 8].ToString("0.00").Replace(",", "."), a[i, 9].ToString("0.00").Replace(",", "."), a[i, 10].ToString("0.00").Replace(",", "."), a[i, 11].ToString("0.00").Replace(",", "."), a[i + 1, 0].ToString("0.00").Replace(",", "."), a[i + 1, 1].ToString("0.00").Replace(",", "."), a[i + 1, 2].ToString("0.00").Replace(",", "."), a[i + 1, 3].ToString("0.00").Replace(",", "."), a[i + 1, 4].ToString("0.00").Replace(",", "."), a[i + 1, 5].ToString("0.00").Replace(",", "."), a[i + 1, 6].ToString("0.00").Replace(",", "."), a[i + 1, 7].ToString("0.00").Replace(",", "."), a[i + 1, 8].ToString("0.00").Replace(",", "."), a[i + 1, 9].ToString("0.00").Replace(",", "."), a[i + 1, 10].ToString("0.00").Replace(",", "."), a[i + 1, 11].ToString("0.00").Replace(",", "."));
            }
        }
        m = a.GetLength(0);
        n = a.GetLength(1);

        if (enableLogging)
        {
            sr.WriteLine("];");
            sr.WriteLine("\nB_rows: {0}; B_columns: {1};\n", m, n);
        }
    }
    
    // computes ransac to remove outliers by generating a number of random samples to be taken in consideration, and selecting the best one
    void RANSAC()
    {
        if (enableRANSAC)
        {
            
            // save the original number of alignments used in the calibration and override it with the minimum number of alignment that can be used
            int n_matches = numberOfMatches;
            // current best match number of inliers
            int bestMatchInliers=1;
            double lowestReprojectionError=9999;
            List<int> bestMatchIndexes = new List<int>();
            int inliers = 0;

            // copy the original arrays
            for (int i=0; i< n_matches; i++)
            {
               ransac_displayed_x.Add(displayed_x[i]);
               ransac_displayed_y.Add(displayed_y[i]);
               ransac_tracker_x.Add(tracker_x[i]);
               ransac_tracker_y.Add(tracker_y[i]);
               ransac_tracker_z.Add(tracker_z[i]);
            }

            //Debug.Log("array data: " + ransac_displayed_x[0] + " " + ransac_displayed_x[5] + " " + displayed_x[0] + " " + displayed_x[5]);
            
            bool temporarilyDisableLogging = enableLogging;
            enableLogging = false;
            updateRansacStoppingCondition(bestMatchInliers, n_matches);
            
            for (int ransac_iteration = 0; ransac_iteration < ransacMaxIterations; ransac_iteration++)
            {
                if (ransac_iteration > 3000)
                    ransacMaxIterations = 3000;

                int min = 1, max = n_matches;
                List<int> listNumbers = new List<int>();

                int[] numbers = new int[max - min + 1];
                int i, len = max - min + 1, number;
                for (i = min; i < max; i++) numbers[i - min] = i;
                for (i = 0; i < ransacPointsPerBatch; i++)
                {
                    number = UnityEngine.Random.Range(0, len - 1);
                    listNumbers.Add(numbers[number]);
                    if (number != (len - 1)) numbers[number] = numbers[len - 1];
                    len--;
                }
               // listNumbers.Sort();
              //  Debug.Log("Selected points: " + listNumbers[0] + " " + listNumbers[1] + " " + listNumbers[2] + " " + listNumbers[3] + " " + listNumbers[4] + " " + listNumbers[5] + " ");

                displayed_x.Clear();
                displayed_y.Clear();
                tracker_x.Clear();
                tracker_y.Clear();
                tracker_z.Clear();
                for(int k=0; k<ransacPointsPerBatch; k++)
                {
                    displayed_x.Add(ransac_displayed_x[listNumbers[k]]);
                    displayed_y.Add(ransac_displayed_y[listNumbers[k]]);
                    tracker_x.Add(ransac_tracker_x[listNumbers[k]]);
                    tracker_y.Add(ransac_tracker_y[listNumbers[k]]);
                    tracker_z.Add(ransac_tracker_z[listNumbers[k]]);
                }

                numberOfMatches = ransacPointsPerBatch;
                buildMatrix();
                SVDdecomposition();
                numberOfMatches = n_matches;
                inliers = findInliers();
              //  Debug.Log("found inliers: " + inliers + " current max inliers: " + bestMatchInliers);
                if (inliers > bestMatchInliers && lowestReprojectionError > meanReprojectionError)
                   // if (lowestReprojectionError > reprojectionError)
                    {
                    //compute number of remaining iterations
                    bestMatchInliers = inliers;
                    lowestReprojectionError = (double) meanReprojectionError;
                    updateRansacStoppingCondition(bestMatchInliers, n_matches);
                    bestMatchIndexes.Clear();
                    for (int k = 0; k < ransacPointsPerBatch; k++)
                    {
                        bestMatchIndexes.Add(listNumbers[k]);

                    }
                    // Debug.Log("Selected points: (ciclo) " + listNumbers[0] + " " + listNumbers[1] + " " + listNumbers[2] + " " + listNumbers[3] + " " + listNumbers[4] + " " + listNumbers[5] + " ");

                  
                }
                if (ransac_iteration == ransacMaxIterations - 1)
                    Debug.Log("Tested " + ransac_iteration + " sets of points using RANSAC.");
            }
            // recompute the parameters from the best match
            displayed_x.Clear();
            displayed_y.Clear();
            tracker_x.Clear();
            tracker_y.Clear();
            tracker_z.Clear();
            bestMatchIndexes.Sort();
            for (int k = 0; k < ransacPointsPerBatch; k++)
            {
                displayed_x.Add(ransac_displayed_x[bestMatchIndexes[k]]);
                displayed_y.Add(ransac_displayed_y[bestMatchIndexes[k]]);
                tracker_x.Add(ransac_tracker_x[bestMatchIndexes[k]]);
                tracker_y.Add(ransac_tracker_y[bestMatchIndexes[k]]);
                tracker_z.Add(ransac_tracker_z[bestMatchIndexes[k]]);
            }
             Debug.Log("Selected points: " + bestMatchIndexes[0] + " " + bestMatchIndexes[1] + " " + bestMatchIndexes[2] + " " + bestMatchIndexes[3] + " " + bestMatchIndexes[4] + " " + bestMatchIndexes[5] + " ");

            

            
            // logging purposes only
            if (temporarilyDisableLogging)
            {
                enableLogging = true;
                numberOfMatches = ransacPointsPerBatch;
                buildMatrix();
                
                SVDdecomposition();
            }
            else
            {
                numberOfMatches = ransacPointsPerBatch;
                buildMatrix();
                SVDdecomposition();
                //numberOfMatches = n_matches;
            }
            Debug.Log("Inliers found:  " + bestMatchInliers + "; Mean Reprojection Error: " + meanReprojectionError);

            applyParameters();

        }
        else
        {
            buildMatrix();
            SVDdecomposition();
            applyParameters();
        }
        
    }

    void updateRansacStoppingCondition(int m, int n)
    {
        ransacMaxIterations =(int) Math.Round(Math.Log10(maxError) / Math.Log10(1 - Math.Pow(((double) m/n), ransacPointsPerBatch)));
        //Debug.Log("RANSAC Number of iterations reduced to: " + ransacMaxIterations);

    }

    int findInliers()
    {
          Debug.Log("number of matches: " + numberOfMatches);

        // least squares error
        double resx = 0, resy = 0;
        double ui=0, vi=0, wi=0;
        double xi = 0, yi = 0;
        meanReprojectionError = 0;
        int InlierCount = 0;
        for (int i=0; i<numberOfMatches; i++)
        {           
            ui = G[0] * ransac_tracker_x[i] + G[1] * ransac_tracker_y[i] + G[2] * ransac_tracker_z[i] + G[3];
            vi = G[4] * ransac_tracker_x[i] + G[5] * ransac_tracker_y[i] + G[6] * ransac_tracker_z[i] + G[7];
            wi = G[8] * ransac_tracker_x[i] + G[9] * ransac_tracker_y[i] + G[10] * ransac_tracker_z[i] + G[11];
            // cross coordinates obtained by reprojecting the points using the computed G matrix

            xi = ui / wi;
            yi = vi / wi;
            resx = (xi - ransac_displayed_x[i]) * (xi - ransac_displayed_x[i]);
            resx = (yi - ransac_displayed_y[i]) * (yi - ransac_displayed_y[i]);
            //  Debug.Log("reprojected x: " + xi + " reprojected y:" + yi + " real x:" + ransac_displayed_x[i] + " real y:" + ransac_displayed_y[i]);
              

            reprojectionError = Math.Sqrt(resx + resy);

            meanReprojectionError += reprojectionError;
            if (reprojectionError < inlierDistanceThreshold)
            {  
                InlierCount++;
            }
          //  else
          //      Debug.Log("point " + i + " not matched. Reprojection error " + reprojectionError);
        }
        meanReprojectionError = meanReprojectionError / numberOfMatches;
        return InlierCount;
    }

    void SVDdecomposition()
    {
        /*
        The algorithm calculates the singular value decomposition of a matrix of size MxN: A = U * S * V ^ T

        The algorithm finds the singular values and, optionally, matrices U and V^ T.
        The algorithm can find both first min(M, N) columns of matrix U and rows of matrix V ^ T(singular vectors), and matrices U and V ^ T wholly(of sizes MxM and NxN respectively).

        Take into account that the subroutine does not return matrix V but V ^ T.

        Input parameters:
        A - matrix to be decomposed.Array whose indexes range within[0..M - 1, 0..N - 1].
        M - number of rows in matrix A.
        N - number of columns in matrix A.
        UNeeded - 0, 1 or 2.See the description of the parameter U.
        VTNeeded - 0, 1 or 2.See the description of the parameter VT.
        AdditionalMemory -
                    If the parameter:
                     *equals 0, the algorithm doesn't use additional memory(lower requirements, lower performance).
                     *equals 1, the algorithm uses additional memory of size min(M, N)*min(M, N) of real numbers. It often speeds up the algorithm.
                     *equals 2, the algorithm uses additional memory of size M*min(M, N) of real numbers. It allows to get a maximum performance.
                    The recommended value of the parameter is 2.

        Output parameters:
        W - contains singular values in descending order.
        U -   if UNeeded = 0, U isn't changed, the left singular vectors are not calculated.
              if Uneeded = 1, U contains left singular vectors(first min(M, N) columns of matrix U).Array whose indexes range within[0..M - 1, 0..Min(M, N) - 1].
              if UNeeded = 2, U contains matrix U wholly.Array whose indexes range within[0..M - 1, 0..M - 1].
        VT -  if VTNeeded = 0, VT isn't changed, the right singular vectors are not calculated.
              if VTNeeded = 1, VT contains right singular vectors(first min(M, N) rows of matrix V ^ T).Array whose indexes range within[0..min(M, N) - 1, 0..N - 1].
              if VTNeeded = 2, VT contains matrix V^ T wholly.Array whose indexes range within[0..N - 1, 0..N - 1].
        */



        /* esempio di wikipedia https://en.wikipedia.org/wiki/Singular_value_decomposition
        double[,] a = new double[4, 5] {
            {1, 0, 0, 0, 2} ,   //  initializers for row indexed by 0 
            {0, 0, 3, 0, 0} ,   //  initializers for row indexed by 1 
            {0, 0, 0, 0, 0},   //  initializers for row indexed by 2 
            {0, 2, 0, 0, 0},   //  initializers for row indexed by 3 
        };

        int m = 4;
        int n = 5;
        */
        


        int uneeded = 0;
        int vtneeded = 2;
        int additionalmemory = 2;
        
        alglib.rmatrixsvd(a, m, n, uneeded, vtneeded, additionalmemory, out w, out u, out vt);

        // per ottenere V facendo la trasposta di VT - computazionalmente inutile, basta prendere l'ultima riga anzichè l'ultima colonna
        /*
        for (int row = 0; row < vt.GetLength(0); row++)
        {
            for (int col = 0; col < vt.GetLength(1); col++)
            {
             //   Debug.Log("PRE row: " + row + " col: " + col + " vt[" + row + "," + col + "]=" + vt[row, col] + " vt[" + col + "," + row + "]=" + vt[col, row]);
                double temp = vt[row, col];
                vt[row, col] = vt[col, row];
                vt[col, row] = temp;
              //  Debug.Log("POST row: " + row + " col: " + col + " vt[" + row + "," + col + "]=" + vt[row, col] + " vt[" + col + "," + row + "]=" + vt[col, row]);

            }

        }
        */


        // l'ultima riga di Vt è la matrice G che serve a noi (bisognerebbe prendere l'ultima colonna di V, ma la funzione ritorna la matrice trasposta)
        G = new double[vt.GetLength(1)];
        string debug = "G MATRIX: ";
        for (int row = 0; row < vt.GetLength(1); row++)
        {
            G[row] = vt[ vt.GetLength(0) - 1, row];
            debug = debug + G[row] + " ";
         
        }
        if(enableDebugLog)
        Debug.Log(debug);

        if (enableLogging)
        {
            sr.WriteLine("\nG = [\n{0:F9} {1:F9} {2:F9} {3:F9};\n{4:F9} {5:F9} {6:F9} {7:F9};\n{8:F9} {9:F9} {10:F9} {11:F9}]\n\n", G[0].ToString("n9").Replace(",", "."), G[1].ToString("n9").Replace(",", "."), G[2].ToString("n9").Replace(",", "."), G[3].ToString("n9").Replace(",", "."), G[4].ToString("n9").Replace(",", "."), G[5].ToString("n9").Replace(",", "."), G[6].ToString("n9").Replace(",", "."), G[7].ToString("n9").Replace(",", "."), G[8].ToString("n9").Replace(",", "."), G[9].ToString("n9").Replace(",", "."), G[10].ToString("n9").Replace(",", "."), G[11].ToString("n9").Replace(",", "."));
            string debug2 = "V = [";
            for (int row = 0; row < vt.GetLength(0); row++)
            {
                for (int col = 0; col < vt.GetLength(1); col++)
                {

                    debug2 = debug2 + vt[row, col].ToString("N6").Replace(",", ".") + " ";
                }
                debug2 = debug2 + ";\n";
            }
            debug2 = debug2 + "]";
            sr.WriteLine(debug2);
        }
/*
        debug2 = "U = [";
        for (int row = 0; row < u.GetLength(0); row++)
        {
            for (int col = 0; col < u.GetLength(1); col++)
            {

                debug2 = debug2 + u[row, col].ToString() + " ";
            }
            debug2 = debug2 + ";\n";
        }
        debug2 = debug2 + "]";
        sr.WriteLine(debug2);

        debug2 = "W = [";
        for (int row = 0; row < w.GetLength(0); row++)
        {
           debug2 = debug2 + w[row].ToString() + " ";
        }
        debug2 = debug2 + "]";
        sr.WriteLine(debug2);
*/

        cameraMatrixDecomposition(G[0], G[1], G[2], G[3], G[4], G[5], G[6], G[7], G[8], G[9], G[10], G[11]);



        /*
         // debug snippet to print content of vt
        debug = "";
        for (int row = 0; row < vt.GetLength(0); row++)
        {
            for (int col = 0; col < vt.GetLength(1); col++)
            {
                debug = debug + vt[row, col] + " ";
               // Debug.Log(vt[row, col] + " ");
            }
            debug = debug + "   newline    ";
        }
        Debug.Log(debug);
        */


        // debug snippet to print content of w
        /*
        debug = "";
        for (int row = 0; row < w.GetLength(0); row++)
        {
          
                debug = debug + w[row] + " ";
               // Debug.Log(w[row, col] + " ");
           
        }
        Debug.Log(debug);
        */

    }


    void cameraMatrixDecomposition(double g1, double g2, double g3, double g4, double g5, double g6, double g7, double g8, double g9, double g10, double g11, double g12)
    {

        

        G_resized = DenseMatrix.OfArray(new double[,]
        {
                {  g1,  g2,  g3 },
                {  g5,  g6,  g7 },
                {  g9,   g10,  g11 }
        });
        if(enableDebugLog)
        Debug.Log("G = " + G_resized[0, 0] + " " + G_resized[0, 1] + " " + G_resized[0, 2] + " " + G_resized[1, 0] + " " + G_resized[1, 1] + " " + G_resized[1, 2] + " " + G_resized[2, 0] + " " + G_resized[2, 1] + " " + G_resized[2, 2]);

        Matrix<double> temp = DenseMatrix.OfArray(new double[,]
        {
                {  G_resized[0,0],  G_resized[0,1],   G_resized[0,2] },
                {  G_resized[1,0],  G_resized[1,1],  G_resized[1,2] },
                {  G_resized[2,0],   G_resized[2,1],  G_resized[2,2] }
        });

        //Debug.Log("temp = " + temp[0, 0] + " " + temp[0, 1] + " " + temp[0, 2] + " " + temp[1, 0] + " " + temp[1, 1] + " " + temp[1, 2] + " " + temp[2, 0] + " " + temp[2, 1] + " " + temp[2, 2]);

        // flipud(G_resized)'

        G_resized[0, 0] = temp[2, 0];
        G_resized[0, 1] = temp[1, 0];
        G_resized[0, 2] = temp[0, 0];
        G_resized[1, 0] = temp[2, 1];
        G_resized[1, 1] = temp[1, 1];
        G_resized[1, 2] = temp[0, 1];
        G_resized[2, 0] = temp[2, 2];
        G_resized[2, 1] = temp[1, 2];
        G_resized[2, 2] = temp[0, 2];

        //qr(flipud(M)')
        var qr = G_resized.QR();

         Q = qr.Q;
         R = qr.R;


        //temp = Q;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                temp[i, j] = Q[i, j];
            }
        }

        // Q è la matrice ortogonale, ovvero la matrice di rotazione R
        // Q = flipud(Q');
        Q[0, 0] = temp[0, 2];
        Q[0, 1] = temp[1, 2];
        Q[0, 2] = temp[2, 2];
        Q[1, 0] = temp[0, 1];
        Q[1, 1] = temp[1, 1];
        Q[1, 2] = temp[2, 1];
        Q[2, 0] = temp[0, 0];
        Q[2, 1] = temp[1, 0];
        Q[2, 2] = temp[2, 0];

        //temp = R;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                temp[i, j] = R[i, j];
            }
        }

        // R è la matrice triangolare superiore, ovvero quella dei parametri intrinseci (K)
        // R = fliplr(flipud(R'));
        R[0, 0] = temp[2, 2];
        R[0, 1] = temp[1, 2];
        R[0, 2] = temp[0, 2];
        R[1, 0] = temp[2, 1];
        R[1, 1] = temp[1, 1];
        R[1, 2] = temp[0, 1];
        R[2, 0] = temp[2, 0];
        R[2, 1] = temp[1, 0];
        R[2, 2] = temp[0, 0];


        // J = diag(sign(diag(K_dec)));
        int sgn1, sgn2, sgn3;
        if (R[0, 0] > 0)
            sgn1 = 1;
        else
            sgn1 = -1;

        if (R[1, 1] > 0)
            sgn2 = 1;
        else
            sgn2 = -1;

        if (R[2, 2] > 0)
            sgn3 = 1;
        else
            sgn3 = -1;
        Matrix<double> J = DenseMatrix.OfArray(new double[,]
        {
                {  sgn1,  0,   0 },
                {  0,  sgn2,  0 },
                {  0,   0,  sgn3 }
        });


        //temp = R;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                temp[i, j] = R[i, j];
            }
        }
        //K = K * J
        R[0, 0] = temp[0, 0] * J[0, 0] + temp[0, 1] * J[1, 0] + temp[0, 2] * J[2, 0];
        R[0, 1] = temp[0, 0] * J[0, 1] + temp[0, 1] * J[1, 1] + temp[0, 2] * J[2, 1];
        R[0, 2] = temp[0, 0] * J[0, 2] + temp[0, 1] * J[1, 2] + temp[0, 2] * J[2, 2];

        R[1, 0] = temp[1, 0] * J[0, 0] + temp[1, 1] * J[1, 0] + temp[1, 2] * J[2, 0];
        R[1, 1] = temp[1, 0] * J[0, 1] + temp[1, 1] * J[1, 1] + temp[1, 2] * J[2, 1];
        R[1, 2] = temp[1, 0] * J[0, 2] + temp[1, 1] * J[1, 2] + temp[1, 2] * J[2, 2];

        R[2, 0] = temp[2, 0] * J[0, 0] + temp[2, 1] * J[1, 0] + temp[2, 2] * J[2, 0];
        R[2, 1] = temp[2, 0] * J[0, 1] + temp[2, 1] * J[1, 1] + temp[2, 2] * J[2, 1];
        R[2, 2] = temp[2, 0] * J[0, 2] + temp[2, 1] * J[1, 2] + temp[2, 2] * J[2, 2];


        //temp = Q;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                temp[i, j] = Q[i, j];
            }
        }

        //R = J * R

        Q[0, 0] = J[0, 0] * temp[0, 0] + J[0, 1] * temp[1, 0] + J[0, 2] * temp[2, 0];
        Q[0, 1] = J[0, 0] * temp[0, 1] + J[0, 1] * temp[1, 1] + J[0, 2] * temp[2, 1];
        Q[0, 2] = J[0, 0] * temp[0, 2] + J[0, 1] * temp[1, 2] + J[0, 2] * temp[2, 2];

        Q[1, 0] = J[1, 0] * temp[0, 0] + J[1, 1] * temp[1, 0] + J[1, 2] * temp[2, 0];
        Q[1, 1] = J[1, 0] * temp[0, 1] + J[1, 1] * temp[1, 1] + J[1, 2] * temp[2, 1];
        Q[1, 2] = J[1, 0] * temp[0, 2] + J[1, 1] * temp[1, 2] + J[1, 2] * temp[2, 2];

        Q[2, 0] = J[2, 0] * temp[0, 0] + J[2, 1] * temp[1, 0] + J[2, 2] * temp[2, 0];
        Q[2, 1] = J[2, 0] * temp[0, 1] + J[2, 1] * temp[1, 1] + J[2, 2] * temp[2, 1];
        Q[2, 2] = J[2, 0] * temp[0, 2] + J[2, 1] * temp[1, 2] + J[2, 2] * temp[2, 2];


        if (flipX)
        {
            // If the image x-axis and camera x-axis point in opposite directions, negate the first column of K and the first row of R.
            R[0, 0] = -R[0, 0];
            R[1, 0] = -R[1, 0];
            R[2, 0] = -R[2, 0];
            Q[0, 0] = -Q[0, 0];
            Q[0, 1] = -Q[0, 1];
            Q[0, 2] = -Q[0, 2];
        }
        if (flipY)
        {
            // If the image y-axis and camera y-axis point in opposite directions, negate the second column of K and the second row of R.
            R[0, 1] = -R[0, 1];
            R[1, 1] = -R[1, 1];
            R[2, 1] = -R[2, 1];
            Q[1, 0] = -Q[1, 0];
            Q[1, 1] = -Q[1, 1];
            Q[1, 2] = -Q[1, 2];
        }
        if (flipZ)
        {
            // If the camera looks down the negative-z axis, negate the third column of K. Also negate the third column of R.
            R[0, 2] = -R[0, 2];
            R[1, 2] = -R[1, 2];
            R[2, 2] = -R[2, 2];
            Q[0, 2] = -Q[0, 2];
            Q[1, 2] = -Q[1, 2];
            Q[2, 2] = -Q[2, 2];
        }

        // computing determinant of the rotation matrix, and change its sign if det = -1
        double det = Q[0, 0] * Q[1, 1] * Q[2, 2] + Q[0, 1] * Q[1, 2] * Q[2, 0] + Q[0, 2] * Q[1, 0] * Q[2, 1] - Q[2, 0] * Q[1, 1] * Q[0, 2] - Q[2, 1] * Q[1, 2] * Q[0, 0] - Q[2, 2] * Q[1, 0] * Q[0, 1];
        if (det < 0)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Q[i, j] = -Q[i, j];
                }
            }
        }

        if (enableDebugLog)
        {
            Debug.Log("R = " + Q[0, 0] + " " + Q[0, 1] + " " + Q[0, 2] + " " + Q[1, 0] + " " + Q[1, 1] + " " + Q[1, 2] + " " + Q[2, 0] + " " + Q[2, 1] + " " + Q[2, 2]);
            Debug.Log("K = " + R[0, 0] + " " + R[0, 1] + " " + R[0, 2] + " " + R[1, 0] + " " + R[1, 1] + " " + R[1, 2] + " " + R[2, 0] + " " + R[2, 1] + " " + R[2, 2]);
        }

        // COMPUTING EULER ANGLES FROM ROTATION MATRIX
        double sy = Math.Sqrt(Q[0, 0] * Q[0, 0] + Q[1, 0] * Q[1, 0]);
        bool singular = sy < 0.0000001f; // If       
        if (!singular)
        {
            x = Math.Atan2(Q[2, 1], Q[2, 2]);
            y = Math.Atan2(-Q[2, 0], sy);
            z = Math.Atan2(Q[1, 0], Q[0, 0]);
        }
        else
        {
            x = Math.Atan2(-Q[1, 2], Q[1, 1]);
            y = Math.Atan2(-Q[2, 0], sy);
            z = 0;
        }
        if (z > -0.0000001f && z < 0.0000001f)
            z = 0;

        // COMPUTING TRANSLATION VECTOR FROM CAMERA MATRIX
        t3 = g12;
        t2 = (g8 - R[1, 2] * g12) / R[1, 1];
        t1 = (g4 - R[0, 1] * t2 - R[0, 2] * g12) / R[0, 0];


    }


    void applyParameters()
    {
        /*
        //GameObject.FindWithTag("MainCamera").transform.rotation = Quaternion.Euler((float)x, (float)y, (float)z);
        GameObject.FindWithTag("MainCamera").transform.localRotation = Quaternion.Euler((float)-(x * Mathf.Rad2Deg), (float)(y * Mathf.Rad2Deg), (float)(z * Mathf.Rad2Deg));
        //GameObject.FindWithTag("MainCamera").transform.localRotation = Quaternion.Inverse(Quaternion.Euler((float)(x * Mathf.Rad2Deg), (float)(y * Mathf.Rad2Deg), (float)(z * Mathf.Rad2Deg)));
        GameObject.FindWithTag("MainCamera").transform.localPosition = new Vector3((float)t1, (float)t2, (float)-Math.Abs(t3));
        */
    
        //GameObject.FindWithTag("MainCamera").transform.rotation = Quaternion.Euler((float)x, (float)y, (float)z);
        GameObject.FindWithTag("TrackerEye").transform.localRotation = Quaternion.Euler((float)-(x * Mathf.Rad2Deg), (float)(y * Mathf.Rad2Deg), (float)(z * Mathf.Rad2Deg));
        //GameObject.FindWithTag("MainCamera").transform.localRotation = Quaternion.Inverse(Quaternion.Euler((float)(x * Mathf.Rad2Deg), (float)(y * Mathf.Rad2Deg), (float)(z * Mathf.Rad2Deg)));
        GameObject.FindWithTag("TrackerEye").transform.localPosition = new Vector3((float)Math.Abs(t1), (float)-Math.Abs(t2), (float)- Math.Abs(t3));

        // qui ci va la differenza tra i valori trovati e quelli già impostati 
        //GameObject.FindWithTag("MainCamera").transform.localRotation = Quaternion.Inverse(GameObject.FindWithTag("MainCamera").transform.localRotation) * GameObject.FindWithTag("TrackerEye").transform.localRotation;
        //GameObject.FindWithTag("MainCamera").transform.localPosition = transform.InverseTransformPoint(GameObject.FindWithTag("TrackerEye").transform.localPosition);
        GameObject.FindWithTag("MainCamera").transform.localRotation =Quaternion.Euler( -GameObject.FindWithTag("TrackerEye").transform.localEulerAngles + GameObject.FindWithTag("TrackerMetaCameraTransform").transform.localEulerAngles );
        GameObject.FindWithTag("MainCamera").transform.localPosition =  GameObject.FindWithTag("TrackerEye").transform.localPosition + GameObject.FindWithTag("TrackerMetaCameraTransform").transform.localPosition;
        //GameObject.FindWithTag("MainCamera").transform.localPosition.z = GameObject.FindWithTag("MainCamera").transform.localPosition.z - GameObject.FindWithTag("TrackerEye").transform.localPosition.z



        /*
        if (enableRigidTransformation)
        {
            GameObject.FindWithTag("MainCamera").transform.rotation *= Quaternion.Euler(xAngle, yAngle, zAngle);
            GameObject cam = GameObject.FindWithTag("MainCamera");
            cam.transform.localPosition = new Vector3(cam.transform.position.x * xMultiplier, cam.transform.position.y * yMultiplier, cam.transform.position.z * zMultiplier);
        }
        */

        Debug.Log("pos x=" + Math.Abs(t1).ToString("0.00") + "; pos y=" + (-Math.Abs(t1)).ToString("0.00") + "; pos z=" + (-Math.Abs(t3)).ToString("0.00") + ";   rot x=" + (x * Mathf.Rad2Deg).ToString("0.00") + "; rot y=" + (y * Mathf.Rad2Deg).ToString("0.00") + "; z=" + (z * Mathf.Rad2Deg).ToString("0.00"));
       // Debug.Log("pos x=" + GameObject.FindWithTag("MainCamera").transform.localPosition.x.ToString("0.00") + "; pos y=" + GameObject.FindWithTag("MainCamera").transform.localPosition.y.ToString("0.00") + "; pos z=" + GameObject.FindWithTag("MainCamera").transform.localPosition.z.ToString("0.00") + ";   rot x=" + GameObject.FindWithTag("MainCamera").transform.localRotation.eulerAngles.x + "; rot y=" + GameObject.FindWithTag("MainCamera").transform.localRotation.eulerAngles.y + "; z=" + GameObject.FindWithTag("MainCamera").transform.localRotation.eulerAngles.z);


        // controllare che la distanza focale sia espressa in millimetri
        double focalLength = 19.20401f; // (R[0, 0] + R[1, 1]) / 2;
        double SensorSizeX = focalLength * resolutionWidth / (R[0, 0]*1000);
        double SensorSizeY = focalLength * resolutionHeight / (R[1, 1]*1000);
        double LensShiftX = -(R[0, 2]*1000 - resolutionWidth / 2) / resolutionWidth; //potrebbe essere a segno invertito
        double LensShiftY = (R[1, 2]*1000 - resolutionHeight / 2) / resolutionHeight;

        GameObject.FindWithTag("MainCamera").GetComponent<smooth>().enabled = false;
        if (useIntrinsics)
        {
            GameObject.FindWithTag("MainCamera").GetComponent<Camera>().usePhysicalProperties = true;
            GameObject.FindWithTag("MainCamera").GetComponent<Camera>().focalLength = (float)focalLength; // in millimeters
            GameObject.FindWithTag("MainCamera").GetComponent<Camera>().lensShift = new Vector2((float)LensShiftX, (float)LensShiftY); // The lens offset of the camera. The lens shift is relative to the sensor size. For example, a lens shift of 0.5 offsets the sensor by half its horizontal size.
            GameObject.FindWithTag("MainCamera").GetComponent<Camera>().sensorSize = new Vector2((float)SensorSizeX, (float)SensorSizeY);
        }
        if (enableLogging)
        {
            sr.WriteLine("\nK = [\n{0:F9} {1:F9} {2:F9};\n{3:F9} {4:F9} {5:F9};\n{6:F9} {7:F9} {8:F9}]\n\n", R[0, 0].ToString("n6").Replace(",", "."), R[0, 1].ToString("n6").Replace(",", "."), R[0, 2].ToString("n6").Replace(",", "."), R[1, 0].ToString("n6").Replace(",", "."), R[1, 1].ToString("n6").Replace(",", "."), R[1, 2].ToString("n6").Replace(",", "."), R[2, 0].ToString("n6").Replace(",", "."), R[2, 1].ToString("n6").Replace(",", "."), R[2, 2].ToString("n6").Replace(",", "."));
            sr.WriteLine("\nR = [\n{0:F9} {1:F9} {2:F9};\n{3:F9} {4:F9} {5:F9};\n{6:F9} {7:F9} {8:F9}]\n\n", Q[0, 0].ToString("n6").Replace(",", "."), Q[0, 1].ToString("n6").Replace(",", "."), Q[0, 2].ToString("n6").Replace(",", "."), Q[1, 0].ToString("n6").Replace(",", "."), Q[1, 1].ToString("n6").Replace(",", "."), Q[1, 2].ToString("n6").Replace(",", "."), Q[2, 0].ToString("n6").Replace(",", "."), Q[2, 1].ToString("n6").Replace(",", "."), Q[2, 2].ToString("n6").Replace(",", "."));
            sr.WriteLine("\nT_Vector = [ {0:F2} {1:F2} {2:F2}]\n", t1.ToString("0.00").Replace(",", "."), t2.ToString("0.00").Replace(",", "."), t3.ToString("0.00").Replace(",", "."));
            sr.WriteLine("\nRotationEulerAngles = [{0:F2} {1:F2} {2:F2}]\n", (x * Mathf.Rad2Deg).ToString("0.00").Replace(",", "."), (y * Mathf.Rad2Deg).ToString("0.00").Replace(",", "."), (z * Mathf.Rad2Deg).ToString("0.00").Replace(",", "."));
            sr.Close();
        }

        calibrationSuccess = true;

        if (!calibrateRightEye)
        {
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().leftEyePosition = GameObject.FindWithTag("MainCamera").transform.localPosition;
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().leftEyeRotation = GameObject.FindWithTag("MainCamera").transform.rotation;
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().SensorSizeX_left = SensorSizeX;
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().SensorSizeY_left = SensorSizeY;
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().LensShiftX_left = LensShiftX;
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().LensShiftY_left = LensShiftY;
        }
        else
        {
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().rightEyePosition = GameObject.FindWithTag("MainCamera").transform.localPosition;
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().rightEyeRotation = GameObject.FindWithTag("MainCamera").transform.rotation;
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().SensorSizeX_right = SensorSizeX;
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().SensorSizeY_right = SensorSizeY;
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().LensShiftX_right = LensShiftX;
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().LensShiftY_right = LensShiftY;
        }
    }

    void changeScene()
    {
        if (!calibrateRightEye)
        {
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().leftEyePosition = GameObject.FindWithTag("MainCamera").transform.localPosition;
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().leftEyeRotation = GameObject.FindWithTag("MainCamera").transform.rotation;
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().leftEyeCalibrated = true;
            // SceneManager.LoadScene(1);

            // RESET ALL VARIABLES TO BEGIN RIGHT EYE CALIBRATION

            
            calibrateRightEye = true;
            GameObject.Find("LeftCamera").GetComponent<smooth>().enabled = false;
            GameObject.Find("RightCamera").GetComponent<smooth>().enabled = true;
            GameObject.Find("RightCamera").tag = "MainCamera";
            GameObject.Find("LeftCamera").tag = "Untagged";
            //GameObject.Find("rightEye").tag = "TrackerEye";
            //GameObject.Find("leftEye").tag = "Untagged";

            numberOfMatches = originalNumberOfMatches;
            logNumber = 0;
            fileWritten = false;
            modifyTranslations = true;
            ready = false;
            tracker_x.Clear();
            tracker_y.Clear();
            tracker_z.Clear();
            ransac_tracker_x.Clear();
            ransac_tracker_y.Clear();
            ransac_tracker_z.Clear();
            displayed_x.Clear();
            displayed_y.Clear();
            ransac_displayed_x.Clear();
            ransac_displayed_y.Clear();
            head_x.Clear();
            head_y.Clear();
            head_z.Clear();
            head_rot_x.Clear();
            head_rot_y.Clear();
            head_rot_z.Clear();
            ransac_indexes.Clear();

            if (enableLogging)
            {
                while (File.Exists("Logs/Log" + logNumber + ".txt"))
                    logNumber++;
                sr = File.CreateText("Logs/Log" + logNumber + ".txt");
            }
            if (enableDebugLog)
                Debug.Log("match number " + (matchesCounter + 1));
            calibrationSuccess = false;
            matchesCounter = 0;

            spawnCross();
        }
        else
        {
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().rightEyePosition = GameObject.FindWithTag("MainCamera").transform.localPosition;
            GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().rightEyeRotation = GameObject.FindWithTag("MainCamera").transform.rotation;
            
            logNumber = 0;
            if (!fileWritten)
            {
                while (File.Exists("Logs/Params_" + logNumber + ".txt"))
                    logNumber++;
                savedParams = File.CreateText("Logs/Params_" + logNumber + ".txt");

                savedParams.WriteLine("{0} {1} {2}\n{3} {4} {5} {6}\n{7} {8} {9} {10}\n\n{11} {12} {13}\n{14} {15} {16} {17}\n{18} {19} {20} {21}",
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().leftEyePosition.x,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().leftEyePosition.y,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().leftEyePosition.z,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().leftEyeRotation.x,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().leftEyeRotation.y,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().leftEyeRotation.z,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().leftEyeRotation.w,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().SensorSizeX_left,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().SensorSizeY_left,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().LensShiftX_left,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().LensShiftY_left,

                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().rightEyePosition.x,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().rightEyePosition.y,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().rightEyePosition.z,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().rightEyeRotation.x,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().rightEyeRotation.y,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().rightEyeRotation.z,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().rightEyeRotation.w,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().SensorSizeX_right,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().SensorSizeY_right,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().LensShiftX_right,
                    GameObject.Find("CalibrationParameters").GetComponent<savedParameters>().LensShiftY_right
                    );
                savedParams.Close();
                fileWritten = true;
            }


        }

    }

}
