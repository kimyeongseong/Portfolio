using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Pathfinding;

[System.Serializable]
public class FloorSystemEvent : UnityEvent<Floor>
{
}

public class FloorSystem : Singleton<FloorSystem>, ISavable {
    [SerializeField, Required]
    private GameObject floorPrefab = null;
    [SerializeField, Required]
    private Transform  floorShowingCameraTransform = null;
    [SerializeField]
    private Vector2Int resolutionSize           = Vector2Int.zero;
    [SerializeField]
    private float      floorTransitionTime      = 0f;
    [SerializeField]
    private int        maxNumOfFloor            = 0;
    [SerializeField]
    private int        initialNumOfOpendFloor   = 0;
    [SerializeField]
    private int        numOfShowingPreviewFloor = 0;

    private List<Floor> floors              = null;

    public FloorSystemEvent onShowingDifferentFloor = new FloorSystemEvent();
    public FloorSystemEvent onBuyFloor              = new FloorSystemEvent();

    private int     currentShowingFloorIndex   = 0;
    private int     numOfOpendFloors           = 0;
    private Vector2 pixelPerfectResolutionSize = Vector2.zero;
    private bool    isMovingNextFloor          = false;

    public bool  IsCanBuyNextFloor   { get; private set; }
    public bool  IsCanShowNextFloor  { get; set; } = true;
    public Floor NextBuyableFloor    { get { return floors[numOfOpendFloors]; } }
    public Floor CurrentShowingFloor { get; private set; }

    private void Awake()
    {
        floors = new List<Floor>(maxNumOfFloor);
        pixelPerfectResolutionSize = (Vector2)resolutionSize * 0.01f;
        numOfOpendFloors = initialNumOfOpendFloor;

        for (int i = 0; i < initialNumOfOpendFloor; i++)
        {
            var floor = CreateFloor();
            floor.Buy(null);
        }

        for (int i = 0; i < numOfShowingPreviewFloor; i++)
            CreateFloor();

        if (floors.Count > 0)
            SelectFloor(0);
    }

    private void Start()
    {
        for (int i = 0; i < numOfOpendFloors; i++)
            floors[i].Setup(CreateGridGraph(i));
    }

    private Floor CreateFloor()
    {
        var floorObj = Instantiate(floorPrefab, transform);
        var floor    = floorObj.GetComponent<Floor>();

        Debug.Assert(floor != null, "FloorSystem::Start - Floor component not exist");

        floor.transform.localPosition = new Vector3(0f, floors.Count * -pixelPerfectResolutionSize.y);
        floor.InteriorTileMap.onBoughtAllTiles.AddListener((item) => IsCanBuyNextFloor = true);

        floors.Add(floor);

        return floor;
    }

    private GridGraph CreateGridGraph(int index)
    {
        var firstGridGraph = (GridGraph)AstarPath.active.graphs[0];

        if (index == 0)
            return firstGridGraph;

        var newGridGraph = (GridGraph)AstarPath.active.data.AddGraph(typeof(GridGraph));
        newGridGraph.center = firstGridGraph.center - new Vector3(0f, pixelPerfectResolutionSize.y * index, 0f);
        newGridGraph.rotation = firstGridGraph.rotation;
        newGridGraph.SetDimensions(firstGridGraph.width, firstGridGraph.depth, firstGridGraph.nodeSize);
        newGridGraph.neighbours = firstGridGraph.neighbours;
        newGridGraph.collision.use2D = firstGridGraph.collision.use2D;
        newGridGraph.collision.mask = firstGridGraph.collision.mask;
        newGridGraph.collision.type = firstGridGraph.collision.type;

        return newGridGraph;
    }

    private void SelectFloor(int index)
    {
        StartCoroutine("MoveTo");

        CurrentShowingFloor = floors[index];

        if (index >= numOfOpendFloors)
            InteriorSystem.Instance.InteriorTargetFloor = null;
        else
            InteriorSystem.Instance.InteriorTargetFloor = CurrentShowingFloor;

        onShowingDifferentFloor.Invoke(CurrentShowingFloor);
    }

    private IEnumerator MoveTo()
    {
        isMovingNextFloor = true;

        Vector3 startPosition = floorShowingCameraTransform.position;
        Vector3 arrivePosition = new Vector3(startPosition.x, -currentShowingFloorIndex * pixelPerfectResolutionSize.y, startPosition.z);

        float currentTime = 0f;
        float timePoint = 0f;

        while (timePoint < 1f)
        {
            currentTime += Time.smoothDeltaTime;
            timePoint = currentTime / floorTransitionTime;

            floorShowingCameraTransform.position = Vector3.Lerp(startPosition, arrivePosition, timePoint);
            yield return null;
        }

        isMovingNextFloor = false;
    }

    public void ShowNextFloor()
    {
        if (!isMovingNextFloor && IsCanShowNextFloor)
        {
            currentShowingFloorIndex = ++currentShowingFloorIndex % floors.Count;
            SelectFloor(currentShowingFloorIndex);
        }
    }

    public void ShowPrevFloor()
    {
        if (!isMovingNextFloor && IsCanShowNextFloor)
        {
            if (--currentShowingFloorIndex < 0)
                currentShowingFloorIndex = floors.Count - 1;

            SelectFloor(currentShowingFloorIndex);
        }
    }

    public void ShowInteriorMapInOpendFloors(bool isShow)
    {
        for (int i = 0; i < numOfOpendFloors; i++)
            floors[i].ShowInteriorMap(isShow);
    }

    public void BuyNextFloor()
    {
        if (IsCanBuyNextFloor)
        {
            IsCanBuyNextFloor   = false;

            CurrentShowingFloor.Setup(CreateGridGraph(numOfOpendFloors));
            CurrentShowingFloor.Buy(gameObject);

            numOfOpendFloors++;

            SelectFloor(numOfOpendFloors - 1);

            onBuyFloor.Invoke(CurrentShowingFloor);

            if (numOfOpendFloors != maxNumOfFloor)
                CreateFloor();
            ;
        }
    }

    public JSONObject SaveToJson()
    {
        var jsonObject = new JSONObject(JSONObject.Type.OBJECT);
        jsonObject.AddField("numOfOpendFloors", numOfOpendFloors);
        jsonObject.AddField("isCanBuyNextFloor", IsCanBuyNextFloor);
        jsonObject.AddField("floors", new JSONObject(JSONObject.Type.ARRAY));
        jsonObject.AddField("businessSystems", new JSONObject(JSONObject.Type.ARRAY));

        foreach (var floor in floors)
        {
            if (floor.CurrentState == FloorState.Bought)
            {
                jsonObject["floors"].Add(floor.SaveToJson());

                var businessSystem = floor.GetComponent<BusinessSystem>();
                jsonObject["businessSystems"].Add(businessSystem.SaveToJson());
            }
        }

        return jsonObject;
    }

    public void LoadFromJson(JSONObject jsonObject)
    {
        numOfOpendFloors  = (int)jsonObject["numOfOpendFloors"].i;
        IsCanBuyNextFloor = jsonObject["isCanBuyNextFloor"].b;

        for (int i = 0; i < jsonObject["floors"].Count; i++)
        {
            if (i > 0)
                CreateFloor();

            floors[i].LoadFromJson(jsonObject["floors"][i]);

            var graphs = AstarPath.active.graphs;
            if (i >= graphs.Length)
                floors[i].Setup(CreateGridGraph(i));
            else
                floors[i].Setup((GridGraph)graphs[i]);

            var businessSystem = floors[i].GetComponent<BusinessSystem>();
            businessSystem.LoadFromJson(jsonObject["businessSystems"][i]);
        }

        AstarPath.active.Scan(AstarPath.active.graphs);
    }
}
