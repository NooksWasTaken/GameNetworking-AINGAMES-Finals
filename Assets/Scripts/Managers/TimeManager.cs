using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class TimeManager : MonoBehaviourPunCallbacks
{
    [Header("Timer Settings")]
    public float countdownDuration = 60f; // seconds
    [Tooltip("Delay before the timer starts (seconds)")]
    public float startDelay = 5f;
    public TextMeshProUGUI timerText;


    private const string ROOM_START_TIME_KEY = "StartTime";
    private float remainingTime;
    private bool timerStarted = false;

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
            SoundManager.PlayLoopingSound(SoundType.BGM, (float)0.5);
            remainingTime = Mathf.Max(0f, countdownDuration - elapsedSinceStart);

            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
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
}
