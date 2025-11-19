using UnityEngine;
using Photon.Pun;

public class PlayerSync : MonoBehaviour, IPunObservable
{
    private PhotonView pv;
    private Rigidbody rb;

    private Vector3 remotePos;
    private float remoteYRot;
    private Vector3 remoteVel;

    public float lerpSpeed = 10f;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!pv.IsMine)
        {
            // smooth position
            rb.MovePosition(Vector3.Lerp(rb.position, remotePos, Time.fixedDeltaTime * lerpSpeed));

            // smooth Y-axis rotation only
            Quaternion targetRot = Quaternion.Euler(0f, remoteYRot, 0f);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * lerpSpeed));

            // apply synced velocity for physics consistency
            rb.linearVelocity = remoteVel;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation.eulerAngles.y);
            stream.SendNext(rb.linearVelocity);
        }
        else
        {
            remotePos = (Vector3)stream.ReceiveNext();
            remoteYRot = (float)stream.ReceiveNext();
            remoteVel = (Vector3)stream.ReceiveNext();
        }
    }
}
