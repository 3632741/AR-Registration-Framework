using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class spawnTargetsExp2 : MonoBehaviour
{

    [Header("Experimental settings")]
    public int numeroAcquisizioni = 0;
    private int datiAcquisiti = 0;
    public string logDataFilename = "data";

    [Header("Interaction area settings")]
    public List<Vector3> spawnPosition = new List<Vector3>(9);
    //private List<int> spawnSequence;
    public List<int> spawnSequence = new List<int>();

    [Header("Target settings")]
    [Tooltip("Prefab of the spawned target.")]
    public List<GameObject> kitchenObjects;
    public GameObject Head;

    private int r, c, h;
    //bool headFound = false;
    private double xVolumeOrigin, yVolumeOrigin, zVolumeOrigin;
    StreamWriter sr, savedParams;
    bool fileWritten = false;
    private float xreal, yreal, zreal;
    private GameObject spawnedItem;
    // Use this for initialization
    void Start()
    {


        for (int i = 0; i < numeroAcquisizioni; i++)
        {
            List<int> singleSequence = GenerateRandom(9, 0, 9);
            for (int k = 0; k < singleSequence.Count; k++)
            {
                spawnSequence.Add(singleSequence[k]);
            }
        }


        // create log file
        int logNumber = 0;
        while (File.Exists("Logs/" + logDataFilename + logNumber + ".txt"))
            logNumber++;
        savedParams = File.CreateText("Logs/" + logDataFilename + logNumber + ".txt");


    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown("space") && datiAcquisiti < numeroAcquisizioni * 9)
        {
            spawn(Random.Range(0, kitchenObjects.Count), spawnSequence[datiAcquisiti]);
        }
        else if (Input.GetKeyDown("space"))
        {
            Debug.Log("Fine acquisizione dati");
            savedParams.Close();
            Application.Quit();
        }
        if ((Input.GetKeyDown("enter") || Input.GetKeyDown("return")) && !fileWritten)
        {
            fileWritten = true;
            save();
            fileWritten = false;
        }

    }



    void save()
    {

        //salva i dati su file
        Debug.Log("salvato su file l'oggetto " + (datiAcquisiti - 1) + ". premere spazio per spawnare il successivo");

        // x y z reali -> x y z percepite -> x y z rilevate dal kinect -> pos tracker -> pos head

        GameObject kinectTracker = GameObject.Find("Target_Data");
        savedParams.WriteLine("{0} {1} {2}\n{3} {4} {5}\n{6} {7} {8}\n{9} {10} {11}\n{12}\n\n",
            xreal,
            yreal,
            zreal,
            kinectTracker.transform.position.x,
            kinectTracker.transform.position.y,
            kinectTracker.transform.position.z,
            Head.transform.position.x,
            Head.transform.position.y,
            Head.transform.position.z,
            Head.transform.rotation.eulerAngles.x,
            Head.transform.rotation.eulerAngles.y,
            Head.transform.rotation.eulerAngles.z,
            spawnSequence[datiAcquisiti - 1]
        );
        Destroy(spawnedItem);

    }


    void spawn(int item, int position)
    {
        //Debug.Log("spawning item numero  " + item + " in kitechen objects, in posizione "+ position);
        spawnedItem = Instantiate(kitchenObjects[item], spawnPosition[position], Quaternion.identity) as GameObject;
        //disk.transform.SetParent(Volume.transform);
        //Debug.Log("spawning target " + datiAcquisiti + " in: " + disk.transform.localPosition.x + "  " + disk.transform.localPosition.y + "  " + disk.transform.localPosition.z );
        Debug.Log("spawning target " + datiAcquisiti + " Press enter to save data.");
        spawnedItem.transform.position = spawnPosition[position];
        xreal = spawnedItem.transform.position.x;
        yreal = spawnedItem.transform.position.y;
        zreal = spawnedItem.transform.position.z;
        datiAcquisiti++;


    }


    public static List<int> GenerateRandom(int count, int min, int max)
    {
        if (max <= min || count < 0 ||
            (count > max - min && max - min > 0))
        {
            throw new System.ArgumentOutOfRangeException("Range " + min + " to " + max +
                " (" + ((System.Int64)max - (System.Int64)min) + " values), or count " + count + " is illegal");
        }

        HashSet<int> candidates = new HashSet<int>();
        System.Random random = new System.Random();
        for (int top = max - count; top < max; top++)
        {
            if (!candidates.Add(random.Next(min, top + 1)))
            {
                candidates.Add(top);
            }
        }

        List<int> result = candidates.ToList();

        for (int i = result.Count - 1; i > 0; i--)
        {
            int k = random.Next(i + 1);
            int tmp = result[k];
            result[k] = result[i];
            result[i] = tmp;
            //Debug.Log (result [i]);
        }
        return result;
    }

}
