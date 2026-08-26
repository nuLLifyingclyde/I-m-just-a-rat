using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CarSummonScript : MonoBehaviour
{   
    public GameObject[] CarPrefab;
    //public float spawnDelay;

    public float minSpawnDelay = 0.5f;
    public float maxSpawnDelay = 3f;

    public CarPool carPool;

    private float nextSpawnTime;
    int randomInt;
   
    // Start is called before the first frame update
    void Start()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnDelay, maxSpawnDelay);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            randomInt = Random.Range(0,3);
            //Instantiate(CarPrefab[randomInt], transform.position, transform.rotation);
            carPool.SpawnObject(CarPrefab[randomInt], transform.position, transform.rotation);
            
            if (carPool != null)
            {
                nextSpawnTime = Time.time + Random.Range(minSpawnDelay, maxSpawnDelay);
            }
        }
    }
}
