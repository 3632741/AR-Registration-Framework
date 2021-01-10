using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
public class tinyCalibration : MonoBehaviour
{
    [Header("Scale factor and rotation correction")]
    public float adjustmentShift = 0;
    public float adjustmentAngle = 0;
    public float focalStep = 0;

    [Header("Toggle to skip calibration and read existing calib file")]
    public bool skipCalibration = false;
    public bool secondExperiment = false;

    StreamWriter sr, savedParams;

    bool modifyTranslations = true;
    public GameObject CheckerBoard;
    private GameObject rotationInterface;
    private GameObject translationInterface;
    private GameObject rightEye, leftEye;
    bool fileWritten = false;
    void Start()
    {
        rotationInterface = GameObject.FindWithTag("RotationInterface");
        translationInterface = GameObject.FindWithTag("TranslationInterface");
        rotationInterface.SetActive(false);
        leftEye = GameObject.FindWithTag("MainCamera");
        rightEye = GameObject.FindWithTag("secondCamera");


        if (skipCalibration)
        {
            // leggere l'ultimo file di calibrazione creato
            int logNumber = 0;
            while (File.Exists("Logs/Params_" + logNumber + ".txt"))
                logNumber++;
            logNumber--;
            Debug.Log("Skipping calibration. Readingcalibration file: Logs/Params_" + logNumber + ".txt");
            StreamReader reader = new StreamReader("Logs/Params_" + logNumber + ".txt");

            leftEye.transform.localPosition = new Vector3(float.Parse(reader.ReadLine()), float.Parse(reader.ReadLine()), float.Parse(reader.ReadLine()));
            //leftEye.GetComponent<Camera>().focalLength = float.Parse(reader.ReadLine());
            //leftEye.GetComponent<Camera>().sensorSize = new Vector2 (float.Parse(reader.ReadLine()), float.Parse(reader.ReadLine()));
            rightEye.transform.localPosition = new Vector3(float.Parse(reader.ReadLine()), float.Parse(reader.ReadLine()), float.Parse(reader.ReadLine()));
            //rightEye.GetComponent<Camera>().focalLength = float.Parse(reader.ReadLine());
            // rightEye.GetComponent<Camera>().sensorSize = new Vector2(float.Parse(reader.ReadLine()), float.Parse(reader.ReadLine()));

            // rightEye.GetComponent<Camera>().sensorSize[0] = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
            //rightEye.GetComponent<Camera>().sensorSize[1] = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
            // impostare la calibrazione sulle due camere


            changeScene();

        }

    }

    void Update()
    {
        if (!skipCalibration)
        {
            if (Input.GetKeyDown("2"))
            {
                rightEye.gameObject.tag = "MainCamera";
                leftEye.gameObject.tag = "secondCamera";
            }
            if (Input.GetKeyDown("1"))
            {
                rightEye.gameObject.tag = "secondCamera";
                leftEye.gameObject.tag = "MainCamera";
            }


            // trasla in avanti (allontana la scacchiera)
            if (Input.GetKeyDown("w") && modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.localPosition = new Vector3(GameObject.FindWithTag("MainCamera").transform.localPosition.x, GameObject.FindWithTag("MainCamera").transform.localPosition.y, GameObject.FindWithTag("MainCamera").transform.localPosition.z - adjustmentShift);
            }
            // trasla indietro (avvicina la scacchiera)
            if (Input.GetKeyDown("s") && modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.localPosition = new Vector3(GameObject.FindWithTag("MainCamera").transform.localPosition.x, GameObject.FindWithTag("MainCamera").transform.localPosition.y, GameObject.FindWithTag("MainCamera").transform.localPosition.z + adjustmentShift);
            }
            // trasla a sinistra
            if (Input.GetKeyDown("a") && modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.localPosition = new Vector3(GameObject.FindWithTag("MainCamera").transform.localPosition.x + adjustmentShift, GameObject.FindWithTag("MainCamera").transform.localPosition.y, GameObject.FindWithTag("MainCamera").transform.localPosition.z);
            }
            // trasla a destra
            if (Input.GetKeyDown("d") && modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.localPosition = new Vector3(GameObject.FindWithTag("MainCamera").transform.localPosition.x - adjustmentShift, GameObject.FindWithTag("MainCamera").transform.localPosition.y, GameObject.FindWithTag("MainCamera").transform.localPosition.z);
            }
            // trasla in basso
            if (Input.GetKeyDown("f") && modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.localPosition = new Vector3(GameObject.FindWithTag("MainCamera").transform.localPosition.x, GameObject.FindWithTag("MainCamera").transform.localPosition.y + adjustmentShift, GameObject.FindWithTag("MainCamera").transform.localPosition.z);
            }
            // trasla in alto
            if (Input.GetKeyDown("r") && modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.localPosition = new Vector3(GameObject.FindWithTag("MainCamera").transform.localPosition.x, GameObject.FindWithTag("MainCamera").transform.localPosition.y - adjustmentShift, GameObject.FindWithTag("MainCamera").transform.localPosition.z);
            }


            // ruota a destra (aumenta rot y)
            if (Input.GetKeyDown("d") && !modifyTranslations)
            {
                // GameObject.FindWithTag("MainCamera").transform.eulerAngles=new Vector3(0, 10, 0);
                // GameObject.FindWithTag("MainCamera").transform.localRotation *= Quaternion.Euler(0, 10, 0);
                GameObject.FindWithTag("MainCamera").transform.RotateAround(GameObject.FindWithTag("MainCamera").transform.position, GameObject.FindWithTag("HMD").transform.up, adjustmentAngle);
            }
            // ruota a sinistra (abbassa rot y)
            if (Input.GetKeyDown("a") && !modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.RotateAround(GameObject.FindWithTag("MainCamera").transform.position, GameObject.FindWithTag("HMD").transform.up, -adjustmentAngle);

            }
            // ruota in avanti (aumenta rot x)
            if (Input.GetKeyDown("w") && !modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.RotateAround(GameObject.FindWithTag("MainCamera").transform.position, GameObject.FindWithTag("HMD").transform.right, adjustmentAngle);
            }
            // ruota indietro (abbassa rot x)
            if (Input.GetKeyDown("s") && !modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.RotateAround(GameObject.FindWithTag("MainCamera").transform.position, GameObject.FindWithTag("HMD").transform.right, -adjustmentAngle);

            }
            // ruota in senso antiorario
            if (Input.GetKeyDown("q") && !modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.RotateAround(GameObject.FindWithTag("MainCamera").transform.position, GameObject.FindWithTag("HMD").transform.forward, adjustmentAngle);
            }
            // ruota in senso orario
            if (Input.GetKeyDown("e") && !modifyTranslations)
            {
                GameObject.FindWithTag("MainCamera").transform.RotateAround(GameObject.FindWithTag("MainCamera").transform.position, GameObject.FindWithTag("HMD").transform.forward, -adjustmentAngle);

            }


            // aumenta lo step
            if (Input.GetKeyDown("x") && modifyTranslations)
            {
                adjustmentShift *= 2;
                focalStep *= 2;
                // adjustmentAngle *= 2;
            }
            else if (Input.GetKeyDown("x"))
            {
                adjustmentAngle *= 2;
                focalStep *= 2;
            }
            // diminuisci lo step
            if (Input.GetKeyDown("z") && modifyTranslations)
            {
                adjustmentShift /= 2;
                focalStep /= 2;
                //  adjustmentAngle /= 2;
            }
            else if (Input.GetKeyDown("z"))
            {
                adjustmentAngle /= 2;
                focalStep /= 2;
            }

            // passa da modifica traslazioni a modifica rotazioni
            if (Input.GetKeyDown("y"))
            {
                //  modifyTranslations = !modifyTranslations;
                modifyTranslations = false;
                translationInterface.SetActive(false);
                rotationInterface.SetActive(true);


            }

            // ingrandisci scala
            /* if (Input.GetKeyDown("v"))
             {
                 GameObject.FindWithTag("MainCamera").GetComponent<Camera>().focalLength += focalStep;

             }
             // diminuisci scala
             if (Input.GetKeyDown(KeyCode.Space))
             {
                 GameObject.FindWithTag("MainCamera").GetComponent<Camera>().focalLength -= focalStep;
             }
             */


            // torna a modificare le traslazioni
            if (Input.GetKeyDown("u"))
            {
                modifyTranslations = true;
                translationInterface.SetActive(true);
                rotationInterface.SetActive(false);
                // modifyTranslations = !modifyTranslations;
            }

            //if (Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Return))
            if (Input.GetKeyDown(KeyCode.Space))
            {
                saveCalibrationSession();
                changeScene();
            }

        }
        //if (Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Return))
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //saveCalibrationSession();
            changeScene();
        }
    }

    void saveCalibrationSession()
    {

        //Debug.Log("dio bestione!");

        if (!fileWritten)
        {
            fileWritten = true;
            int logNumber = 0;
            while (File.Exists("Logs/Params_" + logNumber + ".txt"))
                logNumber++;
            savedParams = File.CreateText("Logs/Params_" + logNumber + ".txt");

            // dati salvati: x y z dei due occhi. focal length/fov/sensor size
            //savedParams.WriteLine("{0}\n {1}\n {2}\n{3}\n {4}\n {5}\n {6}\n{7}\n {8}\n {9}\n {10}\n {11}",
            savedParams.WriteLine("{0}\n {1}\n {2}\n{3}\n {4}\n {5}",
                                    leftEye.transform.localPosition.x,
                                    leftEye.transform.localPosition.y,
                                    leftEye.transform.localPosition.z,
                                    //leftEye.GetComponent<Camera>().focalLength,
                                    //leftEye.GetComponent<Camera>().sensorSize[0],
                                    //leftEye.GetComponent<Camera>().sensorSize[1],                       
                                    rightEye.transform.localPosition.x,
                                    rightEye.transform.localPosition.y,
                                    rightEye.transform.localPosition.z
                //rightEye.GetComponent<Camera>().focalLength,
                //rightEye.GetComponent<Camera>().sensorSize[0],
                //rightEye.GetComponent<Camera>().sensorSize[1]

                );

            savedParams.Close();

        }




    }

    void changeScene()
    {
        //  SceneManager.LoadScene(1);
        //rotationInterface.SetActive(false);
        //translationInterface.SetActive(false);
        CheckerBoard.SetActive(false);
        if (secondExperiment)
        {
            gameObject.GetComponent<spawnTargetsExp2>().enabled = true;
        }
        else
        {
            gameObject.GetComponent<SpawnTargets>().enabled = true;
        }
        gameObject.GetComponent<tinyCalibration>().enabled = false;
        // abilitare il gameobject con la logica dei cerchi
    }

}

