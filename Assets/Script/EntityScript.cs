using UnityEngine;

// Giant that hunts the player along the sidescroller lane, faster than their plain walk speed, so
// walking always loses ground and only the shake boost outruns it. Rigidbody driven: the lane axis
// is steered, the vertical stays with the solver so gravity and ground still work.
[RequireComponent(typeof(Rigidbody))]
public class EntityScript : MonoBehaviour
{
    [Header("Target")] public Transform player;

    [Header("Chase")]
    [Tooltip("Multiple of the player's walk speed. Keep it under their Shake Speed Multiplier or a boosting player can never break away.")] public float walkSpeedMultiplier = 1.25f;
    [Tooltip("Speed and axis used only when the target has no MouseDirectionController to read them from.")] public float fallbackSpeed = 7.5f;
    [Tooltip("Dead band, so the giant settles instead of shoving and jittering once it is on top of the player.")] public float stopDistance = 1.5f;
    public float acceleration = 25f;

    [Header("Lane")] public bool lockToLane = true;
    public Vector3 fallbackAxis = Vector3.right;

    [Header("Facing")] public bool faceTarget = true;
    public float turnSpeed = 720f;

    private static readonly Vector3 Up = Vector3.up; // the property rebuilds a struct on every read

    private Rigidbody body;
    private MouseDirectionController targetController;
    private Vector3 laneOrigin, axis;
    private Quaternion faceForward, faceBackward;
    private float currentSpeed, laneOriginAlongAxis, laneOriginAlongUp;
    private bool hasController, headingForward = true;

    public float ChaseSpeed => hasController ? targetController.moveSpeed * walkSpeedMultiplier : fallbackSpeed;
    public float CurrentSpeed => currentSpeed;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.freezeRotation = true; // physics must not topple the giant; facing is driven by MoveRotation
        body.interpolation = RigidbodyInterpolation.Interpolate;

        // Falling back to a scene lookup keeps the prefab usable without wiring the player by hand.
        if (player != null) targetController = player.GetComponent<MouseDirectionController>();
        else if ((targetController = FindObjectOfType<MouseDirectionController>()) != null) player = targetController.transform;
        hasController = targetController != null; // resolved once: Unity's null check is a native call

        // Chase along the player's own axis, so the giant stays in the lane the level was built on.
        Vector3 source = hasController ? targetController.moveAxis : fallbackAxis;
        axis = source.sqrMagnitude > 0f ? source.normalized : Vector3.right;

        // Pre-projecting the origin keeps a dot product out of the lane clamp every step.
        laneOrigin = body.position;
        laneOriginAlongAxis = Vector3.Dot(laneOrigin, axis);
        laneOriginAlongUp = laneOrigin.y;

        // Facing is only ever +axis or -axis, so build both now and keep LookRotation out of the loop.
        Vector3 worldUp = Mathf.Abs(Vector3.Dot(axis, Up)) > 0.999f ? Vector3.forward : Up;
        faceForward = Quaternion.LookRotation(axis, worldUp);
        faceBackward = Quaternion.LookRotation(-axis, worldUp);

        // Everything above still ran, so re-enabling this later works.
        if (player == null) { Debug.LogWarning($"{name}: no player found to chase, disabling.", this); enabled = false; }
    }

    private void FixedUpdate()
    {
        // Has to stay: a player destroyed mid-run would throw every step. Disabling pays it once.
        if (player == null) { enabled = false; return; }

        float dt = Time.fixedDeltaTime;
        Vector3 position = body.position, velocity = body.linearVelocity; // native reads, so once each

        // Only the lane component of the gap matters; height differences must not slow the chase.
        float gap = Vector3.Dot(player.position - position, axis);
        currentSpeed = Mathf.MoveTowards(currentSpeed,
            gap > stopDistance ? ChaseSpeed : gap < -stopDistance ? -ChaseSpeed : 0f, acceleration * dt);

        // Rebuilt rather than AddForce: a fixed chase speed is the point, so drag or a shove from the
        // player must not slow it. Vertical is left to the solver so gravity and ground still behave.
        Vector3 chase = axis * currentSpeed;
        chase.y += velocity.y;
        if (!lockToLane)
        {
            // Keep the sideways motion physics gave us; lane-locked, it gets dropped instead. The
            // velocity.y subtract beats zeroing drift.y, so a sloped move axis still works.
            Vector3 drift = velocity - axis * Vector3.Dot(velocity, axis);
            drift.y -= velocity.y;
            chase += drift;
        }
        body.linearVelocity = chase;

        if (lockToLane) ClampToLane(position);
        if (faceTarget) TurnTowardsChase(dt);
    }

    // Collisions nudge the giant off the sidescroller plane, so strip any offset that is not along
    // the move axis or straight up, measured against the origin projected back in Awake.
    private void ClampToLane(Vector3 position)
    {
        Vector3 drift = position - laneOrigin
            - axis * (Vector3.Dot(position, axis) - laneOriginAlongAxis)
            - Up * (position.y - laneOriginAlongUp);
        if (drift.sqrMagnitude > 1e-6f) body.MovePosition(position - drift);
    }

    // Heading is held from the last real one, so the dead band does not spin the giant around.
    private void TurnTowardsChase(float dt)
    {
        if (currentSpeed > 0.01f) headingForward = true;
        else if (currentSpeed < -0.01f) headingForward = false;

        Quaternion look = headingForward ? faceForward : faceBackward, rotation = body.rotation;
        // Aligned for most of a chase, and bailing saves a slerp plus a native rotation write.
        if (Mathf.Abs(Quaternion.Dot(rotation, look)) <= 0.99999f)
            body.MoveRotation(Quaternion.RotateTowards(rotation, look, turnSpeed * dt));
    }

    // Editor-only: keeps the inspector from producing values that break the math.
    private void OnValidate()
    {
        walkSpeedMultiplier = Mathf.Max(1.01f, walkSpeedMultiplier); // at 1x it can never close a gap
        acceleration = Mathf.Max(0.01f, acceleration);
        fallbackSpeed = Mathf.Max(0f, fallbackSpeed); stopDistance = Mathf.Max(0f, stopDistance); turnSpeed = Mathf.Max(0f, turnSpeed);
        if (fallbackAxis.sqrMagnitude <= 0f) fallbackAxis = Vector3.right;
    }
}
