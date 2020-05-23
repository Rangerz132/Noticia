using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractBlobLayer : MonoBehaviour
{

    /*public:*/
    /// <summary>
    /// Returns reference to final blobs
    /// </summary>
    public ref BlobData[] BlobsRef
    {
        get { return ref blobs; }
    }

    /// <summary>
    /// Returns BlobCount
    /// </summary>
    public ref int BlobCount 
    {
        get { return ref blobCount; } 
    }
    /****/


    /*private:*/
    private OpenCVBlobTracker tracker;
    private CvBlob[][] trackerBlobs;
    private int[] trackedBlobsCount;

    private int blobCount=0;

    private Coroutine[] abstractionRoutine;
    private Coroutine finalCheck;
    private bool[] absRunning;
    private bool atleastOneRunning = false;
    private BlobData[] blobs;
    private int camCount;

    private bool[] doneCompute;
    /****/


    /******************************************************************************************************/
    /******************************************************************************************************/
    #region Public ************************************************************************************************/
    /******************************************************************************************************/
    /******************************************************************************************************/

    /// <summary>
    /// Initiates Abstract layer. Return false if already Init;
    /// </summary>
    public bool InitAbstractLayer(ref OpenCVBlobTracker _tracker)
    {
        if (tracker == null)
        {
            tracker = _tracker;
            trackerBlobs = tracker.trackedBlobs;
            trackedBlobsCount = tracker.BlobsPerPanels;

            camCount = trackedBlobsCount.Length;
            blobs = new BlobData[tracker.CVParams.maxBlobCount];
            abstractionRoutine = new Coroutine[camCount];
            absRunning = new bool[camCount];
            doneCompute = new bool[camCount];
            return true;
        }
        return false;
    }

    /// <summary>
    /// Starts blob abstraction. Returns true if already started;
    /// </summary>
    public bool StartAbstractLayer()
    {
        int skip = 0;
        for(int i=0; i< camCount; i++)
        {
            if (absRunning[i])
            {
                skip++;
                continue;
            }
            if (abstractionRoutine[i] != null) StopCoroutine(abstractionRoutine[i]);
            abstractionRoutine[i] = StartCoroutine(RefreshBlobList(i));
        }

        if (finalCheck != null) StopCoroutine(finalCheck);
        finalCheck = StartCoroutine(CheckDoneState());
        atleastOneRunning = true;

        if (skip >= camCount) return true;
        return false;
    }

    /// <summary>
    /// Stops blob abstraction. Returns true if already stopped;
    /// </summary>
    public bool StopAbstractLayer()
    {
        int skip = 0;
        for (int i = 0; i < camCount; i++)
        {
            if (abstractionRoutine[i] != null)
            {
                StopCoroutine(abstractionRoutine[i]);
                abstractionRoutine[i] = null;
                absRunning[i] = false;
            }
            else skip++;
        }
        if (finalCheck != null)
        {
            StopCoroutine(finalCheck);
            finalCheck = null;
        }
        atleastOneRunning = false;
        if (skip >= camCount) return true;
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

    private IEnumerator CheckDoneState()
    {
        while(atleastOneRunning)
        {         
            while(true)
            {
                bool isDone = true;
                for (int i = 0; i < camCount; i++)
                {
                    isDone = isDone && doneCompute[i];
                }
                if (isDone) 
                    break;
                yield return null;
            }
            blobCount = 0;
            for (int i = 0; i < camCount; i++)
            {
                blobCount += trackedBlobsCount[i];
                doneCompute[i] = false;
            }
            yield return null;
        }
        yield return null;
    }

    private IEnumerator RefreshBlobList(int idx)
    {
        while(absRunning[idx])
        {
            while(!tracker.Freshblobs[idx])
            {
                yield return null;
            }
            ComputeNewBlobs(idx);
            yield return null;
        }
        yield return null;
    }

    private void ComputeNewBlobs(int idx)
    {
        doneCompute[idx] = true;
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
}
