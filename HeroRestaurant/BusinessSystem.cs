using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusinessSystem : MonoBehaviour, ISavable {
    [SerializeField]
    private int       maxNumOfWaitingStaff = 0;
    [SerializeField]
    private Transform waiterWaitingPoint   = null;
    [SerializeField]
    private Vector3   waitingPointOffset   = Vector2.zero;

    private HashSet<WaitingStaff> hiredWaitingStaffs = new HashSet<WaitingStaff>();
    private HashSet<OrderSheet>   orderSheets        = new HashSet<OrderSheet>();
    private HashSet<Customer>     customers          = new HashSet<Customer>();

    public FoodMenu FoodMenu { get; private set; }

    public int  CustomerCount          { get { return customers.Count; } }
    public bool IsWaitingStaffHireable { get { return hiredWaitingStaffs.Count < maxNumOfWaitingStaff; } }

    private void Awake()
    {
        FoodMenu = new FoodMenu();
    }

    private void Update()
    {
        foreach (var waitingStaff in hiredWaitingStaffs)
        {
            if (waitingStaff.CurrentState == WaitingStaffState.WaitingOrder)
            {
                foreach (var orderSheet in orderSheets)
                {
                    var foodAmount = Inventory.Instance.GetFoodAmount(orderSheet.OrderedFood.FoodData.name);
                    if (foodAmount > 0)
                    {
                        waitingStaff.TakeOrder(orderSheet);
                        orderSheets.Remove(orderSheet);
                        break;
                    }
                }
            }
        }
    }

    public void HireWaitingStaff(WaitingStaff newWaitingStaff)
    {
        newWaitingStaff.transform.parent = transform;
        newWaitingStaff.transform.position = waiterWaitingPoint.position + (waitingPointOffset * hiredWaitingStaffs.Count);
        newWaitingStaff.WaitingPoint = newWaitingStaff.transform.position;

        hiredWaitingStaffs.Add(newWaitingStaff);
    }

    public void AddOrderSheet(OrderSheet orderSheet)
    {
        orderSheets.Add(orderSheet);
    }

    public void RemoveOrderSheet(OrderSheet orderSheet)
    {
        orderSheets.Remove(orderSheet);
    }

    public void AddCustomer(Customer customer)
    {
        customers.Add(customer);
    }

    public void RemoveCustomer(Customer customer)
    {
        customers.Remove(customer);
    }

    public JSONObject SaveToJson()
    {
        var jsonObject = new JSONObject(JSONObject.Type.OBJECT);
        var hiredWaitingStaffsJson = new JSONObject(JSONObject.Type.ARRAY);

        jsonObject.AddField("hiredWaitingStaffs", hiredWaitingStaffsJson);
        foreach (var hiredWaitingStaff in hiredWaitingStaffs)
        {
            hiredWaitingStaffsJson.Add(hiredWaitingStaff.WaitingStaffData.name);
        }

        return jsonObject;
    }

    public void LoadFromJson(JSONObject jsonObject)
    {
        var hiredWaitingStaffsJson = jsonObject["hiredWaitingStaffs"];
        for (int i = 0; i < hiredWaitingStaffsJson.Count; i++)
        {
            var waitingStaff = Resources.Load<GameObject>("NPC/WaitingStaff/" + hiredWaitingStaffsJson[i].str);
            waitingStaff = Instantiate(waitingStaff, transform);
            HireWaitingStaff(waitingStaff.GetComponent<WaitingStaff>());
        }
    }
}
