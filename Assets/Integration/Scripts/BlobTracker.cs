using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobTracker : MonoBehaviour
{

    private WebcamHandle camHandle;
    private OpenCVBlobTracker tracker;

    private void Awake()
    {
        camHandle = GetComponentInChildren<WebcamHandle>();
        tracker = GetComponentInChildren<OpenCVBlobTracker>();
    }

    private void Start()
    {
        bool initiated= camHandle.InitiateWebcamHandler();
       
        if (initiated)
            StartCoroutine(WaitForwebcams());
    }

    private IEnumerator WaitForwebcams()
    {
        while(!camHandle.SystemReady)
        {
            yield return null;
        }
        StartCoroutine(WaitForwebcams32());
        yield return null;
    }


    private IEnumerator WaitForwebcams32()
    {
        camHandle.StartCapture(1);
        while(!camHandle.FramesUpdated[0])
        {
            yield return null;
        }
        bool sucess= tracker.GrabWebcamsReferences(ref camHandle);
        if(sucess)
            tracker.StartDetectionRoutines();
        yield return null;
    }

}
