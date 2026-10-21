using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
    public bool requireRightClickToBoost = true;
    public float shakeSpeedMultiplier = 1.8f;
    public bool infiniteboost = false;
    public int shakesToTrigger = 3;
    public float shakeWindow = 0.4f, minShakePixelsPerSecond = 500f, boostDuration = 2f, boostBlendSpeed = 6f;


    //Camera changing
    [Header("Turn Lock")]
    public bool movementLocked = false;

    [Header("Rat Food")]
    public float foodDecayDelay = 3f;
    public float foodDecayTimer;
    public float accumulatedFoodMultiplier = 0f;
    public float accumulatedFoodDuration = 0f;
    public FoodScript fs;
    public FoodManager fm;

    [Header("Boost Bar")]
    public Image BoostBar;
    public float displayFilledAmount;
    public float barFillSpeed = 5f;
    public float maxBoostTimer = 1f;

    [Header("Animation")]
    public Animator ani;
    private int lastFacingDirection = 1;

    [Header("Part Activator")]
    private CharacterController controller;
    private Transform cachedTransform;
    private Vector3 laneOrigin, axis;
    private Vector2 characterScreenPosition, screenAxisDirection;
    private float currentSpeed, verticalVelocity, lastCursorAxisPosition, lastStrokeSign, lastShakeTime, boostBlend;
    public float boostTimer;
    private bool hasCursorSample;
    private int shakeCount;
    public FoodScript fos;

    public string FoodTag = "Food";
    public float SpeedMultiplier { get; set; } = 1f;
    public float CurrentSpeed => currentSpeed;
    public int MoveDirection => currentSpeed > 0.01f ? 1 : currentSpeed < -0.01f ? -1 : 0;
    public bool IsBoosting => boostTimer > 0f;
    public float ShakeBoost => Mathf.Lerp(1f, shakeSpeedMultiplier, boostBlend);

   public  AudioManager audioManager;


    private void Awake() //Codecredit: Mao
    {
        
        controller = GetComponent<CharacterController>(); // find CharacterController
        cachedTransform = transform; //find transform
        if (targetCamera == null) targetCamera = Camera.main; //if there's a camera then the target camera is the main one
        axis = moveAxis.sqrMagnitude > 0f ? moveAxis.normalized : Vector3.right; //axis is moving axis that is in the magnitute bigger than 0
        laneOrigin = cachedTransform.position; //form a lane which is the position
        lastShakeTime = float.NegativeInfinity; // so the first reversal always starts a fresh streak
        boostTimer = 0f; //Boost Timer is 0f;
        maxBoostTimer = 1f; //The timer is 1f
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

        //boostTimer code made by Krystal
    }

    public void Update() 
    {
        if (Time.timeScale == 0f) return; //stop character movement in pause menu

        // Native property reads, so sample them once and pass them down.
        float dt = Time.deltaTime; //declare deltatime
        Vector2 cursor = Input.mousePosition; //declare mouse position as vector2 so that i can declare it to activate whatever tf i want

        // Without a usable projection there is no way to tell which side the cursor is on, so coast.
        bool canReadCursor = UpdateScreenProjection();//the thing that reads the cursor is set to bool (true/false) and declared as UpdateScreenProjection
        //int input = canReadCursor ? ReadMouseDirection(cursor) : 0;

        int input = (!movementLocked && canReadCursor) ? ReadMouseDirection(cursor) : 0; //lock character movement while turning
        //set input as interger, make it equal to movementlock and cursor reader declared above at line 94
        //line 89-99 is credited to Mao

        // Flip the character
        if (input != 0)
        {
            lastFacingDirection = input; //self explainatory
            Vector3 scale = transform.localScale; //the scale is declared as Vector 3
            scale.x = Mathf.Abs(scale.x) * (lastFacingDirection != 0 ? lastFacingDirection : 1); //the scale of x is equal to the math abstraction of scale x multiplied by the last facing direction not equal by zero and by one
            transform.localScale = scale; //the current localscale is the main scale
            //code credited by Nao
        }

        // Corrected: check if speed is greater than a tiny threshold


        //animation and audio
        bool isHoldingRightClick = Input.GetMouseButton(1);
        bool isTryingToBoost = isHoldingRightClick && boostTimer > 0f;

        //normal walking animation
        bool isMoving = Mathf.Abs(currentSpeed) > 0.01f;
        ani.SetBool("isWalking", isMoving && !isTryingToBoost);

        //running animation
        ani.SetBool("isBoosting", isTryingToBoost);

        //boost timer
        if (isTryingToBoost)
        {
            boostTimer -= dt;

            if (boostTimer <= 0f)
            {
                boostTimer = 0f;
                boostDuration = 1f;
                accumulatedFoodDuration = 0f;
                accumulatedFoodMultiplier = 0f;
                Debug.Log("Boost Out");
            }
        }
        AnimatorStateInfo state = ani.GetCurrentAnimatorStateInfo(0);

        bool isWalkingAnimation = state.IsName("Rat Walking");
        bool isRunningAnimation = state.IsName("Rat Boosting");

        //choose audio based on the current animation
        if (isRunningAnimation)
        {
            audioManager.PlayMovement(audioManager.RunningSFX);
        }
        else if (isWalkingAnimation)
        {
            audioManager.PlayMovement(audioManager.WalkSFX);
        }
        else
        {
            audioManager.StopMovement();
        }



        float targetMultiplier = isTryingToBoost ? (1f + accumulatedFoodMultiplier) : 1f;
        SpeedMultiplier = targetMultiplier;

        float targetSpeed = movementLocked ? 0f : input * moveSpeed * SpeedMultiplier * ShakeBoost;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, (input == 0 ? deceleration : acceleration) * dt);

        // Clamped while grounded; letting gravity accumulate would launch us off the first ledge.
        verticalVelocity = controller.isGrounded && verticalVelocity < 0f ? groundedStick : verticalVelocity + gravity * dt;

        controller.Move((axis * currentSpeed + Vector3.up * verticalVelocity) * dt);
        if (lockToLane) ClampToLane();

        if (accumulatedFoodMultiplier > 0f)
        {

            if (IsBoosting)
            {
                foodDecayTimer = foodDecayDelay;
            }

            else

                foodDecayTimer -= dt;

            if (foodDecayTimer <= 0f)
            {


                SpeedMultiplier = Mathf.Max(1f, SpeedMultiplier - accumulatedFoodMultiplier);
                accumulatedFoodMultiplier = 0f;
                accumulatedFoodDuration = 0f;
                Debug.Log("Boost is gone");
            }
        }


        float fillTarget = maxBoostTimer > 0f ? Mathf.Clamp01(boostTimer / maxBoostTimer) : 0f;
        displayFilledAmount = Mathf.MoveTowards(displayFilledAmount, fillTarget, barFillSpeed * dt);



        if (BoostBar != null)
        {

            BoostBar.fillAmount = displayFilledAmount;
        }


    }

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag(FoodTag))
        {
            audioManager.PlaySFX(audioManager.EatCheeseSFX, 0.8f);

            float fillTarget = (fos.foodAmount > 0f) ? Mathf.Clamp01((float)fos.foodAmount) : 0f;

            displayFilledAmount = Mathf.MoveTowards(displayFilledAmount, fillTarget, barFillSpeed * Time.deltaTime);
            if (BoostBar != null)
            {

                BoostBar.fillAmount = displayFilledAmount;
            }
        }
    }


    //Camera changing
    public void SetMoveAxis(Vector3 newAxis)
    {
        if (newAxis.sqrMagnitude < 0.0001f) return;

        axis = newAxis.normalized;
        moveAxis = axis;

        laneOrigin = cachedTransform.position;

        hasCursorSample = false;
        lastStrokeSign = 0f;
        shakeCount = 0;
    }
    public void StopMovement()
    {
        currentSpeed = 0f;
        hasCursorSample = false;
        lastStrokeSign = 0f;
        shakeCount = 0;
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


    public void AddFoodBoost(float addDuration, float addMultiplier)
    {

        accumulatedFoodDuration += addDuration;
        accumulatedFoodMultiplier += addMultiplier;


        float maxFoodCompacity = 5f;


        boostTimer = Mathf.Min (boostTimer + addDuration, maxFoodCompacity);
        maxBoostTimer = maxFoodCompacity;
        foodDecayTimer = foodDecayDelay;
    }



    // Waggling the cursor along the move axis, on the side the character already heads towards,
    // charges up a short speed boost.


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

        shakeWindow = Mathf.Max(0.01f, shakeWindow);
        minShakePixelsPerSecond = Mathf.Max(0f, minShakePixelsPerSecond);
        boostDuration = Mathf.Max(0f, boostDuration);
        boostBlendSpeed = Mathf.Max(0.01f, boostBlendSpeed);
    }
}