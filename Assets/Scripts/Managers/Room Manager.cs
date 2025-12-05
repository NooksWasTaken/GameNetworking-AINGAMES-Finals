using UnityEngine;
using Photon.Pun;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("Player Spawn Settings")]
    public GameObject playerPrefab;
    public Transform spawnPoint;

    private bool playerSpawned = false;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        TrySpawnPlayer();
    }

    public override void OnJoinedRoom()
    {
        TrySpawnPlayer();
    }

    private void TrySpawnPlayer()
    {
        if (playerSpawned) return;
        if (!PhotonNetwork.InRoom) return;
        if (playerPrefab == null || spawnPoint == null) return;

        PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        playerSpawned = true;

        Debug.Log("Player INSTantiated for: " + PhotonNetwork.NickName);
    }
}
