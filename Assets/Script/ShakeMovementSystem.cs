using UnityEngine;

public class ShakeMovementSystem : MonoBehaviour
{
    public float movespeed = 5f;
    public float shakeThreshold = 15f;
    public CharacterController controller;
    public float direction = 1f;

    //declare variables of the movement speed, the shake needed to move, the character controller and the direction being right
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        //find character controller
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            direction *= -1f;
            //pressing left mouse will switch mouse direction
        }

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        float mousespeed = new Vector2(mouseX, mouseY).magnitude / Time.deltaTime;

        //find input and axis of the mouse

        if (mousespeed > shakeThreshold)
            //if the movespeed is higher than the shake threshold
        {
            Vector3 moveDir = Vector3.right * direction * movespeed * Time.deltaTime;
            //movement code, move the mouse forward in the direction with the movespeed alongside a consistent framerate

            if (controller != null)
                //if there is a controller
            {
                controller.Move(moveDir);
                //move the mouse
            }
            else
            {
                transform.Translate(Vector3.right * direction * movespeed * Time.deltaTime);
                //move without charactercontroller
            }  
        }
    }
}
