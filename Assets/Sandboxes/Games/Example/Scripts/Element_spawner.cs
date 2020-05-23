using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Element_spawner : MonoBehaviour
{

    public GameObject[] DragElements;
    public Vector3 spawnValues;
    public float spanWait;
    public float SpawnMostWait;
    public float spawnLeastWait;
    public int startWait;
    public bool stopSpawn;
    int randDragElements;




    void Start()
    {
        StartCoroutine(waitSpawner());
    }


    void Update()
    {
        spanWait = Random.Range(spawnLeastWait, SpawnMostWait);
    }


    IEnumerator waitSpawner() {
        yield return new WaitForSeconds(startWait);

        while (!stopSpawn) {

            randDragElements = Random.Range(0, 2);

            Vector3 spawnPosition = new Vector3(Random.Range(-spawnValues.x, spawnValues.x), Random.Range(-spawnValues.y, spawnValues.y), 1);
            Instantiate(DragElements[randDragElements], spawnPosition + transform.TransformPoint(0, 0, 0), gameObject.transform.rotation);
            yield return new WaitForSeconds(spanWait);
        }
    }
}
