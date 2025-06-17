using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleDatabase;

public class FoodMenu  {
    private FoodData[] foodDatas = null;

    public FoodMenu()
    {
        foodDatas = Database.Instance.Select<FoodData>("FoodDataTable").Rows;
    }

    public Food GetOrderableFoodByRandom()
    {
        int randomIndex = Random.Range(0, foodDatas.Length);
        var food = Inventory.Instance.GetFood(foodDatas[randomIndex].name);

        return food;
    }
}
