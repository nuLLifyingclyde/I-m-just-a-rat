using UnityEngine;

public class ClimbActivator : MonoBehaviour
{
    public Climb cl;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cl.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            cl.enabled = true;

        }
    }
}
