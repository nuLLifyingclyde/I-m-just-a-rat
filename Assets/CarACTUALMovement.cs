using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarACTUALMovement : MonoBehaviour
{
    public float carspeed = 5f;
    public string RatTag = "Rat";
    public GameObject rat;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0, 0, carspeed);



    }

    
}
