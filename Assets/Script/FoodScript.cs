using NUnit.Framework.Internal.Filters;
using Unity.VisualScripting;
using UnityEngine;

public class FoodScript : MonoBehaviour

{
    public FoodManager fm;
    public int foodAmount = 1;
    public GameObject gameObject;
    public MouseDirectionController mdc;
    public float multiplierboost = 1f;
    public float durationBoost = 1f;

    public void OnTriggerEnter(Collider collision)
    {

        CharacterController charController = collision.GetComponent<CharacterController>();

        //Declare charactercontroller as something and find collision as character controller
        if (charController != null)
        //if there is a character controller
        {

            MouseDirectionController mdc = charController.GetComponent<MouseDirectionController>();
            //The movement code is declared it equals to the character controller finding the movement code
            if (mdc != null)
            {
                //if there is the movement code

               mdc.AddFoodBoost(durationBoost, multiplierboost);
                float fillTarget = (foodAmount > 0f) ? Mathf.Clamp01((float)foodAmount) : 0f;

                mdc.displayFilledAmount = Mathf.MoveTowards(mdc.displayFilledAmount, fillTarget, mdc.barFillSpeed * Time.deltaTime);
                if (mdc.BoostBar != null)
                {

                    mdc.BoostBar.fillAmount = mdc.displayFilledAmount;
                }
                //and add the boost from the AddFoodBoost code that has the duration and multiplier boost
            }


            FoodManager fm = FindFirstObjectByType<FoodManager>();

            if (fm != null)
            {
                fm.AddFood(foodAmount);    
            }

            Destroy(gameObject);

        }
    }
}

