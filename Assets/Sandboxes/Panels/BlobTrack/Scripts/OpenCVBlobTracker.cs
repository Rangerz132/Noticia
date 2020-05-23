using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class OpenCVBlobTracker : MonoBehaviour
{
    /**serialized**/
    [SerializeField] private bool debug = false;
    /****/


    /*public:*/

    /// <summary>
    /// OpenCV parameters;
    /// </summary>
    public OpenCVParams CVParams;

    /// <summary>
    /// Returns blob count per panel;
    /// </summary>
    public ref int[] BlobsPerPanels
    {
        get { return ref blobspercams; }
    }

    /// <summary>
    /// Returns total amount of blobs. Addition is computed each time;
    /// </summary>
    public int totalBlobs
    {
        get
        {
            int buff = 0;
            foreach (int i in blobspercams)
                buff += i;
            return buff;
        }
    }

    /// <summary>
    /// Returns ref of blob array from the tracker
    /// </summary>
    public ref CvBlob[][] trackedBlobs
    {
        get { return ref blobs; }
    }

    public bool[] Freshblobs
    {
        get { return newblobs; }
    }
    /****/


    /*private:*/
    private bool initiated;
    private WebcamHandle WCHandle;
    private Material[] debuggerMat;

    private CvBlob[][] blobs;
    private Color32[][] camsColor32;
    private Vector2[][] blobsPositionNormal;
    private int camCount = 0;
    private Coroutine[] detectRoutines;
    private bool[] detecting;
    private bool color32Connected = false;
    private int[] blobspercams;
    private Texture2D[] debugTex;

    private bool[] newblobs;
    /****/

    private void Awake()
    {
        if(debug)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            int debugCount = transform.GetChild(0).childCount;
            debuggerMat= new Material[debugCount];
            for (int i=0; i< debugCount; i++)
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
    /// Initiates the plugin;
    /// </summary>
    public void InitOpenCV()
    {
        if (!initiated)
        {
            initiated = true;
            OpenCVPlugin.Init(CVParams);
        }
    }

    /// <summary>
    /// Tries to link with webcam's Color32 arrays. Return false if webcams not ready;
    /// </summary>
    public bool GrabWebcamsReferences(ref WebcamHandle _wchandle)
    {
        if (WCHandle == null) WCHandle = _wchandle;

        if (!color32Connected)
        {
            if (!LinkWithColor32())
                return false;

            InitVariables();
            color32Connected = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Starts detection routines. Returns false if webcams not playing;
    /// </summary>
    public bool StartDetectionRoutines()
    {
        for(int i=0; i< camCount; i++)
        {
            if (detectRoutines[i] != null) StopCoroutine(detectRoutines[i]);
            detectRoutines[i] = StartCoroutine(DetectionRoutine(i));
        }
        return true;
    }

    public void StopDetectionRoutines()
    {
        for(int i=0; i<camCount; i++)
        {
            if(detectRoutines[i] != null)
            {
                StopCoroutine(detectRoutines[i]);
                detectRoutines[i] = null;
                detecting[i] = false;
            }
        }
    }
    #endregion
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////




    /******************************************************************************************************/
    /******************************************************************************************************/
    #region Private ***********************************************************************************************/
    /******************************************************************************************************/
    /******************************************************************************************************/


    private bool LinkWithColor32()
    {
        if (WCHandle.SystemReady && camsColor32 == null)
        {
            camsColor32 = WCHandle.LinkCamColors();
            return true;
        }
        else return false;
    }

    private void InitVariables()
    {
        camCount = camsColor32.Length;
        detectRoutines = new Coroutine[camCount];
        detecting = new bool[camCount];
        newblobs = new bool[camCount];
        blobspercams = new int[camCount];
        debugTex = new Texture2D[camCount];
        blobs = new CvBlob[camCount][];
        blobsPositionNormal = new Vector2[camCount][];
        for (int i = 0; i < blobs.Length; i++)
        {
            blobs[i] = new CvBlob[CVParams.maxBlobCount];
            blobsPositionNormal[i] = new Vector2[CVParams.maxBlobCount];
        }
        if (debug)
        {
            for (int i = 0; i < debuggerMat.Length; i++)
            {
                debugTex[i] = new Texture2D((int)WCHandle.WebcamDimensions[i].x, (int)WCHandle.WebcamDimensions[i].y, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
                debuggerMat[i].SetTexture("_BaseMap", debugTex[i]);
            }
        }
    }

    private IEnumerator DetectionRoutine(int idx)
    {
        detecting[idx] = true;
        while(detecting[idx])
        {
            while(!WCHandle.FramesUpdated[idx])
            {
                yield return null;
            }

            blobspercams[idx] = DetectBlobsFrame(idx);
            WCHandle.FramesUpdated[idx] = false;
            NormalizePos(idx);
            Freshblobs[idx] = true;
            yield return null;
        }
        yield return null;
    }

    private int DetectBlobsFrame(int idx)
    {
        int detectedFaceCount = 0;
        unsafe
        {
            fixed (CvBlob* outBlobs = blobs[idx])
            {
                detectedFaceCount = OpenCVPlugin.DetectFrame(ref camsColor32[idx], outBlobs, (int)WCHandle.WebcamDimensions[idx].x, (int)WCHandle.WebcamDimensions[idx].y);
                if (debug)
                {
                    debugTex[idx].SetPixels32(camsColor32[idx]);
                    debugTex[idx].Apply();
                }         
            }
        }
        return detectedFaceCount;
    }

    private void NormalizePos(int idx)
    {
        for(int i=0; i<blobspercams[idx]; i++)
        {
            Vector2 buff = new Vector2(blobs[idx][i].X, blobs[idx][i].Y);
            buff.x = 1-(buff.x / WCHandle.WebcamDimensions[idx].x);
            buff.y = 1 - (buff.y / WCHandle.WebcamDimensions[idx].y);
        }        
    }
    #endregion
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////


    /*
    private void ShaderDebug(int count)
    {
        Vector2[] normPos = new Vector2[count];
        for(int i=0; i< count; i++)
        {
            normPos[i] = NormalizePos(blobs[i].X, blobs[i].Y, source.width, source.height);
        }
        debugSpheres.MoveSpheres(normPos, count);
        //debugPlane.GetComponent<Renderer>().material.SetVector("_Center", normalpos);
    }
    */

}

/******************************************************************************************************/
/******************************************************************************************************/
#region OpenCV ************************************************************************************************/
/******************************************************************************************************/
/******************************************************************************************************/


/// <summary>
/// parameter holder
/// </summary>
[Serializable]
[StructLayout(LayoutKind.Sequential, Size = 45)]
public class OpenCVParams
{
    [SerializeField] public int minThresh = 1;
    [SerializeField] public int maxThresh = 200;

    [SerializeField] public bool areaFilter = false;
    [SerializeField] public int minArea = 1;
    [SerializeField] public int maxArea = 200;

    [SerializeField] public bool circularFilter = false;
    [SerializeField] public float circularity = 0.1f;

    [SerializeField] public bool convexFilter = false;
    [SerializeField] public float convexity = 0.37f;

    [SerializeField] public bool inertiaFilter = false;
    [SerializeField] public float minInertia = 0.01f;

    [SerializeField] public bool colorFilter = true;
    [SerializeField] public float blobColor = 255;
    [SerializeField] public float scale = 1.0f;
    [SerializeField] public int maxBlobCount= 10;
}

/*****/
/*****/

/// <summary>
/// OpenCV Plugin's exposed methods
/// </summary>
internal static class OpenCVPlugin
{
    [DllImport("unity_blobTRack_openCV")]
    internal static extern void Init(OpenCVParams parameters);

    [DllImport("unity_blobTRack_openCV")]
    internal unsafe static extern string InitCameras(int numCams, int[] indexes);

    [DllImport("unity_blobTRack_openCV")]
    internal unsafe static extern void ChangeBlobCount(int newMax);

    [DllImport("unity_blobTRack_openCV")]
    internal unsafe static extern void ChangeDownscale(int newScale);

    [DllImport("unity_blobTRack_openCV")]
    internal unsafe static extern int Detect(CvBlob* outBlobs);

    [DllImport("unity_blobTRack_openCV")]
    internal unsafe static extern int DetectFrame(ref Color32[] rawImage, CvBlob* outBlobs, int width, int height);

    [DllImport("unity_blobTRack_openCV")]
    internal unsafe static extern int DetectDebugFrame(ref Color32[] rawImage, CvBlob* outBlobs, int width, int height);
}

/*****/
/*****/

/// <summary>
    /// Blob Struct
    /// </summary>
[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct CvBlob
{
        public int X, Y, Radius, Idx;
}

#endregion
////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////