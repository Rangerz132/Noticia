using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target_elements : MonoBehaviour
{
    public static bool DetectFish = false;
    public static bool DetectGarbage = false;
    public float raylength;
    public LayerMask layermask;

    void Start()
    {
        
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


            if (Physics.Raycast(ray, out hit, raylength, layermask))
            {

                if (hit.transform.gameObject.tag == "Fish")
                {
                    DetectFish = true;
                    Debug.Log("poisson");
                }

                if (hit.transform.gameObject.tag == "Garbage")
                {
                    DetectGarbage = true;
                    Debug.Log("déchet");
                }


            }
        }

    }
}
