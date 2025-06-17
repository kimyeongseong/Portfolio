using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorTileManager : MonoBehaviour
{
    [Header("Manager")]
    public EditorManager editorManager;
    public EditorCitizenManager editorCitizenManager;
    public EditorBuildingManager editorBuildingManager;

    [Header("TileObject")]
    public GameObject objTileContainer;
    [SerializeField]
    List<GameObject> listTileObjects;

    [Header("UI Contents")]
    public GameObject objSetUIContainer;
    [SerializeField]
    List<GameObject> listSelectObjects;
    [SerializeField]
    List<Text> listKindValueTexts;
    public InputField infX;
    public InputField infY;

    [Header("Checker")]
    public GameObject objTileCheckContainer;
    [SerializeField]
    List<GameObject> listTileCheckObjects;
    public Color colorOK;
    public Color colorNG;

    [Header("Others")]
    public Camera camera;

    [Header("Values")]
    public List<TileStats> tiles;
    [SerializeField]
    List<bool> isTiles;
    [SerializeField]
    bool isBlind;
    [SerializeField]
    bool isWhite;
    [SerializeField]
    bool isRed;
    [SerializeField]
    bool isBlue;
    public int intWhiteLayer;
    public int intColorLayer;
    public bool isCheck;

    public int intX;
    public int intY;


    private void Awake()
    {
        //SetTiles();
        SetUI();
        InitCheckers();
    }

    private void Update()
    {
        if(Input.GetMouseButton(0))
            ShootRay();
    }
    void InitCheckers()
    {
        for(int i = 0; i < objTileCheckContainer.transform.childCount; i++)
        {
            listTileCheckObjects.Add(objTileCheckContainer.transform.GetChild(i).gameObject);
            listTileCheckObjects[i].SetActive(false);
        }
    }
    public void SetCheckerPos(int index, Vector2 indexPos, int x, int y, bool isCitizen)
    {
        FindEmptyTiles(index, isCitizen);
        objTileCheckContainer.transform.position = new Vector2(indexPos.x * GameConstants.TILE_AXIX_X, indexPos.y * GameConstants.TILE_AXIX_Y);
        int _count = 0;
        for(int i = 0; i < x; i++)
        {
            for(int j = 0; j < y; j++)
            {
                listTileCheckObjects[_count].SetActive(true);
                listTileCheckObjects[_count].transform.localPosition = new Vector2(i * GameConstants.TILE_AXIX_X, j * GameConstants.TILE_AXIX_Y);
                //if(FindEmptyTile((int)indexPos.x + i, (int)indexPos.y + j, indexPos, x, y))
                if(isTiles[((int)indexPos.x + i) + ((int)indexPos.y + j) * intX])
                {
                    listTileCheckObjects[_count].GetComponent<SpriteRenderer>().color = colorOK;
                }
                else
                {
                    listTileCheckObjects[_count].GetComponent<SpriteRenderer>().color = colorNG;
                }
                _count++;
            }
        }
    }
    public void SetCheckers()
    {
        for(int i = 0; i < listTileCheckObjects.Count; i++)
        {
            listTileCheckObjects[i].SetActive(false);
            listTileCheckObjects[i].transform.localPosition = new Vector2(0, 0);
        }
    }

    void FindEmptyTiles(int index, bool isCitizen)
    {
        for (int i = 0; i < editorBuildingManager.buildings.Count; i++)
        {
            BuildingStats _building = editorBuildingManager.buildings[i];
            for (int posX = 0; posX < _building.Pos_x; posX++)
            {
                for (int posY = 0; posY < _building.Pos_y; posY++)
                {
                    if (_building.Index == index && !isCitizen)
                    {
                        isTiles[((int)_building.Position.x + posX) + ((int)_building.Position.y + posY) * intX] = true;
                    }
                    else
                    {
                        isTiles[((int)_building.Position.x + posX) + ((int)_building.Position.y + posY) * intX] = false;
                    }
                }
            }
        }
        for (int i = 0; i < editorCitizenManager.citizens.Count; i++)
        {
            CitizenStats _citizen = editorCitizenManager.citizens[i];
            if (_citizen.Index == index && isCitizen)
            {
                isTiles[((int)_citizen.Position.x) + ((int)_citizen.Position.y) * intX] = true;
            }
            else
            {
                isTiles[((int)_citizen.Position.x) + ((int)_citizen.Position.y) * intX] = false;
            }
        }
    }

    void ShootRay()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 100f);

        if (hit.collider != null && hit.collider.CompareTag("Tile"))
        {
            if (isWhite || isRed || isBlue || isBlind)
            {
                for (int i = 0; i < listTileObjects.Count; i++)
                {
                    if (listTileObjects[i] == hit.collider.gameObject)
                    {
                        SetColorChange(i);
                    }
                }
            }
        }
    }

    void SetUI()
    {
        for(int i = 0; i < objSetUIContainer.transform.childCount; i++)
        {
            listSelectObjects.Add(objSetUIContainer.transform.GetChild(i).GetChild(0).gameObject);
            listSelectObjects[i].SetActive(false);
            listKindValueTexts.Add(objSetUIContainer.transform.GetChild(i).GetChild(2).GetComponent<Text>());
        }
        SetColorValue();
    }

    public void OnSetTile()
    {
        intX = int.Parse(infX.text);
        intY = int.Parse(infY.text);
        SetTiles(intX, intY);
        SetAllTileClear();
        editorCitizenManager.ClearAllCitizens();
        editorBuildingManager.ClearAllBuildings();
    }

    void InitTileObjects()
    {
        for(int i = 0; i < objTileContainer.transform.childCount; i ++ )
        {
            for(int j = 0; j < objTileContainer.transform.GetChild(i).childCount; j++)
            {
                objTileContainer.transform.GetChild(i).GetChild(j).gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 타일들을 축 개수에 맞게 위치시킴
    /// </summary>
    public void SetTiles(int x, int y)
    {
        tiles.Clear();
        listTileObjects.Clear();
        InitTileObjects();
        intX = x;
        intY = y;
        for (int i = 0; i < intY; i++)//y축
        {
            objTileContainer.transform.GetChild(i).localPosition = new Vector3(0, i * GameConstants.TILE_AXIX_Y, 0);
            for (int j = 0; j < intX; j++)//x축
            {
                GameObject _objTile = objTileContainer.transform.GetChild(i).GetChild(j).gameObject;
                _objTile.SetActive(true);
                _objTile.transform.localPosition = new Vector3(j * GameConstants.TILE_AXIX_X, 0, 0);
                listTileObjects.Add(_objTile);

                TileStats _tile = SetTileData(i * 15 + j);
                _tile.Position = new Vector2(j, i);
                tiles.Add(_tile);

                isTiles.Add(true);

                SetTileOrderLayer(j, objTileContainer.transform.GetChild(i).GetChild(j).gameObject);
                SetTileColor(_tile.Kind, objTileContainer.transform.GetChild(i).GetChild(j).gameObject);
            }
        }
        SetCamera();
        SetAxisInputs();
    }

    void SetCamera()
    {
        camera.transform.position = new Vector3(((intX - 1) / 2f) * GameConstants.TILE_AXIX_X, ((intY - 1) / 2f) * GameConstants.TILE_AXIX_Y, -10);
        float _big = 0;
        if (intX >= intY)
            _big = intX * 1.3f;
        else
            _big = intY * 1.3f;

        camera.orthographicSize = 1.5f + (_big * 0.22f);
    }

    void SetAxisInputs()
    {
        infX.text = intX.ToString();
        infY.text = intY.ToString();
    }

    /// <summary>
    /// 타일들을 위치시킴
    /// </summary>
    //public void SetTiles()
    //{
    //    tiles.Clear();
    //    listTileObjects.Clear();
    //    for (int i = 0; i < objTileContainer.transform.childCount; i++)//y축
    //    {
    //        for(int j = 0; j < objTileContainer.transform.GetChild(i).childCount; j++)//x축
    //        {
    //            objTileContainer.transform.GetChild(i).GetChild(j).localPosition = new Vector3(j * 0.54f, 0, 0);
    //            listTileObjects.Add(objTileContainer.transform.GetChild(i).GetChild(j).gameObject);

    //            TileStats _tile = SetTileData(i * 15 + j);
    //            _tile.Position = new Vector2(j, i);
    //            tiles.Add(_tile);
    //            SetTileOrderLayer(j, objTileContainer.transform.GetChild(i).GetChild(j).gameObject);
    //            SetTileColor(_tile.Kind, objTileContainer.transform.GetChild(i).GetChild(j).gameObject);
    //        }
    //    }
    //}

    public void SetAllTileClear()
    {
        for(int i = 0; i < listTileObjects.Count; i++)
        {
            SetTileColor(1, listTileObjects[i]);
        }
        for(int i = 0; i < tiles.Count; i++)
        {
            tiles[i].Kind = 1;
        }
        
    }

    void SetTileOrderLayer(int y, GameObject obj)
    {
        obj.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = intWhiteLayer;
        obj.transform.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = intColorLayer + y;
        obj.transform.GetChild(2).GetComponent<SpriteRenderer>().sortingOrder = intColorLayer + y;
    }
    TileStats SetTileData(int index)
    {
        int _tileIndex = -1;
        for (int i = 0; i < editorManager.currentStage.Tiles.Count; i++)
        {
            _tileIndex = ((int)editorManager.currentStage.Tiles[i].Position.y * 15) + (int)editorManager.currentStage.Tiles[i].Position.x;
            if (index == _tileIndex)
            {
                return editorManager.currentStage.Tiles[i];
            }
        }
        TileStats _tile = new TileStats()
        {
            Kind = 1
        };
        return _tile;
    }
    void SetTileColor(int index, GameObject obj)
    {
        switch (index)
        {
            case 0:
                obj.transform.GetChild(0).gameObject.SetActive(false);
                obj.transform.GetChild(1).gameObject.SetActive(false);
                obj.transform.GetChild(2).gameObject.SetActive(false);
                break;
            case 1:
                obj.transform.GetChild(0).gameObject.SetActive(true);
                obj.transform.GetChild(1).gameObject.SetActive(false);
                obj.transform.GetChild(2).gameObject.SetActive(false);
                break;
            case 2:
                obj.transform.GetChild(0).gameObject.SetActive(false);
                obj.transform.GetChild(1).gameObject.SetActive(true);
                obj.transform.GetChild(2).gameObject.SetActive(false);
                break;
            case 3:
                obj.transform.GetChild(0).gameObject.SetActive(false);
                obj.transform.GetChild(1).gameObject.SetActive(false);
                obj.transform.GetChild(2).gameObject.SetActive(true);
                break;
        };
    }

    public void OnTileColorClick(int index)
    {
        switch (index)
        {
            case 0:
                {
                    isBlind = !isBlind;
                    isWhite = false;
                    isRed = false;
                    isBlue = false;
                    SetColorSelect();
                    break;
                }
            case 1:
                {
                    isBlind = false;
                    isWhite = !isWhite;
                    isRed = false;
                    isBlue = false;
                    SetColorSelect();
                    break;
                }
            case 2:
                {
                    isBlind = false;
                    isWhite = false;
                    isRed = !isRed;
                    isBlue = false;
                    SetColorSelect();
                    break;
                }
            case 3:
                {
                    isBlind = false;
                    isWhite = false;
                    isRed = false;
                    isBlue = !isBlue;
                    SetColorSelect();
                    break;
                }
        }
    }
    void SetColorSelect()
    {
        listSelectObjects[0].SetActive(isBlind);
        listSelectObjects[1].SetActive(isWhite);
        listSelectObjects[2].SetActive(isRed);
        listSelectObjects[3].SetActive(isBlue);
    }
    void SetColorValue()
    {
        int counter = 0;
        for(int i = 0; i < 4; i++)
        {
            for(int j = 0; j < tiles.Count; j++)
            {
                if(tiles[j].Kind == i)
                {
                    counter++;
                }
            }
            listKindValueTexts[i].text = counter.ToString();
            counter = 0;
        }
    }
    void SetColorChange(int index)
    {
        int _kind = -1;
        if(isBlind)
            _kind = 0;
        else if (isWhite)
            _kind = 1;
        else if (isRed)
            _kind = 2;
        else if (isBlue)
            _kind = 3;

        SetColorObjectClear(index);
        if(_kind > 0)
            listTileObjects[index].transform.GetChild(_kind - 1).gameObject.SetActive(true);
        else
        {
            for(int i = 0; i < listTileObjects[index].transform.childCount; i++)
            {
                listTileObjects[index].transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        tiles[index].Kind = _kind;

        SetColorValue();
        Debug.Log("Color Change!!");
    }
    void SetColorObjectClear(int index)
    {
        listTileObjects[index].transform.GetChild(0).gameObject.SetActive(false);
        listTileObjects[index].transform.GetChild(1).gameObject.SetActive(false);
        listTileObjects[index].transform.GetChild(2).gameObject.SetActive(false);
    }
}
