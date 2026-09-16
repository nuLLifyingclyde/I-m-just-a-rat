using UnityEngine;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;

public class FoodManager : MonoBehaviour
{
    public static FoodManager instance;
    public int foodcount;
    public TMP_Text FoodText;


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void Start()
    {
        UpdateFoodUI();
        
    }
    public void AddFood(int amount)
    {

        foodcount += amount;
        UpdateFoodUI();



        Debug.Log("Food: " + foodcount);
    }

    public void RemoveFood(int amount)
    {
        foodcount -= amount;
        UpdateFoodUI();


        

        Debug.Log("Food: " + foodcount);
    }

    public void UpdateFoodUI()
    {
        FoodText.text = foodcount.ToString() + "Foodcount";
    }
}