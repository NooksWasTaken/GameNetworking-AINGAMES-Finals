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
    private bool isReady = false; // NEW

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        SoundManager.PlayLoopingSound(SoundType.BGM2, 1);

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Connecting to Photon...");
        }
    }

    // NEW — Now we wait until truly connected before allowing Join/Create
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master. Joining Lobby...");
        PhotonNetwork.JoinLobby();
    }

    // NEW — Matchmaking is ready only after reaching lobby
    public override void OnJoinedLobby()
    {
        isReady = true;
        Debug.Log("Joined Lobby. Matchmaking Ready.");
    }

    public void CreateRoom()
    {
        if (!isReady || isConnecting) return; // UPDATED

        isConnecting = true;

        string roomName = createRoomInput.text.Trim();
        if (string.IsNullOrEmpty(roomName))
            roomName = "Room" + Random.Range(1000, 9999);

        PlayerPrefs.SetString("RoomSceneToLoad", sceneToLoad);

        RoomOptions options = new RoomOptions
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 4
        };

        PhotonNetwork.CreateRoom(roomName, options);
        Debug.Log("Creating room: " + roomName);
    }

    public void JoinRoom()
    {
        if (!isReady || isConnecting) return; // UPDATED

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

        string scene = PlayerPrefs.GetString("RoomSceneToLoad", "");
        if (!string.IsNullOrEmpty(scene) && PhotonNetwork.IsMasterClient)
        {
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
        #endif
    }
}
