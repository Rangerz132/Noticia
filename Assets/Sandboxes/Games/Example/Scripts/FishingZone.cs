using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingZone : MonoBehaviour
{
    // Start is called before the first frame update

    bool grabbedObject;
    Vector2 pos; 
    void Start()
    {
        grabbedObject = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Fish" || other.tag == "Garbage" && !grabbedObject)
        {
            Grabbable gb = other.gameObject.GetComponent<Grabbable>();
            grabbedObject = true;
            Debug.Log("entered");
            gb.isActive = false;
            gb.followedObject = this.gameObject;
        }
    }
}
