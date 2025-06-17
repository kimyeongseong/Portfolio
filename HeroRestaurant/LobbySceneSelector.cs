using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySceneSelector : MonoBehaviour {
    [SerializeField]
    private string prologueSceneName = string.Empty;
    [SerializeField]
    private string mainSceneName     = string.Empty;

    public void LoadNextScene()
    {
        var root = SaveSlotSystem.Instance.SavedSlotDataToJsonObject();
        if (root.keys.Count > 0)
            SceneManager.LoadScene(mainSceneName);
        else
            SceneManager.LoadScene(prologueSceneName);
    }
}
