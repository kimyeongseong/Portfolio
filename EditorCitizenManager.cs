using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorCitizenManager : MonoBehaviour
{
    [Header("Managers")]
    public EditorManager editorManager;
    public EditorBuildingManager editorBuildingManager;
    public EditorTileManager editorTileManager;

    [Header("CitizenObject")]
    public GameObject objCitizenContainer;
    [SerializeField]
    List<GameObject> listCitizenObjects;
    [SerializeField]
    GameObject objSelectedCitizen;

    [Header("UI Contents")]
    public GameObject objSetUIContainer;
    [SerializeField]
    List<GameObject> listSelectObjects;
    [SerializeField]
    List<Text> listKindValueTexts;

    public GameObject objCitizenDetail;
    public Dropdown drpHouses;
    public GameObject objColorContainer;
    public List<GameObject> listSelectedColorObject;

    [Header("Values")]
    public List<CitizenStats> citizens;
    [SerializeField]
    CitizenStats selectedCitizen;
    [SerializeField]
    int selectedIndex = -1;
    [SerializeField]
    int citizenCount = 0;
    [SerializeField]
    bool isSelected = false;
    [SerializeField]
    bool isSet = false;
    [SerializeField]
    Vector2 setTilePos;
    [SerializeField]
    Vector2 setTileXY;


    private void Awake()
    {
        SetUI();
        //SetCitizenObjects();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && !isSelected)
            OnMouseDown();
        if(Input.GetMouseButton(0) && isSelected)
            OnMouseDrag();
        if (Input.GetMouseButtonUp(0) && isSelected)
            OnMouseUp();
    }

    private void OnMouseDown()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 200f);

        if (hit.collider != null && hit.collider.CompareTag("Citizen"))
        {
            Debug.Log("Click Citizen");
            objSelectedCitizen = hit.collider.gameObject;
            objSelectedCitizen.transform.GetComponent<BoxCollider2D>().enabled = false;
            for(int i = 0; i < listCitizenObjects.Count; i++)
            {
                if(objSelectedCitizen == listCitizenObjects[i])
                {
                    selectedCitizen = citizens[i];
                    selectedIndex = i;
                }
            }
            isSelected = true;
            OnDetail();
        }
        else if(hit.collider != null && hit.collider.CompareTag("Tile"))
        {
            Debug.Log("Tile");
        }
    }

    private void OnMouseDrag()
    {
        Vector2 citizenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        objSelectedCitizen.transform.position = citizenPos;

        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 200f);

        if (hit.collider != null && hit.collider.CompareTag("Tile"))
        {
            isSet = true;
            setTilePos = hit.collider.transform.position;
            setTileXY = new Vector2(hit.collider.transform.GetSiblingIndex(), hit.collider.transform.parent.GetSiblingIndex());
            editorTileManager.SetCheckerPos(selectedCitizen.Index, setTileXY, 1, 1, true);
        }
        else
        {
            isSet = false;
            setTilePos = new Vector2(-1, -1);
        }
    }
    private void OnMouseUp()
    {
        if(isSet)
        {
            Debug.Log("Set Citizen");
            objSelectedCitizen.transform.position = setTilePos;
            objSelectedCitizen.transform.GetComponent<BoxCollider2D>().enabled = true;
            selectedCitizen.Position = setTileXY;
            SetCitizen();
            objSelectedCitizen = null;
        }
        else
        {
            Debug.Log("Cancle Citizen");
            CreatCancle();
        }
        isSelected = false;
        editorTileManager.SetCheckers();
        SetCitizenCount();
    }

    void SetUI()
    {
        for (int i = 0; i < objSetUIContainer.transform.childCount; i++)
        {
            listSelectObjects.Add(objSetUIContainer.transform.GetChild(i).GetChild(0).gameObject);
            listSelectObjects[i].SetActive(false);
            listKindValueTexts.Add(objSetUIContainer.transform.GetChild(i).GetChild(2).GetComponent<Text>());
        }
        for(int i = 0; i < objColorContainer.transform.childCount; i++)
        {
            listSelectedColorObject.Add(objColorContainer.transform.GetChild(i).GetChild(0).gameObject);
            listSelectedColorObject[i].SetActive(false);
        }
        objCitizenDetail.SetActive(false);
        SetCitizenCount();
    }

    void SetCitizenCount()
    {
        listKindValueTexts[0].text = "0";
        listKindValueTexts[1].text = "0";
        listKindValueTexts[2].text = "0";
        listKindValueTexts[3].text = "0";
        for (int i = 0; i < citizens.Count; i++)
        {
            listKindValueTexts[citizens[i].Kind].text = (int.Parse(listKindValueTexts[citizens[i].Kind].text) + 1).ToString();
        }
    }

    public void ClearAllCitizens()
    {
        for (int i = 0; i < listCitizenObjects.Count; i++)
        {
            Destroy(listCitizenObjects[i]);
        }
        listCitizenObjects.Clear();
        objSelectedCitizen = null;
        citizens.Clear();
        selectedCitizen = null;
        selectedIndex = -1;
        citizenCount = 0;
        isSelected = false;
        isSet = false;
        setTilePos = new Vector2(0, 0);
        setTileXY = new Vector2(0, 0);
    }

    public void LoadCitizens()
    {
        ClearAllCitizens();
        citizenCount = editorManager.currentStage.Citizens.Count;
        for (int i = 0; i < editorManager.currentStage.Citizens.Count; i++)
        {
            CitizenStats _citizen = new CitizenStats()
            {
                Kind = editorManager.currentStage.Citizens[i].Kind,
                Index = editorManager.currentStage.Citizens[i].Index,
                Color = editorManager.currentStage.Citizens[i].Color,
                Position = editorManager.currentStage.Citizens[i].Position,
                Home = editorManager.currentStage.Citizens[i].Home,
            };
            citizens.Add(_citizen);
        }
        for(int i = 0; i < citizens.Count; i++)
        {
            GameObject _objCitizen = Instantiate(Resources.Load<GameObject>("Prefabs/Citizen_ingame"), objCitizenContainer.transform) as GameObject;
            objSelectedCitizen = _objCitizen;

            SetCitizenObjects(_objCitizen);
            _objCitizen.GetComponent<BoxCollider2D>().enabled = true;

            SetSelectedCitizenActive(citizens[i].Kind);
            listCitizenObjects.Add(_objCitizen);

            SetCitizenPos(citizens[i].Position);
        }
        SetCitizenCount();
    }
    void SetCitizenPos(Vector2 pos)
    {
        objSelectedCitizen.transform.localPosition = new Vector3(GameConstants.TILE_AXIX_X * pos.x, GameConstants.TILE_AXIX_Y * pos.y, -1);
    }

    void SetCitizen()
    {
        CitizenStats _citizen = new CitizenStats()
        {
            Kind = selectedCitizen.Kind,
            Index = selectedCitizen.Index,
            Color = selectedCitizen.Color,
            Position = selectedCitizen.Position,
            Home = selectedCitizen.Home,
        };
        if (selectedIndex >= citizens.Count)
        {
            citizens.Add(_citizen);
            listCitizenObjects.Add(objSelectedCitizen);
        }
        else
        {
            citizens[selectedIndex] = _citizen;
        }
        //selectedCitizen = null;
        selectedIndex = -1;
    }
    //void SetCitizenObjects()
    //{
    //    listCitizenObjects.Clear();
    //    for (int i = 0; i < objCitizenContainer.transform.childCount; i++)
    //    {
    //        listCitizenObjects.Add(objCitizenContainer.transform.GetChild(i).gameObject);
    //        for(int j = 0; j < listCitizenObjects[i].transform.childCount; j++)
    //        {
    //            listCitizenObjects[i].transform.GetChild(j).gameObject.SetActive(false);
    //        }
    //        listCitizenObjects[i].SetActive(false);
    //        listCitizenObjects[i].GetComponent<CapsuleCollider2D>().enabled = false;
    //    }
    //}
    void SetCitizenObjects(GameObject obj)
    {
        for (int j = 0; j < obj.transform.childCount; j++)
        {
            obj.transform.GetChild(j).gameObject.SetActive(false);
        }
        obj.GetComponent<BoxCollider2D>().enabled = false;
    }

    public void OnCitizenCreate(int index)
    {
        objSelectedCitizen = Instantiate(Resources.Load<GameObject>("Prefabs/Citizen_ingame"), objCitizenContainer.transform) as GameObject;
        SetCitizenObjects(objSelectedCitizen);
        //objSelectedCitizen = listCitizenObjects[citizenCount];
        objSelectedCitizen.SetActive(true);
        isSelected = true;
        selectedIndex = citizenCount;

        CitizenStats _citizen = new CitizenStats()
        {
            Kind = index,
            Index = citizenCount,
            Color = 0,
            Position = new Vector2(-1, -1),
            Home = 0,
        };
        selectedCitizen = _citizen;
        SetSelectedCitizenActive(selectedCitizen.Kind);

        citizenCount++;
    }
    void SetSelectedCitizenActive(int kind)
    {
        for(int i = 0; i < objSelectedCitizen.transform.childCount; i++)
        {
            objSelectedCitizen.transform.GetChild(i).gameObject.SetActive(false);
        }
        objSelectedCitizen.transform.GetChild(kind).gameObject.SetActive(true);
    }
    void CreatCancle()
    {
        Destroy(objSelectedCitizen);

        if (citizenCount <= citizens.Count)
        {
            citizens.RemoveAt(selectedIndex);

            listCitizenObjects.RemoveAt(selectedIndex);
        }
        selectedCitizen = null;

        citizenCount--;
        selectedIndex = -1;
        SetCitizenIndex();
        SetCitizenCount();
        OffDetail();
    }
    void SetCitizenIndex()
    {
        for(int i = 0; i < citizens.Count; i++)
        {
            citizens[i].Index = i;
        }
    }

    public void OnDetail()
    {
        editorBuildingManager.OffDetail();
        objCitizenDetail.transform.GetChild(0).GetComponent<Text>().text = "주민 " + selectedCitizen.Index.ToString();
        SetDetailDrops();
        SetDetailColor();
        objCitizenDetail.SetActive(true);
        //Debug.Log(selectedCitizen.Color);
    }
    public void OffDetail()
    {
        objCitizenDetail.SetActive(false);
    }
    public void SetDetailDrops()
    {
        drpHouses.options.Clear();
        for (int i = 0; i < editorBuildingManager.buildings.Count; i++)
        {
            if (editorBuildingManager.buildings[i].Kind < 3)
            {
                Dropdown.OptionData _option = new Dropdown.OptionData()
                {
                    text = "집 " + editorBuildingManager.buildings[i].Index.ToString()
                };
                drpHouses.options.Add(_option);
                if (selectedCitizen.Home == editorBuildingManager.buildings[i].Index)
                    drpHouses.value = i;
            }
        }
    }
    void SetDetailColor()
    {
        for(int i = 0; i < listSelectedColorObject.Count; i++)
        {
            listSelectedColorObject[i].SetActive(false);
        }
        listSelectedColorObject[selectedCitizen.Color].SetActive(true);
    }

    public void ChangeDetailColor(int index)
    {
        selectedCitizen.Color = index;
        SetDetailColor();
        SaveDetailChange(selectedCitizen.Index);
    }

    public void HomeDropdownChanged(int index)
    {
        Debug.Log("DropDownChanged!!");
        drpHouses.value = index;
        int count = 0;
        for (int i = 0; i < editorBuildingManager.buildings.Count; i++)
        {
            if(editorBuildingManager.buildings[i].Kind < 3)
            {
                if(index == count)
                {
                    selectedCitizen.Home = editorBuildingManager.buildings[i].Index;
                    SaveDetailChange(selectedCitizen.Index);
                }
                count++;
            }
        }
    }

    void SaveDetailChange(int index)
    {
        CitizenStats _citizen = new CitizenStats()
        {
            Color = selectedCitizen.Color,
            Home = selectedCitizen.Home,
        };

        citizens[index] = _citizen;
    }
}
