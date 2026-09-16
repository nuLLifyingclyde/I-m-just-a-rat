using UnityEngine;

public class FoodManager : MonoBehaviour
{
    public int foodcount = 0;
    public void AddFood(int amount)
    {
        foodcount += amount;
        Debug.Log("Food: " + foodcount);
    }

    public void RemoveFood(int amount)
    {
        foodcount -= amount;
        foodcount = Mathf.Max(foodcount, 0);

        Debug.Log("Food: " + foodcount);
    }
}