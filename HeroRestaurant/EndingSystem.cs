using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using SimpleDatabase;
using UnityEngine.SceneManagement;

public class EndingSystem : MonoBehaviour, ISavable {
    [SerializeField, Required]
    private QuestScriptView hiddenQuestView = null;

    private List<string> refusaledEndingEventNames = new List<string>();

    private string endingEventName            = string.Empty; 
    private string reservationEndingSceneName = string.Empty;
    private bool   isReservationEndingEvent   = false;
    private bool   isShowedEnding             = false;

    public void MakeReservationEndingEvent()
    {
        isReservationEndingEvent = true;
        SaveSlotSystem.Instance.SaveToPlayerPrefs();
    }

    public void RefualEndingEvent()
    {
        refusaledEndingEventNames.Add(endingEventName);
    }

    public void StartEndingScriptIfPossible()
    {
        if (isShowedEnding == false && isReservationEndingEvent == false)
        {
            var questSystems = FindObjectsOfType<QuestSystem>();
            foreach (var questSystem in questSystems)
            {
                var hiddenQuests = questSystem.GetCompletedQuestsOfType(QuestType.Hidden);
                if (hiddenQuests.Length > 0)
                {
                    string questTitle = hiddenQuests[0].QuestData.title;

                    bool isRefusaledEndingEvent = refusaledEndingEventNames.Exists(name => name == questTitle);

                    if (!isRefusaledEndingEvent)
                    {
                        var selectedHiddenQuestData = Database.Instance.Select<HiddenQuestData>("HiddenQuestDataTable").Select(questTitle);

                        var questScriptData = Database.Instance.Select<QuestScriptData>("QuestScriptDataTable")
                                                               .Select(selectedHiddenQuestData.scriptTableID);

                        endingEventName            = selectedHiddenQuestData.title;
                        reservationEndingSceneName = selectedHiddenQuestData.endingSceneName;

                        hiddenQuestView.Show(questScriptData);
                        return;
                    }
                }
            }
        }
    }

    public void ShowEndingEventIfPossible()
    {
        if (isReservationEndingEvent)
        {
            SaveSlotSystem.Instance.SaveToPlayerPrefs();
            SceneManager.LoadScene(reservationEndingSceneName);
        }
    }

    public JSONObject SaveToJson()
    {
        var root = new JSONObject(JSONObject.Type.OBJECT);
        root.SetField("isShowndEnding", isShowedEnding);
        root.SetField("isReservationEndingEvent", isReservationEndingEvent);
        root.SetField("reservationEndingSceneName", reservationEndingSceneName);
        root.SetField("refusaledEndingEventNames", new JSONObject(JSONObject.Type.ARRAY));

        foreach (var endingEventName in refusaledEndingEventNames)
            root.Add(endingEventName);

        return root;
    }

    public void LoadFromJson(JSONObject json)
    {
        isShowedEnding             = json["isShowndEnding"].b;
        isReservationEndingEvent   = json["isReservationEndingEvent"].b;
        reservationEndingSceneName = json["reservationEndingSceneName"].str;

        for (int i = 0; i < json["refusaledEndingEventNames"].Count; i++)
            refusaledEndingEventNames.Add(json["refusaledEndingEventNames"][i].str);

        Invoke("StartEndingScriptIfPossible", Time.smoothDeltaTime);
        Invoke("ShowEndingEventIfPossible", Time.smoothDeltaTime);
    }
}
