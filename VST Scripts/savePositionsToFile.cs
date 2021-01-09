using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class savePositionsToFile : MonoBehaviour {
	StreamWriter savedParams;
	bool fileWritten = false;
	public string logDataFilename="realPositions";
	// Use this for initialization
	void Start () {

		// create log file
		int logNumber = 0;
		while (File.Exists("Logs/" + logDataFilename + logNumber + ".txt"))
			logNumber++;
		savedParams = File.CreateText("Logs/" + logDataFilename + logNumber + ".txt");
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown("t")  && !fileWritten)
		{
			fileWritten = true;
			saveTracker();
			fileWritten = false;
		}    
		if (Input.GetKeyDown("k")  && !fileWritten)
		{
			fileWritten = true;
			saveKinect();
			fileWritten = false;
		}   
		if (Input.GetKeyDown("o")  && !fileWritten)
		{
			Debug.Log("Fine scrittura su file. E' ora possibile uscire dall'editor.");
			savedParams.Close();
		}   

	}


	void saveTracker()
	{

		//salva i dati su file
		Debug.Log("salvo posizione tracker su file");

		GameObject kinectTracker = GameObject.FindWithTag("kinectTracker");
		savedParams.WriteLine("{0} {1} {2}\n",
			kinectTracker.transform.position.x,
			kinectTracker.transform.position.y,
			kinectTracker.transform.position.z
		);

	}

	void saveKinect()
	{

		//salva i dati su file
		Debug.Log("salvo posizione kinect su file");

		// x y z reali -> x y z percepite -> x y z rilevate dal kinect -> pos tracker -> pos head
		GameObject trackedPoint = GameObject.FindWithTag("kinect");
		GameObject kinectTracker = GameObject.FindWithTag("kinectTracker");
		savedParams.WriteLine("{0} {1} {2}\n{3} {4} {5}\n{6} {7} {8}\n\n\n",
			kinectTracker.transform.position.x,
			kinectTracker.transform.position.y,
			kinectTracker.transform.position.z,
			trackedPoint.transform.position.x,
			trackedPoint.transform.position.y,
			trackedPoint.transform.position.z,
			trackedPoint.transform.localPosition.x,
			trackedPoint.transform.localPosition.y,
			trackedPoint.transform.localPosition.z
		);

	}

}
