using UnityEngine;

public class DoorOpen : MonoBehaviour
{

    public float timer;
    public Vector3 rotation;
    public Vector3 originalposition;
    public Quaternion originalrotation;
    public float speed;
    public bool isRotating = true;
  

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
        originalposition = transform.position;
        originalrotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
       
        if (isRotating)
        {
            timer += Time.deltaTime;
            transform.Rotate(rotation = Vector3.up * speed * Time.deltaTime);
        }

        if (timer > 5)
        {
            PauseRotation();
        }

        
    }

  
   public void PauseRotation()

    {
        isRotating = false;
        transform.position = originalposition;
        transform.rotation = originalrotation;
    }
}
