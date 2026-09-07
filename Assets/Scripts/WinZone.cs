using TMPro;
using UnityEngine;

public class WinZone : MonoBehaviour
{
    public GameObject winText; 

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("canPickUp") && other.GetComponent<BreakableVase>() != null) {
            Debug.Log("You win! With vase");
            winText.SetActive(true); // Show the win text
        } else
        {
            Debug.Log("You win!");
            winText.SetActive(true); // Show the win text
        }
    }
}
