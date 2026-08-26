
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] public float jumpForce = 5f;
    [SerializeField] public float ValueofGravity = -25f;


    private CharacterController ratController;
    public bool GroundedPlayer;
    private Vector3 playerVelocity;


    // Start is called before the first frame update
    void Start()
    {
        ratController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        GroundedPlayer = ratController.isGrounded;

        if (GroundedPlayer)
        {
            playerVelocity.y = -2f;
        }

        if (Input.GetMouseButtonDown(0) && GroundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(jumpForce * -2.0f * ValueofGravity);

        }

        playerVelocity.y += ValueofGravity * Time.deltaTime;

        ratController.Move(playerVelocity * Time.deltaTime);


    }
}

  


