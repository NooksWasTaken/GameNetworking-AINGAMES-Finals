using UnityEngine;
using Photon.Pun;

public class EquipmentVacuum : EquipmentBase
{
    [Header("Vacuum Settings")]
    public Transform vacuumCenter;
    public float vacuumRadius = 3f;
    public float cleanRate = 0.5f;
    public LayerMask dirtMask;

    private PhotonView ownerView;

    void Start()
    {
        ownerView = GetComponentInParent<PhotonView>();
    }

    void Update()
    {
        if (!ownerView.IsMine) return;

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("VACUUM LOOP START");
            SoundManager.PlayLoopingSound(SoundType.VACUUM, (float)0.8);
        }

        if (Input.GetMouseButtonUp(0))
        {
            SoundManager.StopLoopingSound(SoundType.VACUUM);
        }

        if (Input.GetMouseButton(0))
        {
            VacuumDirt();
        }
    }

    void VacuumDirt()
    {
        Vector3 center = vacuumCenter ? vacuumCenter.position : transform.position;

        Collider[] hits = Physics.OverlapSphere(center, vacuumRadius, dirtMask);

        foreach (Collider hit in hits)
        {
            Dirt dirt = hit.GetComponent<Dirt>();
            if (dirt != null)
            {
                dirt.CleanUp(cleanRate * Time.deltaTime);

                if (dirt.cleanliness <= 0f)
                    GameManager.Instance?.OnDirtCleaned();
            }
        }
    }

    public override void DisableEquipment()
    {
        // Stop vacuum sound immediately
        SoundManager.StopLoopingSound(SoundType.VACUUM);
        base.DisableEquipment();
    }

    private void OnDestroy()
    {
        // Ensure sound stops if object is destroyed
        SoundManager.StopLoopingSound(SoundType.VACUUM);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (vacuumCenter != null)
            Gizmos.DrawWireSphere(vacuumCenter.position, vacuumRadius);
    }
}
