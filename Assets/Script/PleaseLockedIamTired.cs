using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PleaseLockedIamTired : MonoBehaviour
{
    [Header("PLEASE lock the axis")]
    public bool lockX = false;
    public float xValue = 0f;
    public bool lockZ = true;
    public float zValue = -35f;

    [Header("Optional")]
    public Vector3 newMoveAxis = Vector3.zero;

    private MouseDirectionController rat;

    private void Reset() { GetComponent<BoxCollider>().isTrigger = true; }

    private void OnTriggerEnter(Collider other)
    {
        var m = other.GetComponentInParent<MouseDirectionController>();
        if (m == null) return;

        rat = m;
        Snap();
        //snapped position
        rat.SetMoveAxis(newMoveAxis != Vector3.zero ? newMoveAxis : rat.moveAxis);
    }

    private void OnTriggerExit(Collider other)
    {
        if (rat != null && other.GetComponentInParent<MouseDirectionController>() == rat) rat = null;
    }

    private void LateUpdate() { if (rat != null) Snap(); }

    private void Snap()
    {
        Vector3 p = rat.transform.position;
        if (lockX) p.x = xValue;
        if (lockZ) p.z = zValue;
        if (p != rat.transform.position)
        {
            rat.transform.position = p;
            Physics.SyncTransforms(); //characterController accept the new position
        }
    }
}