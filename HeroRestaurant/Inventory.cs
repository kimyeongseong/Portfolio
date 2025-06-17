using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SimpleDatabase;

[System.Serializable]
public class FoodUpdateEvent : UnityEvent<Food>
{
}

[System.Serializable]
public class MoneyEvent : UnityEvent<int>
{
}

public class Inventory : Singleton<Inventory>, ISavable {
    [SerializeField]
    private int initialMoney = 0;

    private Dictionary<string, Food> foodDic = new Dictionary<string, Food>();

    public FoodUpdateEvent onFoodAmountUpdated  = new FoodUpdateEvent();
    public MoneyEvent      onMoneyAmountUpdated = new MoneyEvent();

    public int CurrentMoney { get; private set; }

    private void Awake()
    {
        CurrentMoney = initialMoney;

        FoodData[] foodDatas = Database.Instance.Select<FoodData>("FoodDataTable").Rows;
        foreach (var foodData in foodDatas)
            foodDic.Add(foodData.name, new Food(foodData, 0));
    }

    public void IncreaseMoney(int value)
    {
        CurrentMoney = Mathf.Clamp(CurrentMoney + value, 0, int.MaxValue);
        onMoneyAmountUpdated.Invoke(CurrentMoney);
    }

    public void IncreaseFoodAmount(string foodName, int amount = 1)
    {
        Debug.Assert(foodDic.ContainsKey(foodName), $"Inventory::IncreaseFoodAmount({foodName}, {amount}) - food Not Exist");

        Food food = foodDic[foodName];
        food.IncreaseAmount(amount);

        onFoodAmountUpdated.Invoke(food);
    }

    public int GetFoodAmount(string foodName)
    {
        Debug.Assert(foodDic.ContainsKey(foodName), $"Inventory::IncreaseFoodAmount({foodName}) - food Not Exist");

        return foodDic[foodName].Amount;
    }

    public Food GetFood(string foodName)
    {
        Debug.Assert(foodDic.ContainsKey(foodName), $"Inventory::GetFood({foodName}) - food Not Exist");

        return foodDic[foodName];
    }

    public void ClearAllFoods()
    {
        foreach (var foodPair in foodDic)
        {
            foodPair.Value.IncreaseAmount(-foodPair.Value.Amount);
        }
    }

    public JSONObject SaveToJson()
    {
        var jsonObject = new JSONObject(JSONObject.Type.OBJECT);
        jsonObject.AddField("money", CurrentMoney);

        jsonObject.AddField("foods", new JSONObject(JSONObject.Type.OBJECT));
        foreach (var keyPair in foodDic)
        {
            jsonObject["foods"].SetField(keyPair.Key, keyPair.Value.SaveToJson());
        }

        return jsonObject;
    }

    public void LoadFromJson(JSONObject root)
    {
        CurrentMoney = (int)root["money"].i;
        onMoneyAmountUpdated.Invoke(CurrentMoney);

        foreach (var keyPair in foodDic)
        {
            var foodJson = root["foods"][keyPair.Key];
            keyPair.Value.LoadFromJson(foodJson);
        }
    }
}