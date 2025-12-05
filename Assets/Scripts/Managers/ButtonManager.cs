using UnityEngine;
using Photon.Pun;
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
        if (PhotonNetwork.IsConnected)
        {
            StartCoroutine(DisconnectAndLoad());
        }
        else
        {
            LoadMenuScene();
        }
    }

    private System.Collections.IEnumerator DisconnectAndLoad()
    {
        PhotonNetwork.Disconnect();

        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }

        LoadMenuScene();
    }

    private void LoadMenuScene()
    {
        if (!string.IsNullOrEmpty(SceneToLoad))
            SceneManager.LoadScene(SceneToLoad);
    }
}
