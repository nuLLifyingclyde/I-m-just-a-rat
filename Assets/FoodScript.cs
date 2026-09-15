using UnityEngine;

public class FoodScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.GetComponent<CharacterController>())
        {
            Destroy(gameObject);
            Debug.Log("Food Collected");
        }
    }
}
