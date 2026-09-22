using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ragdoll : MonoBehaviour
{
    private enum State { Normal, Launched, Recovering }

    [Min(0f)] public float launchForce = 12f, launchLift = 8f, carSpeedInfluence = 0.5f;
    public Transform tumbleVisual; 
    public float tumbleDegreesPerSecond = 720f;
    [Range(0f, 1f)] public float bounciness = 0.55f;
    [Min(0f)] public float minBounceSpeed = 2.5f, bounceGrace = 0.08f, ragdollDrag = 0.15f;
    [Min(0.01f)] public float settleSpeed = 1.2f, standUpTime = 0.45f;
    [Min(0f)] public float settleHold = 0.15f, minRagdollTime = 0.4f, invulnerabilityTime = 1f;
    [Min(0.1f)] public float maxRagdollTime = 5f;

    private Rigidbody body;
    private CharacterController characterController;
    private MouseDirectionController movement;
    private Behaviour[] controlScripts;
    private Quaternion tumbleRest;
    private State state;
    private Vector3 lanePoint, laneAxis, laneNormal, velocityBeforeContact;
    private float laneYaw, launchTime, invulnerableUntil, settleTimer, tumbleSpeed, restingDrag;

    public bool IsRagdolling => state != State.Normal;
    public bool CanBeHit => state == State.Normal && Time.time >= invulnerableUntil;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
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
        laneAxis = movement != null && movement.moveAxis.sqrMagnitude > 0.0001f
            ? movement.moveAxis.normalized : transform.rotation * Vector3.right;
        laneNormal = Vector3.Cross(laneAxis, Vector3.up);
        laneNormal = laneNormal.sqrMagnitude > 0.0001f ? laneNormal.normalized : transform.forward;
        lanePoint = transform.position;
        laneYaw = transform.eulerAngles.y;
        SetControl(false);
        SetPhysicsDriven(true);
        float side = Vector3.Dot(transform.position - sourcePosition, laneAxis) < 0f ? -1f : 1f;
        body.AddForce(laneAxis * (side * (launchForce + Mathf.Abs(sourceSpeed) * carSpeedInfluence))
                      + Vector3.up * launchLift, ForceMode.Impulse);
        velocityBeforeContact = body.linearVelocity;
        tumbleSpeed = tumbleDegreesPerSecond * -side; // tumble the way it is thrown
        settleTimer = 0f;
        launchTime = Time.time;
        state = State.Launched;
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
        foreach (Behaviour script in controlScripts) if (script != null) script.enabled = on;
        if (characterController != null) characterController.enabled = on;
        if (movement == null) return;
        movement.StopMovement();
        if (!on) return;
        movement.movementLocked = false;
        movement.SetMoveAxis(laneAxis); 
    }
    private Vector3 OnLane(Vector3 point) => point - laneNormal * Vector3.Dot(point - lanePoint, laneNormal);
    private Vector3 AlongLane(Vector3 vector) => vector - laneNormal * Vector3.Dot(vector, laneNormal);

    private void FixedUpdate()
    {
        if (state != State.Launched) return;
        Vector3 onLane = OnLane(body.position);
        if (onLane != body.position) body.position = onLane; 
        body.linearVelocity = velocityBeforeContact = AlongLane(body.linearVelocity);
    }

    private void Update()
    {
        if (state != State.Launched) return; 
        if (tumbleVisual != null) tumbleVisual.Rotate(laneNormal, tumbleSpeed * Time.deltaTime, Space.World);
        settleTimer = body.linearVelocity.magnitude < settleSpeed ? settleTimer + Time.deltaTime : 0f;
        float airTime = Time.time - launchTime;
        if ((airTime > minRagdollTime && settleTimer > settleHold) || airTime > maxRagdollTime) StartCoroutine(Recover());
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Launched || Time.time - launchTime < bounceGrace) return; 
        if (velocityBeforeContact.magnitude < minBounceSpeed) return;
        body.linearVelocity = velocityBeforeContact =
            AlongLane(Vector3.Reflect(velocityBeforeContact, collision.GetContact(0).normal) * bounciness);
        tumbleSpeed *= bounciness; 
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
