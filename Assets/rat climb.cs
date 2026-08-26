
using UnityEngine;



public class LadderMovement : MonoBehaviour

{

    private float vertical;

   private float speed = 8f;

    private bool isLadder;

    private bool isClimbing;



    [SerializeField] private Rigidbody rb;



    void Update()

    {

        vertical = Input.GetAxisRaw("Vertical");



        if (isLadder && Mathf.Abs(vertical) > 0f)

        {

            isClimbing = true;

        }

    }



    private void FixedUpdate()

    {

        if (isClimbing)

        {

            rb.useGravity = false;

            rb.velocity = new Vector2(rb.velocity.x, vertical * speed);

        }

        else

        {

            rb.useGravity = true;

        }

    }



    private void OnTriggerEnter(Collider collision)

    {

        if (collision.CompareTag("Ladder"))

        {

            isLadder = true;

        }

    }



    private void OnTriggerExit(Collider collision)

    {

        if (collision.CompareTag("Ladder"))

        {

            isLadder = false;

            isClimbing = false;

        }

    }

}

