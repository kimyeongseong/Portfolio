using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

[System.Serializable]
public class DataLoadEvent : UnityEvent<string>
{
}

[System.Serializable]
public class DataSaveEvent : UnityEvent
{
}

[DefaultExecutionOrder(100)]
public class SaveSlotSystem : Singleton<SaveSlotSystem>
{
    private const string kSavePath       = "Save";
    private const string kSlotNumberPath = "SlotNumber";

    [SerializeField]
    private bool         isAutoLoad       = true;
    [SerializeField]
    private TextAsset    defaultSavedData = null;
    [SerializeField]
    private int          maxNumOfSlots    = 0;
    [SerializeField]
    private GameObject[] savableObjects   = null;

    public DataSaveEvent onSaveToPlayerPrefs   = new DataSaveEvent();
    public DataLoadEvent onDataLoadedFromCloud = new DataLoadEvent();
    public DataSaveEvent onDataSavedToCloud    = new DataSaveEvent();

    public int MaxNumOfSlots { get { return maxNumOfSlots; } }

    private string SaveSlotPath
    {
        get
        {
            return kSavePath + PlayerPrefs.GetInt(kSlotNumberPath, 0);
        }
    }

    private JSONObject root = null;

    private void Awake()
    {
        LoadDefaultSaveData();
    }

    private void Start()
    {
        if (isAutoLoad)
            LoadFromPlayerPrefs();
    }

    private void LoadDefaultSaveData()
    {
        if (defaultSavedData != null)
        {
            string jsonText = defaultSavedData.text;
            var root = new JSONObject(jsonText);
            foreach (var key in root.keys)
            {
                if (!PlayerPrefs.HasKey(key))
                    PlayerPrefs.SetString(key, root[key].ToString());
            }
        }
    }

    public void CreateSaveSlot()
    {
        PlayerPrefs.SetString(SaveSlotPath, "{}");
    }

    [Button]
    public void SaveToPlayerPrefs()
    {
        root = new JSONObject(JSONObject.Type.OBJECT);

        foreach (var savableObject in savableObjects)
        {
            var savables = savableObject.GetComponents<ISavable>();
            Debug.Assert(savables.Length != 0, $"SaveSyste - {savableObject.name} is not savableObject");

            var componentsJson = new JSONObject(JSONObject.Type.ARRAY);
            root.AddField(savableObject.name, componentsJson);

            for (int i = 0; i < savables.Length; i++)
            {
                componentsJson.Add(savables[i].SaveToJson());
            }
        }

#if UNITY_EDITOR
        PlayerPrefs.SetString(SaveSlotPath, root.ToString(true));
#else
        PlayerPrefs.SetString(SaveSlotPath, root.ToString());
#endif

        onSaveToPlayerPrefs.Invoke();
    }

    [Button]
    public void LoadFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey(SaveSlotPath))
        {
            root = new JSONObject(PlayerPrefs.GetString(SaveSlotPath));
            if (root.keys.Count > 0)
            {
                foreach (var savableObject in savableObjects)
                {
                    var savables = savableObject.GetComponents<ISavable>();
                    Debug.Assert(savables.Length != 0, $"SaveSyste - {savableObject.name} is not savableObject");

                    var componentsJson = root[savableObject.name];
                    for (int i = 0; i < savables.Length; i++)
                    {
                        savables[i].LoadFromJson(componentsJson[i]);
                    }
                }
            }
        }
    }

    public void SetBool(string key, bool data)
    {
        root["Ending System"][0].SetField(key, data);
        PlayerPrefs.SetString(SaveSlotPath, root.ToString(true));
    }

    public void RemoveSaveData()
    {
        PlayerPrefs.DeleteKey(SaveSlotPath);
    }

    public string SaveSlotsToJson()
    {
        var fromJson = new JSONObject(JSONObject.Type.OBJECT);
        for (int i = 0; i < maxNumOfSlots; i++)
        {
            SelectSlot(i);
            if (PlayerPrefs.HasKey(SaveSlotPath))
                fromJson.SetField(i.ToString(), PlayerPrefs.GetString(SaveSlotPath));
        }

        return fromJson.ToString();
    }

    public void LoadSlotsFromJson(string json)
    {
        var fromJson = new JSONObject(json);
        for (int i = 0; i < maxNumOfSlots; i++)
        {
            SelectSlot(i);
            if (fromJson.HasField(SaveSlotPath))
            {
                PlayerPrefs.SetString(SaveSlotPath, fromJson[SaveSlotPath].ToString());
            }
        }
    }

    public void SelectSlot(int slotNumber)
    {
        PlayerPrefs.SetInt(kSlotNumberPath, slotNumber);
    }

    public bool IsExistSavedSlotData(int slotNumber)
    {
        return PlayerPrefs.HasKey(kSavePath + slotNumber);
    }

    public string SavedSlotDataToJson()
    {
        return SavedSlotDataToJson(PlayerPrefs.GetInt(kSlotNumberPath));
    }

    public string SavedSlotDataToJson(int slotNumber)
    {
        return PlayerPrefs.GetString(kSavePath + slotNumber, string.Empty);
    }

    public JSONObject SavedSlotDataToJsonObject()
    {
        return SavedSlotDataToJsonObject(PlayerPrefs.GetInt(kSlotNumberPath));
    }

    public JSONObject SavedSlotDataToJsonObject(int slotNumber)
    {
        return new JSONObject(PlayerPrefs.GetString(kSavePath + slotNumber, string.Empty));
    }

    public string SavedSlotsDataToJson()
    {
        return SavedSlotsDataToJsonObject().ToString();
    }

    public JSONObject SavedSlotsDataToJsonObject()
    {
        var root = new JSONObject(JSONObject.Type.OBJECT);
        for (int i = 0; i < maxNumOfSlots; i++)
        {
            if (IsExistSavedSlotData(i))
            {
                SelectSlot(i);
                root.AddField(SaveSlotPath, SavedSlotDataToJsonObject());
            }
        }

        return root;
    }

    public void LoadFromCloud()
    {
#if !NO_GPGS
        if (GPGSManager.IsLogined)
            GPGSManager.LoadFromCloud((data) => onDataLoadedFromCloud.Invoke(data));
#endif
    }

    public void SaveToCloud()
    {
#if !NO_GPGS
        if (GPGSManager.IsLogined)
        {
            string saveData = SaveSlotSystem.Instance.SavedSlotsDataToJson();
            GPGSManager.SaveToCloud(saveData, () => onDataSavedToCloud.Invoke());
        }
#endif
    }

#if UNITY_EDITOR
    [Button]
    public void ExtractSaveData()
    {
        var savedData = PlayerPrefs.GetString(SaveSlotPath, string.Empty);
        if (savedData != string.Empty)
        {
            var path = UnityEditor.EditorUtility.SaveFilePanel("Save Json", Application.dataPath, "save.json", "json");
            if (path.Length != 0)
            {
                var bytes = System.Text.Encoding.Default.GetBytes(savedData);
                System.IO.File.WriteAllBytes(path, bytes);
            }
        }
    }

    [Button]
    public void ExtractSaveDataAll()
    {
        var root = new JSONObject(JSONObject.Type.OBJECT);
        for (int i = 0; i < maxNumOfSlots; i++)
        {
            if (IsExistSavedSlotData(i))
            {
                SelectSlot(i);
                root.AddField(SaveSlotPath, SavedSlotDataToJsonObject());
            }
        }

        string rootToString = root.ToString();
        if (rootToString != string.Empty)
        {
            var path = UnityEditor.EditorUtility.SaveFilePanel("Save Json", Application.dataPath, "save.json", "json");
            if (path.Length != 0)
            {
                var bytes = System.Text.Encoding.Default.GetBytes(rootToString);
                System.IO.File.WriteAllBytes(path, bytes);
            }
        }
    }
#endif
}