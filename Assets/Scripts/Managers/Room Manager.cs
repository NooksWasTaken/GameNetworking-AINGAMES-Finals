using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public GameObject player;
    public Transform spawn;

    void Start()
    {
        Debug.Log("Connecting...");

        // connects to master server using the settings from Photon Server Settings
        // (Assets > Photon > PhotonUnityNetworking > Resources)
        PhotonNetwork.ConnectUsingSettings(); 
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log("Connected to server!");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        // joins a specific room by name or creates it on demand
        // parameters needed are: name, roomOptions, typedLobby.
        // leaving the last 2 as null is fine for now probably
        PhotonNetwork.JoinOrCreateRoom("Placeholder", null, null);

        Debug.Log("Succesfully joined a lobby!");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("Succesfully joined a room!");

        // instantiate a new player upon joining a room
        GameObject _player = PhotonNetwork.Instantiate(player.name, spawn.position, Quaternion.identity);
    }
}
