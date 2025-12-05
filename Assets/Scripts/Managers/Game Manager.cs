using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class GameManager : MonoBehaviourPun
{
    [Header("UI Elements")]
    public Image trashFillImage;
    public TMP_Text trashPercentageText;

    [Header("Trash Settings")]
    public int maxTrashCount = 10;
    public int currentTrashCount = 0;

    [Header("Win Screen")]
    public GameObject WinScreen;

    [Header("Camera")]
    public GameObject screenCamera;

    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        UpdateFill();
        screenCamera.SetActive(false);
    }

    public void TrashDumped()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_IncrementTrashCounter), RpcTarget.AllBuffered);
        }
    }

    public void OnDirtCleaned()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_IncrementTrashCounter), RpcTarget.AllBuffered);
        }
    }


    // Call this function for when the AI needs to hinder cleaning progress
    // GameManager.Instance?.OnDirtAdded(); <- Use this line, no reference in your AI script needed
    public void OnDirtAdded()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_DecreaseTrashCounter), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void RPC_IncrementTrashCounter()
    {
        currentTrashCount++;
        currentTrashCount = Mathf.Clamp(currentTrashCount, 0, maxTrashCount);
        UpdateFill();
    }

    [PunRPC]
    void RPC_DecreaseTrashCounter()
    {
        currentTrashCount--;
        currentTrashCount = Mathf.Clamp(currentTrashCount, 0, maxTrashCount);
        UpdateFill();
    }

    void UpdateFill()
    {
        if (trashFillImage != null)
        {
            trashFillImage.fillAmount = (float)currentTrashCount / maxTrashCount;

        if (trashFillImage.fillAmount >= 1f && WinScreen != null)
            {
                WinScreen.SetActive(true);
                screenCamera.SetActive(true);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                if (PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC("RPC_GameWin", RpcTarget.All);
                }
                    
            }
        }

        if (trashPercentageText != null)
        {
            float percentage = ((float)currentTrashCount / maxTrashCount) * 100f;
            trashPercentageText.text = Mathf.RoundToInt(percentage) + "%";
        }
    }

    [PunRPC]
    private void RPC_GameWin()
    {
        RB_PlayerMove[] players = GameObject.FindObjectsByType<RB_PlayerMove>(FindObjectsSortMode.None);

        foreach (var player in players)
            player.gameObject.SetActive(false);

        Time.timeScale = 0f;

        SoundManager.StopLoopingSound(SoundType.BGM);
    }

}
