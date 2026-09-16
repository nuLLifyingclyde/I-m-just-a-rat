using UnityEngine;

public class CornerTurn : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Corner playerTurn = other.GetComponent<Corner>();

        if (playerTurn == null)
        return;

        playerTurn.StartCornerTurn();
    }
}