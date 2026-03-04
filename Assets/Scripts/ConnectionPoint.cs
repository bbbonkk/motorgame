using UnityEngine;

public class ConnectionPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out ConnectionPoint otherPoint))
            return;

        Rigidbody myRb = GetComponentInParent<Rigidbody>();
        Rigidbody otherRb = otherPoint.GetComponentInParent<Rigidbody>();

        if (myRb == null || otherRb == null)
            return;

        // Prevent snapping to itself
        if (myRb == otherRb)
            return;

        SnapObjects(myRb, otherPoint);
    }

    private void SnapObjects(Rigidbody myRb, ConnectionPoint targetPoint)
    {
        if (myRb.GetComponent<FixedJoint>() != null)
            return;

        Transform myPoint = transform;
        Transform target = targetPoint.transform;

        // Calculate position difference
        Vector3 positionOffset = target.position - myPoint.position;
        myRb.transform.position += positionOffset;

        // Calculate rotation difference
        Quaternion rotationOffset = target.rotation * Quaternion.Inverse(myPoint.rotation);
        myRb.transform.rotation = rotationOffset * myRb.transform.rotation;

        FixedJoint joint = myRb.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = targetPoint.GetComponentInParent<Rigidbody>();

        joint.breakForce = 150f;
        joint.breakTorque = 150f;

        // Force release from player if being held
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.ForceRelease(myRb);
        }

        Debug.Log("Snapped instantly!");
    }
}