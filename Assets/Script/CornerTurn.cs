using UnityEngine;

public class CornerTurn : MonoBehaviour
{
    [SerializeField] private float forwardRotation = -90f;
    [SerializeField] private float reverseRotation = 0f;

    private void OnTriggerEnter(Collider other)
    {
        Corner playerTurn = other.GetComponent<Corner>();

        if (playerTurn == null)
        return;

        float currentY = other.transform.eulerAngles.y;

        float forwardDifference =
        Mathf.Abs(Mathf.DeltaAngle(currentY, forwardRotation));

        float reverseDifference =
        Mathf.Abs(Mathf.DeltaAngle(currentY, reverseRotation));

        if (forwardDifference < reverseDifference)
        {
            playerTurn.StartCornerTurn(reverseRotation);
        }
        else
        {
            playerTurn.StartCornerTurn(forwardRotation);
        }
    }
}