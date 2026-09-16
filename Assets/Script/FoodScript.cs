using UnityEngine;

public class FoodScript : MonoBehaviour
/*{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.GetComponent<CharacterController>())
        {
            Destroy(gameObject);
            Debug.Log("Food Collected");
        }
    }
}*/

{
    public int foodAmount = 1;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.GetComponent<CharacterController>())
        {
            //food count +1 before disappear
            FoodManager fm = FindFirstObjectByType<FoodManager>();

            if (fm != null)
            {
                fm.AddFood(foodAmount);
               
            }

            Destroy(gameObject);
            Debug.Log("Food Collected");
        }
    }
}
