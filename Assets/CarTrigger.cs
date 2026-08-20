using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CarMovement : MonoBehaviour
{

    public Transform RatTransform;    
    public GameObject Car;            
    public float triggerDistance = 10f; 

    private bool hasTriggered = false;
   
    void Start()
    {
        if (Car != null)
        {
            Car.SetActive(false);
        }

        else
        {
            Debug.LogError("no car in here dumbass");
        }

        if (RatTransform == null)
        {
            Debug.LogError("rat is not here dumbass");
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasTriggered && RatTransform != null && Car != null)
        {
            float distance = Vector3.Distance(transform.position, RatTransform.position);

            if (distance <= triggerDistance)
            {
                Car.SetActive(true);
                hasTriggered = true;

                Debug.Log("Car appear");
            }
        }



    }

}
            

    

   