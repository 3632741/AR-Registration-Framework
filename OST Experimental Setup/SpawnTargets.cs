using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SpawnTargets : MonoBehaviour {

    [Header("Experimental settings")]
    public int trialsNumber = 0;
    private int datiAcquisiti = 0;
    public string logDataFilename="data";

    [Header("Interaction area settings")]

    [Tooltip("Number of possible depth spawn positions in the interaction area. Distance between spawn points defined by SPAWN DISTANCE parameter.")]
    // z
    public int depth=3;
    [Tooltip("Number of possible width spawn positions in the interaction area. Distance between spawn points defined by SPAWN DISTANCE parameter.")]
    // x
    public int width=3;
    [Tooltip("Number of possible height spawn positions in the interaction area. Distance between spawn points defined by SPAWN DISTANCE parameter.")]
    // y
    public int height=3;
    [Tooltip("Minimum distance between spawn positions (centimeters).")]
    public float spawnDistance = 5;
    [Tooltip("Distance of the volume from the user (centimeters). Measured from the centre of the volume.")]
    public float userDistance = 50;
    [Tooltip("Origin of the spawning volume.")]
    public GameObject Volume;
    public GameObject Head;

    [Header("Target settings")]
    [Tooltip("Prefab of the spawned target.")]
    public GameObject target;
    [Tooltip("Radius of the target (centimeters).")]
    public float targetSize = 1;

    private int r, c, h;
    private double xVolumeOrigin, yVolumeOrigin, zVolumeOrigin;
    StreamWriter sr, savedParams;
    bool fileWritten = false;
    bool headFound = false;
    private float xreal, yreal, zreal;
    private List<Vector3> spawnPosition = new List<Vector3>(26);
    // Use this for initialization
    void Start () {
        // conversion into centimeters
        spawnDistance = spawnDistance * 0.01f;
        userDistance = userDistance * 0.01f;

        // creates a random order of spawning points
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    float xSpawningPosition, ySpawningPosition, zSpawningPosition;
                    xSpawningPosition = i * spawnDistance - spawnDistance;
                    ySpawningPosition = j * spawnDistance - spawnDistance;
                    zSpawningPosition = k * spawnDistance - spawnDistance;
                    spawnPosition.Add(new Vector3(xSpawningPosition, ySpawningPosition, zSpawningPosition));
                }
            }
        }
        spawnPosition = ShuffleList(spawnPosition);

        //create log file
        int logNumber = 0;
        while (File.Exists("Logs/" + logDataFilename + logNumber + ".txt"))
            logNumber++;
        savedParams = File.CreateText("Logs/" + logDataFilename + logNumber + ".txt");

        //adapt spawning volume to user position
       /* if (depth % 2 == 0) {
            zVolumeOrigin = Head.transform.position.z - (depth / 2 * spawnDistance)+(0.5*spawnDistance)+userDistance;
            }
        else
        {
            zVolumeOrigin = Head.transform.position.z - ((float)(depth-1) / 2 * spawnDistance) + userDistance;
        }
        if (height % 2 == 0)
        {
            yVolumeOrigin = Head.transform.position.y - ((float)height / 2 * spawnDistance) + (0.5 * spawnDistance);
        }
        else
        {
            yVolumeOrigin = Head.transform.position.y - ((float)(height - 1) / 2 * spawnDistance);
        }

        if (width % 2 == 0)
        {
            xVolumeOrigin = Head.transform.position.x;
        }
        else
        {
            xVolumeOrigin = Head.transform.position.x ;
        }
        Volume.transform.position = new Vector3((float)xVolumeOrigin, (float)yVolumeOrigin, (float)zVolumeOrigin);
*/

        // spawn the entire volume
        /*
        if (depth % 2 == 0 && width % 2 == 0 && height % 2 == 0)
        {
          for (int i = -width / 2; i < width / 2; i++)
          {
              for (int j = -height / 2; j < height / 2; j++)
              {
                    for (int k = -depth / 2; k < depth / 2; k++)
                    {
                        GameObject disk = Instantiate(target, new Vector3(i * spawnDistance, j * spawnDistance, k * spawnDistance), Quaternion.identity) as GameObject;
                        disk.transform.SetParent(Volume.transform);
                        disk.transform.localPosition = new Vector3(i * spawnDistance, j * spawnDistance, k * spawnDistance);
                        xreal = disk.transform.position.x;
                        yreal = disk.transform.position.y;
                        zreal = disk.transform.position.z;
                        Debug.Log("i = " + i + " j = " + j + " k = " + k);
                    }
                }
            }
        }
        else
        {
            for (int i = -width / 2; i < width / 2+1; i++)
            {
                for (int j = - height / 2; j < height / 2+1; j++)
                {
                    for (int k = -depth / 2; k < depth / 2+1; k++)
                    {
                        GameObject disk = Instantiate(target, new Vector3(i * spawnDistance, j * spawnDistance, k * spawnDistance), Quaternion.identity) as GameObject;
                        disk.transform.SetParent(Volume.transform);
                        disk.transform.localPosition = new Vector3(i * spawnDistance, j * spawnDistance, k * spawnDistance);
                        xreal = disk.transform.position.x;
                        yreal = disk.transform.position.y;
                        zreal = disk.transform.position.z;
                        //   Instantiate(target, new Vector3(j * spawnDistance, i * spawnDistance, k * spawnDistance), Quaternion.Euler(-90, 0, 0));
                        // Debug.Log("i = " + i + " j = " + j + " k = " + k);
                    }
                }
            }
        }
        */
	}
	
	// Update is called once per frame
	void Update () {
        if (!headFound)
        {
            //adapt spawning volume to user position
            if (!(Head.transform.position.x == 0 && Head.transform.position.y == 0 && Head.transform.position.z == 0))
            {
                headFound = true;


                //adapt spawning volume to user position
                if (depth % 2 == 0)
                {
                    zVolumeOrigin = Head.transform.position.z - (depth / 2 * spawnDistance) + (0.5 * spawnDistance) + userDistance;
                }
                else
                {
                    zVolumeOrigin = Head.transform.position.z - ((float)(depth - 1) / 2 * spawnDistance) + userDistance;
                }
                if (height % 2 == 0)
                {
                    yVolumeOrigin = Head.transform.position.y - ((float)height / 2 * spawnDistance) + (0.5 * spawnDistance);
                }
                else
                {
                    yVolumeOrigin = Head.transform.position.y - ((float)(height - 1) / 2 * spawnDistance);
                }

                if (width % 2 == 0)
                {
                    xVolumeOrigin = Head.transform.position.x /*- ((float)width / 2 * spawnDistance) + (0.5 * spawnDistance)*/;
                }
                else
                {
                    xVolumeOrigin = Head.transform.position.x /*- ((float)(width - 1) / 2 * spawnDistance)*/;
                }
                Volume.transform.position = new Vector3((float)xVolumeOrigin, (float)yVolumeOrigin, (float)zVolumeOrigin);
            }

        }
        else
        {
            if (Input.GetKeyDown("space") && datiAcquisiti < trialsNumber)
            {
                spawn();
            }
            else if (Input.GetKeyDown("space"))
            {
                Debug.Log("Fine acquisizione dati");
                savedParams.Close();
               // Application.Quit();
            }
            if ((Input.GetKeyDown("enter") || Input.GetKeyDown("return")) && !fileWritten)
            {
                fileWritten = true;
                save();
                fileWritten = false;
            }
        }
    }


    void save()
    {

        //salva i dati su file
        Debug.Log("salvo su file");

        // x y z reali -> x y z percepite -> x y z rilevate dal kinect -> pos tracker
        GameObject trackedPoint = GameObject.FindWithTag("kinect");
        GameObject kinectTracker = GameObject.Find("Target_Data");
        savedParams.WriteLine("{0} {1} {2}\n{3} {4} {5}\n{6} {7} {8}\n{9} {10} {11}\n{12} {13} {14}\n\n",
                                          xreal,
                                          yreal,
                                          zreal,
                                          trackedPoint.transform.position.x,
                                          trackedPoint.transform.position.y,
                                          trackedPoint.transform.position.z,
               trackedPoint.transform.localPosition.x,
               trackedPoint.transform.localPosition.y,
               trackedPoint.transform.localPosition.z,
               kinectTracker.transform.position.x,
               kinectTracker.transform.position.y,
               kinectTracker.transform.position.z,
               Head.transform.position.x,
               Head.transform.position.y,
               Head.transform.position.z
      
                      );

    }

    void spawn()
    {

        GameObject disk = Instantiate(target, spawnPosition[datiAcquisiti], Quaternion.identity) as GameObject;
        disk.transform.SetParent(Volume.transform);
        //Debug.Log("spawning target " + datiAcquisiti + " in: " + disk.transform.localPosition.x + "  " + disk.transform.localPosition.y + "  " + disk.transform.localPosition.z );
        Debug.Log("spawning target " + datiAcquisiti + " Press enter to save data.");
        disk.transform.localPosition = spawnPosition[datiAcquisiti];
        xreal = disk.transform.position.x;
        yreal = disk.transform.position.y;
        zreal = disk.transform.position.z;
        datiAcquisiti++;

        /*
        Debug.Log("spawning a target");
        float xSpawningPosition, ySpawningPosition, zSpawningPosition;
        if (depth % 2 == 0)
        {
            r = Random.Range(-depth / 2, depth / 2 + 1);
            // if positive, remove 0.5*spawndistance
            // if negative, add 0.5*spawndistance
            zSpawningPosition =  r > 0 ? ( r * spawnDistance - 0.5f * spawnDistance) : (r * spawnDistance + 0.5f * spawnDistance);
        }
        else
        {
            r = Random.Range(-depth / 2, depth / 2 + 1);
            zSpawningPosition = r * spawnDistance;

        }
        if (width % 2 == 0)
        {
            c = Random.Range(-width / 2, width / 2 + 1);
            xSpawningPosition = c > 0 ? (c * spawnDistance - 0.5f * spawnDistance) : (c * spawnDistance + 0.5f * spawnDistance);
        }
        else
        {
            c = Random.Range(-width / 2, width / 2 + 1);
            xSpawningPosition = c * spawnDistance;
        }
        if (height % 2 == 0)
        {
            h = Random.Range(-height / 2, height / 2 + 1);
            ySpawningPosition= h > 0 ? (h * spawnDistance - 0.5f * spawnDistance) : (h * spawnDistance + 0.5f * spawnDistance);
        }
        else
        {
            h = Random.Range(-height / 2, height / 2 + 1);
            ySpawningPosition = h * spawnDistance;
        }

        
        GameObject disk = Instantiate(target, new Vector3(xSpawningPosition, ySpawningPosition, zSpawningPosition), Quaternion.identity) as GameObject;
        disk.transform.SetParent(Volume.transform);
        disk.transform.localPosition = new Vector3(xSpawningPosition, ySpawningPosition, zSpawningPosition);
        xreal = disk.transform.position.x;
        yreal = disk.transform.position.y;
        zreal = disk.transform.position.z;

    */
    }

    private List<Vector3> ShuffleList<Vector3>(List<Vector3> inputList)
    {
        List<Vector3> randomList = new List<Vector3>();

        System.Random r = new System.Random();
        int randomIndex = 0;
        while (inputList.Count > 0)
        {
            randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
            randomList.Add(inputList[randomIndex]); //add it to the new, random list
            inputList.RemoveAt(randomIndex); //remove to avoid duplicates
        }

        return randomList; //return the new random list
    }


}
