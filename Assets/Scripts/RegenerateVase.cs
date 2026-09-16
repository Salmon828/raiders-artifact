using System.Collections;
using UnityEngine;

// Logic to make vase respawn after being broken
// Attach to an empty game object located in the spot vase should always respawn to
public class RegenerateVase : MonoBehaviour {
    private GameObject currentVase;
    [SerializeField] private GameObject vase;
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private ParticleSystem respawnEffect;
    private float respawnEffectDuration = 0f;

    [Tooltip("How early the vase appears before the effect ends. Tuned by feel.")]
    [SerializeField] private float spawnLeadTime = 0f; 

    private bool isRespawning = false;

    // Spawns a vase right when the scene is run/played
    void Start(){
        if (vase == null) {
            Debug.LogWarning($"[{name}] no vase prefab assigned on {GetType().Name}. Assign a vase in the inspector.");
            return;
        }
        if (respawnEffect != null) {
            respawnEffectDuration = respawnEffect.main.duration;
        }
        SpawnVase();
    }

    public void SpawnVase() {
        // Prevent spawning if there is already a vase scheduled or present
        isRespawning = false;
        if (currentVase != null) {
            Debug.LogWarning($"[{name}] SpawnVase called but currentVase already exists ('{currentVase.name}'). Skipping duplicate spawn.");
            return;
        }

        currentVase = Instantiate(vase, transform.position, transform.rotation);
        var breakable = currentVase.GetComponent<BreakableVase>();
        if (breakable != null) {
            breakable.spawner = this;
        } else {
            Debug.LogWarning($"[{name}] spawned object '{currentVase.name}' does not have a BreakableVase component.\n" +
                "Assign the component to the vase prefab so it can notify the spawner when broken.");
        }
    }

    private void OnValidate()
    {
        respawnEffectDuration = (respawnEffect != null ? respawnEffect.main.duration : 0f);
        if (respawnDelay < respawnEffectDuration)
        {
            Debug.LogWarning($"[{name}] respawnDelay ({respawnDelay}) is less than respawnEffectDuration ({respawnEffectDuration}). This may cause problems with the respawn timing.");
        }

        if (respawnEffectDuration < spawnLeadTime)
        {
            Debug.LogWarning($"[{name}] spawnLeadTime ({spawnLeadTime}) is greater than respawnEffectDuration ({respawnEffectDuration}). This may cause the vase to spawn before the effect starts.");
        }
    }

    // Plays the respawn effect after a delay, then spawns a new vase after a set amount of time, usually the effect duration.
    IEnumerator RespawnVaseWithEffect(float timeBefore, float timeAfter)
    {
        yield return new WaitForSeconds(timeBefore);
        respawnEffect.Play();
        yield return new WaitForSeconds(timeAfter);
        SpawnVase();
    }

    public event System.Action OnVaseBreak;
    public void OnVaseBroken() {
        if (isRespawning)
        {
            Debug.LogWarning($"[{name}] OnVaseBroken called but vase is already respawning. Ignoring duplicate call.");
            return;
        }
        isRespawning = true;
        OnVaseBreak?.Invoke();
        if (respawnEffect != null)
        {
            StartCoroutine(RespawnVaseWithEffect(respawnDelay - respawnEffectDuration + spawnLeadTime, respawnEffectDuration - spawnLeadTime));
        }
        else
        {
            Invoke(nameof(SpawnVase), respawnDelay);
        }
    }
}
