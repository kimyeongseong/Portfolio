using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorDataBase : MonoBehaviour
{
    private static EditorDataBase instance;
    public static EditorDataBase Instance
    {
        get { return instance; }
        set { instance = value; }
    }

    [SerializeField]
    private StageSaveData stageData;
    public static StageSaveData StageData
    {
        get { return instance.stageData; }
        set { instance.stageData = value; }
    }

    void Awake()
    {
        if (instance != this)
            instance = this;
    }
}
