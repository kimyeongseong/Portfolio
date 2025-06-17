using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;

public class EditorLoginManager : MonoBehaviour
{
    public GameObject pnlLogin;

    public string userName;
    public string email;
    public string password;

    private void Awake()
    {
        Login();
    }

    public void Login()
    {
        LoginBase login = new EmailLogin(userName, password, "");
        login.Login((result) =>
        {
            StartCoroutine(GetServerDatas(result));
        });
    }
    IEnumerator GetServerDatas(LoginResult _result)
    {
        GetDataBase.GetUserData(_result.PlayFabId);
        GetDataBase.GetItemCatalogData();
        GetDataBase.GetTownCatalogData();
        yield return new WaitWhile(() => DataBase.BoolGetUserData);
        yield return new WaitWhile(() => DataBase.BoolGetItemCatalog);
        yield return new WaitWhile(() => DataBase.BoolGetTownCatalog);
        Debug.Log("로그인 성공!");
        pnlLogin.SetActive(false);
    }

    public void Register()
    {
        LoginBase register = new EmailLogin(userName, password, email);
        register.Register((result) =>
        {
            Debug.Log("계정 등록!");
            // UpdateDataBase updateUserSetupData = new UpdatingUserSetupData(GameConstants.SETUP_EXP, GameConstants.SETUP_MAX_EXP, GameConstants.SETUP_LEVEL);
            // updateUserSetupData.UpdateUserData((_result) => { Debug.Log("초기값 등록!"); });
            // UpdateDataBase updateStatisticData = new UpdatingStatisticsData(GameConstants.SETUP_TROPHIES);
            // updateStatisticData.UpdateStatisticData((_result) => { Debug.Log("통계 등록!"); });
        });
    }

    void Init()
    {
        for (int i = 0; i < DataBase.MyItems.Count; i++)
        {
            DataBase.ItemUpgradeCondition.Add(false);
        }
        UpdateDataBase.UpdateItemUpgrade();

        SelectedStats _select = new SelectedStats()
        {
            Index_0 = GameConstants.ITEM_INDEX_0,
            Index_1 = GameConstants.ITEM_INDEX_1,
            Index_2 = GameConstants.ITEM_INDEX_2,
            Index_3 = GameConstants.ITEM_INDEX_3,
            Index_4 = GameConstants.ITEM_INDEX_4,
        };
        DataBase.SelectedItems = _select;

        UpdateDataBase.UpdateSelectedItems();
    }
}
