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
    
    // Start is called before the first frame update 
    void Start()
    {
        
    }
    // Update is called once per frame 
    
    void Update() 
    { 
        vertical = Input.GetAxisRaw("Vertical"); 
        if (isLadder && Input.GetMouseButtonDown(1) && Mathf.Abs(vertical) > 0f) 
        { 
            isClimbing = true; 
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
            isClimbing = true; 
            isLadder = true; 
        } 
    } 
    
    private void OnTriggerExit(Collider collision) 
    { 
        if (collision.CompareTag("Ladder")) 
        { 
            isClimbing = false; 
            isLadder = false; 
        } 
    } 
}