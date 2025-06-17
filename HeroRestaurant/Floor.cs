using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Pathfinding;

public enum FloorState
{
    NotBuy,
    Bought
}

[System.Serializable]
public class FurnitureListEvent : UnityEvent<Furniture[]>
{
}

public class Floor : MonoBehaviour, IBuyable, ISavable {
    [SerializeField]
    private InteriorTileMap2D interiorTileMap = null;
    [SerializeField]
    private InteriorTileMap2D doorTileMap     = null;
    [SerializeField]
    private QuestBoard        questBoard      = null;
    [SerializeField]
    private Counter           counter         = null;

    private HashSet<Furniture> arrangedFurniture = new HashSet<Furniture>();
    private GridGraph          gridGraph         = null;

    public FurnitureListEvent onFurnitureListUpdate = new FurnitureListEvent();

    public InteriorTileMap2D InteriorTileMap { get { return interiorTileMap; } }
    public InteriorTileMap2D DoorTileMap     { get { return doorTileMap;     } }
    public QuestBoard        QuestBoard      { get { return questBoard;      } }
    public Counter           Counter         { get { return counter;         } }

    public FloorState CurrentState { get; private set; }

    private void Start()
    {
        if(CurrentState != FloorState.Bought)
            ShowInteriorMap(false);
    }

    public void Buy(GameObject buyer)
    {
        CurrentState    = FloorState.Bought;
        ShowInteriorMap(true);

        GameMode.Instance.onBusinessModeStarted.AddListener(() => SetArragnedFurnituresInteractionEnable(false));
        GameMode.Instance.onEditorModeStarted.AddListener(() => SetArragnedFurnituresInteractionEnable(true));
    }

    public void InsertFurniture(Furniture furniture)
    {
        arrangedFurniture.Add(furniture);
        furniture.transform.parent = transform;

        onFurnitureListUpdate.Invoke(arrangedFurniture.ToArray());
    }

    public void RemoveFurniture(Furniture furniture)
    {
        arrangedFurniture.Remove(furniture);
        furniture.transform.parent = null;

        onFurnitureListUpdate.Invoke(arrangedFurniture.ToArray());
    }

    public void Setup(GridGraph gridGraph)
    {
        this.gridGraph = gridGraph;
    }

    public FurnitureSearchInfo<Furniture>[] SearchhSurroundingFurniture(Furniture aroundTarget)
    {
        return SearchSurroundingFurniture<Furniture>(aroundTarget);
    }

    public FurnitureSearchInfo<T>[] SearchSurroundingFurniture<T>(Furniture aroundTarget) where T : Furniture
    {
        if (!arrangedFurniture.Contains(aroundTarget))
            return null;

        List<FurnitureSearchInfo<T>> furnitureSearchInfos = new List<FurnitureSearchInfo<T>>();
        Vector2Int mapStartIndex = aroundTarget.MapStartIndex;
        Vector2Int searchEndIndex = mapStartIndex + aroundTarget.GetComponent<FurnitureController>().MarkTileMap.MapSize;

        int topIndex = mapStartIndex.y - 1;
        if (topIndex >= 0)
        {
            for (int row = mapStartIndex.x; row < searchEndIndex.x; row++)
            {
                var furniture = interiorTileMap[topIndex, row].LinkedFurniture;
                if (furniture != null && furniture is T)
                {
                    furnitureSearchInfos.Add(new FurnitureSearchInfo<T>(Direction.Up, (T)furniture));
                }
            }
        }

        int bottomIndex = searchEndIndex.y;
        if (bottomIndex < interiorTileMap.MapSize.y)
        {
            for (int row = mapStartIndex.x; row < searchEndIndex.x; row++)
            {
                var furniture = interiorTileMap[bottomIndex, row].LinkedFurniture;
                if (furniture != null && furniture is T)
                {
                    furnitureSearchInfos.Add(new FurnitureSearchInfo<T>(Direction.Down, (T)furniture));
                }
            }
        }

        int leftIndex = mapStartIndex.x - 1;
        if (leftIndex >= 0)
        {
            for (int column = mapStartIndex.y; column < searchEndIndex.y; column++)
            {
                var furniture = interiorTileMap[column, leftIndex].LinkedFurniture;
                if (furniture != null && furniture is T)
                {
                    furnitureSearchInfos.Add(new FurnitureSearchInfo<T>(Direction.Left, (T)furniture));
                }
            }
        }

        int rightIndex = searchEndIndex.x;
        if (rightIndex < interiorTileMap.MapSize.x)
        {
            for (int column = mapStartIndex.y; column < searchEndIndex.y; column++)
            {
                var furniture = interiorTileMap[column, rightIndex].LinkedFurniture;
                if (furniture != null && furniture is T)
                {
                    furnitureSearchInfos.Add(new FurnitureSearchInfo<T>(Direction.Right, (T)furniture));
                }
            }
        }

        return furnitureSearchInfos.Count > 0 ? furnitureSearchInfos.ToArray() : null;
    }

    public Furniture[] GetFurnitures()
    {
        return arrangedFurniture.ToArray();
    }

    public void SetArragnedFurnituresInteractionEnable(bool isEnable)
    {
        foreach (var arrangedFurniture in arrangedFurniture)
        {
            arrangedFurniture.GetComponent<FurnitureController>().IsTouchEnable = isEnable;
        }
    }

    public T[] GetFurnitures<T>() where T : Furniture
    {
        return arrangedFurniture
               .Where(item => item.GetType() == typeof(T))
               .Select(item => (T)item)
               .ToArray();
    }

    public JSONObject SaveToJson()
    {
        var root = new JSONObject(JSONObject.Type.OBJECT);
        root.AddField("floorState", (int)CurrentState);
        root.AddField("interiorTileMap", interiorTileMap.SaveToJson());
        root.AddField("questSystem", questBoard.GetComponent<QuestSystem>().SaveToJson());

        root.AddField("furnitures", new JSONObject(JSONObject.Type.ARRAY));
        foreach (var furniture in arrangedFurniture)
        {
            var jsonObject = new JSONObject(JSONObject.Type.OBJECT);
            jsonObject.AddField("name", furniture.FurnitureData.name);
            jsonObject.AddField("column", furniture.MapStartIndex.y);
            jsonObject.AddField("row", furniture.MapStartIndex.x);
            jsonObject.AddField("direction", (int)furniture.CurrentDirection);
            jsonObject.AddField("positionX", furniture.transform.position.x);
            jsonObject.AddField("positionY", furniture.transform.position.y);
            jsonObject.AddField("furnitureUniqueData", furniture.SaveToJson());

            root["furnitures"].Add(jsonObject);
        }

        return root;
    }

    /*
     * TODO : InteriorSystem TODO 참고
     */
    public void LoadFromJson(JSONObject jsonObject)
    {
        if ((FloorState)jsonObject["floorState"].i == FloorState.Bought &&
            CurrentState == FloorState.NotBuy)
        {
            Buy(null);
        }

        interiorTileMap.LoadFromJson(jsonObject["interiorTileMap"]);
        questBoard.GetComponent<QuestSystem>().LoadFromJson(jsonObject["questSystem"]);


        var furnitures = jsonObject["furnitures"];
        for (int i = 0; i < furnitures.Count; i++)
        {
            var    furnitureJson = furnitures[i];
            string furnitureName = furnitureJson["name"].str;

            var furnitureObj = Resources.Load<GameObject>("Furniture/" + furnitureName);
            furnitureObj = Instantiate(furnitureObj, transform);
            furnitureObj.transform.position = new Vector3(furnitureJson["positionX"].f, furnitureJson["positionY"].f);

            Vector2Int mapTileIndex = new Vector2Int((int)furnitureJson["row"].i, (int)furnitureJson["column"].i);
            var furnitureController = furnitureObj.GetComponent<FurnitureController>();
            furnitureController.ArrangeTo(this, mapTileIndex);
            furnitureController.BuildCompleteImmediate();

            for (int direction = 0; direction < furnitureJson["direction"].i; direction++)
            {
                furnitureController.Rotate();
            }

            var selectedTileMap = furnitureController.Furniture.GetType() == typeof(Door) ? doorTileMap : interiorTileMap;

            InteriorSystem.Instance.LinkFurnitureToTiles(selectedTileMap,
                                                         furnitureController.Furniture,
                                                         mapTileIndex,
                                                         furnitureController.MarkTileMap.MapSize);

            InsertFurniture(furnitureController.Furniture);

            furnitureController.Relocate(mapTileIndex);

            furnitureObj.GetComponent<Furniture>().LoadFromJson(furnitureJson["furnitureUniqueData"]);
        }
    }

    public void ShowInteriorMap(bool isShow)
    {
        interiorTileMap.gameObject.SetActive(isShow);
        doorTileMap.gameObject.SetActive(isShow);
    }
    
    public void GraphScan()
    {
        gridGraph.Scan();
    }
}
