//------------------------------------------------------------------------------
// <copyright file="CoordinateMappingBasics.cpp" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#include "stdafx.h"
#include <strsafe.h>
#include <math.h>
#include <limits>
#include <Wincodec.h>
#include "resource.h"
#include "CoordinateMappingBasics.h"
#include <iostream>
#include <opencv2/opencv.hpp>
#include "opencv2/highgui/highgui.hpp"
#include "opencv2/imgproc/imgproc.hpp"
#include <math.h>
#include <fstream>
#include <sstream>

#include<stdio.h>
#include<winsock2.h>

#pragma comment(lib,"ws2_32.lib") //Winsock Library

#define SERVER "127.0.0.1"	//ip address of udp server
#define BUFLEN 512	//Max length of buffer
#define PORT 8888	//The port on which to listen for incoming data


using namespace cv;
using namespace std;


int iLowH, iHighH, iLowS, iHighS, iLowV, iHighV;
int iLastX = -1;
int iLastY = -1;
USHORT depth = 0;
int posX = 0;
int posY = 0;
USHORT bestFit;
cv::Mat mat;
float final_x = 0;
float final_y = 0;
float final_z = 0;

struct sockaddr_in si_other;
int s, slen = sizeof(si_other);
char buf[BUFLEN];
char message[BUFLEN];
WSADATA wsa;




#ifndef HINST_THISCOMPONENT
EXTERN_C IMAGE_DOS_HEADER __ImageBase;
#define HINST_THISCOMPONENT ((HINSTANCE)&__ImageBase)
#endif

/// <summary>
/// Entry point for the application
/// </summary>
/// <param name="hInstance">handle to the application instance</param>
/// <param name="hPrevInstance">always 0</param>
/// <param name="lpCmdLine">command line arguments</param>
/// <param name="nCmdShow">whether to display minimized, maximized, or normally</param>
/// <returns>status</returns>
int APIENTRY wWinMain(

	   	  
    _In_ HINSTANCE hInstance,
    _In_opt_ HINSTANCE hPrevInstance,
    _In_ LPWSTR lpCmdLine,
    _In_ int nShowCmd
)
{
    UNREFERENCED_PARAMETER(hPrevInstance);
    UNREFERENCED_PARAMETER(lpCmdLine);

    CCoordinateMappingBasics application;
    application.Run(hInstance, nShowCmd);
}

/// <summary>
/// Constructor
/// </summary>
CCoordinateMappingBasics::CCoordinateMappingBasics() :
    m_hWnd(NULL),
    m_nStartTime(0),
    m_nLastCounter(0),
    m_nFramesSinceUpdate(0),
    m_fFreq(0),
    m_nNextStatusTime(0LL),
    m_bSaveScreenshot(false),
    m_pKinectSensor(NULL),
    m_pCoordinateMapper(NULL),
    m_pMultiSourceFrameReader(NULL),
    m_pDepthCoordinates(NULL),
    m_pD2DFactory(NULL),
    m_pDrawCoordinateMapping(NULL),
    m_pOutputRGBX(NULL),
    m_pBackgroundRGBX(NULL),
    m_pColorRGBX(NULL)
{
    LARGE_INTEGER qpf = {0};
    if (QueryPerformanceFrequency(&qpf))
    {
        m_fFreq = double(qpf.QuadPart);
    }

    // create heap storage for composite image pixel data in RGBX format
    m_pOutputRGBX = new RGBQUAD[cColorWidth * cColorHeight];

    // create heap storage for background image pixel data in RGBX format
    m_pBackgroundRGBX = new RGBQUAD[cColorWidth * cColorHeight];

    // create heap storage for color pixel data in RGBX format
    m_pColorRGBX = new RGBQUAD[cColorWidth * cColorHeight];

    // create heap storage for the coorinate mapping from color to depth
    m_pDepthCoordinates = new DepthSpacePoint[cColorWidth * cColorHeight];


}
  

/// <summary>
/// Destructor
/// </summary>
CCoordinateMappingBasics::~CCoordinateMappingBasics()
{


    // clean up Direct2D renderer
    if (m_pDrawCoordinateMapping)
    {
        delete m_pDrawCoordinateMapping;
        m_pDrawCoordinateMapping = NULL;
    }

    if (m_pOutputRGBX)
    {
        delete [] m_pOutputRGBX;
        m_pOutputRGBX = NULL;
    }

    if (m_pBackgroundRGBX)
    {
        delete [] m_pBackgroundRGBX;
        m_pBackgroundRGBX = NULL;
    }

    if (m_pColorRGBX)
    {
        delete [] m_pColorRGBX;
        m_pColorRGBX = NULL;
    }

    if (m_pDepthCoordinates)
    {
        delete[] m_pDepthCoordinates;
        m_pDepthCoordinates = NULL;
    }

    // clean up Direct2D
    SafeRelease(m_pD2DFactory);

    // done with frame reader
    SafeRelease(m_pMultiSourceFrameReader);

    // done with coordinate mapper
    SafeRelease(m_pCoordinateMapper);

    // close the Kinect Sensor
    if (m_pKinectSensor)
    {
        m_pKinectSensor->Close();
    }

    SafeRelease(m_pKinectSensor);

	closesocket(s);
	WSACleanup();
}

/// <summary>
/// Creates the main window and begins processing
/// </summary>
/// <param name="hInstance">handle to the application instance</param>
/// <param name="nCmdShow">whether to display minimized, maximized, or normally</param>
int CCoordinateMappingBasics::Run(HINSTANCE hInstance, int nCmdShow)
{
	/*
    if (m_pBackgroundRGBX)
    {
        if (FAILED(LoadResourceImage(L"Background", L"Image", cColorWidth, cColorHeight, m_pBackgroundRGBX)))
        {
            const RGBQUAD c_green = {0, 255, 0}; 

            // Fill in with a background colour of green if we can't load the background image
            for (int i = 0 ; i < cColorWidth * cColorHeight ; ++i)
            {
                m_pBackgroundRGBX[i] = c_green;
            }
        }
    }
	*/

	

    MSG       msg = {0};
    WNDCLASS  wc;

    // Dialog custom window class
    ZeroMemory(&wc, sizeof(wc));
    wc.style         = CS_HREDRAW | CS_VREDRAW;
    wc.cbWndExtra    = DLGWINDOWEXTRA;
    wc.hCursor       = LoadCursorW(NULL, IDC_ARROW);
    wc.hIcon         = LoadIconW(hInstance, MAKEINTRESOURCE(IDI_APP));
    wc.lpfnWndProc   = DefDlgProcW;
    wc.lpszClassName = L"CoordinateMappingBasicsAppDlgWndClass";

    if (!RegisterClassW(&wc))
    {
        return 0;
    }

    // Create main application window
    HWND hWndApp = CreateDialogParamW(
        NULL,
		MAKEINTRESOURCE(IDD_APP),
        NULL,
        (DLGPROC)CCoordinateMappingBasics::MessageRouter, 
        reinterpret_cast<LPARAM>(this));

    // Show window
    //ShowWindow(hWndApp, nCmdShow);



    // Main message loop
    while (WM_QUIT != msg.message)
    {

        Update();

        while (PeekMessageW(&msg, NULL, 0, 0, PM_REMOVE))
        {
            // If a dialog message will be taken care of by the dialog proc
            if (hWndApp && IsDialogMessageW(hWndApp, &msg))
            {
                continue;
            }

            TranslateMessage(&msg);
            DispatchMessageW(&msg);
        }
    }

    return static_cast<int>(msg.wParam);
}

/// <summary>
/// Main processing function
/// </summary>
void CCoordinateMappingBasics::Update()
{
	// SOCKET ############################################################
	if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
	{
		printf("Failed. Error Code : %d", WSAGetLastError());
		exit(EXIT_FAILURE);
	}
	printf("Initialised.\n");

	//create socket
	if ((s = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)) == SOCKET_ERROR)
	{
		printf("socket() failed with error code : %d", WSAGetLastError());
		exit(EXIT_FAILURE);
	}

	//setup address structure
	memset((char *)&si_other, 0, sizeof(si_other));
	si_other.sin_family = AF_INET;
	si_other.sin_port = htons(PORT);
	si_other.sin_addr.S_un.S_addr = inet_addr(SERVER);

	// END SOCKET #########################################################
    if (!m_pMultiSourceFrameReader)
    {
        return;
    }

    IMultiSourceFrame* pMultiSourceFrame = NULL;
    IDepthFrame* pDepthFrame = NULL;
    IColorFrame* pColorFrame = NULL;
    //IBodyIndexFrame* pBodyIndexFrame = NULL;

    HRESULT hr = m_pMultiSourceFrameReader->AcquireLatestFrame(&pMultiSourceFrame);

    if (SUCCEEDED(hr))
    {
        IDepthFrameReference* pDepthFrameReference = NULL;

        hr = pMultiSourceFrame->get_DepthFrameReference(&pDepthFrameReference);
        if (SUCCEEDED(hr))
        {
            hr = pDepthFrameReference->AcquireFrame(&pDepthFrame);
        }

        SafeRelease(pDepthFrameReference);
    }

    if (SUCCEEDED(hr))
    {
        IColorFrameReference* pColorFrameReference = NULL;

        hr = pMultiSourceFrame->get_ColorFrameReference(&pColorFrameReference);
        if (SUCCEEDED(hr))
        {
            hr = pColorFrameReference->AcquireFrame(&pColorFrame);
        }

        SafeRelease(pColorFrameReference);
    }

	/*
    if (SUCCEEDED(hr))
    {
        IBodyIndexFrameReference* pBodyIndexFrameReference = NULL;

        hr = pMultiSourceFrame->get_BodyIndexFrameReference(&pBodyIndexFrameReference);
        if (SUCCEEDED(hr))
        {
            hr = pBodyIndexFrameReference->AcquireFrame(&pBodyIndexFrame);
        }

        SafeRelease(pBodyIndexFrameReference);
    }
	*/

    if (SUCCEEDED(hr))
    {
        INT64 nDepthTime = 0;
        IFrameDescription* pDepthFrameDescription = NULL;
        int nDepthWidth = 0;
        int nDepthHeight = 0;
        UINT nDepthBufferSize = 0;
        UINT16 *pDepthBuffer = NULL;

        IFrameDescription* pColorFrameDescription = NULL;
        int nColorWidth = 0;
        int nColorHeight = 0;
        ColorImageFormat imageFormat = ColorImageFormat_None;
        UINT nColorBufferSize = 0;
        RGBQUAD *pColorBuffer = NULL;

		/*
        IFrameDescription* pBodyIndexFrameDescription = NULL;
        int nBodyIndexWidth = 0;
        int nBodyIndexHeight = 0;
        UINT nBodyIndexBufferSize = 0;
        BYTE *pBodyIndexBuffer = NULL;
		*/

        // get depth frame data

        hr = pDepthFrame->get_RelativeTime(&nDepthTime);

        if (SUCCEEDED(hr))
        {
            hr = pDepthFrame->get_FrameDescription(&pDepthFrameDescription);
        }

        if (SUCCEEDED(hr))
        {
            hr = pDepthFrameDescription->get_Width(&nDepthWidth);
        }

        if (SUCCEEDED(hr))
        {
            hr = pDepthFrameDescription->get_Height(&nDepthHeight);
        }

        if (SUCCEEDED(hr))
        {
            hr = pDepthFrame->AccessUnderlyingBuffer(&nDepthBufferSize, &pDepthBuffer);            
        }

        // get color frame data

        if (SUCCEEDED(hr))
        {
            hr = pColorFrame->get_FrameDescription(&pColorFrameDescription);
        }

        if (SUCCEEDED(hr))
        {
            hr = pColorFrameDescription->get_Width(&nColorWidth);
        }

        if (SUCCEEDED(hr))
        {
            hr = pColorFrameDescription->get_Height(&nColorHeight);
        }

        if (SUCCEEDED(hr))
        {
            hr = pColorFrame->get_RawColorImageFormat(&imageFormat);
        }

        if (SUCCEEDED(hr))
        {
            if (imageFormat == ColorImageFormat_Bgra)
            {
                hr = pColorFrame->AccessRawUnderlyingBuffer(&nColorBufferSize, reinterpret_cast<BYTE**>(&pColorBuffer));
            }
            else if (m_pColorRGBX)
            {
                pColorBuffer = m_pColorRGBX;
                nColorBufferSize = cColorWidth * cColorHeight * sizeof(RGBQUAD);
               // hr = pColorFrame->CopyConvertedFrameDataToArray(nColorBufferSize, reinterpret_cast<BYTE*>(pColorBuffer), ColorImageFormat_Bgra);
				mat.create(nColorHeight, nColorWidth, CV_8UC4);
				BYTE* imgDataPtr = (BYTE*)mat.data;
				pColorFrame->CopyConvertedFrameDataToArray(nColorBufferSize, imgDataPtr, ColorImageFormat_Bgra);
				cvtColor(mat, mat, COLOR_BGRA2BGR);
				Mat imgThresholded;
				Mat imgLines = Mat::zeros(mat.size(), CV_8UC3);; inRange(mat, Scalar(iLowH, iLowS, iLowV), Scalar(iHighH, iHighS, iHighV), imgThresholded); //Threshold the image

				//morphological opening (remove small objects from the foreground)
				erode(imgThresholded, imgThresholded, getStructuringElement(MORPH_ELLIPSE, Size(5, 5)));
				dilate(imgThresholded, imgThresholded, getStructuringElement(MORPH_ELLIPSE, Size(5, 5)));

				//morphological closing (fill small holes in the foreground)
				dilate(imgThresholded, imgThresholded, getStructuringElement(MORPH_ELLIPSE, Size(5, 5)));
				erode(imgThresholded, imgThresholded, getStructuringElement(MORPH_ELLIPSE, Size(5, 5)));



				//Calculate the moments of the thresholded image
				Moments oMoments = moments(imgThresholded);

				double dM01 = oMoments.m01;
				double dM10 = oMoments.m10;
				double dArea = oMoments.m00;

				posX = 0, posY = 0;
				// if the area <= 10000, I consider that the there are no object in the image and it's because of the noise, the area is not zero 
				if (dArea > 10000)
				{
					//calculate the position of the ball
					posX = dM10 / dArea;
					posY = dM01 / dArea;

					//if (iLastX >= 0 && iLastY >= 0 && posX >= 0 && posY >= 0)
					//{
						//Draw a red line from the previous point to the current point
					//	line(imgLines, Point(posX, posY), Point(iLastX, iLastY), Scalar(0, 0, 255), 2);
					//}

					//iLastX = posX;
					//iLastY = posY;

				}

				mat = mat + imgLines;

				//____________ depth processing start __________
				ushort depthValue = 0;
				if (NULL != pDepthBuffer)
				{
					ColorSpacePoint *depP = new ColorSpacePoint[cDepthWidth * cDepthHeight];
					CameraSpacePoint *camera_space = new CameraSpacePoint[cDepthWidth * cDepthHeight];
					//ColorSpacePoint *depP = new ColorSpacePoint[512 * 424];
					// spotted
					//MapColorFrameToDepthSpace
					m_pCoordinateMapper->MapDepthFrameToColorSpace(cDepthWidth * cDepthHeight, (UINT16*) pDepthBuffer, cDepthWidth * cDepthHeight, depP);
					m_pCoordinateMapper->MapDepthFrameToCameraSpace(cDepthWidth * cDepthHeight, (UINT16*)pDepthBuffer, cDepthWidth * cDepthHeight, camera_space);
					//mapper->MapDepthFrameToColorSpace(
					//	width*height, buf,        // Depth frame data and size of depth frame
					//	width*height, depth2rgb); // Output ColorSpacePoint array and size

					int depthIndex = -1;
					float closestPoint = 9999999999999999;
					for (int j = 0; j < cDepthWidth * cDepthHeight; ++j)
					{
						float xx = depP[j].X - posX;
						float yy = depP[j].Y - posY;
						float dis = sqrt(xx * xx + yy * yy);


						if (dis < closestPoint)
						{
							closestPoint = dis;
							depthIndex = j;
						}
					}

					if (depthIndex < 0) {}
					//Console.WriteLine("-1");
					else
					{
						depthValue = pDepthBuffer[depthIndex];
						depth = depthValue;
						
						//m_pDepthCoordinates[depthIndex] � il depthspacepoint sbagliato!
						// m_pCoordinateMapper->MapDepthPointToCameraSpace(dsp, depthValue, &final_point);
						 
						 final_x = camera_space[depthIndex].X;
						 final_y = camera_space[depthIndex].Y;
						 final_z = camera_space[depthIndex].Z;

					}
					
				}

				// ___________ finished processing depth __________

				circle(mat, Point(posX, posY), 10, Scalar(0, 0, 255), 3); //int thickness = 1, int lineType = 8, int shift = 0
				stringstream stream;
				stream << "X: " << setprecision(2) << posX << " Y: " <<  posY << " ";
				string pos = stream.str();
				//string pos = "X: " + to_string(posX) + " Y: " + to_string(posY) + " ";
				putText(mat, "Image Coordinates", Point(10, 50), FONT_HERSHEY_SIMPLEX, 2, Scalar(0, 0, 255), 6, LINE_AA);
				putText(mat, pos, Point(10, 125), FONT_HERSHEY_SIMPLEX, 2, Scalar(0, 0, 255), 6, LINE_AA);
				string d = " "+ to_string(depth) + "mm";
				putText(mat, d, Point(500, 125), FONT_HERSHEY_SIMPLEX, 2, Scalar(0, 0, 255), 6, LINE_AA);


				//string kinectpos = "X: " + to_string(final_x) + " Y: " + to_string(final_y) + " Z: " + to_string(final_z);
				stringstream stream2;
				stream2 << "X: " << setprecision(3) << final_x << " Y: " <<  final_y << " Z: " <<  final_z << " ";
				
				string kinectpos = stream2.str();
				sendto(s, kinectpos.c_str(), strlen(kinectpos.c_str()), 0, (struct sockaddr *) &si_other, slen);
				

				putText(mat, "Kinect Coordinates", Point(1000, 50), FONT_HERSHEY_SIMPLEX, 2, Scalar(0, 0, 255), 6, LINE_AA);
				putText(mat, kinectpos, Point(1000, 125), FONT_HERSHEY_SIMPLEX, 2, Scalar(0, 0, 255), 6, LINE_AA);
				//string d = to_string(depth) + "mm";
				//putText(mat, d, Point(500, 125), FONT_HERSHEY_SIMPLEX, 2, Scalar(255, 255, 255), 6, LINE_AA);

				//show the thresholded image
				resize(imgThresholded, imgThresholded, Size(imgThresholded.cols / 3, imgThresholded.rows / 3)); // to 1/3 size (or even smaller)
				//namedWindow("Thresholded Image", WINDOW_AUTOSIZE);
				//imshow("Thresholded Image", imgThresholded);
				//imshow("Thresholded Image", imgThresholded); 
				
				//show the original image
				resize(mat, mat, Size(mat.cols / 3, mat.rows / 3)); // to 1/3 size (or even smaller)

				//convert grayscale to rgb to merge the images
				cvtColor(imgThresholded, imgThresholded, cv::COLOR_GRAY2BGR);


				//merge the thresholded image with the raw color stream
				int dstWidth = imgThresholded.cols;
				int dstHeight = imgThresholded.rows * 2;
				cv::Mat dst = cv::Mat(dstHeight, dstWidth, CV_8UC3, cv::Scalar(0, 0, 0));
				cv::Rect roi(cv::Rect(0, 0, imgThresholded.cols, imgThresholded.rows));
				cv::Mat targetROI = dst(roi);
				imgThresholded.copyTo(targetROI);
				targetROI = dst(cv::Rect(0, imgThresholded.rows, imgThresholded.cols, imgThresholded.rows));
				mat.copyTo(targetROI);

				//display both images
				namedWindow("Color segmentation - change HSV threshold parameters with the sliders", WINDOW_AUTOSIZE);
				imshow("Color segmentation - change HSV threshold parameters with the sliders", dst);
				
				if (waitKey(30) == 27)
				{
					ofstream myfile;
					myfile.open("savedParameters.txt");
					//salvo su file iLowH, iHighH, iLowS, iHighS, iLowV, iHighV per non doverli inserire ogni volta
					myfile << iLowH << endl << iHighH  << endl << iLowS << endl << iHighS << endl << iLowV << endl<< iHighV << "\n\nThe Thresholding parameters are written in this order:\nHue (low-high), Saturation (low-high), Value (low-high)";
					myfile.close();

					destroyAllWindows();
					exit(0);
					
					
				}
				//imshow("Original", mat); 

				//___________________________________________________________________________
            }
            else
            {
                hr = E_FAIL;
            }
        }

        // get body index frame data
		/*
        if (SUCCEEDED(hr))
        {
            hr = pBodyIndexFrame->get_FrameDescription(&pBodyIndexFrameDescription);
        }

        if (SUCCEEDED(hr))
        {
            hr = pBodyIndexFrameDescription->get_Width(&nBodyIndexWidth);
        }

        if (SUCCEEDED(hr))
        {
            hr = pBodyIndexFrameDescription->get_Height(&nBodyIndexHeight);
        }

        if (SUCCEEDED(hr))
        {
            hr = pBodyIndexFrame->AccessUnderlyingBuffer(&nBodyIndexBufferSize, &pBodyIndexBuffer);            
        }
		

        if (SUCCEEDED(hr))
        {
            ProcessFrame(nDepthTime, pDepthBuffer, nDepthWidth, nDepthHeight, 
                pColorBuffer, nColorWidth, nColorHeight,
                pBodyIndexBuffer, nBodyIndexWidth, nBodyIndexHeight);
        }
		*/
        SafeRelease(pDepthFrameDescription);
        SafeRelease(pColorFrameDescription);
        //SafeRelease(pBodyIndexFrameDescription);
    }

    SafeRelease(pDepthFrame);
    SafeRelease(pColorFrame);
   // SafeRelease(pBodyIndexFrame);
    SafeRelease(pMultiSourceFrame);
}

/// <summary>
/// Handles window messages, passes most to the class instance to handle
/// </summary>
/// <param name="hWnd">window message is for</param>
/// <param name="uMsg">message</param>
/// <param name="wParam">message data</param>
/// <param name="lParam">additional message data</param>
/// <returns>result of message processing</returns>
LRESULT CALLBACK CCoordinateMappingBasics::MessageRouter(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    CCoordinateMappingBasics* pThis = NULL;
    
    if (WM_INITDIALOG == uMsg)
    {
        pThis = reinterpret_cast<CCoordinateMappingBasics*>(lParam);
        SetWindowLongPtr(hWnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(pThis));
    }
    else
    {
        pThis = reinterpret_cast<CCoordinateMappingBasics*>(::GetWindowLongPtr(hWnd, GWLP_USERDATA));
    }

    if (pThis)
    {
        return pThis->DlgProc(hWnd, uMsg, wParam, lParam);
    }

    return 0;
}

/// <summary>
/// Handle windows messages for the class instance
/// </summary>
/// <param name="hWnd">window message is for</param>
/// <param name="uMsg">message</param>
/// <param name="wParam">message data</param>
/// <param name="lParam">additional message data</param>
/// <returns>result of message processing</returns>
LRESULT CALLBACK CCoordinateMappingBasics::DlgProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    UNREFERENCED_PARAMETER(wParam);
    UNREFERENCED_PARAMETER(lParam);

    switch (message)
    {
        case WM_INITDIALOG:
        {
            // Bind application window handle
            m_hWnd = hWnd;

            // Init Direct2D
            D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &m_pD2DFactory);

			// creazione della finestra
			//____________________________________________________________________

			cv::namedWindow("Press ESC to exit application", WINDOW_AUTOSIZE); //create a window called "Control"


			ifstream myfile("savedParameters.txt");
			if (myfile.is_open())
			{
				
				myfile >> iLowH >> iHighH >> iLowS >> iHighS >>iLowV >> iHighV;
				
				myfile.close();
			}
			else {
				
				iLowH = 0;
				iHighH = 179;

				iLowS = 0;
				iHighS = 255;

				iLowV = 0;
				iHighV = 255;
			}

			//Create trackbars with given starting values

			createTrackbar("Hue Low", "Press ESC to exit application", &iLowH, 179); //Hue (0 - 179)
			createTrackbar("Hue High", "Press ESC to exit application", &iHighH, 179);

			createTrackbar("Sat Low", "Press ESC to exit application", &iLowS, 255); //Saturation (0 - 255)
			createTrackbar("Sat High", "Press ESC to exit application", &iHighS, 255);

			createTrackbar("Value Low", "Press ESC to exit application", &iLowV, 255); //Value (0 - 255)
			createTrackbar("Value High", "Press ESC to exit application", &iHighV, 255);


			// fine creazione finestra
			//____________________________________________________________________

            // Create and initialize a new Direct2D image renderer (take a look at ImageRenderer.h)
            // We'll use this to draw the data we receive from the Kinect to the screen
            m_pDrawCoordinateMapping = new ImageRenderer(); 
            HRESULT hr = m_pDrawCoordinateMapping->Initialize(GetDlgItem(m_hWnd, IDC_VIDEOVIEW), m_pD2DFactory, cColorWidth, cColorHeight, cColorWidth * sizeof(RGBQUAD)); 
            if (FAILED(hr))
            {
                SetStatusMessage(L"Failed to initialize the Direct2D draw device.", 10000, true);
            }

            // Get and initialize the default Kinect sensor
            InitializeDefaultSensor();
        }
        break;

        // If the titlebar X is clicked, destroy app
        case WM_CLOSE:
            DestroyWindow(hWnd);
            break;

        case WM_DESTROY:
            // Quit the main message pump
            PostQuitMessage(0);
            break;

		

        // Handle button press
		/*
        case WM_COMMAND:
            // If it was for the screenshot control and a button clicked event, save a screenshot next frame 
            if (IDC_BUTTON_SCREENSHOT == LOWORD(wParam) && BN_CLICKED == HIWORD(wParam))
            {
                m_bSaveScreenshot = true;
            }
            break;
			*/
    }

    return FALSE;
}

/// <summary>
/// Initializes the default Kinect sensor
/// </summary>
/// <returns>indicates success or failure</returns>
HRESULT CCoordinateMappingBasics::InitializeDefaultSensor()
{
    HRESULT hr;

    hr = GetDefaultKinectSensor(&m_pKinectSensor);
    if (FAILED(hr))
    {
        return hr;
    }

    if (m_pKinectSensor)
    {
        // Initialize the Kinect and get coordinate mapper and the frame reader

        if (SUCCEEDED(hr))
        {
            hr = m_pKinectSensor->get_CoordinateMapper(&m_pCoordinateMapper);
        }

        hr = m_pKinectSensor->Open();

        if (SUCCEEDED(hr))
        {
            hr = m_pKinectSensor->OpenMultiSourceFrameReader(
                FrameSourceTypes::FrameSourceTypes_Depth | FrameSourceTypes::FrameSourceTypes_Color | FrameSourceTypes::FrameSourceTypes_BodyIndex,
                &m_pMultiSourceFrameReader);
        }
    }

    if (!m_pKinectSensor || FAILED(hr))
    {
        SetStatusMessage(L"No ready Kinect found!", 10000, true);
        return E_FAIL;
    }

    return hr;
}


/*
/// <summary>
/// Handle new depth and color data
/// <param name="nTime">timestamp of frame</param>
/// <param name="pDepthBuffer">pointer to depth frame data</param>
/// <param name="nDepthWidth">width (in pixels) of input depth image data</param>
/// <param name="nDepthHeight">height (in pixels) of input depth image data</param>
/// <param name="pColorBuffer">pointer to color frame data</param>
/// <param name="nColorWidth">width (in pixels) of input color image data</param>
/// <param name="nColorHeight">height (in pixels) of input color image data</param>
/// <param name="pBodyIndexBuffer">pointer to body index frame data</param>
/// <param name="nBodyIndexWidth">width (in pixels) of input body index data</param>
/// <param name="nBodyIndexHeight">height (in pixels) of input body index data</param>
/// </summary>
void CCoordinateMappingBasics::ProcessFrame(INT64 nTime, 
                                            const UINT16* pDepthBuffer, int nDepthWidth, int nDepthHeight, 
                                            const RGBQUAD* pColorBuffer, int nColorWidth, int nColorHeight,
                                            const BYTE* pBodyIndexBuffer, int nBodyIndexWidth, int nBodyIndexHeight)
{

	
    if (m_hWnd)
    {
        if (!m_nStartTime)
        {
            m_nStartTime = nTime;
        }

        double fps = 0.0;

        LARGE_INTEGER qpcNow = {0};
        if (m_fFreq)
        {
            if (QueryPerformanceCounter(&qpcNow))
            {
                if (m_nLastCounter)
                {
                    m_nFramesSinceUpdate++;
                    fps = m_fFreq * m_nFramesSinceUpdate / double(qpcNow.QuadPart - m_nLastCounter);
                }
            }
        }

        WCHAR szStatusMessage[64];
        StringCchPrintf(szStatusMessage, _countof(szStatusMessage), L" FPS = %0.2f    Time = %I64d", fps, (nTime - m_nStartTime));

        if (SetStatusMessage(szStatusMessage, 1000, false))
        {
            m_nLastCounter = qpcNow.QuadPart;
            m_nFramesSinceUpdate = 0;
        }
    }
	

    // Make sure we've received valid data
    if (m_pCoordinateMapper && m_pDepthCoordinates && m_pOutputRGBX && 
        pDepthBuffer && (nDepthWidth == cDepthWidth) && (nDepthHeight == cDepthHeight) && 
        pColorBuffer && (nColorWidth == cColorWidth) && (nColorHeight == cColorHeight) &&
        pBodyIndexBuffer && (nBodyIndexWidth == cDepthWidth) && (nBodyIndexHeight == cDepthHeight))
    {
		
        HRESULT hr = m_pCoordinateMapper->MapColorFrameToDepthSpace(nDepthWidth * nDepthHeight, (UINT16*)pDepthBuffer, nColorWidth * nColorHeight, m_pDepthCoordinates);
        if (SUCCEEDED(hr))
        {
            // loop over output pixels
            for (int colorIndex = 0; colorIndex < (nColorWidth*nColorHeight); ++colorIndex)
            {
                // default setting source to copy from the background pixel
                const RGBQUAD* pSrc = m_pBackgroundRGBX + colorIndex;

                DepthSpacePoint p = m_pDepthCoordinates[colorIndex];

                // Values that are negative infinity means it is an invalid color to depth mapping so we
                // skip processing for this pixel
                if (p.X != -std::numeric_limits<float>::infinity() && p.Y != -std::numeric_limits<float>::infinity())
                {
                    int depthX = static_cast<int>(p.X + 0.5f);
                    int depthY = static_cast<int>(p.Y + 0.5f);

                    if ((depthX >= 0 && depthX < nDepthWidth) && (depthY >= 0 && depthY < nDepthHeight))
                    {
                        BYTE player = pBodyIndexBuffer[depthX + (depthY * cDepthWidth)];

                        // if we're tracking a player for the current pixel, draw from the color camera
                        if (player != 0xff)
                        {
                            // set source for copy to the color pixel
                            pSrc = m_pColorRGBX + colorIndex;
                        }
                    }
                }

                // write output
                m_pOutputRGBX[colorIndex] = *pSrc;
            }

            // Draw the data with Direct2D
            m_pDrawCoordinateMapping->Draw(reinterpret_cast<BYTE*>(m_pOutputRGBX), cColorWidth * cColorHeight * sizeof(RGBQUAD));

            if (m_bSaveScreenshot)
            {
                WCHAR szScreenshotPath[MAX_PATH];

                // Retrieve the path to My Photos
                GetScreenshotFileName(szScreenshotPath, _countof(szScreenshotPath));

                // Write out the bitmap to disk
                hr = SaveBitmapToFile(reinterpret_cast<BYTE*>(m_pOutputRGBX), nColorWidth, nColorHeight, sizeof(RGBQUAD) * 8, szScreenshotPath);

                WCHAR szStatusMessage[64 + MAX_PATH];
                if (SUCCEEDED(hr))
                {
                    // Set the status bar to show where the screenshot was saved
                    StringCchPrintf(szStatusMessage, _countof(szStatusMessage), L"Screenshot saved to %s", szScreenshotPath);
                }
                else
                {
                    StringCchPrintf(szStatusMessage, _countof(szStatusMessage), L"Failed to write screenshot to %s", szScreenshotPath);
                }

                SetStatusMessage(szStatusMessage, 5000, true);

                // toggle off so we don't save a screenshot again next frame
                m_bSaveScreenshot = false;
            }
        }
    }
}

*/


/// <summary>
/// Set the status bar message
/// </summary>
/// <param name="szMessage">message to display</param>
/// <param name="showTimeMsec">time in milliseconds to ignore future status messages</param>
/// <param name="bForce">force status update</param>
bool CCoordinateMappingBasics::SetStatusMessage(_In_z_ WCHAR* szMessage, DWORD nShowTimeMsec, bool bForce)
{
    INT64 now = GetTickCount64();

    if (m_hWnd && (bForce || (m_nNextStatusTime <= now)))
    {
        SetDlgItemText(m_hWnd, IDC_STATUS, szMessage);
        m_nNextStatusTime = now + nShowTimeMsec;

        return true;
    }

    return false;
}

/*
/// <summary>
/// Get the name of the file where screenshot will be stored.
/// </summary>
/// <param name="lpszFilePath">string buffer that will receive screenshot file name.</param>
/// <param name="nFilePathSize">number of characters in lpszFilePath string buffer.</param>
/// <returns>
/// S_OK on success, otherwise failure code.
/// </returns>
HRESULT CCoordinateMappingBasics::GetScreenshotFileName(_Out_writes_z_(nFilePathSize) LPWSTR lpszFilePath, UINT nFilePathSize)
{
	
    WCHAR* pszKnownPath = NULL;
    HRESULT hr = SHGetKnownFolderPath(FOLDERID_Pictures, 0, NULL, &pszKnownPath);

    if (SUCCEEDED(hr))
    {
        // Get the time
        WCHAR szTimeString[MAX_PATH];
        GetTimeFormatEx(NULL, 0, NULL, L"hh'-'mm'-'ss", szTimeString, _countof(szTimeString));

        // File name will be KinectScreenshotDepth-HH-MM-SS.bmp
        StringCchPrintfW(lpszFilePath, nFilePathSize, L"%s\\KinectScreenshot-CoordinateMapping-%s.bmp", pszKnownPath, szTimeString);
    }

    if (pszKnownPath)
    {
        CoTaskMemFree(pszKnownPath);
    }

    return hr;
}

/// <summary>
/// Save passed in image data to disk as a bitmap
/// </summary>
/// <param name="pBitmapBits">image data to save</param>
/// <param name="lWidth">width (in pixels) of input image data</param>
/// <param name="lHeight">height (in pixels) of input image data</param>
/// <param name="wBitsPerPixel">bits per pixel of image data</param>
/// <param name="lpszFilePath">full file path to output bitmap to</param>
/// <returns>indicates success or failure</returns>
HRESULT CCoordinateMappingBasics::SaveBitmapToFile(BYTE* pBitmapBits, LONG lWidth, LONG lHeight, WORD wBitsPerPixel, LPCWSTR lpszFilePath)
{
    DWORD dwByteCount = lWidth * lHeight * (wBitsPerPixel / 8);

    BITMAPINFOHEADER bmpInfoHeader = {0};

    bmpInfoHeader.biSize        = sizeof(BITMAPINFOHEADER);  // Size of the header
    bmpInfoHeader.biBitCount    = wBitsPerPixel;             // Bit count
    bmpInfoHeader.biCompression = BI_RGB;                    // Standard RGB, no compression
    bmpInfoHeader.biWidth       = lWidth;                    // Width in pixels
    bmpInfoHeader.biHeight      = -lHeight;                  // Height in pixels, negative indicates it's stored right-side-up
    bmpInfoHeader.biPlanes      = 1;                         // Default
    bmpInfoHeader.biSizeImage   = dwByteCount;               // Image size in bytes

    BITMAPFILEHEADER bfh = {0};

    bfh.bfType    = 0x4D42;                                           // 'M''B', indicates bitmap
    bfh.bfOffBits = bmpInfoHeader.biSize + sizeof(BITMAPFILEHEADER);  // Offset to the start of pixel data
    bfh.bfSize    = bfh.bfOffBits + bmpInfoHeader.biSizeImage;        // Size of image + headers

    // Create the file on disk to write to
    HANDLE hFile = CreateFileW(lpszFilePath, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

    // Return if error opening file
    if (NULL == hFile) 
    {
        return E_ACCESSDENIED;
    }

    DWORD dwBytesWritten = 0;
    
    // Write the bitmap file header
    if (!WriteFile(hFile, &bfh, sizeof(bfh), &dwBytesWritten, NULL))
    {
        CloseHandle(hFile);
        return E_FAIL;
    }
    
    // Write the bitmap info header
    if (!WriteFile(hFile, &bmpInfoHeader, sizeof(bmpInfoHeader), &dwBytesWritten, NULL))
    {
        CloseHandle(hFile);
        return E_FAIL;
    }
    
    // Write the RGB Data
    if (!WriteFile(hFile, pBitmapBits, bmpInfoHeader.biSizeImage, &dwBytesWritten, NULL))
    {
        CloseHandle(hFile);
        return E_FAIL;
    }    

    // Close the file
    CloseHandle(hFile);
    return S_OK;
}


/// <summary>
/// Load an image from a resource into a buffer
/// </summary>
/// <param name="resourceName">name of image resource to load</param>
/// <param name="resourceType">type of resource to load</param>
/// <param name="nOutputWidth">width (in pixels) of scaled output bitmap</param>
/// <param name="nOutputHeight">height (in pixels) of scaled output bitmap</param>
/// <param name="pOutputBuffer">buffer that will hold the loaded image</param>
/// <returns>S_OK on success, otherwise failure code</returns>
HRESULT CCoordinateMappingBasics::LoadResourceImage(PCWSTR resourceName, PCWSTR resourceType, UINT nOutputWidth, UINT nOutputHeight, RGBQUAD* pOutputBuffer)
{
    IWICImagingFactory* pIWICFactory = NULL;
    IWICBitmapDecoder* pDecoder = NULL;
    IWICBitmapFrameDecode* pSource = NULL;
    IWICStream* pStream = NULL;
    IWICFormatConverter* pConverter = NULL;
    IWICBitmapScaler* pScaler = NULL;

    HRSRC imageResHandle = NULL;
    HGLOBAL imageResDataHandle = NULL;
    void *pImageFile = NULL;
    DWORD imageFileSize = 0;

    HRESULT hrCoInit = CoInitialize(NULL);
    HRESULT hr = hrCoInit;

    if (SUCCEEDED(hr))
    {
        hr = CoCreateInstance(CLSID_WICImagingFactory, NULL, CLSCTX_INPROC_SERVER, IID_IWICImagingFactory, (LPVOID*)&pIWICFactory);
    }

    if (SUCCEEDED(hr))
    {
        // Locate the resource
        imageResHandle = FindResourceW(HINST_THISCOMPONENT, resourceName, resourceType);
        hr = imageResHandle ? S_OK : E_FAIL;
    }

    if (SUCCEEDED(hr))
    {
        // Load the resource
        imageResDataHandle = LoadResource(HINST_THISCOMPONENT, imageResHandle);
        hr = imageResDataHandle ? S_OK : E_FAIL;
    }

    if (SUCCEEDED(hr))
    {
        // Lock it to get a system memory pointer.
        pImageFile = LockResource(imageResDataHandle);
        hr = pImageFile ? S_OK : E_FAIL;
    }

    if (SUCCEEDED(hr))
    {
        // Calculate the size.
        imageFileSize = SizeofResource(HINST_THISCOMPONENT, imageResHandle);
        hr = imageFileSize ? S_OK : E_FAIL;
    }

    if (SUCCEEDED(hr))
    {
        // Create a WIC stream to map onto the memory.
        hr = pIWICFactory->CreateStream(&pStream);
    }

    if (SUCCEEDED(hr))
    {
        // Initialize the stream with the memory pointer and size.
        hr = pStream->InitializeFromMemory(
            reinterpret_cast<BYTE*>(pImageFile),
            imageFileSize);
    }

    if (SUCCEEDED(hr))
    {
        // Create a decoder for the stream.
        hr = pIWICFactory->CreateDecoderFromStream(
            pStream,
            NULL,
            WICDecodeMetadataCacheOnLoad,
            &pDecoder);
    }

    if (SUCCEEDED(hr))
    {
        // Create the initial frame.
        hr = pDecoder->GetFrame(0, &pSource);
    }

    if (SUCCEEDED(hr))
    {
        // Convert the image format to 32bppPBGRA
        // (DXGI_FORMAT_B8G8R8A8_UNORM + D2D1_ALPHA_MODE_PREMULTIPLIED).
        hr = pIWICFactory->CreateFormatConverter(&pConverter);
    }

    if (SUCCEEDED(hr))
    {
        hr = pIWICFactory->CreateBitmapScaler(&pScaler);
    }

    if (SUCCEEDED(hr))
    {
        hr = pScaler->Initialize(
            pSource,
            nOutputWidth,
            nOutputHeight,
            WICBitmapInterpolationModeCubic
            );
    }

    if (SUCCEEDED(hr))
    {
        hr = pConverter->Initialize(
            pScaler,
            GUID_WICPixelFormat32bppPBGRA,
            WICBitmapDitherTypeNone,
            NULL,
            0.f,
            WICBitmapPaletteTypeMedianCut);
    }

    UINT width = 0;
    UINT height = 0;
    if (SUCCEEDED(hr))
    {
        hr = pConverter->GetSize(&width, &height);
    }

    // make sure the image scaled correctly so the output buffer is big enough
    if (SUCCEEDED(hr))
    {
        if ((width != nOutputWidth) || (height != nOutputHeight))
        {
            hr = E_FAIL;
        }
    }

    if (SUCCEEDED(hr))
    {
        hr = pConverter->CopyPixels(NULL, width * sizeof(RGBQUAD), nOutputWidth * nOutputHeight * sizeof(RGBQUAD), reinterpret_cast<BYTE*>(pOutputBuffer));
    }

    SafeRelease(pScaler);
    SafeRelease(pConverter);
    SafeRelease(pSource);
    SafeRelease(pDecoder);
    SafeRelease(pStream);
    SafeRelease(pIWICFactory);

    if (SUCCEEDED(hrCoInit))
    {
        CoUninitialize();
    }

    return hr;
}

*/