using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorBuildingManager : MonoBehaviour
{
    [Header("EditorManager")]
    public EditorManager editorManager;
    public EditorTileManager editorTileManager;
    public EditorCitizenManager editorCitizenManager;

    [Header("BuildingObject")]
    public GameObject objBuildingContainer;
    [SerializeField]
    List<GameObject> listBuildingObjects;
    [SerializeField]
    GameObject objSelectedBuilding;

    [Header("UI Contents")]
    //public GameObject objSetUIContainer;

    public GameObject objBuildingDetail;
    public GameObject objCitizenContainer;
    public List<GameObject> listInnerCitizenObject;
    public List<Text> listInnerCitizenText;

    [Header("Values")]
    public List<BuildingStats> buildings;

    public BuildingStats selectedBuilding;
    [SerializeField]
    int selectedIndex = -1;
    [SerializeField]
    int buildingCount = 0;
    [SerializeField]
    bool isSelected = false;
    [SerializeField]
    bool isSet = false;
    [SerializeField]
    Vector2 setTilePos;
    [SerializeField]
    Vector2 setTileXY;
    [SerializeField]
    int selectedCitizenKind = -1;

    bool isCitizenSelected;

    private void Awake()
    {
        SetUI();
        //selectedBuilding = new BuildingStats();
        selectedBuilding.Citizens = new List<CitizenStats>();
    }

    void SetUI()
    {
        for(int i = 0; i < objCitizenContainer.transform.childCount; i++)
        {
            listInnerCitizenObject.Add(objCitizenContainer.transform.GetChild(i).GetChild(0).gameObject);
            listInnerCitizenObject[i].SetActive(false);
            listInnerCitizenText.Add(objCitizenContainer.transform.GetChild(i).GetChild(2).GetComponent<Text>());
            listInnerCitizenText[i].text = "0";
        }
        objBuildingDetail.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isSelected)
            OnMouseDown();
        if (Input.GetMouseButton(0) && isSelected)
            OnMouseDrag();
        if (Input.GetMouseButtonUp(0) && isSelected)
            OnMouseUp();
    }
    private void OnMouseDown()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 200f);

        if (hit.collider != null && hit.collider.CompareTag("Building"))
        {
            Debug.Log("Click Building");
            objSelectedBuilding = hit.collider.gameObject;
            objSelectedBuilding.transform.GetComponent<BoxCollider2D>().enabled = false;
            for (int i = 0; i < listBuildingObjects.Count; i++)
            {
                if (objSelectedBuilding == listBuildingObjects[i])
                {
                    selectedBuilding = buildings[i];
                    selectedIndex = i;
                    Debug.Log("이미 있음");
                }
            }
            isSelected = true;
        }
        else if (hit.collider != null && hit.collider.CompareTag("Tile"))
        {
            Debug.Log("Tile");
        }
    }

    private void OnMouseDrag()
    {
        Vector2 buildingPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        objSelectedBuilding.transform.position = buildingPos + new Vector2(-0.27f, 0);

        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(-0.27f, 0, 0);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 200f);

        if (hit.collider != null && hit.collider.CompareTag("Tile"))
        {
            isSet = true;
            setTilePos = hit.collider.transform.position;
            setTileXY = new Vector2(hit.collider.transform.GetSiblingIndex(), hit.collider.transform.parent.GetSiblingIndex());
            editorTileManager.SetCheckerPos(selectedBuilding.Index, setTileXY, selectedBuilding.Pos_x, selectedBuilding.Pos_y, false);
        }
        else
        {
            isSet = false;
            setTilePos = new Vector2(-1, -1);
        }
    }
    private void OnMouseUp()
    {
        if (isSet)
        {
            Debug.Log("Set Building");
            objSelectedBuilding.transform.position = setTilePos;
            objSelectedBuilding.transform.GetComponent<BoxCollider2D>().enabled = true;
            selectedBuilding.Position = setTileXY;
            SetBuilding();
            OnDetail();
            objSelectedBuilding = null;
        }
        else
        {
            Debug.Log("Cancle Building");
            CreatCancle();
        }
        editorTileManager.SetCheckers();
        isSelected = false;
    }
    void SetBuilding()
    {
        BuildingStats _building = new BuildingStats()
        {
            Kind = selectedBuilding.Kind,
            Index = selectedBuilding.Index,
            Position = selectedBuilding.Position,
            Pos_x = selectedBuilding.Pos_x,
            Pos_y = selectedBuilding.Pos_y,
        };
        if (selectedIndex >= buildings.Count)
        {
            buildings.Add(_building);
            listBuildingObjects.Add(objSelectedBuilding);
        }
        else
        {
            buildings[selectedIndex] = _building;
        }
        //selectedBuilding = null;
        selectedIndex = -1;
    }

    public void OnBuildingCreate(int index)
    {
        int x = -1;
        int y = -1;
        if (index < 3)
        {
            x = 2;
            y = 2;
            //objSelectedBuilding = Instantiate(Resources.Load<GameObject>("Prefabs/House_ingame"), objBuildingContainer.transform) as GameObject;
        }
        else
        {
            x = 2;
            y = 3;
            //if(index == 3)
            //{
            //    objSelectedBuilding = Instantiate(Resources.Load<GameObject>("Prefabs/Sing_ingame"), objBuildingContainer.transform) as GameObject;
            //}
            //else if(index == 4)
            //{
            //    objSelectedBuilding = Instantiate(Resources.Load<GameObject>("Prefabs/School_ingame"), objBuildingContainer.transform) as GameObject;
            //}
            //else if (index == 5)
            //{
            //    objSelectedBuilding = Instantiate(Resources.Load<GameObject>("Prefabs/Bath_ingame"), objBuildingContainer.transform) as GameObject;
            //}
        }
        objSelectedBuilding = Instantiate(Resources.Load<GameObject>("Prefabs/Building"), objBuildingContainer.transform) as GameObject;

        SetBuildingObjects(objSelectedBuilding);

        objSelectedBuilding.SetActive(true);
        isSelected = true;
        selectedIndex = buildingCount;

        BuildingStats _building = new BuildingStats()
        {
            Kind = index,
            Index = buildingCount,
            Position = new Vector2(-1, -1),
            Pos_x = x,
            Pos_y = y,
        };
        selectedBuilding = _building;
        SetSelectedBuildingActive(selectedBuilding.Kind);

        buildingCount++;
    }
    void SetBuildingObjects(GameObject obj)
    {
        for (int j = 0; j < obj.transform.childCount; j++)
        {
            obj.transform.GetChild(j).gameObject.SetActive(false);
        }
        obj.GetComponent<BoxCollider2D>().enabled = false;
    }
    void SetSelectedBuildingActive(int kind)
    {
        for (int i = 0; i < objSelectedBuilding.transform.childCount; i++)
        {
            objSelectedBuilding.transform.GetChild(i).gameObject.SetActive(false);
        }
        objSelectedBuilding.transform.GetChild(kind).gameObject.SetActive(true);
    }

    void CreatCancle()
    {
        Destroy(objSelectedBuilding);

        if (buildingCount <= buildings.Count)
        {
            buildings.RemoveAt(selectedIndex);
            listBuildingObjects.RemoveAt(selectedIndex);
        }
        selectedBuilding = null;

        buildingCount--;
        selectedIndex = -1;
        SetBuildingIndex();
        OffDetail();
    }

    public void ClearAllBuildings()
    {
        for(int i = 0; i < listBuildingObjects.Count; i++)
        {
            Destroy(listBuildingObjects[i]);
        }
        listBuildingObjects.Clear();
        objSelectedBuilding = null;
        buildings.Clear();
        selectedBuilding = null;
        selectedIndex = -1;
        buildingCount = 0;
        isSelected = false;
        isSet = false;
        setTilePos = new Vector2(0, 0);
        setTileXY = new Vector2(0, 0);
    }

    public void LoadBuildings()
    {
        ClearAllBuildings();
        buildingCount = editorManager.currentStage.Buildings.Count;
        for (int i = 0; i < editorManager.currentStage.Buildings.Count; i++)
        {
            BuildingStats _building = new BuildingStats()
            {
                Kind = editorManager.currentStage.Buildings[i].Kind,
                Index = editorManager.currentStage.Buildings[i].Index,
                Position = editorManager.currentStage.Buildings[i].Position,
                Pos_x = editorManager.currentStage.Buildings[i].Pos_x,
                Pos_y = editorManager.currentStage.Buildings[i].Pos_y,
                Citizens = editorManager.currentStage.Buildings[i].Citizens
            };
            buildings.Add(_building);
        }
        for (int i = 0; i < buildings.Count; i++)
        {
            //GameObject _objBuilding = null;
            //if (buildings[i].Kind < 3)
            //{
            //    _objBuilding = Instantiate(Resources.Load<GameObject>("Prefabs/House_ingame"), objBuildingContainer.transform) as GameObject;
            //}
            //else
            //{
            //    if (buildings[i].Kind == 3)
            //    {
            //        _objBuilding = Instantiate(Resources.Load<GameObject>("Prefabs/Sing_ingame"), objBuildingContainer.transform) as GameObject;
            //    }
            //    else if (buildings[i].Kind == 4)
            //    {
            //        _objBuilding = Instantiate(Resources.Load<GameObject>("Prefabs/School_ingame"), objBuildingContainer.transform) as GameObject;
            //    }
            //    else if (buildings[i].Kind == 5)
            //    {
            //        _objBuilding = Instantiate(Resources.Load<GameObject>("Prefabs/Bath_ingame"), objBuildingContainer.transform) as GameObject;
            //    }
            //}
            GameObject _objBuilding = Instantiate(Resources.Load<GameObject>("Prefabs/Building"), objBuildingContainer.transform) as GameObject;
            objSelectedBuilding = _objBuilding;

            SetBuildingObjects(_objBuilding);
            _objBuilding.GetComponent<BoxCollider2D>().enabled = true;

            SetSelectedBuildingActive(buildings[i].Kind);
            listBuildingObjects.Add(_objBuilding);

            SetCitizenPos(buildings[i].Position);
        }
    }
    void SetCitizenPos(Vector2 pos)
    {
        objSelectedBuilding.transform.localPosition = new Vector3(GameConstants.TILE_AXIX_X * pos.x, GameConstants.TILE_AXIX_Y * pos.y, -1);
    }
    void SetBuildingIndex()
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            buildings[i].Index = i;
        }
    }

    void OnDetail()
    {
        editorCitizenManager.OffDetail();

        string _kind = "";
        switch (selectedBuilding.Kind)
        {
            case 0:
                {
                    _kind = "집(소)";
                    break;
                }
            case 1:
                {
                    _kind = "집(중)";
                    break;
                }
            case 2:
                {
                    _kind = "집(대)";
                    break;
                }
            case 3:
                {
                    _kind = "노래방";
                    break;
                }
            case 4:
                {
                    _kind = "학교";
                    break;
                }
            case 5:
                {
                    _kind = "목욕탕";
                    break;
                }
        }

        OnCitizenKindClick(selectedCitizenKind);
        objBuildingDetail.transform.GetChild(0).GetComponent<Text>().text = _kind + " " + selectedBuilding.Index.ToString();
        SetBuildingInnerCitizensCount();
        objBuildingDetail.SetActive(true);
    }
    public void OffDetail()
    {
        objBuildingDetail.SetActive(false);
    }
    public void OnCitizenKindClick(int index)
    {
        if(selectedCitizenKind == index)
        {
            selectedCitizenKind = -1;
            isCitizenSelected = false;
        }
        else
        {
            isCitizenSelected = true;
            selectedCitizenKind = index;
        }
        for(int i = 0; i < listInnerCitizenObject.Count; i++)
        {
            listInnerCitizenObject[i].SetActive(false);
            if (selectedCitizenKind == i)
                listInnerCitizenObject[i].SetActive(true);
        }
    }
    public void OnAddCitizen()
    {
        if (isCitizenSelected && selectedBuilding != null)
        {
            CitizenStats _citizen = new CitizenStats()
            {
                Kind = selectedCitizenKind,
                //Index = selectedBuilding.Citizens.Count,
                //Home = selectedBuilding.Index,
                //Position = selectedBuilding.Position,
            };
            List<CitizenStats> _list = new List<CitizenStats>();
            if (selectedBuilding.Citizens != null)
            {
                for (int i = 0; i < selectedBuilding.Citizens.Count; i++)
                {
                    _list.Add(selectedBuilding.Citizens[i]);
                }
            }
            _list.Add(_citizen);
            selectedBuilding.Citizens = _list;
            SaveDetailChange(selectedBuilding.Index);
            SetBuildingInnerCitizensCount();
        }
    }
    public void OnSubCitizen()
    {
        if (isCitizenSelected && selectedBuilding != null)
        {
            for(int i = 0; i < selectedBuilding.Citizens.Count; i++)
            {
                if (selectedBuilding.Citizens[i].Kind == selectedCitizenKind)
                {
                    selectedBuilding.Citizens.RemoveAt(i);
                    break;
                }
            }
            SetBuildingInnerCitizensCount();
        }
    }
    void SetBuildingInnerCitizensCount()
    {
        listInnerCitizenText[0].text = "0";
        listInnerCitizenText[1].text = "0";
        listInnerCitizenText[2].text = "0";
        if (selectedBuilding.Citizens != null)
        {
            for (int i = 0; i < selectedBuilding.Citizens.Count; i++)
            {
                if (selectedBuilding.Citizens[i].Kind == 0)
                    listInnerCitizenText[0].text = (int.Parse(listInnerCitizenText[0].text) + 1).ToString();
                if (selectedBuilding.Citizens[i].Kind == 1)
                    listInnerCitizenText[1].text = (int.Parse(listInnerCitizenText[1].text) + 1).ToString();
                if (selectedBuilding.Citizens[i].Kind == 2)
                    listInnerCitizenText[2].text = (int.Parse(listInnerCitizenText[2].text) + 1).ToString();
            }
        }
    }
    void SaveDetailChange(int index)
    {
        buildings[index].Citizens = selectedBuilding.Citizens;
    }
}
