using System.Collections;
using UnityEngine;

public class Corner : MonoBehaviour
{
    [SerializeField] private MouseDirectionController movementController;
    [SerializeField] private float turnDuration = 0.6f;
    [SerializeField] private float targetYRotation = 0f;

    private bool isTurning = false;

    private void Awake()
    {
        if (movementController == null)
        movementController = GetComponent<MouseDirectionController>();
    }

    public void StartCornerTurn(float targetRotation)
    {
        if (isTurning) return;
        StartCoroutine(TurnPlayer(targetRotation));
    }

    private IEnumerator TurnPlayer(float targetRotation)
    {
        isTurning = true;

        if (movementController != null)
        {
            movementController.movementLocked = true;
            movementController.StopMovement();
        }

        float startingY = transform.eulerAngles.y;
        float elapsed = 0f;

        while (elapsed < turnDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / turnDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            float currentY = Mathf.LerpAngle(startingY, targetRotation, t);

            transform.rotation = Quaternion.Euler(0f, currentY, 0f);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0f, targetRotation, 0f);

        if (movementController != null)
        {
            Vector3 newAxis = transform.rotation * Vector3.right;
            movementController.SetMoveAxis(newAxis);
            movementController.movementLocked = false;
        }

        isTurning = false;
    }
}