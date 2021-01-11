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
	// OPEN SOCKET CONNECTION #############################################
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

	// ####################################################################
    if (!m_pMultiSourceFrameReader)
    {
        return;
    }

    IMultiSourceFrame* pMultiSourceFrame = NULL;
    IDepthFrame* pDepthFrame = NULL;
    IColorFrame* pColorFrame = NULL;

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
				}

				mat = mat + imgLines;

				//____________ depth processing __________
				ushort depthValue = 0;
				if (NULL != pDepthBuffer)
				{
					ColorSpacePoint *depP = new ColorSpacePoint[cDepthWidth * cDepthHeight];
					CameraSpacePoint *camera_space = new CameraSpacePoint[cDepthWidth * cDepthHeight];
					m_pCoordinateMapper->MapDepthFrameToColorSpace(cDepthWidth * cDepthHeight, (UINT16*) pDepthBuffer, cDepthWidth * cDepthHeight, depP);
					m_pCoordinateMapper->MapDepthFrameToCameraSpace(cDepthWidth * cDepthHeight, (UINT16*)pDepthBuffer, cDepthWidth * cDepthHeight, camera_space);
				
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
					else
					{
						depthValue = pDepthBuffer[depthIndex];
						depth = depthValue;

						 final_x = camera_space[depthIndex].X;
						 final_y = camera_space[depthIndex].Y;
						 final_z = camera_space[depthIndex].Z;

					}
					
				}

				// ___________ end of depth processing __________

				circle(mat, Point(posX, posY), 10, Scalar(0, 0, 255), 3); //int thickness = 1, int lineType = 8, int shift = 0
				stringstream stream;
				stream << "X: " << setprecision(2) << posX << " Y: " <<  posY << " ";
				string pos = stream.str();
				putText(mat, "Image Coordinates", Point(10, 50), FONT_HERSHEY_SIMPLEX, 2, Scalar(0, 0, 255), 6, LINE_AA);
				putText(mat, pos, Point(10, 125), FONT_HERSHEY_SIMPLEX, 2, Scalar(0, 0, 255), 6, LINE_AA);
				string d = " "+ to_string(depth) + "mm";
				putText(mat, d, Point(500, 125), FONT_HERSHEY_SIMPLEX, 2, Scalar(0, 0, 255), 6, LINE_AA);

				stringstream stream2;
				stream2 << "X: " << setprecision(3) << final_x << " Y: " <<  final_y << " Z: " <<  final_z << " ";
				
				string kinectpos = stream2.str();
				sendto(s, kinectpos.c_str(), strlen(kinectpos.c_str()), 0, (struct sockaddr *) &si_other, slen);
				

				putText(mat, "Kinect Coordinates", Point(1000, 50), FONT_HERSHEY_SIMPLEX, 2, Scalar(0, 0, 255), 6, LINE_AA);
				putText(mat, kinectpos, Point(1000, 125), FONT_HERSHEY_SIMPLEX, 2, Scalar(0, 0, 255), 6, LINE_AA);

				//show the thresholded image
				resize(imgThresholded, imgThresholded, Size(imgThresholded.cols / 3, imgThresholded.rows / 3)); // to 1/3 size (or even smaller)
				
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
					//saving parameters to file: iLowH, iHighH, iLowS, iHighS, iLowV, iHighV to reuse the next time the program starts
					myfile << iLowH << endl << iHighH  << endl << iLowS << endl << iHighS << endl << iLowV << endl<< iHighV << "\n\nThe Thresholding parameters are written in this order:\nHue (low-high), Saturation (low-high), Value (low-high)";
					myfile.close();

					destroyAllWindows();
					exit(0);	
				}

            }
            else
            {
                hr = E_FAIL;
            }
        }

        SafeRelease(pDepthFrameDescription);
        SafeRelease(pColorFrameDescription);
    }

    SafeRelease(pDepthFrame);
    SafeRelease(pColorFrame);
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
