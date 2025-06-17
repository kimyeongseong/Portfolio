using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour
{
    [Header("SaveData")]
    public StageSaveData stageSaveData;

    [Header("Stage")]
    public Dropdown drpStage;
    public Dropdown drpMode;
    public Text txtCondition_0;
    public InputField infCondition_0;
    public Text txtCondition_1;
    public InputField infCondition_1;
    public InputField infPrize;

    [Header("Tile")]
    public EditorTileManager tileManager;

    [Header("Citizen")]
    public EditorCitizenManager citizenManager;

    [Header("Building")]
    public EditorBuildingManager buildingManager;

    [Header("Value")]
    public StageStats currentStage;


    private void Start()
    {
        SetStageDropDownData();
        SetCurrentStageData(0);
        SetStageData();
        tileManager.SetTiles(stageSaveData.stageList[drpStage.value].X, stageSaveData.stageList[drpStage.value].Y);
        citizenManager.LoadCitizens();
        buildingManager.LoadBuildings();
    }

    public void OnAddStage()
    {
        int _stageNum = ((stageSaveData.stageList.Count / 5) + 1) * 10 + (((stageSaveData.stageList.Count) % 5) + 1);

        StageStats _stage = new StageStats()
        {
            Index = _stageNum,
        };
        stageSaveData.stageList.Add(_stage);
        SetStageDropDownData();
    }
    public void OnSaveStage()
    {
        stageSaveData.stageList[drpStage.value].Tiles.Clear();
        GetTileData();

        stageSaveData.stageList[drpStage.value].Citizens.Clear();
        GetCitizenData();

        stageSaveData.stageList[drpStage.value].Buildings.Clear();
        GetBuildingData();
        StageStats _stage = new StageStats()
        {
            Index = currentStage.Index,
            X = tileManager.intX,
            Y = tileManager.intY,
            Kind = currentStage.Kind,
            Condition_0 = currentStage.Condition_0,
            Condition_1 = currentStage.Condition_1,
            Prize = currentStage.Prize,
            Tiles = currentStage.Tiles,
            Buildings = currentStage.Buildings,
            Citizens = currentStage.Citizens,
        };
        stageSaveData.stageList[drpStage.value] = _stage;
        Debug.Log("스테이지 데이터 저장");
    }

    public void OnServerSave()
    {
        UpdateDataBase.UpdateStageTitleData();
    }

    public void OnRemoveStage()
    {
        StageStats _stage = new StageStats()
        {
            Index = currentStage.Index,
            Kind = 0,
            Condition_0 = 0,
            Condition_1 = 0,
            Prize = "",
            Tiles = new List<TileStats>(),
            Buildings = new List<BuildingStats>(),
            Citizens = new List<CitizenStats>(),
        };
        Debug.Log("스테이지 데이터 삭제");
    }

    public void StageDropdownChanged(int index)
    {
        drpStage.value = index;
        SetCurrentStageData(index);
        SetStageData();
        tileManager.SetTiles(stageSaveData.stageList[drpStage.value].X, stageSaveData.stageList[drpStage.value].Y);
        citizenManager.LoadCitizens();
        buildingManager.LoadBuildings();
    }

    public void ModeDropdownChanged(int index)
    {
        drpMode.value = index;
        currentStage.Kind = index;
    }

    public void OnConditionChange_0()
    {
        currentStage.Condition_0 = int.Parse(infCondition_0.text);
    }

    public void OnConditionChange_1()
    {
        currentStage.Condition_1 = int.Parse(infCondition_1.text);
    }

    public void OnPrizeChange()
    {
        currentStage.Prize = infPrize.text;
    }

    void SetStageData()
    {
        drpMode.value = currentStage.Kind;
        infCondition_0.text = currentStage.Condition_0.ToString();
        infCondition_1.text = currentStage.Condition_1.ToString();
        if(currentStage.Prize != null)
            infPrize.text = currentStage.Prize.ToString();
    }
    /// <summary>
    /// 하양타일이 아닌 타일들만 currentStage에 저장
    /// </summary>
    void GetTileData()
    {
        for(int i = 0; i < tileManager.tiles.Count; i++)
        {
            if(tileManager.tiles[i].Kind != 1)
            {
                currentStage.Tiles.Add(tileManager.tiles[i]);
            }
        }
        //currentStage.Tiles = tileManager.tiles;
    }

    void GetCitizenData()
    {
        for (int i = 0; i < citizenManager.citizens.Count; i++)
        {
            currentStage.Citizens.Add(citizenManager.citizens[i]);
        }
    }

    void GetBuildingData()
    {
        for (int i = 0; i < buildingManager.buildings.Count; i++)
        {
            BuildingStats _building = new BuildingStats()
            {
                Kind = buildingManager.buildings[i].Kind,
                Index = buildingManager.buildings[i].Index,
                Position = buildingManager.buildings[i].Position,
                Pos_x = buildingManager.buildings[i].Pos_x,
                Pos_y = buildingManager.buildings[i].Pos_y,
                Count = buildingManager.buildings[i].Count,
                Citizens = buildingManager.buildings[i].Citizens,
            };
            currentStage.Buildings.Add(_building);
        }
    }

    /// <summary>
    /// index에 해당하는 스테이지 데이터를 currentStage에 가져옴
    /// </summary>
    /// <param name="index"></param>
    void SetCurrentStageData(int index)
    {
        drpStage.value = index;

        StageStats _stage = new StageStats()
        {
            Index = stageSaveData.stageList[drpStage.value].Index,
            Kind = stageSaveData.stageList[drpStage.value].Kind,
            Condition_0 = stageSaveData.stageList[drpStage.value].Condition_0,
            Condition_1 = stageSaveData.stageList[drpStage.value].Condition_1,
            Prize = stageSaveData.stageList[drpStage.value].Prize,
            Tiles = stageSaveData.stageList[drpStage.value].Tiles,
            Buildings = stageSaveData.stageList[drpStage.value].Buildings,
            Citizens = stageSaveData.stageList[drpStage.value].Citizens,
        };
        currentStage = _stage;

    }
    /// <summary>
    /// 스테이지 드롭다운의 컨텐츠들 추가
    /// </summary>
    void SetStageDropDownData()
    {
        drpStage.options.Clear();
        for (int i = 0; i < stageSaveData.stageList.Count; i++)
        {
            Dropdown.OptionData _option = new Dropdown.OptionData()
            {
                text = (stageSaveData.stageList[i].Index / 10).ToString() 
                    + " - " + (stageSaveData.stageList[i].Index % 10).ToString()
            };
            drpStage.options.Add(_option);
        }
    }
}
