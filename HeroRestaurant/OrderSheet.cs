using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OerderState
{
    Waiting,
    Cancel,
    Served
}

public class OrderSheet : MonoBehaviour {
    [SerializeField]
    private SpriteRenderer previewRenderer = null;

    private bool isFoodServed = false;

    public Food OrderedFood { get; private set; }

    public Customer    Customer          { get; private set; }
    public Floor       Floor             { get; private set; }
    public OerderState CurrentOrderState { get; private set; }

    public void Order(Customer customer, Floor floor, Food orderedFood)
    {
        OrderedFood = orderedFood;

        previewRenderer.sprite = SpriteAtlasUtility.GetSprite("Atlas/FoodAtlas", orderedFood.FoodData.spritePath);

        Customer = customer;
        Floor    = floor;

        Floor.GetComponent<BusinessSystem>().AddOrderSheet(this);

        ChangeState(OerderState.Waiting);
    }

    public void Cancel()
    {
        Floor.GetComponent<BusinessSystem>().RemoveOrderSheet(this);
        ChangeState(OerderState.Cancel);
    }

    public void OnClicked(GameObject target)
    {
        if (Inventory.Instance.GetFoodAmount(OrderedFood.FoodData.name) > 0)
        {
            Inventory.Instance.IncreaseFoodAmount(OrderedFood.FoodData.name, -1);
            Floor.GetComponent<BusinessSystem>().RemoveOrderSheet(this);

            Customer.TakeFood();
            ChangeState(OerderState.Served);
        }
    }

    public void ChangeState(OerderState newState)
    {
        CurrentOrderState = newState;
        
        switch (CurrentOrderState)
        {
            case OerderState.Waiting:
                gameObject.SetActive(true);
                break;

            case OerderState.Cancel:
            case OerderState.Served:
                gameObject.SetActive(false);
                break;
        }
    }
}
