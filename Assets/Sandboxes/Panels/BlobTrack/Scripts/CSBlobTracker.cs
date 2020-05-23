using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSBlobTracker : MonoBehaviour
{
    [SerializeField] private CustomRenderTexture rawInput; //to change for webcam texture


    /****************************************************************/
    #region public data   
    //////////////////////////////////
    public BlobData[] Blobs
    {
        get { return blobs; }
    }
    public int BlobCounts
    {
        get { return blobCounts; }
    }

    //////////////////////////////////
    #endregion
    /****************************************************************/


    /****************************************************************/
    #region blob data   
    //////////////////////////////////

    public struct BlobData
    {
        public Vector2 center;
        public Vector2 size;
        public int index;
    }
    private BlobData[] blobs;
    private int blobCounts;

    //////////////////////////////////
    #endregion
    /****************************************************************/


    /****************************************************************/
    #region buffer and compute data
    ///////////////////////////////////

    [SerializeField] private int maxBlobsPerPanels = 10;

    private struct RecursiveTracking
    {
        float placeholder;
    }
    private RecursiveTracking[] tracking;

    private ComputeBuffer blobTrackingBuffer;
    private int blobsStrideSize = 20;


    [SerializeField] private ComputeShader computeTracker;
    private int warpX = 16;
    private int warpY = 16;
    private int warpZ = 1;
    private int computeKernelID;
    private int blobDataID;
    private int texInputID;
   

    //////////////////////////////////
    #endregion
    /****************************************************************/


    /*****************************************************************************************************************/
    /*****************************************************************************************************************/
    void Start()
    {
        blobTrackingBuffer = new ComputeBuffer(maxBlobsPerPanels, blobsStrideSize);
        SetupBlobBuffer(maxBlobsPerPanels);
        SetupComputeShader();      
    }

    
    void Update()
    {
        computeTracker.SetTexture(computeKernelID, texInputID, rawInput);
        computeTracker.Dispatch(computeKernelID, warpX, warpY, warpZ);
    }
    /*****************************************************************************************************************/
    /*****************************************************************************************************************/



    /*****************************************************************************************************************/
    #region buffer Setups
    ///////////////////////////////////////////////////////////////////////////////////////////////////////

    private void SetupBlobBuffer(int _length)
    {
        blobs = new BlobData[_length];
        for(int i=0; i< blobs.Length; i++)
        {
            blobs[i] = new BlobData();
            blobs[i].index= -1;
        }

        blobTrackingBuffer.SetData(blobs);
    }

    private void SetupComputeShader()
    {
        computeKernelID = computeTracker.FindKernel("CSBlobTracker");
        texInputID = Shader.PropertyToID("InputTexture");
        blobDataID = Shader.PropertyToID("blobData");
        computeTracker.SetBuffer(computeKernelID, blobDataID, blobTrackingBuffer);
        computeTracker.SetTexture(computeKernelID, texInputID, rawInput);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    #endregion
    /*****************************************************************************************************************/
}
