using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

[System.Serializable]
public class GameModeEvent : UnityEvent
{
}

[System.Serializable]
public struct GameModeData
{
    public float businessTime;
}

public enum GameModeType
{
    Editor,
    Business
}

public class GameMode : Singleton<GameMode>, ISavable {
    [SerializeField]
    private GameModeData            gameModeData;
    [SerializeField, Required]
    private ColorFadeAnimationForUI businessCloseFade = null;

    private int day = 1;

    public GameModeEvent onEditorModeStarted   = new GameModeEvent();
    public GameModeEvent onBusinessTimeOvered  = new GameModeEvent();
    public GameModeEvent onBusinessModeStarted = new GameModeEvent();

    public float BusinessTime         { get { return gameModeData.businessTime; } }
    public float RemainedBusinessTime { get; private set; }

    private void Awake()
    {
        businessCloseFade.onCompleted.AddListener(() =>
        {
            onEditorModeStarted.Invoke();
        });
    }

    private IEnumerator BusinessTimer()
    {
        var businessSystems = FindObjectsOfType<BusinessSystem>();

        float currentTime = 0f;
        RemainedBusinessTime = 0f;

        while (currentTime < gameModeData.businessTime)
        {
            yield return null;

            currentTime += Time.smoothDeltaTime;
            RemainedBusinessTime = gameModeData.businessTime - currentTime;
        }

        RemainedBusinessTime = 0;
        onBusinessTimeOvered.Invoke();

        int remainedCustomerCount = 0;
        while (true)
        {
            foreach (var businessSystem in businessSystems)
                remainedCustomerCount += businessSystem.CustomerCount;

            if (remainedCustomerCount == 0)
                break;
            else
            {
                remainedCustomerCount = 0;
                yield return null;
            }
        }

        day++;

        businessCloseFade.gameObject.SetActive(true);
        businessCloseFade.StartAnimation();
    }

    public void StartBusiness()
    {
        onBusinessModeStarted.Invoke();
        StartCoroutine("BusinessTimer");
    }

    public void BusinessCloseImmediate()
    {
        StopCoroutine("BusinessTimer");
        SaveSlotSystem.Instance.SaveToPlayerPrefs();
        businessCloseFade.onCompleted.RemoveAllListeners();
        businessCloseFade.onCompleted.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
        businessCloseFade.StartAnimation();
    }

    public JSONObject SaveToJson()
    {
        var root = new JSONObject(JSONObject.Type.OBJECT);
        root.SetField("day", day);

        return root;
    }

    public void LoadFromJson(JSONObject json)
    {
        day = (int)json["day"].i;
    }
}
