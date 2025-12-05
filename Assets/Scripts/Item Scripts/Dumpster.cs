using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Dumpster : MonoBehaviourPun
{
    [Header("Detection")]
    public BoxCollider boxCollider;
    private Collider[] overlapResults = new Collider[16];
    public LayerMask dumpsterItems;

    [Header("Smoke Effect")]
    public GameObject smokeEffectPrefab;
    public float smokeLifetime = 3f;

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
            if (trash == null) continue;

            InteractableItem item = trash.GetComponent<InteractableItem>();
            if (item != null && item.isPickedUp) continue;

            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(DestroyTrashSafely(trash));
            }
            else
            {
                photonView.RPC(nameof(RPC_RequestDumpTrash), RpcTarget.MasterClient, trash.photonView.ViewID);
            }
        }
    }

    private IEnumerator DestroyTrashSafely(Trash trash)
    {
        if (trash == null || trash.photonView == null) yield break;

        PhotonView targetView = trash.photonView;
        if (!targetView.IsMine)
        {
            targetView.TransferOwnership(PhotonNetwork.LocalPlayer);
            yield return null;
        }

        Collider col = trash.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Vector3 position = trash.transform.position;

        PhotonNetwork.Destroy(targetView.gameObject);

        GameManager gm = FindFirstObjectByType<GameManager>();
        gm?.TrashDumped();

        photonView.RPC(nameof(RPC_SpawnSmoke), RpcTarget.All, position);
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
            StartCoroutine(DestroyTrashSafely(trash));
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
