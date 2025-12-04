using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class AISpawn : MonoBehaviourPun
{
    [Header("AI Spawning")]
    public string AI;
    public Transform[] spawnpoint;
    public int AIcount;
    public int milestone = 0;

    public GameManager gameManager;


    void Start()
    {

    }
    void Update()
    {
        // Only Master Client calls the RPC
        if (PhotonNetwork.IsMasterClient)
        {
            if (gameManager.currentTrashCount == milestone)
            {
                photonView.RPC("SpawnAI", RpcTarget.AllBuffered);
                milestone += 10;
            }
        }
    }

    [PunRPC]
    void SpawnAI()
    {
        // Only Master Client should instantiate
        if (PhotonNetwork.IsMasterClient)
        {
            AIcount = 0;
            do
            {
                int index = Random.Range(0, spawnpoint.Length);
                Transform point = spawnpoint[index];

                PhotonNetwork.Instantiate(AI, point.position, point.rotation);
                Debug.Log("Success");
                ++AIcount;
            }
            while (AIcount != 3);
        }


    }
}