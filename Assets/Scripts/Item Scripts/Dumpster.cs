using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Dumpster : MonoBehaviourPun
{
    [Header("Detection")]
    public BoxCollider boxCollider;
    public LayerMask dumpsterItems;
    private Collider[] overlapResults = new Collider[16];

    [Header("Smoke Effect")]
    public GameObject smokeEffectPrefab;
    public float smokeLifetime = 3f;

    private HashSet<PhotonView> processingTrash = new HashSet<PhotonView>();

    void Start()
    {
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider>();
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

            if (!PhotonNetwork.IsMasterClient)
            {
                if (!processingTrash.Contains(trash.photonView))
                {
                    processingTrash.Add(trash.photonView);
                    photonView.RPC(nameof(RPC_RequestDumpTrash), RpcTarget.MasterClient, trash.photonView.ViewID);
                }
                continue;
            }

            if (!processingTrash.Contains(trash.photonView))
            {
                processingTrash.Add(trash.photonView);
                DestroyTrashImmediately(trash);
            }
        }
    }

    private void DestroyTrashImmediately(Trash trash)
    {
        if (trash == null || trash.photonView == null) return;

        PhotonView targetView = trash.photonView;

        if (!targetView.IsMine)
            targetView.TransferOwnership(PhotonNetwork.LocalPlayer);

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
        if (trash != null && !processingTrash.Contains(targetView))
        {
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

    private void OnTriggerEnter(Collider other)
    {
        Trash trash = other.GetComponent<Trash>();
        if (trash == null || trash.photonView == null) return;

        InteractableItem item = trash.GetComponent<InteractableItem>();
        if (item != null && item.isPickedUp) return;

        if (processingTrash.Contains(trash.photonView)) return;
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
