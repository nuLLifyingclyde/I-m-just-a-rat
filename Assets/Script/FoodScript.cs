using Unity.VisualScripting;
using UnityEngine;

public class FoodScript : MonoBehaviour

{
    public int foodAmount = 1;
    public GameObject gameObject;
    public MouseDirectionController mdc;
    public float multiplierboost = 1f ;
    public float durationBoost = 1f; 
   

    private void OnTriggerEnter(Collider collision)
    {
        CharacterController charController = collision.GetComponent<CharacterController>();
        if (charController != null)
        {
            if (mdc == null)
            {
                mdc = charController.GetComponent<MouseDirectionController>();
                
            }


           
            
                //food count +1 before disappear
                FoodManager fm = FindFirstObjectByType<FoodManager>();

                if (fm != null)
                {
                    fm.AddFood(foodAmount);
                }

                if (mdc != null)
            {
                mdc.shakeSpeedMultiplier += multiplierboost;
                mdc.AddFoodBoost(durationBoost, multiplierboost);
               
            }

                Destroy(gameObject);







            
        }
    }
}
