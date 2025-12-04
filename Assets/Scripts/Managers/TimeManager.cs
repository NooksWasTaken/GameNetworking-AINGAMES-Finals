using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class TimeManager : MonoBehaviourPunCallbacks
{
    [Header("Timer Settings")]
    public float countdownDuration = 60f;
    [Tooltip("Delay before the timer starts (seconds)")]
    public float startDelay = 5f;
    public TextMeshProUGUI timerText;

    [Header("Game Over Screen")]
    public GameObject GameOverScreen;

    private const string ROOM_START_TIME_KEY = "StartTime";
    private float remainingTime;
    private bool timerStarted = false;
    private bool gameOverTriggered = false;

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Master schedules timer start
            float startTime = (float)PhotonNetwork.Time + startDelay;
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable() { { ROOM_START_TIME_KEY, startTime } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
        else
        {
            // Non-master players check if start time already exists
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ROOM_START_TIME_KEY))
            {
                float startTime = (float)PhotonNetwork.CurrentRoom.CustomProperties[ROOM_START_TIME_KEY];
                timerStarted = PhotonNetwork.Time >= startTime;
            }
        }
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ROOM_START_TIME_KEY)) return;

        float startTime = (float)PhotonNetwork.CurrentRoom.CustomProperties[ROOM_START_TIME_KEY];
        float elapsedSinceStart = (float)PhotonNetwork.Time - startTime;

        if (elapsedSinceStart >= 0f)
        {
            timerStarted = true;
            remainingTime = Mathf.Max(0f, countdownDuration - elapsedSinceStart);

            // Play BGM with adjustable volume
            SoundManager.PlayLoopingSound(SoundType.BGM, 0.5f);

            // Update timer text
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }

            // Trigger game over when timer reaches 0 (once)
            if (remainingTime <= 0f && !gameOverTriggered)
            {
                gameOverTriggered = true;

                // Only the master client sends the RPC
                if (PhotonNetwork.IsMasterClient)
                    photonView.RPC("RPC_GameOver", RpcTarget.All);
            }
        }
        else
        {
            // Timer hasn't started yet, show countdown to start
            if (timerText != null)
            {
                int secondsUntilStart = Mathf.CeilToInt(-elapsedSinceStart);
                timerText.text = $"Starting in: {secondsUntilStart}";
            }
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(ROOM_START_TIME_KEY))
        {
            float startTime = (float)propertiesThatChanged[ROOM_START_TIME_KEY];
            timerStarted = PhotonNetwork.Time >= startTime;
        }
    }

    public bool IsTimerFinished()
    {
        return timerStarted && remainingTime <= 0f;
    }

    [PunRPC]
    private void RPC_GameOver()
    {
        if (GameOverScreen != null)
            GameOverScreen.SetActive(true);

        RB_PlayerMove[] players = GameObject.FindObjectsByType<RB_PlayerMove>(FindObjectsSortMode.None);
        foreach (var player in players)
            player.enabled = false;

        SoundManager.StopLoopingSound(SoundType.BGM);
    }
}
