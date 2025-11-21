using UnityEngine;
using Photon.Pun;

public class ItemEquipable : MonoBehaviourPun, IPunObservable
{
    private Rigidbody rb;
    private Transform itemEquipPointTransform;

    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private Vector3 networkVelocity;
    private Vector3 networkAngularVelocity;

    private float followSpeed = 20f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // called when the player grabs the item
    public void Grab(Transform itemEquipPointTransform)
    {
        this.itemEquipPointTransform = itemEquipPointTransform;

        if (photonView != null)
        {
            photonView.RequestOwnership();
        }

        rb.linearDamping = 5f;
        rb.freezeRotation = true;
        rb.useGravity = false;
    }

    // called when the player drops the item
    public void Drop()
    {
        this.itemEquipPointTransform = null;

        transform.SetParent(null);
        rb.linearDamping = 0f;
        rb.useGravity = true;
        rb.freezeRotation = false;
    }

    private void LateUpdate()
    {
        if (itemEquipPointTransform != null && photonView.IsMine)
        {
            // smooth follow (same as ItemGrabable)
            Vector3 newPos = Vector3.Lerp(rb.position, itemEquipPointTransform.position, followSpeed * Time.fixedDeltaTime);
            Quaternion newRot = Quaternion.Lerp(rb.rotation, itemEquipPointTransform.rotation, followSpeed * Time.fixedDeltaTime);

            rb.MovePosition(newPos);
            rb.MoveRotation(newRot);
        }
        else if (!photonView.IsMine)
        {
            // remote smoothing
            rb.MovePosition(Vector3.Lerp(rb.position, networkPosition, followSpeed * Time.fixedDeltaTime));
            rb.MoveRotation(Quaternion.Lerp(rb.rotation, networkRotation, followSpeed * Time.fixedDeltaTime));

            // apply synced velocity to keep physics aligned
            rb.linearVelocity = networkVelocity;
            rb.angularVelocity = networkAngularVelocity;
        }
    }

    // Photon sync
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // send full rigidbody state
            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation);
            stream.SendNext(rb.linearVelocity);
            stream.SendNext(rb.angularVelocity);
        }
        else
        {
            // receive state
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkVelocity = (Vector3)stream.ReceiveNext();
            networkAngularVelocity = (Vector3)stream.ReceiveNext();
        }
    }
}
