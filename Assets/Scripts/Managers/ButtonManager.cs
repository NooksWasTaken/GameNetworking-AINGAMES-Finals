using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviourPunCallbacks
{
    [Header("Menu Scene")]
    public string SceneToLoad;

    public void OnQuitButton()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void OnMenuButton()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            LoadMenuScene();
        }
    }

    public override void OnLeftRoom()
    {
        LoadMenuScene();
    }

    private void LoadMenuScene()
    {
        if (!string.IsNullOrEmpty(SceneToLoad))
        {
            SceneManager.LoadScene(SceneToLoad);
        }
        else
        {
            Debug.LogWarning("u forgot to set the scene bozo");
        }
    }
}
