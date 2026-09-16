using System.Collections; 
using System.Collections.Generic; 
using Unity.VisualScripting; 
using UnityEngine; 

public class Climb : MonoBehaviour 
{ 
    [SerializeField] private float climbspeed = 5f; 
    private bool isLadder; 
    private bool isClimbing; 
    private float vertical; 
    [SerializeField] private Rigidbody rb; 
    [SerializeField] private string LadderTag = "Ladder"; 
    [SerializeField] private MouseDirectionController movementController;
    
    private void Update() 
    { 
        vertical = Input.GetAxisRaw("Vertical"); 
        if (isLadder && Input.GetMouseButtonDown(2)) //&& Mathf.Abs(vertical) > 0f 
        { 
            isClimbing = true; 
            movementController.movementLocked = true;
        } 

        if (!isLadder) //isClimbing && 
        {
            isClimbing = false;
        }
    } 
    
    private void FixedUpdate() 
    { 
        if (isClimbing) 
        { 
            rb.useGravity = false; 
            transform.Translate(0, climbspeed, 0); 
            rb.linearVelocity = new Vector3(rb.linearVelocity.y, vertical * climbspeed); 
        }
        
        else 
        { 
            rb.useGravity = true; 
        } 
    } 
    
    private void OnTriggerEnter (Collider collision) 
    { 
        if (collision.CompareTag("Ladder")) 
        { 
            //isClimbing = true; 
            isLadder = true; 
        } 
    } 
    
    private void OnTriggerExit(Collider collision) 
    { 
        if (collision.CompareTag("Ladder")) 
        { 
            isClimbing = false; 
            isLadder = false; 

            movementController.movementLocked = false;
        } 
    } 
}