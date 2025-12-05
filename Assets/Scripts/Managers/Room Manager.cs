using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [Header("Player Settings")]
    public GameObject playerPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private GameObject localPlayerInstance;

    void Start()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Not connected to Photon or not in a room yet.");
            return;
        }

        TrySpawnPlayer();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room, checking if player exists...");
        TrySpawnPlayer();
    }

    void TrySpawnPlayer()
    {
        GameObject existingPlayer = PhotonNetwork.LocalPlayer.TagObject as GameObject;

        if (existingPlayer == null)
        {
            SpawnPlayer();
        }
        else
        {
            localPlayerInstance = existingPlayer;
            Debug.Log("Player already exists, skipping spawn.");
        }

        StartCoroutine(ResetPlayerPositionAfterSceneLoad());
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Missing player prefab in inspector!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        Transform spawnLocation = GetSpawnLocation();

        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnLocation.position, spawnLocation.rotation);

        PhotonNetwork.LocalPlayer.TagObject = player;
        localPlayerInstance = player;

        Debug.Log($"Spawned Player at {spawnLocation.name}");
    }

    IEnumerator ResetPlayerPositionAfterSceneLoad()
    {
        yield return new WaitForSeconds(0.2f);

        GameObject playerObj = PhotonNetwork.LocalPlayer.TagObject as GameObject;

        if (playerObj == null)
        {
            Debug.Log("Player object missing after scene load, respawning...");
            SpawnPlayer();
            yield break;
        }

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform spawnLocation = GetSpawnLocation();

            playerObj.transform.position = spawnLocation.position;
            playerObj.transform.rotation = spawnLocation.rotation;

            Debug.Log($"Reset Player to spawn point {spawnLocation.name}");
        }
        else
        {
            Debug.LogWarning("No spawn points found in this scene to reset position.");
        }
    }

    Transform GetSpawnLocation()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return transform;

        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    public override void OnEnable()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
