using UnityEngine;

// Mob that paces a stretch of the sidescroller lane, breaks into a chase when it actually sees the
// player, and gives up at a no-go trigger before turning round and pacing again. Built on the same
// lane model as EntityScript: the lane axis is steered by hand, the vertical is left to the solver
// so gravity and ground still behave.
[RequireComponent(typeof(Rigidbody))]
public class MobRoamChase : MonoBehaviour
{
    public enum State { Roaming, Chasing, Blocked }

    [Header("Target")]
    [Tooltip("Left empty, the player is found from the MouseDirectionController in the scene.")] public Transform player;

    [Header("Lane")]
    [Tooltip("Used only when the target has no MouseDirectionController to read the lane axis from.")] public Vector3 fallbackAxis = Vector3.right;

    [Header("Roaming")]
    public float roamSpeed = 2f;
    [Tooltip("How far either side of its spawn point the mob paces, measured along the lane.")] public float patrolDistance = 5f;
    [Tooltip("Seconds spent standing still at each end of the patrol before heading back.")] public float pauseAtTurn = 0.75f;
    [Tooltip("Forward probe that turns the mob early rather than letting it grind into a wall.")] public float wallProbeDistance = 0.6f;

    [Header("Chase")]
    public float chaseSpeed = 4.5f;
    [Tooltip("Dead band, so the mob settles instead of shoving and jittering once it is on top of the player.")] public float stopDistance = 1f;
    public float acceleration = 20f;

    [Header("Vision")]
    public float sightRange = 8f;
    [Tooltip("Kept above Sight Range so a chase does not flicker on and off at the edge of detection.")] public float chaseRange = 11f;
    [Tooltip("Total cone width in degrees, centred on the way the mob is facing. 360 sees all round.")] [Range(1f, 360f)] public float viewAngle = 110f;
    [Tooltip("Grace period before a chase is dropped, so a lamppost passing between them is not enough.")] public float loseSightDelay = 1.5f;
    [Tooltip("Seconds between sight checks. The chase itself still updates every physics step.")] public float visionInterval = 0.1f;
    [Tooltip("What blocks line of sight. Set this to your wall layer, or the mob sees through everything.")] public LayerMask obstacleMask = 0;
    [Tooltip("Height the mob looks from, and the height on the player it looks at.")] public float eyeHeight = 0.5f;
    public float targetHeight = 0.5f;

    [Header("No-Go Zones")]
    [Tooltip("Tag on the isTrigger colliders the mob refuses to enter.")] public string noGoTag = "MobNoGo";
    [Tooltip("Seconds the mob stands still after hitting a no-go zone, before roaming away from it.")] public float blockedPause = 0.5f;

    [Header("Facing")]
    [Tooltip("Mirrored instead of the root, so a negative scale never reaches the Rigidbody's collider. Falls back to this object when left empty.")] public Transform spriteTransform;
    [Tooltip("Clear this if the art is drawn facing the other way down the lane.")] public bool spriteFacesPositiveAxis = true;

    private static readonly Vector3 Up = Vector3.up; // the property rebuilds a struct on every read

    private Rigidbody body;
    private Transform mirrorTransform;
    private Vector3 laneOrigin, axis;
    private float laneOriginAlongAxis, laneOriginAlongUp, patrolMin, patrolMax;
    private float currentSpeed, cosHalfView, visionTimer, lostSightTimer, pauseTimer, blockedTimer;
    private bool headingForward = true, hasSight;
    private int mirrorSign; // 0 until the first apply, so an authored scale is corrected on spawn
    private int noGoOverlaps; // overlaps are counted, so leaving one of two zones does not free the mob
    private State state = State.Roaming;

    public State CurrentState => state;
    public float CurrentSpeed => currentSpeed;
    public bool HasSight => hasSight;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.freezeRotation = true; // physics must not topple the mob; facing is driven by MoveRotation
        body.interpolation = RigidbodyInterpolation.Interpolate;

        // Falling back to a scene lookup keeps the prefab usable without wiring the player by hand.
        MouseDirectionController controller = player != null
            ? player.GetComponent<MouseDirectionController>()
            : FindFirstObjectByType<MouseDirectionController>();
        if (player == null && controller != null) player = controller.transform;

        // Pace the lane the level was built on, so corners and the player's axis agree.
        Vector3 source = controller != null ? controller.moveAxis : fallbackAxis;
        axis = source.sqrMagnitude > 0f ? source.normalized : Vector3.right;

        // Pre-projecting the origin keeps a dot product out of the lane clamp every step.
        laneOrigin = body.position;
        laneOriginAlongAxis = Vector3.Dot(laneOrigin, axis);
        laneOriginAlongUp = laneOrigin.y;
        patrolMin = laneOriginAlongAxis - patrolDistance;
        patrolMax = laneOriginAlongAxis + patrolDistance;

        // Mirroring a child keeps the negative scale away from the Rigidbody's own collider.
        mirrorTransform = spriteTransform != null ? spriteTransform : transform;

        cosHalfView = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);

        ApplyMirror(); // so art authored facing the wrong way is already right on the first frame

        // No player is not fatal the way it is for the giant: the mob still has a patrol to walk.
        if (player == null) Debug.LogWarning(name + ": no player found, so it will only roam.", this);
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        Vector3 position = body.position, velocity = body.linearVelocity; // native reads, so once each

        UpdateSight(position, dt);

        float targetSpeed;
        switch (state)
        {
            case State.Blocked:
                blockedTimer -= dt;
                if (blockedTimer <= 0f) state = State.Roaming;
                targetSpeed = 0f;
                break;

            case State.Chasing:
                // Held from the last sighting, so a chase survives brief cover before being dropped.
                if (hasSight) lostSightTimer = 0f;
                else if ((lostSightTimer += dt) >= loseSightDelay) { state = State.Roaming; pauseTimer = 0f; }
                targetSpeed = state == State.Chasing ? ChaseSpeed(position) : RoamSpeed(position, dt);
                break;

            default:
                if (hasSight) { state = State.Chasing; lostSightTimer = 0f; targetSpeed = ChaseSpeed(position); }
                else targetSpeed = RoamSpeed(position, dt);
                break;
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * dt);

        // Rebuilt rather than AddForce: a fixed pace is the point, so drag or a shove from the player
        // must not slow it. Vertical is left to the solver so gravity and ground still behave.
        Vector3 move = axis * currentSpeed;
        move.y += velocity.y;
        body.linearVelocity = move;

        ClampToLane(position);
        FacePlayer();
        ApplyMirror();
    }

    // Throttled, because a cone test plus a linecast every physics step buys nothing at these speeds.
    // The lose-sight timer in FixedUpdate still runs every step off the cached result.
    private void UpdateSight(Vector3 position, float dt)
    {
        // Standing in a zone it is not allowed to enter, the mob must not re-acquire and walk back in.
        if (player == null || state == State.Blocked || noGoOverlaps > 0) { hasSight = false; return; }

        if (visionTimer <= 0f)
        {
            visionTimer = visionInterval;
            hasSight = CanSeePlayer(position);
        }
        visionTimer -= dt;
    }

    private bool CanSeePlayer(Vector3 position)
    {
        Vector3 eye = position + Up * eyeHeight, targetEye = player.position + Up * targetHeight;
        Vector3 toTarget = targetEye - eye;

        // Chase range is the wider one, so the mob keeps hold of a player it is already running down.
        float range = state == State.Chasing ? chaseRange : sightRange;
        float sqrDistance = toTarget.sqrMagnitude;
        if (sqrDistance > range * range) return false;

        float distance = Mathf.Sqrt(sqrDistance);
        // Practically on top of each other, the direction is noise, so only the line of sight matters.
        if (distance > 1e-4f)
        {
            Vector3 facing = headingForward ? axis : -axis;
            if (Vector3.Dot(toTarget / distance, facing) < cosHalfView) return false;
        }

        return !Physics.Linecast(eye, targetEye, obstacleMask, QueryTriggerInteraction.Ignore);
    }

    // Only the lane component of the gap matters; a height difference must not slow the chase.
    private float ChaseSpeed(Vector3 position)
    {
        float gap = Vector3.Dot(player.position - position, axis);
        return gap > stopDistance ? chaseSpeed : gap < -stopDistance ? -chaseSpeed : 0f;
    }

    private float RoamSpeed(Vector3 position, float dt)
    {
        if (pauseTimer > 0f) { pauseTimer -= dt; return 0f; }

        float along = Vector3.Dot(position, axis);
        if (headingForward ? along >= patrolMax : along <= patrolMin) { TurnAround(); return 0f; }
        if (WallAhead(position)) { TurnAround(); return 0f; }

        return headingForward ? roamSpeed : -roamSpeed;
    }

    private bool WallAhead(Vector3 position)
    {
        if (wallProbeDistance <= 0f) return false;
        Vector3 facing = headingForward ? axis : -axis;
        return Physics.Raycast(position + Up * eyeHeight, facing, wallProbeDistance, obstacleMask,
            QueryTriggerInteraction.Ignore);
    }

    private void TurnAround()
    {
        headingForward = !headingForward;
        pauseTimer = pauseAtTurn;
        currentSpeed = 0f; // snapped, so the mob does not coast past the end of its patrol
    }

    // Trigger callbacks reach both parties, so a zone needs nothing on it but isTrigger and the tag.
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(noGoTag)) return;
        noGoOverlaps++;

        state = State.Blocked;
        blockedTimer = blockedPause;
        currentSpeed = 0f;
        lostSightTimer = 0f;
        hasSight = false;
        pauseTimer = 0f;

        // Face back the way it came, so the pause is followed by walking off rather than further in.
        headingForward = Vector3.Dot(other.bounds.center - body.position, axis) < 0f;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(noGoTag) && noGoOverlaps > 0) noGoOverlaps--;
    }

    // Collisions nudge the mob off the sidescroller plane, so strip any offset that is not along the
    // move axis or straight up, measured against the origin projected back in Awake.
    private void ClampToLane(Vector3 position)
    {
        Vector3 drift = position - laneOrigin
            - axis * (Vector3.Dot(position, axis) - laneOriginAlongAxis)
            - Up * (position.y - laneOriginAlongUp);
        if (drift.sqrMagnitude > 1e-6f) body.MovePosition(position - drift);
    }

    // The sprite is held square to however the player is oriented, so a corner turn carries the mob
    // round with it. Which way along the lane it is actually walking is carried by the mirror, not by
    // this rotation, which is why the vision cone reads headingForward and not transform.forward.
    private void FacePlayer()
    {
        if (player == null) return;

        Quaternion look = player.rotation, rotation = body.rotation;
        // Equal for all but the few frames of a corner turn, so bailing saves a native write.
        if (Mathf.Abs(Quaternion.Dot(rotation, look)) <= 0.99999f) body.MoveRotation(look);
    }

    // Same flip MouseDirectionController uses on the player: the sign of scale.x carries the facing.
    private void ApplyMirror()
    {
        int sign = headingForward == spriteFacesPositiveAxis ? 1 : -1;
        if (sign == mirrorSign) return; // so only the turns cost a native scale write
        mirrorSign = sign;

        Vector3 scale = mirrorTransform.localScale;
        scale.x = Mathf.Abs(scale.x) * sign;
        mirrorTransform.localScale = scale;
    }

    // Editor-only: keeps the inspector from producing values that break the math.
    private void OnValidate()
    {
        roamSpeed = Mathf.Max(0f, roamSpeed);
        chaseSpeed = Mathf.Max(0f, chaseSpeed);
        patrolDistance = Mathf.Max(0f, patrolDistance);
        pauseAtTurn = Mathf.Max(0f, pauseAtTurn);
        wallProbeDistance = Mathf.Max(0f, wallProbeDistance);
        stopDistance = Mathf.Max(0f, stopDistance);
        acceleration = Mathf.Max(0.01f, acceleration);
        sightRange = Mathf.Max(0f, sightRange);
        chaseRange = Mathf.Max(sightRange, chaseRange); // narrower than sight range would flicker
        loseSightDelay = Mathf.Max(0f, loseSightDelay);
        visionInterval = Mathf.Max(0.01f, visionInterval);
        blockedPause = Mathf.Max(0f, blockedPause);
        if (fallbackAxis.sqrMagnitude <= 0f) fallbackAxis = Vector3.right;

        cosHalfView = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad); // so cone tweaks apply while playing
    }

    // There is no test harness here, so the ranges, cone and patrol are drawn to be tuned by eye.
    private void OnDrawGizmosSelected()
    {
        Vector3 drawAxis = Application.isPlaying && axis.sqrMagnitude > 0f
            ? axis
            : (fallbackAxis.sqrMagnitude > 0f ? fallbackAxis.normalized : Vector3.right);
        Vector3 eye = transform.position + Up * eyeHeight;
        Vector3 facing = Application.isPlaying && !headingForward ? -drawAxis : drawAxis;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eye, sightRange);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(eye, chaseRange);

        // Cone edges, swept in the plane the mob actually lives in: the lane axis and up.
        Gizmos.color = Color.red;
        float half = viewAngle * 0.5f;
        Vector3 planeNormal = Vector3.Cross(drawAxis, Up);
        planeNormal = planeNormal.sqrMagnitude > 0f ? planeNormal.normalized : Vector3.forward;
        Gizmos.DrawLine(eye, eye + Quaternion.AngleAxis(half, planeNormal) * facing * sightRange);
        Gizmos.DrawLine(eye, eye + Quaternion.AngleAxis(-half, planeNormal) * facing * sightRange);

        // In play mode the patrol is anchored to the spawn point, which the mob may have walked from.
        Vector3 centre = Application.isPlaying ? laneOrigin : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(centre - drawAxis * patrolDistance, centre + drawAxis * patrolDistance);
        Gizmos.DrawWireCube(centre - drawAxis * patrolDistance, Vector3.one * 0.2f);
        Gizmos.DrawWireCube(centre + drawAxis * patrolDistance, Vector3.one * 0.2f);
    }
}
