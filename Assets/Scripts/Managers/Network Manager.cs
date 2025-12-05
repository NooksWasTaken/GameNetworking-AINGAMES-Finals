using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class MenuRoomController : MonoBehaviourPunCallbacks
{
    [Header("UI Inputs")]
    public TMP_InputField createRoomInput;
    public TMP_InputField joinRoomInput;

    [Header("Scene To Load")]
    public string sceneToLoad;

    [Header("UI Feedback")]
    public TMP_Text errorText;
    public float errorDisplayTime = 3f;

    private bool isConnecting = false;

    private void Start()
    {
        SoundManager.PlayLoopingSound(SoundType.BGM2, 1);

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Connecting to Photon...");
        }
    }

    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnected || isConnecting) return;
        isConnecting = true;

        string roomName = createRoomInput.text.Trim();
        if (string.IsNullOrEmpty(roomName))
            roomName = "Room" + Random.Range(1000, 9999);

        PlayerPrefs.SetString("RoomSceneToLoad", sceneToLoad);

        RoomOptions options = new RoomOptions { IsVisible = true, IsOpen = true, MaxPlayers = 4 };
        PhotonNetwork.CreateRoom(roomName, options);
        Debug.Log("Creating room: " + roomName);

    }

    public void JoinRoom()
    {
        if (!PhotonNetwork.IsConnected || isConnecting) return;
        isConnecting = true;

        string roomName = joinRoomInput.text.Trim();
        if (string.IsNullOrEmpty(roomName))
            PhotonNetwork.JoinRandomRoom();
        else
            PhotonNetwork.JoinRoom(roomName);

        Debug.Log("Joining room: " + roomName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to join room: " + message);
        isConnecting = false;
        ShowError("Room not found or unavailable!");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to create room: " + message);
        isConnecting = false;
        ShowError("Room creation failed!");
    }

    public override void OnJoinedRoom()
    {
        isConnecting = false;
        Debug.Log("Joined room successfully!");
        if (PhotonNetwork.IsMasterClient)
        {
            string scene = PlayerPrefs.GetString("RoomSceneToLoad", "");
            if (!string.IsNullOrEmpty(scene))
                PhotonNetwork.LoadLevel(scene);
        }

        SoundManager.StopLoopingSound(SoundType.BGM2);
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
            StartCoroutine(HideErrorAfterDelay());
        }
    }

    private IEnumerator HideErrorAfterDelay()
    {
        yield return new WaitForSeconds(errorDisplayTime);
        if (errorText != null)
            errorText.gameObject.SetActive(false);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
    #   endif
    }
}
