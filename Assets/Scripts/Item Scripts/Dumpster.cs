using System.Collections;
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class Dumpster : MonoBehaviourPun
{
    [Header("Detection")]
    public BoxCollider boxCollider;
    private Collider[] overlapResults = new Collider[16];
    public LayerMask dumpsterItems;

    [Header("Smoke Effect")]
    public GameObject smokeEffectPrefab;
    public float smokeLifetime = 3f;

    private HashSet<PhotonView> processingTrash = new HashSet<PhotonView>();

    void Start()
    {
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider>();
    }

    void FixedUpdate()
    {
        DetectTrash();
    }

    void DetectTrash()
    {
        Vector3 center = boxCollider.bounds.center;
        Vector3 halfExtents = boxCollider.bounds.extents;
        Quaternion rotation = transform.rotation;

        int hits = Physics.OverlapBoxNonAlloc(center, halfExtents, overlapResults, rotation, dumpsterItems);

        for (int i = 0; i < hits; i++)
        {
            Collider col = overlapResults[i];
            if (col == null) continue;

            Trash trash = col.GetComponent<Trash>();
            if (trash == null || trash.photonView == null) continue;

            InteractableItem item = trash.GetComponent<InteractableItem>();
            if (item != null && item.isPickedUp) continue;

            if (processingTrash.Contains(trash.photonView)) continue;

            processingTrash.Add(trash.photonView);

            if (PhotonNetwork.IsMasterClient)
            {
                DestroyTrashImmediately(trash);
            }
            else
            {
                photonView.RPC(nameof(RPC_RequestDumpTrash), RpcTarget.MasterClient, trash.photonView.ViewID);
            }
        }
    }

    private void DestroyTrashImmediately(Trash trash)
    {
        if (trash == null || trash.photonView == null) return;

        PhotonView targetView = trash.photonView;

        if (!targetView.IsMine)
        {
            targetView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }

        Vector3 position = trash.transform.position;

        photonView.RPC(nameof(RPC_SpawnSmoke), RpcTarget.All, position);
        GameManager gm = FindFirstObjectByType<GameManager>();
        gm?.TrashDumped();

        PhotonNetwork.Destroy(targetView.gameObject);

        processingTrash.Remove(targetView);
    }

    [PunRPC]
    void RPC_RequestDumpTrash(int targetViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView targetView = PhotonView.Find(targetViewID);
        if (targetView == null) return;

        Trash trash = targetView.GetComponent<Trash>();
        if (trash != null)
        {
            if (processingTrash.Contains(targetView)) return;
            processingTrash.Add(targetView);

            DestroyTrashImmediately(trash);
        }
    }

    [PunRPC]
    void RPC_SpawnSmoke(Vector3 position)
    {
        if (smokeEffectPrefab != null)
        {
            SoundManager.PlaySound(SoundType.TRASH_ITEM);
            GameObject smoke = Instantiate(smokeEffectPrefab, position, Quaternion.identity);
            Destroy(smoke, smokeLifetime);
        }
    }
    private void OnDrawGizmos()
    {
        if (boxCollider == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(boxCollider.bounds.center, boxCollider.bounds.size);
    }
}
