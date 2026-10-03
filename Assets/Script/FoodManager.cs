using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;

public class FoodManager : MonoBehaviour
{
    public static FoodManager instance;
    public TMP_Text FoodText;
    public Image Fillbar;
    public float DisplayBarFillAmount;
    public float BarFillSpeed = 1f;
    public int foodcount;
    public int maxfood = 0;
    public int minfood = 50;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void Start()
    {
        UpdateFoodUI();
    }

    public void Update()
    {
        Bar();
    }

    public void AddFood(int amount)
    {
        foodcount += amount;
        UpdateFoodUI();

        Debug.Log("Food: " + foodcount);
    }

    public void RemoveFood(int amount)
    {
        foodcount = Mathf.Max(0, foodcount - amount);
        UpdateFoodUI();
        
        Debug.Log("Food: " + foodcount);
    }

    public void UpdateFoodUI()
    {
        FoodText.text = foodcount.ToString();
    }

    public void Bar()
    {
        float fillTarget = (maxfood > 0f) ? Mathf.Clamp01((float)foodcount/maxfood) : 0f;

        DisplayBarFillAmount = Mathf.MoveTowards(DisplayBarFillAmount, fillTarget, BarFillSpeed * Time.deltaTime);

        if (Fillbar != null)
        {
            Fillbar.fillAmount = DisplayBarFillAmount;
        }
    }
}