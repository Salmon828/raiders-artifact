using UnityEngine;

public class SpawnerPositionRandomizer : MonoBehaviour
{
    [SerializeField]
    private Transform[] spawnPoints; // Array of spawn points to choose from

    [SerializeField] private RegenerateVase spawner; // Reference to the spawner logic
    private Transform selectedSpawnPoint;
    private bool activateUpdate = false; // Makes sure the continuous code doesn't run until the vase is broken and the event is called.

    private void OnEnable()
    {
        if (spawner != null)
        {
            spawner.OnVaseBreak += RandomizePosition; // Subscribe to the OnVaseBreak event
        }
        else
        {
            Debug.LogWarning("Spawner reference is not assigned in SpawnerPositionRandomizer.");
        }
    }

    private void OnDisable()
    {
        if (spawner != null)
        {
            spawner.OnVaseBreak -= RandomizePosition; // Unsubscribe from the OnVaseBreak event
        }
    }
    private void LateUpdate()
    {
        if (selectedSpawnPoint != null && activateUpdate)
        {
            transform.position = selectedSpawnPoint.position;

        }
    }

    public void RandomizePosition()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned to SpawnerPositionRandomizer.");
            return;
        }
        activateUpdate = true;
        // Choose a random index from the spawnPoints array
        int randomIndex = Random.Range(0, spawnPoints.Length);
        selectedSpawnPoint = spawnPoints[randomIndex];
        // Set the position of this GameObject to the position of the randomly chosen spawn point
        transform.position = selectedSpawnPoint.position;
    }
}
