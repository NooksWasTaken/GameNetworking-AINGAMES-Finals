using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("Player Spawn Settings")]
    public GameObject playerPrefab;
    public Transform spawnPoint;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);

        if (PhotonNetwork.IsMasterClient)
        {
            string sceneToLoad = PlayerPrefs.GetString("RoomSceneToLoad", "");

            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.Log("MasterClient loading scene: " + sceneToLoad);
                PhotonNetwork.LoadLevel(sceneToLoad);
            }
            else
            {
                Debug.LogError("No scene stored from MenuRoomController!");
            }
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join room failed: " + message);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Create room failed: " + message);
    }

    void Start()
    {
        if (PhotonNetwork.InRoom && playerPrefab != null && spawnPoint != null)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
