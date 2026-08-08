using UnityEngine;

// Mouse-driven sidescroller movement: the character walks along one world axis towards whichever
// side of itself the cursor is on, and gets a short speed boost when the cursor is shaken there.
[RequireComponent(typeof(CharacterController))]
public class MouseDirectionController : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 moveAxis = Vector3.right;
    public float moveSpeed = 6f, acceleration = 40f, deceleration = 60f;

    [Header("Mouse")]
    public Camera targetCamera;
    public float deadZonePixels = 20f;

    [Header("Gravity")]
    public float gravity = -25f, groundedStick = -2f;

    [Header("Lane")]
    public bool lockToLane = true;

    [Header("Shake Boost")]
    public bool enableShakeBoost = true;
    public float shakeSpeedMultiplier = 1.8f;
    public int shakesToTrigger = 3;
    public float shakeWindow = 0.4f, minShakePixelsPerSecond = 500f, boostDuration = 2f, boostBlendSpeed = 6f;

    private CharacterController controller;
    private Transform cachedTransform;
    private Vector3 laneOrigin, axis;
    private Vector2 characterScreenPosition, screenAxisDirection;
    private float currentSpeed, verticalVelocity, lastCursorAxisPosition, lastStrokeSign, lastShakeTime, boostTimer, boostBlend;
    private bool hasCursorSample;
    private int shakeCount;

    public float SpeedMultiplier { get; set; } = 1f;
    public float CurrentSpeed => currentSpeed;
    public int MoveDirection => currentSpeed > 0.01f ? 1 : currentSpeed < -0.01f ? -1 : 0;
    public bool IsBoosting => boostTimer > 0f;
    public float ShakeBoost => Mathf.Lerp(1f, shakeSpeedMultiplier, boostBlend);

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        cachedTransform = transform;
        if (targetCamera == null) targetCamera = Camera.main;
        axis = moveAxis.sqrMagnitude > 0f ? moveAxis.normalized : Vector3.right;
        laneOrigin = cachedTransform.position;
        lastShakeTime = float.NegativeInfinity; // so the first reversal always starts a fresh streak
    }

    private void Update()
    {
        // Native property reads, so sample them once and pass them down.
        float dt = Time.deltaTime;
        Vector2 cursor = Input.mousePosition;

        // Without a usable projection there is no way to tell which side the cursor is on, so coast.
        bool canReadCursor = UpdateScreenProjection();
        int input = canReadCursor ? ReadMouseDirection(cursor) : 0;
        UpdateShakeBoost(input, cursor, canReadCursor, dt);

        // Two rates, so coasting to a stop feels different from driving into a direction.
        currentSpeed = Mathf.MoveTowards(currentSpeed, input * moveSpeed * SpeedMultiplier * ShakeBoost,
            (input == 0 ? deceleration : acceleration) * dt);

        // Clamped while grounded; letting gravity accumulate would launch us off the first ledge.
        verticalVelocity = controller.isGrounded && verticalVelocity < 0f ? groundedStick : verticalVelocity + gravity * dt;

        controller.Move((axis * currentSpeed + Vector3.up * verticalVelocity) * dt);
        if (lockToLane) ClampToLane();
    }

    // Caches the character's screen position and the screen direction of the move axis. False when
    // there is no camera, or the axis points straight at/away from it and projects to nothing.
    private bool UpdateScreenProjection()
    {
        if (targetCamera == null) return false;

        Vector3 position = cachedTransform.position;
        Vector3 origin = targetCamera.WorldToScreenPoint(position);
        Vector2 screenAxis = (Vector2)targetCamera.WorldToScreenPoint(position + axis) - (Vector2)origin;
        if (screenAxis.sqrMagnitude < 0.0001f) return false;

        characterScreenPosition = origin;
        screenAxisDirection = screenAxis.normalized;
        return true;
    }

    // Which side of the character the cursor is on, measured along the move axis rather than raw
    // screen X so rotated or tilted cameras still work.
    private int ReadMouseDirection(Vector2 cursor)
    {
        float distance = Vector2.Dot(cursor - characterScreenPosition, screenAxisDirection);
        return distance > deadZonePixels ? 1 : distance < -deadZonePixels ? -1 : 0;
    }

    // Waggling the cursor along the move axis, on the side the character already heads towards,
    // charges up a short speed boost.
    private void UpdateShakeBoost(int input, Vector2 cursor, bool canReadCursor, float dt)
    {
        // Ticked unconditionally so an in-flight boost still fades out if input or the feature drops.
        if (boostTimer > 0f) boostTimer -= dt;
        boostBlend = Mathf.MoveTowards(boostBlend, IsBoosting ? 1f : 0f, boostBlendSpeed * dt);

        // The cursor has to lead the movement; either side counts while still spinning up or turning.
        bool tracking = enableShakeBoost && canReadCursor && dt > 0f
            && input != 0 && (MoveDirection == 0 || input == MoveDirection);
        if (!tracking)
        {
            hasCursorSample = false;
            lastStrokeSign = 0f;
            shakeCount = 0;
            return;
        }

        // Raw cursor, not its offset from the character, so a fast character overtaking a still
        // cursor cannot fake a reversal.
        float cursorAxisPosition = Vector2.Dot(cursor, screenAxisDirection);
        float delta = cursorAxisPosition - lastCursorAxisPosition;
        bool hadSample = hasCursorSample;
        lastCursorAxisPosition = cursorAxisPosition;
        hasCursorSample = true;

        // Ignore drift and jitter. Scaling by dt is |delta| / dt < threshold without the divide.
        if (!hadSample || Mathf.Abs(delta) < minShakePixelsPerSecond * dt) return;

        // A shake is a change of stroke direction, not a stroke: left-right-left is two shakes.
        float strokeSign = delta > 0f ? 1f : -1f;
        if (lastStrokeSign != 0f && strokeSign != lastStrokeSign)
        {
            if (Time.time - lastShakeTime > shakeWindow) shakeCount = 0; // too slow, start a new streak
            lastShakeTime = Time.time;
            if (++shakeCount >= shakesToTrigger)
            {
                shakeCount = 0; // consume the streak so it retriggers every Nth reversal, not every one
                boostTimer = boostDuration;
            }
        }
        lastStrokeSign = strokeSign;
    }

    // 3D collisions can nudge the character off the sidescroller plane, so strip any movement that
    // is not along the move axis or straight up.
    private void ClampToLane()
    {
        Vector3 position = cachedTransform.position;
        Vector3 offset = position - laneOrigin;
        Vector3 drift = offset - axis * Vector3.Dot(offset, axis) - Vector3.up * Vector3.Dot(offset, Vector3.up);
        // Writing position resyncs the controller with the physics scene, so only when it drifted.
        if (drift.sqrMagnitude > 1e-6f) cachedTransform.position = position - drift;
    }

    // Editor-only: keeps the inspector from producing values that break the math.
    private void OnValidate()
    {
        axis = moveAxis.sqrMagnitude > 0f ? moveAxis.normalized : Vector3.right;
        deadZonePixels = Mathf.Max(0f, deadZonePixels);
        shakeSpeedMultiplier = Mathf.Max(1f, shakeSpeedMultiplier);
        shakesToTrigger = Mathf.Max(1, shakesToTrigger);
        shakeWindow = Mathf.Max(0.01f, shakeWindow);
        minShakePixelsPerSecond = Mathf.Max(0f, minShakePixelsPerSecond);
        boostDuration = Mathf.Max(0f, boostDuration);
        boostBlendSpeed = Mathf.Max(0.01f, boostBlendSpeed);
    }
}
