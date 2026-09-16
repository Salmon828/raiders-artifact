using UnityEngine;

// Respawn script for rooms that need to constantly respawn the player on fail condition
// Needs to have a collider on the same object.
// Takes a respawn point transform, boss and vase initialization is optional.
public class RespawnTrigger : MonoBehaviour
{
    public Transform respawnPoint;
    public Boss boss;
    public GameObject vaseInit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("canPickUp"))
        {
            BreakableVase vase = other.GetComponent<BreakableVase>();
            if (vase != null)
            {
                vase.BreakFromTrigger();
            }
        }
        else if (other.CompareTag("Player"))
        {
            if (respawnPoint != null)
            {
                CharacterController cc = other.GetComponent<CharacterController>();
                cc.enabled = false;
                other.transform.position = respawnPoint.position;
                cc.enabled = true;
                if (boss != null)
                {
                    boss.ResetPosition();
                }
                if (vaseInit != null)
                {
                    vaseInit.SetActive(true);
                }
            }
        }
    }
}
