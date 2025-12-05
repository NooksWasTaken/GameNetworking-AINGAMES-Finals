using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [Header("Player Settings")]
    public GameObject playerPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private GameObject localPlayerInstance;

    public override void OnEnable()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.InRoom)
        {
            TrySpawnPlayer();
        }

        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnJoinedRoom()
    {
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

        GameObject player = PhotonNetwork.Instantiate(
            playerPrefab.name,
            spawnLocation.position,
            spawnLocation.rotation
        );

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
            SpawnPlayer();
            yield break;
        }

        Transform spawnLocation = GetSpawnLocation();
        playerObj.transform.SetPositionAndRotation(spawnLocation.position, spawnLocation.rotation);
    }

    Transform GetSpawnLocation()
    {
        return spawnPoints.Length == 0 ? transform : spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}
