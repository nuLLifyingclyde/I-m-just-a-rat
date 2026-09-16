using System.Collections;
using UnityEngine;

public class Corner : MonoBehaviour
{
    [SerializeField] private MouseDirectionController movementController;
    [SerializeField] private float turnDuration = 0.6f;
    [SerializeField] private float turnAmount = -90f;

    private bool isTurning = false;

    private void Awake()
    {
        if (movementController == null) movementController = GetComponent<MouseDirectionController>();
    }

    public void StartCornerTurn()
    {
        if (isTurning) return;
        StartCoroutine(TurnPlayer());
    }

    private IEnumerator TurnPlayer()
{
    isTurning = true;

    if (movementController != null)
    {
        movementController.movementLocked = true;
        movementController.StopMovement();
    }

    float startingY = transform.eulerAngles.y;

    //turn left when facing the original direction
    //turn right when coming back
    float turnAmount = Mathf.Abs(Mathf.DeltaAngle(0f, startingY)) < 45f ? -90f : 90f;

    float targetY = startingY + turnAmount;
    float elapsed = 0f;

    while (elapsed < turnDuration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / turnDuration);
        t = Mathf.SmoothStep(0f, 1f, t);
        float currentY = Mathf.LerpAngle(startingY, targetY, t);
        transform.rotation = Quaternion.Euler(0f, currentY, 0f);
        yield return null;
    }

    transform.rotation = Quaternion.Euler(0f, targetY, 0f);

    if (movementController != null)
    {
        Vector3 newAxis = transform.rotation * Vector3.right;
        movementController.SetMoveAxis(newAxis);
        movementController.movementLocked = false;
    }

    isTurning = false;
}
}