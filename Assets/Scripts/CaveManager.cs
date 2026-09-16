using UnityEngine;

public class CaveManager : MonoBehaviour
{
    [SerializeField] private GameObject CaveRespawn; // Cave respawn object to turn on once inside.
    [SerializeField] private GameObject VaseRespawn; // Vase respawn object to turn on if player got the vase here.
    [SerializeField] private RespawnTrigger respawnTrigger; // Used to initialize the vase spawner after the first fall

    private bool playerReached = false;
    private bool vaseReached = false;



    private void OnTriggerEnter(Collider other)
    {
        // Vase transported to cave
        if (other.CompareTag("canPickUp") && other.GetComponent<BreakableVase>() != null && !vaseReached)
        {
            if (VaseRespawn != null && respawnTrigger != null)
            {
                respawnTrigger.vaseInit = VaseRespawn;
            }
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
