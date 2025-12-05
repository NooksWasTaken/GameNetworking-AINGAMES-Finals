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

    // Prevent repeated processing on MasterClient
    private HashSet<PhotonView> processingTrashMaster = new HashSet<PhotonView>();

    // Remote clients track which ViewIDs they've already requested
    private HashSet<int> requestedTrashIDs = new HashSet<int>();

    void Start()
    {
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider>();
    }

    void FixedUpdate()
    {
        // Only detect on MasterClient or mark requests on remote
        if (PhotonNetwork.IsMasterClient)
        {
            DetectAndDestroyTrashMaster();
        }
        else
        {
            DetectAndRequestTrashRemote();
        }
    }

    #region MasterClient Logic
    private void DetectAndDestroyTrashMaster()
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
            if (item != null && item.isPickedUp) continue; // skip held items

            // Only process each trash once
            if (!processingTrashMaster.Contains(trash.photonView))
            {
                processingTrashMaster.Add(trash.photonView);
                DestroyTrashImmediately(trash);
            }
        }
    }

    private void DestroyTrashImmediately(Trash trash)
    {
        if (trash == null || trash.photonView == null) return;

        PhotonView targetView = trash.photonView;

        // Ensure MasterClient owns it
        if (!targetView.IsMine)
            targetView.TransferOwnership(PhotonNetwork.LocalPlayer);

        Vector3 position = trash.transform.position;

        // Spawn smoke and increment score immediately
        photonView.RPC(nameof(RPC_SpawnSmoke), RpcTarget.All, position);
        GameManager gm = FindFirstObjectByType<GameManager>();
        gm?.TrashDumped();

        // Destroy the object across all clients
        PhotonNetwork.Destroy(targetView.gameObject);

        // Clean up
        processingTrashMaster.Remove(targetView);
    }
    #endregion

    #region Remote Client Logic
    private void DetectAndRequestTrashRemote()
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

            int viewID = trash.photonView.ViewID;
            if (!requestedTrashIDs.Contains(viewID))
            {
                requestedTrashIDs.Add(viewID);
                photonView.RPC(nameof(RPC_RequestDumpTrash), RpcTarget.MasterClient, viewID);
            }
        }
    }
    #endregion

    [PunRPC]
    void RPC_RequestDumpTrash(int targetViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView targetView = PhotonView.Find(targetViewID);
        if (targetView == null) return;

        Trash trash = targetView.GetComponent<Trash>();
        if (trash != null && !processingTrashMaster.Contains(targetView))
        {
            processingTrashMaster.Add(targetView);
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
