using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleDatabase;

public class Food : ISavable  {
    private FoodData foodData;

    public FoodData FoodData
    {
        get
        {
            FoodData readonlyData = foodData;
            return readonlyData;
        }
    }

    public int  Amount        { get; private set; } = 1;
    public int  Satisfaction { get; private set; } = 0;
    public bool IsEmpty      { get { return Amount == 0; } }

    public Food(string foodName)
    {
        foodData = Database.Instance.Select<FoodData>("FoodDataTable").Select(foodName);
    }

    public Food(FoodData data, int amount = 1)
    {
        foodData = data;
        Amount   = amount;
    }

    public void Clear()
    {
        Amount = 0;
    }

    public void IncreaseAmount(int value)
    {
        int newAmount = (int)Amount + value;

        Debug.Assert(newAmount >= 0, $"Food({foodData.name})::IncreaseAmount - Amount can not be negative");

        Amount = newAmount;
    }

    public void IncreaseSatisfaction()
    {
        Satisfaction = Mathf.Clamp(Satisfaction + foodData.satisfactionPerSale, 0, 100);
    }

    public JSONObject SaveToJson()
    {
        JSONObject root = new JSONObject(JSONObject.Type.OBJECT);
        root.SetField("satisfaction", Satisfaction);

        return root;
    }

    public void LoadFromJson(JSONObject json)
    {
        Satisfaction = (int)json["satisfaction"].i;
    }
}
