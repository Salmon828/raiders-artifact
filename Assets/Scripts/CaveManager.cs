using UnityEngine;

public class CaveManager : MonoBehaviour
{
    [SerializeField] private GameObject CaveRespawn; // Cave respawn object to turn on once inside.
    [SerializeField] private GameObject VaseRespawn; // Vase respawn object to turn on if player got the vase here.

    private bool playerReached = false;
    private bool vaseReached = false;


    private void OnTriggerEnter(Collider other)
    {
        // Vase transported to cave
        if (other.CompareTag("canPickUp") && other.GetComponent<BreakableVase>() != null && !vaseReached)
        {
            VaseRespawn.SetActive(true);
            vaseReached = true;
        }

        // Player reached cave
        if (other.CompareTag("Player") && !playerReached)
        {
            CaveRespawn.SetActive(true);
            playerReached = true;
        }
    }
}
