using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    //
    public bool isActive; //Will only be true if the object can be interacted with (Will be used for animations)
    


    public GameObject followedObject; //Used for grab and animation


    // Méthode
    void Start()
    {
        isActive = false;
    }
    void Update()
    {
       if(isActive)
       {

       } 
       else
       {
           transform.position = followedObject.transform.position;
       }
    }

    public void setGrabOn() //SetupBlobData & pos;
    {
        isActive = false;
    }

    public void setGrabOff()
    {
        isActive = true;
        //TODO del ref
    }
}
