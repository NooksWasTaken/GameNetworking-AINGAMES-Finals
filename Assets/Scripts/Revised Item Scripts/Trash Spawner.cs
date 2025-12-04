using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections;

public class ItemSpawner : MonoBehaviourPun
{
    [Header("Items to Spawn")]
    public GameObject[] prefabReferences;
    public int numberOfItems = 20;

    [Header("Spawn Area")]
    public Vector3 areaCenter;
    public Vector3 areaSize;

    private const string SpawnFlag = "itemsSpawned";

    private void Start()
    {
        areaCenter = transform.position;
        StartCoroutine(SpawnWhenReady());
    }

    private IEnumerator SpawnWhenReady()
    {
        while (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            yield return null;
        }

        if (!PhotonNetwork.IsMasterClient) yield break;

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(SpawnFlag)) yield break;

        for (int i = 0; i < numberOfItems; i++)
        {
            GameObject selected = prefabReferences[Random.Range(0, prefabReferences.Length)];
            string prefabName = selected.name;

            Vector3 pos = GetRandomPosition();
            PhotonNetwork.Instantiate(prefabName, pos, Quaternion.identity);
        }

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { SpawnFlag, true }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

    }

    private Vector3 GetRandomPosition()
    {
        return areaCenter + new Vector3(
            Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
            Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f),
            Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f)
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(areaCenter, areaSize);
    }
}
