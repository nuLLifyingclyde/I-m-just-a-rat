using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CarSummonScript : MonoBehaviour
{

    public GameObject CarPrefab;
   
    public float spawnDelay;
    
   public float nextSpawnTime;

    // Start is called before the first frame update
    void Start()
    {

        nextSpawnTime = Time.time + spawnDelay;

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            Instantiate(CarPrefab);
           
        }

        nextSpawnTime = Time.time + spawnDelay;
    }

  
     
}
