using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WebcamHandle : MonoBehaviour
{

    /**serialized**/
    [SerializeField] private bool debug = false;
    /****/


    /*public:*/

    #region webcam  Params
    [Serializable]
    public class WebcamParams
    {
        [SerializeField] public Vector2 DesiredDimensions = Vector2.zero;
        [SerializeField] public Vector4 CropPercent = Vector4.zero;
        [SerializeField] public float CameraTimeout = 4000;
        [SerializeField] public string[] CameraNames;
        [SerializeField] public Vector2[] AutofocusPoint;
        [SerializeField] public string[] DetectedNames;
        [SerializeField] public int TargetFPS;
    }
    #endregion
    public WebcamParams WCparams;

    /// <summary>
    /// Check if frames were updated. Close the light when you're done checking;
    /// </summary>
    public bool[] FramesUpdated
    {
        get { return newFrames; }
        set { newFrames = value; }
    }

    /// <summary>
    /// Check if webcams are sending frames and set up;
    /// </summary>
    public bool SystemReady
    {
        get { return systemReady; }
    }

    /// <summary>
    /// Returns actual webcams resolutions;
    /// </summary>
    public Vector2[] WebcamDimensions
    {
        get { return camsDims; }
    }

    /****/


    /*private:*/
    /// <summary>
    /// Material of plane for debug purpose. Length must be = camCount
    /// </summary>
    private Material[] debuggerMat;

    private bool[] newFrames;
    private WebCamTexture[] cams;
    private Color32[][] cams32;
    private Coroutine[] captureRoutine;
    private bool systemReady = false;
    private bool timeout = false;
    private bool[] camsReady;
    private bool[] camsPlaying;
    private Vector2[] camsDims;

    /****/
    private void Awake()
    {
        WCparams.DetectedNames = GetDeviceList();
        if (debug)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            int debugCount = transform.GetChild(0).childCount;
            debuggerMat = new Material[debugCount];
            for (int i = 0; i < debugCount; i++)
            {
                debuggerMat[i] = transform.GetChild(0).GetChild(i).GetComponent<Renderer>().material;
            }
        }
        else transform.GetChild(0).gameObject.SetActive(false);
    }

    /******************************************************************************************************/
    /******************************************************************************************************/
    #region Public ************************************************************************************************/
    /******************************************************************************************************/
    /******************************************************************************************************/

    /// <summary>
    /// Returns list of devices in a string array;
    /// </summary>
    public string[] GetDeviceList()
    {
        WebCamDevice[] camsDevices = WebCamTexture.devices;
        string[] camsName = new string[camsDevices.Length];
        for(int i=0; i<camsName.Length; i++)
        {
            camsName[i] = camsDevices[i].name;
        }
        return camsName;
    }


    /// <summary>
    /// Initiates webcams and handler. Returns true if successfull;
    /// </summary>
    public bool InitiateWebcamHandler()
    {
        if (WCparams.CameraNames.Length <= 0)
            return false;
        bool inited= InitWebcams(WCparams.CameraNames);
        if (!inited)
            return false;
        StartCoroutine(WaitForCamReady(WCparams.CameraNames.Length));
        return true;
    }


    /// <summary>
    /// Starts frame grabbing routines;
    /// </summary>
    public bool StartCapture(int camCount)
    {
        for(int i=0; i<camCount; i++)
        {
            if (captureRoutine[i] != null) StopCoroutine(captureRoutine[i]);
            captureRoutine[i] = StartCoroutine(RefreshFrames(i));
        }
        return false;
    }


    /// <summary>
    /// Returns a reference to the webcam's Color32 arrays. WARNING webcams must be initiated;
    /// </summary>
    public ref Color32[][] LinkCamColors()
    {
        return ref cams32;
    }


    /// <summary>
    /// Stops frame grabbing routines;
    /// </summary>
    public bool StopCapture(int[] camsIdx)
    {
        for(int i=0; i<camsIdx.Length; i++)
        {
            if (captureRoutine[i] != null)
            {
                StopCoroutine(captureRoutine[camsIdx[i]]);
                captureRoutine[camsIdx[i]] = null;
            }
        }
        
        return false;
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////



    /******************************************************************************************************/
    /******************************************************************************************************/
    #region Private ***********************************************************************************************/
    /******************************************************************************************************/
    /******************************************************************************************************/

    private bool InitWebcams(string[] _names)
    {
        int camCount = _names.Length;
        InitVariables(camCount);
        for (int i = 0; i < camCount; i++)
        {
            if (!SetupWebcam(i, _names[i])) return false;
        }
        return true;
    }

    private void InitVariables(int _camCount)
    {       
        cams = new WebCamTexture[_camCount];
        camsReady = new bool[_camCount];
        captureRoutine = new Coroutine[_camCount];
        camsPlaying = new bool[_camCount];
        camsDims = new Vector2[_camCount];
        cams32 = new Color32[_camCount][];
        newFrames = new bool[_camCount];
    }
    private bool SetupWebcam(int idx, string _name)
    {
        cams[idx] = new WebCamTexture(_name, (int)WCparams.DesiredDimensions.x, (int)WCparams.DesiredDimensions.y, WCparams.TargetFPS);
        if (cams[idx] == null)
            return false;
        cams[idx].autoFocusPoint= WCparams.AutofocusPoint[idx];
        cams[idx].Play();
        camsPlaying[idx] = true;
        return true;
    }



    private IEnumerator WaitForCamReady(int camCount)
    {
        float watchdog = Time.time;
        int breakout = 0;
        while (Time.time- watchdog <= WCparams.CameraTimeout)
        {
            for(int i=0; i<camCount; i++)
            {
                if (camsReady[i])
                    continue;
                camsReady[i] = cams[i].isPlaying && cams[i].didUpdateThisFrame;
                if (camsReady[i])
                {
                    SetupVariables(i);
                    breakout++;
                }
            }
            if (breakout >= camCount)
            {
                systemReady = true;
                break;
            }
            yield return null;
        }
        if(!systemReady) timeout = true;
        yield return null;
    }
    private void SetupVariables(int idx)
    {
        camsDims[idx] = new Vector2(cams[idx].width, cams[idx].height);      
        int arrSize = (cams[idx].width - 1) + (cams[idx].width*(cams[idx].height-1));
        cams32[idx] = new Color32[arrSize];
        if(debug) debuggerMat[idx].SetTexture("_BaseMap", cams[idx]);
    }



    private IEnumerator RefreshFrames(int idx)
    {
        while(camsPlaying[idx])
        {
            while(!cams[idx].didUpdateThisFrame)
            {
                //waiting lol
                yield return null;
            }  
            newFrames[idx] = true;
            cams32[idx] = cams[idx].GetPixels32();
            yield return null;
        }
        yield return null;
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////

}
