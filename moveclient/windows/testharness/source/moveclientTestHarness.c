#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <io.h>
#include <fcntl.h>
#include "moveclient.h"
#include "resource.h"


#pragma comment(lib, "user32.lib")
#pragma comment(lib, "gdi32.lib")


// Typedefs
typedef INT (__cdecl *CONNECTPROC) (PCSTR, PCSTR, LPMoveStateDeferred);
typedef INT (__cdecl *DISCONNECTPROC) ();
typedef INT (__cdecl *PAUSEPROC) ();
typedef INT (__cdecl *RESUMEPROC) ();
typedef INT (__cdecl *PAUSECAMERAPROC) ();
typedef INT (__cdecl *RESUMECAMERAPROC) ();
typedef INT (__cdecl *FREQUENCYPROC) (uint32_t);
typedef INT (__cdecl *CAMERAFREQUENCYPROC) (uint32_t);
typedef INT (__cdecl *RUMBLEPROC) (uint32_t, uint32_t);
typedef INT (__cdecl *FORCERGBPROC) (uint32_t, float, float, float);
typedef INT (__cdecl *TRACKHUESPROC) (uint32_t, uint32_t, uint32_t, uint32_t);
typedef INT (__cdecl *PREPARECAMERAPROC) (uint32_t, float);
typedef INT (__cdecl *CAMERASLICESPROC) (uint32_t);


// Prototypes
LRESULT CALLBACK WndProc(HWND, UINT, WPARAM, LPARAM);
void cleanMain(LPMoveStateDeferred, LPMoveServerPacket, LPMoveServerCameraFrameSlicePacket, HINSTANCE);
INT UpdateGemState(LPMoveServerPacket);
INT UpdateFailure(INT);
INT UpdateCamera(LPMoveServerCameraFrameSlicePacket);
INT UpdateCameraFailure(INT);


// Constants
static const char g_szClassName[] = "GemTestHarness";
static const WORD MAX_CONSOLE_LINES = 500;


// Locals
HINSTANCE ghInstance;
HANDLE ghConsole;
HDC ghDC;

CONNECTPROC gconnectProcAdd;
DISCONNECTPROC gdisconnectProcAdd;
PAUSEPROC gpauseProcAdd;
RESUMEPROC gresumeProcAdd;
PAUSECAMERAPROC gpauseCameraProcAdd;
RESUMECAMERAPROC gresumeCameraProcAdd;
FREQUENCYPROC gdelayProcAdd;
CAMERAFREQUENCYPROC gcameraDelayProcAdd;
RUMBLEPROC grumbleProcAdd;
FORCERGBPROC gforceRGBProcAdd;
TRACKHUESPROC gtrackHuesProcAdd;
PREPARECAMERAPROC gprepareCameraProcAdd;
CAMERASLICESPROC gcameraSlicesProcAdd;


/* Initialize Application */
INT WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow) {
  WNDCLASSEX wc;
  HWND hwnd;
  HMENU hmenu;
  BOOL bRet;
  MSG Msg;
  HINSTANCE hinstLib;
  CONSOLE_SCREEN_BUFFER_INFO hConsoleInfo;
  LPMoveStateDeferred lpMoveStateDeferred;
  LPMoveServerPacket lpMoveServerPacket;
  LPMoveServerCameraFrameSlicePacket lpMoveServerCameraFrameSlicePacket;
  int retVal;

  // Allocate a new console
  AllocConsole();

  // Setup the screen buffer
  GetConsoleScreenBufferInfo(GetStdHandle(STD_OUTPUT_HANDLE), &hConsoleInfo);
  hConsoleInfo.dwSize.Y = MAX_CONSOLE_LINES;
  SetConsoleScreenBufferSize(GetStdHandle(STD_OUTPUT_HANDLE), hConsoleInfo.dwSize);

  // Redirect unbuffered STDOUT to the console
  ghConsole = GetStdHandle(STD_OUTPUT_HANDLE);

  // Allocate structs on heap
  lpMoveStateDeferred = (LPMoveStateDeferred)malloc((sizeof(MoveStateDeferred)));
  lpMoveServerPacket = (LPMoveServerPacket)malloc((sizeof(MoveServerPacket)));
  lpMoveServerCameraFrameSlicePacket = (LPMoveServerCameraFrameSlicePacket)malloc((sizeof(MoveServerCameraFrameSlicePacket)));

  // Get a handle to the dll module
  hinstLib = LoadLibrary(TEXT("moveclient.dll"));

  // If the handle is valid, try to get the function addresses
  if (hinstLib != NULL) {
    gconnectProcAdd = (CONNECTPROC) GetProcAddress(hinstLib, "Connect");
    gdisconnectProcAdd = (DISCONNECTPROC) GetProcAddress(hinstLib, "Disconnect");
    gpauseProcAdd = (PAUSEPROC) GetProcAddress(hinstLib, "Pause");
    gresumeProcAdd = (RESUMEPROC) GetProcAddress(hinstLib, "Resume");
    gpauseCameraProcAdd = (PAUSECAMERAPROC) GetProcAddress(hinstLib, "PauseCamera");
    gresumeCameraProcAdd = (RESUMECAMERAPROC) GetProcAddress(hinstLib, "ResumeCamera");
    gdelayProcAdd = (FREQUENCYPROC) GetProcAddress(hinstLib, "UpdateDelay");
    gcameraDelayProcAdd = (CAMERAFREQUENCYPROC) GetProcAddress(hinstLib, "UpdateCameraDelay");
    grumbleProcAdd = (RUMBLEPROC) GetProcAddress(hinstLib, "Rumble");
    gforceRGBProcAdd = (FORCERGBPROC) GetProcAddress(hinstLib, "ForceRGB");
    gtrackHuesProcAdd = (TRACKHUESPROC) GetProcAddress(hinstLib, "TrackHues");
    gprepareCameraProcAdd = (PREPARECAMERAPROC) GetProcAddress(hinstLib, "PrepareCamera");
    gcameraSlicesProcAdd = (CAMERASLICESPROC) GetProcAddress(hinstLib, "CameraSetNumSlices");

    // Verify functions
    if (gconnectProcAdd == NULL) {
      MessageBox(NULL, "Resolving Connect Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
      cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

      return 0;

    }

    if (gdisconnectProcAdd == NULL) {
      MessageBox(NULL, "Resolving Disconnect Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
      cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

      return 0;

    }

    if (gpauseProcAdd == NULL) {
      MessageBox(NULL, "Resolving Pause Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
      cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

      return 0;

    }

    if (gresumeProcAdd == NULL) {
      MessageBox(NULL, "Resolving Resume Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
      cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

      return 0;

    }

    if (gpauseCameraProcAdd == NULL) {
      MessageBox(NULL, "Resolving PauseCamera Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
      cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

      return 0;

    }

    if (gresumeCameraProcAdd == NULL) {
      MessageBox(NULL, "Resolving ResumeCamera Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
      cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

      return 0;

    }

    if (gdelayProcAdd == NULL) {
      MessageBox(NULL, "Resolving Delay Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
      cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

      return 0;

    }

    if (gcameraDelayProcAdd == NULL) {
      MessageBox(NULL, "Resolving Camera Delay Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
      cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

      return 0;

    }

    if (grumbleProcAdd == NULL) {
      MessageBox(NULL, "Resolving Rumble Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
      cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

      return 0;

    }

    if (gforceRGBProcAdd == NULL) {
      MessageBox(NULL, "Resolving ForceRGB Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
      cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

      return 0;

    }

    if (gtrackHuesProcAdd == NULL) {
      MessageBox(NULL, "Resolving TrackHues Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
      cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

      return 0;

    }

    if (gprepareCameraProcAdd == NULL) {
      MessageBox(NULL, "Resolving PrepareCamera Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
      cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

      return 0;

    }

    if (gcameraSlicesProcAdd == NULL) {
      MessageBox(NULL, "Resolving CameraSetNumSlices Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
      cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

      return 0;

    }

  }

  // Register window class
  if (!hPrevInstance) {
    wc.cbSize = sizeof(WNDCLASSEX);
    wc.style = CS_OWNDC | CS_VREDRAW | CS_HREDRAW;;
    wc.lpfnWndProc = WndProc;
    wc.cbClsExtra = 0;
    wc.cbWndExtra = 0;
    wc.hInstance = hInstance;
    wc.hIcon = LoadIcon(NULL, IDI_APPLICATION);
    wc.hCursor = LoadCursor(NULL, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
    wc.lpszMenuName = MAKEINTRESOURCE(IDR_MENU1);
    wc.lpszClassName = g_szClassName;
    wc.hIconSm = LoadIcon(NULL, IDI_APPLICATION);

    if (!RegisterClassEx(&wc)) {
      MessageBox(NULL, "Window Registration Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
      cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

      return 0;

    }

  }

  ghInstance = hInstance;
  hmenu = LoadMenu(hInstance, MAKEINTRESOURCE(IDR_MENU1));

  // Create window
  hwnd = CreateWindowEx(WS_EX_CLIENTEDGE, 
      g_szClassName, 
      "Gem Test Harness", 
      WS_OVERLAPPEDWINDOW|ES_AUTOVSCROLL|ES_AUTOHSCROLL, 
      CW_USEDEFAULT, 
      CW_USEDEFAULT, 
      500, 
      300, 
      NULL,
      hmenu,
      hInstance,
      NULL);

  if (hwnd == NULL) {
    MessageBox(NULL, "Window Creation Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
    cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

    return 0;
  }

  ShowWindow(hwnd, nCmdShow);
  UpdateWindow(hwnd);

  // Connect to PS3
  lpMoveStateDeferred->fpUpdateSuccess = UpdateGemState;
  lpMoveStateDeferred->fpUpdateFailure = UpdateFailure;
  lpMoveStateDeferred->fpUpdateCameraSuccess = UpdateCamera;
  lpMoveStateDeferred->fpUpdateCameraFailure = UpdateCameraFailure;
  lpMoveStateDeferred->lpMoveServerPacket = lpMoveServerPacket;
  lpMoveStateDeferred->lpMoveServerCameraFrameSlicePacket = lpMoveServerCameraFrameSlicePacket;

  if (retVal = (gconnectProcAdd)("10.98.35.189", "7899", lpMoveStateDeferred)) {
    MessageBox(NULL, "PS3 Connect Failed", "Error", MB_ICONEXCLAMATION | MB_OK);
    cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

    return 0;

  }

  // Message loop
  while (bRet = GetMessage(&Msg, NULL, 0, 0)) {

    if (bRet == -1) {
      MessageBox(NULL, "GetMessage error", "Error", MB_ICONEXCLAMATION | MB_OK);
      cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

      return 0;

    }
    else {
      TranslateMessage(&Msg);
      DispatchMessage(&Msg);

    }

  }

  // Disconnect from PS3
  if ((gdisconnectProcAdd)()){
    MessageBox(NULL, "PS3 Disconnect Failed", "Error", MB_ICONEXCLAMATION | MB_OK);
    cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

    return 0;

  }

  cleanMain(lpMoveStateDeferred, lpMoveServerPacket, lpMoveServerCameraFrameSlicePacket, hinstLib);

  return (int)Msg.wParam;

}


void cleanMain(LPMoveStateDeferred lpMoveStateDeferred, LPMoveServerPacket lpMoveServerPacket, LPMoveServerCameraFrameSlicePacket lpMoveServerCameraFrameSlicePacket, HINSTANCE hinstLib) {

  // Deallocate heap memory
  free(lpMoveServerPacket);
  free(lpMoveServerCameraFrameSlicePacket);
  free(lpMoveStateDeferred);

  // Close console handle
  CloseHandle(ghConsole);

  // Close console
  FreeConsole();

  // Free the dll module
  if (!FreeLibrary(hinstLib)) {
    MessageBox(NULL, "DLL Free Failed!", "Error", MB_ICONEXCLAMATION | MB_OK);

  }

}


/* Window Procedure */
LRESULT CALLBACK WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam) {
  PAINTSTRUCT ps;
  HDC hDC;

  switch (msg) {

    case WM_CREATE:
      hDC = GetDC(hWnd);
      ghDC = hDC;
      break;

    case WM_PAINT:
      hDC = BeginPaint(hWnd, &ps);
      TextOut(hDC, 10, 10, TEXT("Started writing to file, close this window to exit program"), 58);
      EndPaint(hWnd, &ps);
      break;

    case WM_CLOSE:
      DestroyWindow(hWnd);
      break;

    case WM_DESTROY:
      PostQuitMessage(0);
      break;

    case WM_COMMAND:

      switch (wParam) {

        case ID_FILE_PAUSE:

          if ((gpauseProcAdd)())
            MessageBox(NULL, "PS3 Pause Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_FILE_RESUME:

          if ((gresumeProcAdd)())
            MessageBox(NULL, "PS3 Resume Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;


          // Camera
        case ID_FILE_PREPARECAMERA:

          if ((gprepareCameraProcAdd)(24, 1.0))
            MessageBox(NULL, "PS3 Prepare Camera Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_FILE_PAUSECAMERA:

          if ((gpauseCameraProcAdd)())
            MessageBox(NULL, "PS3 Pause Camera Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_FILE_RESUMECAMERA:

          if ((gresumeCameraProcAdd)())
            MessageBox(NULL, "PS3 Resume Camera Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

          // Delay
        case ID_FREQUENCY_360HZ:

          if ((gdelayProcAdd)(2))
            MessageBox(NULL, "PS3 Frequency 360hz Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_FREQUENCY_240HZ:

          if ((gdelayProcAdd)(4))
            MessageBox(NULL, "PS3 Frequency 240hz Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_FREQUENCY_120HZ:

          if ((gdelayProcAdd)(8))
            MessageBox(NULL, "PS3 Frequency 120hz Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_FREQUENCY_60HZ:

          if ((gdelayProcAdd)(16))
            MessageBox(NULL, "PS3 Frequency 60hz Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;


          // Camera Delay
        case ID_CAMERAFREQUENCY_360HZ:

          if ((gcameraDelayProcAdd)(2))
            MessageBox(NULL, "PS3 Camera Frequency 360hz Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_CAMERAFREQUENCY_240HZ:

          if ((gcameraDelayProcAdd)(4))
            MessageBox(NULL, "PS3 Camera Frequency 240hz Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_CAMERAFREQUENCY_120HZ:

          if ((gcameraDelayProcAdd)(8))
            MessageBox(NULL, "PS3 Camera Frequency 120hz Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_CAMERAFREQUENCY_60HZ:

          if ((gcameraDelayProcAdd)(16))
            MessageBox(NULL, "PS3 Camera Frequency 60hz Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;


          // Rumble
        case ID_RUMBLE_CONTROLLER0_ON:

          if ((grumbleProcAdd)(0, 100))
            MessageBox(NULL, "PS3 Rumble Controller 0 On Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_RUMBLE_CONTROLLER0_OFF:

          if ((grumbleProcAdd)(0, 0))
            MessageBox(NULL, "PS3 Rumble Controller 0 Off Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_RUMBLE_CONTROLLER1_ON:

          if ((grumbleProcAdd)(1, 100))
            MessageBox(NULL, "PS3 Rumble Controller 1 On Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_RUMBLE_CONTROLLER1_OFF:

          if ((grumbleProcAdd)(1, 0))
            MessageBox(NULL, "PS3 Rumble Controller 1 Off Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_RUMBLE_CONTROLLER2_ON:

          if ((grumbleProcAdd)(2, 100))
            MessageBox(NULL, "PS3 Rumble Controller 2 On Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_RUMBLE_CONTROLLER2_OFF:

          if ((grumbleProcAdd)(2, 0))
            MessageBox(NULL, "PS3 Rumble Controller 2 Off Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_RUMBLE_CONTROLLER3_ON:

          if ((grumbleProcAdd)(3, 100))
            MessageBox(NULL, "PS3 Rumble Controller 3 On Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_RUMBLE_CONTROLLER3_OFF:

          if ((grumbleProcAdd)(3, 0))
            MessageBox(NULL, "PS3 Rumble Controller 3 Off Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;


          // Color
        case ID_COLOR_CONTROLLER0_RED:

          if ((gforceRGBProcAdd)(0, 1.0, 0.0, 0.0))
            MessageBox(NULL, "PS3 ForceRGB Controller 0 Red Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_COLOR_CONTROLLER0_BLUE:

          if ((gforceRGBProcAdd)(0, 0.0, 0.0, 1.0))
            MessageBox(NULL, "PS3 ForceRGB Controller 0 Blue Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_COLOR_CONTROLLER0_GREEN:

          if ((gforceRGBProcAdd)(0, 0.0, 1.0, 0.0))
            MessageBox(NULL, "PS3 ForceRGB Controller 0 Green Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_COLOR_CONTROLLER1_RED:

          if ((gforceRGBProcAdd)(1, 1.0, 0.0, 0.0))
            MessageBox(NULL, "PS3 ForceRGB Controller 1 Red Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_COLOR_CONTROLLER1_BLUE:

          if ((gforceRGBProcAdd)(1, 0.0, 0.0, 1.0))
            MessageBox(NULL, "PS3 ForceRGB Controller 1 Blue Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_COLOR_CONTROLLER1_GREEN:

          if ((gforceRGBProcAdd)(1, 0.0, 1.0, 0.0))
            MessageBox(NULL, "PS3 ForceRGB Controller 1 Green Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_COLOR_CONTROLLER2_RED:

          if ((gforceRGBProcAdd)(2, 1.0, 0.0, 0.0))
            MessageBox(NULL, "PS3 ForceRGB Controller 2 Red Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_COLOR_CONTROLLER2_BLUE:

          if ((gforceRGBProcAdd)(2, 0.0, 0.0, 1.0))
            MessageBox(NULL, "PS3 ForceRGB Controller 2 Blue Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_COLOR_CONTROLLER2_GREEN:

          if ((gforceRGBProcAdd)(2, 0.0, 1.0, 0.0))
            MessageBox(NULL, "PS3 ForceRGB Controller 2 Green Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_COLOR_CONTROLLER3_RED:

          if ((gforceRGBProcAdd)(3, 1.0, 0.0, 0.0))
            MessageBox(NULL, "PS3 ForceRGB Controller 3 Red Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_COLOR_CONTROLLER3_BLUE:

          if ((gforceRGBProcAdd)(3, 0.0, 0.0, 1.0))
            MessageBox(NULL, "PS3 ForceRGB Controller 3 Blue Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_COLOR_CONTROLLER3_GREEN:

          if ((gforceRGBProcAdd)(3, 0.0, 1.0, 0.0))
            MessageBox(NULL, "PS3 ForceRGB Controller 3 Green Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;


          // Track Hues
        case ID_TRACKHUES_CONTROLLER0_ON:

          if ((gtrackHuesProcAdd)(1, 0, 0, 0))
            MessageBox(NULL, "PS3 Track Hues Controller 0 On Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_TRACKHUES_CONTROLLER0_OFF:

          if ((gtrackHuesProcAdd)(0, 0, 0, 0))
            MessageBox(NULL, "PS3 Track Hues Controller 0 Off Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_TRACKHUES_CONTROLLER1_ON:

          if ((gtrackHuesProcAdd)(0, 1, 0, 0))
            MessageBox(NULL, "PS3 Track Hues Controller 1 On Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_TRACKHUES_CONTROLLER1_OFF:

          if ((gtrackHuesProcAdd)(0, 0, 0, 0))
            MessageBox(NULL, "PS3 Track Hues Controller 1 Off Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_TRACKHUES_CONTROLLER2_ON:

          if ((gtrackHuesProcAdd)(0, 0, 1, 0))
            MessageBox(NULL, "PS3 Track Hues Controller 2 On Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_TRACKHUES_CONTROLLER2_OFF:

          if ((gtrackHuesProcAdd)(0, 0, 0, 0))
            MessageBox(NULL, "PS3 Track Hues Controller 2 Off Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_TRACKHUES_CONTROLLER3_ON:

          if ((gtrackHuesProcAdd)(0, 0, 0, 1))
            MessageBox(NULL, "PS3 Track Hues Controller 3 On Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_TRACKHUES_CONTROLLER3_OFF:

          if ((gtrackHuesProcAdd)(0, 0, 0 ,0))
            MessageBox(NULL, "PS3 Track Hues Controller 3 Off Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;


          // Camera Slices
        case ID_CAMERASLICES_8:

          if ((gcameraSlicesProcAdd)(8))
            MessageBox(NULL, "PS3 Camera Slices 8 Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_CAMERASLICES_4:

          if ((gcameraSlicesProcAdd)(4))
            MessageBox(NULL, "PS3 Camera Slices 4 Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_CAMERASLICES_2:

          if ((gcameraSlicesProcAdd)(2))
            MessageBox(NULL, "PS3 Camera Slices 2 Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;

        case ID_CAMERASLICES_1:

          if ((gcameraSlicesProcAdd)(1))
            MessageBox(NULL, "PS3 Camera Slices 1 Failed", "Error", MB_ICONEXCLAMATION | MB_OK);

          break;


          // Exit
        case ID_FILE_EXIT:
          PostQuitMessage(0);
          break;

        default:
          return DefWindowProc(hWnd, msg, wParam, lParam);

      }

      break;

    default:
      return DefWindowProc(hWnd, msg, wParam, lParam);

  }

  return 0;

}


INT UpdateGemState(LPMoveServerPacket lpMoveServerPacket) {
  DWORD bytes;
  char buffer[1024];
  int gem = 0;

  sprintf_s(buffer, _countof(buffer), "pos = <%f, %f, %f, %f>, time = <%lld>\r\n", lpMoveServerPacket->state[gem].pos[0], lpMoveServerPacket->state[gem].pos[1], 
      lpMoveServerPacket->state[gem].pos[2], lpMoveServerPacket->state[gem].pos[3], lpMoveServerPacket->state[gem].timestamp);

  WriteFile(ghConsole, buffer, strlen(buffer), &bytes, NULL);

  return 0;
}


INT UpdateFailure(INT failure) {
  char buffer[64];

  sprintf_s(buffer, _countof(buffer), "Got Failure: %d", failure);
  TextOut(ghDC, 10, 40, TEXT(buffer), strlen(buffer));

  (gdisconnectProcAdd)();

  return 0;
}


INT UpdateCamera(LPMoveServerCameraFrameSlicePacket lpMoveServerCameraFrameSlicePacket) {

  return 0;
}


INT UpdateCameraFailure(INT failure) {
  char buffer[64];

  sprintf_s(buffer, _countof(buffer), "Got Camera Failure: %d", failure);
  TextOut(ghDC, 10, 40, TEXT(buffer), strlen(buffer));

  (gdisconnectProcAdd)();

  return 0;
}
