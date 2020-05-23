using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hologramme_Emitter : MonoBehaviour
{

    public GameObject Falling_Fish;
    public GameObject Falling_Garbage;
    public Vector3 SpawnValues;
    public float spawnPosition;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TriggeredElement();
    }


    void TriggeredElement() {

        if (Target_elements.DetectFish == true) {
            Vector3 spawnPosition = new Vector3(Random.Range(-SpawnValues.x, SpawnValues.x), 1,1);
            Instantiate(Falling_Fish, spawnPosition, Quaternion.identity);
            Target_elements.DetectFish = false;
        }

        if (Target_elements.DetectGarbage == true)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-SpawnValues.x, SpawnValues.x), 1, 1);
            Instantiate(Falling_Garbage, spawnPosition, Quaternion.identity);
            Target_elements.DetectGarbage = false;
        }


    }
}


