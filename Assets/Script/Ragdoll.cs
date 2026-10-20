using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ragdoll : MonoBehaviour
{
    public AudioManager audioManager;

    private enum State { Normal, Launched, Recovering }

    [Min(0f)] public float launchForce = 12f, launchLift = 8f, carSpeedInfluence = 0.5f;
    public Transform tumbleVisual;
    public float tumbleDegreesPerSecond = 720f;
    [Range(0f, 1f)] public float bounciness = 0.55f;
    [Min(0f)] public float minBounceSpeed = 2.5f, bounceGrace = 0.08f, ragdollDrag = 0.15f;
    [Min(0.01f)] public float settleSpeed = 1.2f, standUpTime = 0.45f;
    [Min(0f)] public float settleHold = 0.15f, minRagdollTime = 0.4f, invulnerabilityTime = 1f;
    [Min(0.1f)] public float maxRagdollTime = 5f;
    [Min(0f)] public float fallRagdollDelay = 1f;

    [Header("Wall Impacts")]
    [Min(1.01f)] public float wallImpactSpeedMultiplier = 1.5f;
    [Min(0f)] public float wallImpactLift = 3f;

    private Rigidbody body;
    private CharacterController characterController;
    private MouseDirectionController movement;
    private Behaviour[] controlScripts;
    private Quaternion tumbleRest;
    private State state;
    private Vector3 lanePoint, laneAxis, laneNormal, velocityBeforeContact;
    private float laneYaw, launchTime, invulnerableUntil, settleTimer, tumbleSpeed, restingDrag;
    private float fallTimer;
    private Vector3 previousPosition;
    private Vector3 pendingWallVelocity, pendingWallNormal;
    private float pendingWallImpactSpeed;

    public bool IsRagdolling => state != State.Normal;
    public bool CanBeHit => state == State.Normal && Time.time >= invulnerableUntil;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        previousPosition = transform.position;
        characterController = GetComponent<CharacterController>();
        movement = GetComponent<MouseDirectionController>();
        controlScripts = new Behaviour[] { movement, GetComponent<PlayerJump>(), GetComponent<Climb>(), GetComponent<Corner>() };
        restingDrag = body.linearDamping;
        Renderer sprite = GetComponentInChildren<Renderer>();
        if (tumbleVisual == null && sprite != null && sprite.transform != transform) tumbleVisual = sprite.transform;
        if (tumbleVisual != null) tumbleRest = tumbleVisual.localRotation;
    }

    public void HitByCar(Vector3 sourcePosition, float sourceSpeed)
    {
        if (!CanBeHit) return;
        BeginRagdoll();
        float side = Vector3.Dot(transform.position - sourcePosition, laneAxis) < 0f ? -1f : 1f;
        body.AddForce(laneAxis * (side * (launchForce + Mathf.Abs(sourceSpeed) * carSpeedInfluence))
                      + Vector3.up * launchLift, ForceMode.Impulse);
        velocityBeforeContact = body.linearVelocity;
        tumbleSpeed = tumbleDegreesPerSecond * -side;
    }

    private void BeginRagdoll()
    {
        laneAxis = GetMoveAxis();
        laneNormal = Vector3.Cross(laneAxis, Vector3.up);
        laneNormal = laneNormal.sqrMagnitude > 0.0001f ? laneNormal.normalized : transform.forward;
        lanePoint = transform.position;
        laneYaw = transform.eulerAngles.y;
        SetControl(false);
        SetPhysicsDriven(true);
        fallTimer = 0f;
        settleTimer = 0f;
        launchTime = Time.time;
        state = State.Launched;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!CanBeHit || movement == null || !movement.enabled || movement.movementLocked) return;
        if (Mathf.Abs(hit.normal.y) >= 0.5f) return;

        float requiredSpeed = Mathf.Max(0.01f, Mathf.Abs(movement.moveSpeed)) * wallImpactSpeedMultiplier;
        if (Mathf.Abs(movement.CurrentSpeed) < requiredSpeed) return;

        Vector3 incomingVelocity = GetMoveAxis() * movement.CurrentSpeed;
        float impactSpeed = -Vector3.Dot(incomingVelocity, hit.normal);
        if (impactSpeed < requiredSpeed || impactSpeed <= pendingWallImpactSpeed) return;

        pendingWallVelocity = incomingVelocity;
        pendingWallNormal = hit.normal;
        pendingWallImpactSpeed = impactSpeed;
    }

    private void LateUpdate()
    {
        Vector3 displacement = transform.position - previousPosition;
        previousPosition = transform.position;
        if (pendingWallImpactSpeed > 0f)
        {
            pendingWallImpactSpeed = 0f;
            if (CanBeHit)
            {
                Vector3 rebound = Vector3.Reflect(pendingWallVelocity, pendingWallNormal) * bounciness;
                BeginRagdoll();
                body.linearVelocity = velocityBeforeContact = AlongLane(rebound) + Vector3.up * wallImpactLift;
                tumbleSpeed = tumbleDegreesPerSecond * -Mathf.Sign(Vector3.Dot(pendingWallVelocity, laneAxis));
            }
        }
        float dt = Time.deltaTime;
        if (state != State.Normal || dt <= 0f)
        {
            fallTimer = 0f;
            return;
        }

        bool grounded = characterController != null && characterController.enabled && characterController.isGrounded;
        bool falling = !grounded && displacement.y < -0.01f * dt;
        fallTimer = falling ? fallTimer + dt : 0f;
        if (!falling || fallTimer < fallRagdollDelay) return;

        Vector3 fallVelocity = displacement / dt;
        BeginRagdoll();
        body.linearVelocity = velocityBeforeContact = AlongLane(fallVelocity);
        tumbleSpeed = tumbleDegreesPerSecond;
    }

    private void SetPhysicsDriven(bool on)
    {
        body.isKinematic = false;
        body.linearVelocity = body.angularVelocity = Vector3.zero;
        body.collisionDetectionMode = on ? CollisionDetectionMode.ContinuousDynamic : CollisionDetectionMode.Discrete;
        body.isKinematic = !on;
        body.useGravity = on;
        body.constraints = on ? RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.None;
        body.interpolation = on ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
        body.linearDamping = on ? ragdollDrag : restingDrag;
    }

    private void SetControl(bool on)
    {
        foreach (Behaviour script in controlScripts)
        {
            if (script != null) script.enabled = on;
        }

        if (characterController != null) characterController.enabled = on;
        if (movement == null) return;
        movement.StopMovement();
        if (!on) return;
        movement.movementLocked = false;
        movement.SetMoveAxis(laneAxis);
    }

    private Vector3 GetMoveAxis() => movement != null && movement.moveAxis.sqrMagnitude > 0.0001f
        ? movement.moveAxis.normalized : transform.right;

    private Vector3 OnLane(Vector3 point) => point - laneNormal * Vector3.Dot(point - lanePoint, laneNormal);
    private Vector3 AlongLane(Vector3 vector) => vector - laneNormal * Vector3.Dot(vector, laneNormal);

    private void FixedUpdate()
    {
        if (state != State.Launched) return;
        Vector3 position = body.position;
        Vector3 onLane = OnLane(position);
        if (onLane != position) body.position = onLane;
        body.linearVelocity = velocityBeforeContact = AlongLane(body.linearVelocity);
    }

    private void Update()
    {
        if (state != State.Launched) return;
        if (tumbleVisual != null) tumbleVisual.Rotate(laneNormal, tumbleSpeed * Time.deltaTime, Space.World);
        settleTimer = body.linearVelocity.sqrMagnitude < settleSpeed * settleSpeed ? settleTimer + Time.deltaTime : 0f;
        float airTime = Time.time - launchTime;
        if ((airTime > minRagdollTime && settleTimer > settleHold) || airTime > maxRagdollTime) StartCoroutine(Recover());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Launched || Time.time - launchTime < bounceGrace) return;
        if (velocityBeforeContact.sqrMagnitude < minBounceSpeed * minBounceSpeed) return;
        body.linearVelocity = velocityBeforeContact =
            AlongLane(Vector3.Reflect(velocityBeforeContact, collision.GetContact(0).normal) * bounciness);
        tumbleSpeed *= bounciness;

        audioManager.PlaySFX(audioManager.BumpSFX, 0.4f);
    }

    private IEnumerator Recover()
    {
        state = State.Recovering;
        SetPhysicsDriven(false);
        Vector3 fromPosition = transform.position, toPosition = OnLane(fromPosition);
        Quaternion fromRotation = transform.rotation, toRotation = Quaternion.Euler(0f, laneYaw, 0f);
        Quaternion fromTumble = tumbleVisual != null ? tumbleVisual.localRotation : Quaternion.identity;
        for (float elapsed = 0f; elapsed < standUpTime; elapsed += Time.deltaTime)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / standUpTime));
            transform.position = Vector3.Lerp(fromPosition, toPosition, t);
            transform.rotation = Quaternion.Slerp(fromRotation, toRotation, t);
            if (tumbleVisual != null) tumbleVisual.localRotation = Quaternion.Slerp(fromTumble, tumbleRest, t);
            yield return null;
        }
        transform.position = toPosition;
        transform.rotation = toRotation;
        tumbleSpeed = 0f;
        if (tumbleVisual != null) tumbleVisual.localRotation = tumbleRest;
        SetControl(true);
        invulnerableUntil = Time.time + invulnerabilityTime;
        state = State.Normal;
    }
}
