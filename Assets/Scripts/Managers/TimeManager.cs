using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEditor;

public class TimeManager : MonoBehaviourPunCallbacks
{
    [Header("Timer Settings")]
    public float countdownDuration = 60f;
    public float preMatchCountdown = 5f;
    public TextMeshProUGUI timerText;

    [Header("Game Over")]
    public GameObject GameOverScreen;

    [Header("Camera")]
    public GameObject screenCamera;

    private double startTime;
    private float currentTime;
    private bool timerRunning = false;
    private bool preMatchRunning = false;
    private bool bgmPlaying = false;
    private bool gameOverTriggered = false;

    private bool timerInitialized = false;

    void Awake()
    {
        if (timerText != null)
            timerText.text = "";

        if (GameOverScreen != null)
            GameOverScreen.SetActive(false);
    }

    void Start()
    {
        // If the player is already in a room when this scene loads
        if (PhotonNetwork.InRoom)
        {
            TryInitializeTimer();
            screenCamera.SetActive(false);
        }
    }

    void Update()
    {
        if (preMatchRunning)
        {
            double elapsedPre = PhotonNetwork.Time - startTime;
            float remainingPre = Mathf.Max(0f, preMatchCountdown - (float)elapsedPre);

            if (timerText != null)
                timerText.text = $"Starting in: {Mathf.CeilToInt(remainingPre)}";

            if (remainingPre <= 0f)
            {
                preMatchRunning = false;
                StartMainTimer();
            }
        }

        if (timerRunning)
        {
            double elapsed = PhotonNetwork.Time - startTime - preMatchCountdown;
            currentTime = Mathf.Max(0f, countdownDuration - (float)elapsed);

            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(currentTime / 60f);
                int seconds = Mathf.FloorToInt(currentTime % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }

            if (!bgmPlaying)
            {
                SoundManager.PlayLoopingSound(SoundType.BGM, 0.5f);
                bgmPlaying = true;
            }

            if (currentTime <= 0 && !gameOverTriggered)
            {
                gameOverTriggered = true;
                if (PhotonNetwork.IsMasterClient)
                    photonView.RPC(nameof(RPC_GameOver), RpcTarget.AllBuffered);
            }
        }
    }

    public override void OnJoinedRoom()
    {
        TryInitializeTimer();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        TryInitializeTimer();
    }

    private void TryInitializeTimer()
    {
        if (!timerInitialized && PhotonNetwork.IsMasterClient)
        {
            timerInitialized = true;
            double networkTime = PhotonNetwork.Time + 0.1; // small offset to ensure full join
            photonView.RPC(nameof(RPC_StartTimer), RpcTarget.AllBuffered, networkTime);
        }
    }

    [PunRPC]
    void RPC_StartTimer(double networkTimeFromMaster)
    {
        startTime = networkTimeFromMaster;
        preMatchRunning = true;
        timerRunning = false;
        bgmPlaying = false;
        gameOverTriggered = false;

        if (GameOverScreen != null)
            GameOverScreen.SetActive(false);
    }

    private void StartMainTimer()
    {
        timerRunning = true;
        currentTime = countdownDuration;
    }

    [PunRPC]
    void RPC_GameOver()
    {
        timerRunning = false;
        preMatchRunning = false;

        Time.timeScale = 0f;

        if (GameOverScreen != null)
            GameOverScreen.SetActive(true);

        screenCamera.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SoundManager.StopLoopingSound(SoundType.BGM);
        SoundManager.StopLoopingSound(SoundType.WALK);
        SoundManager.StopLoopingSound(SoundType.RUN);

        var players = GameObject.FindObjectsByType<RB_PlayerMove>(FindObjectsSortMode.None);
        foreach (var p in players)
            p.gameObject.SetActive(false);
        Debug.Log("Disabled Player!");
    }
}
