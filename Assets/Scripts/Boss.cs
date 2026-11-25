using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    public BackgroundMusicManager music; // for end of boss fight
    public Transform player;
    public float rotationSpeed = 5f;
    public bool smoothRotation = true;
    public bool lockVerticalAxis = true;
    public bool isAttacking = false;
    public Animator animator;

    [Header("Attack Timer")]
    public float spawnDelay = 2f;
    public float attackInterval = 4f;

    [Header("Charge Attack")]
    public float chargeSpeed = 10f;
    public float chargeDelay = 0.6f;
    public float chargeOvershoot = 2f;

    [Header("Proj Attack")]
    public float projDelay = 0.6f;
    public float projSpinSpeed = 360f;
    public GameObject projectilePrefab;
    public int projectileCount = 3;
    public float projectileInterval = 0.4f;
    public float projectileArcHeight = 5f;
    public float projectileTravelTime = 1.5f;

    [Header("Health")]
    public int maxHealth = 10;
    public int phase2Threshold = 2;
    public Image healthBarFill;

    [Header("Death")]
    public float deathSpinSpeed = 720f;
    public float deathSpinDuration = 2f;
    public GameObject deathExplosionPrefab;
    public PlayableDirector deathTimeline;

    [Header("Damage Effect")]
    public float flashDuration = 0.2f;
    public Color flashColor = Color.red;
    public float knockbackSpeed = 8f;
    public float knockbackDuration = 0.4f;

    [Header("Low Health Effects")]
    public float newAttackInterval = 0.5f;
    [Tooltip("How often the boss picks to do it's ranged attack")]
    public float newRangeWeight = 0.2f;

    private int _currentHealth;

    private bool isDying = false;
    private float deathTimer = 0f;

    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private Vector3 knockbackDirection;

    private Renderer[] _renderers;
    private Color[] _originalColors;

    private float spawnDelayTimer = 0f;
    private bool spawnDelayDone = false;

    private float attackTimer = 0f;

    private bool isCharging = false;
    private bool chargeQueued = false;
    private float chargeTimer = 0f;
    private Vector3 chargeTarget;

    private bool projQueued = false;
    private float projTimer = 0f;
    private bool isSpinning = false;
    private Vector3 startPosition;

    private float rangeWeight = 0.7f;


    public void TriggerAttack()
    {
        if (Random.value < rangeWeight)
            animator.SetTrigger("ProjAttack");
        else
            animator.SetTrigger("ChargeAttack");
    }

    void Start()
    {
        _currentHealth = maxHealth;
        UpdateHealthBar();
        animator = GetComponentInChildren<Animator>();
        startPosition = transform.position;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogError("Player not found! Please assign the player Transform or tag an object as 'Player'.");
        }

        // Used for flashing red
        _renderers = GetComponentsInChildren<Renderer>();
        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _originalColors[i] = _renderers[i].material.color;
    }

    public void TakeDamage()
    {
        if (_currentHealth <= 0) return;

        _currentHealth--;
        UpdateHealthBar();
        Debug.Log($"Boss health: {_currentHealth}/{maxHealth}");

        if (_currentHealth <= phase2Threshold)
        {
            attackInterval = newAttackInterval;
            rangeWeight = newRangeWeight;
            Debug.Log("I'm getting angry!!!");
        }

        if (_currentHealth <= 0)
        {
            Die();
            return;
        }

        HandlePickupContact();
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)_currentHealth / maxHealth;
    }

    void Die()
    {
        isDying = true;
        deathTimer = 0f;
        isKnockedBack = false;
        animator.speed = 0f;
        StartCoroutine(PlayMusicDelayed(1f)); // play calm music after boss is defeated
        Debug.Log("CALM MUSIC STARTS BC BOSS DEAD");
        if (deathExplosionPrefab != null)
            Instantiate(deathExplosionPrefab, transform.position, Quaternion.identity);
    }

    void HandlePickupContact()
    {
        // Cancel any in-progress attacks
        isCharging = false;
        chargeQueued = false;
        projQueued = false;
        projTimer = 0f;
        isSpinning = false;
        attackTimer = 0f;

        // Freeze animation
        animator.speed = 0f;

        // Knock back directly away from player (flat)
        Vector3 awayFromPlayer = transform.position - player.position;
        awayFromPlayer.y = 0;
        knockbackDirection = awayFromPlayer.normalized;
        isKnockedBack = true;
        knockbackTimer = 0f;

        StopCoroutine("FlashRed");
        StartCoroutine("FlashRed");
    }

    IEnumerator FlashRed()
    {
        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].material.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].material.color = _originalColors[i];
    }
    void Update()
    {
        // --- Death Spin ---
        if (isDying)
        {
            deathTimer += Time.deltaTime;
            transform.RotateAround(transform.position, Vector3.up, deathSpinSpeed * Time.deltaTime);

            if (deathTimer >= deathSpinDuration)
            {
                Destroy(gameObject);
                deathTimeline.Play();
            }
            return;
        }

        // --- Knockback ---
        if (isKnockedBack)
        {
            transform.position += knockbackDirection * knockbackSpeed * Time.deltaTime;
            knockbackTimer += Time.deltaTime;
            if (knockbackTimer >= knockbackDuration)
            {
                isKnockedBack = false;
                animator.speed = 1f;
            }
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        bool wasAttacking = isAttacking;
        bool isChargeAttack = stateInfo.IsName("Armature|ChargeAttack");
        bool isProjAttack   = stateInfo.IsName("Armature|ProjAttack");
        isAttacking = isChargeAttack || isProjAttack;

        // Spawn delay — idle only until timer elapses
        if (!spawnDelayDone)
        {
            spawnDelayTimer += Time.deltaTime;
            if (spawnDelayTimer >= spawnDelay)
                spawnDelayDone = true;
            else
                return;
        }

        // Attack timer — only ticks when not already attacking
        if (!isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                TriggerAttack();
            }
        }

        // --- Charge Attack ---
        if (isChargeAttack && !wasAttacking)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            Vector3 playerFlat = new Vector3(player.position.x, transform.position.y, player.position.z);
            chargeTarget = playerFlat + new Vector3(dirToPlayer.x, 0, dirToPlayer.z) * chargeOvershoot;
            chargeQueued = true;
            isCharging = false;
            chargeTimer = 0f;
        }

        if (chargeQueued)
        {
            chargeTimer += Time.deltaTime;
            if (chargeTimer >= chargeDelay)
            {
                chargeQueued = false;
                isCharging = true;
            }
        }

        if (isCharging)
        {
            transform.position = Vector3.MoveTowards(transform.position, chargeTarget, chargeSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, chargeTarget) < 0.05f)
                isCharging = false;
        }

        // --- Proj Attack ---
        if (isProjAttack && !wasAttacking)
        {
            projQueued = true;
            isSpinning = false;
            projTimer = 0f;
        }

        if (projQueued)
        {
            projTimer += Time.deltaTime;
            if (projTimer >= projDelay)
            {
                projQueued = false;
                isSpinning = true;
                StartCoroutine(FireProjectiles());
            }
        }

        if (isSpinning)
        {
            transform.RotateAround(transform.position, Vector3.up, projSpinSpeed * Time.deltaTime);
            if (!isProjAttack)
                isSpinning = false;
        }

        // --- Idle rotation towards player ---
        if (player == null || isAttacking) return;

        Vector3 directionToPlayer = player.position - transform.position;

        if (lockVerticalAxis)
            directionToPlayer.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer) * Quaternion.Euler(0, 90f, 0);

        if (smoothRotation)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        else
            transform.rotation = targetRotation;
    }

    IEnumerator FireProjectiles()
    {
        if (projectilePrefab == null) yield break;

        for (int i = 0; i < projectileCount; i++)
        {
            Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + 1.75f, transform.position.z);
            Vector3 targetPos = new Vector3(player.position.x, player.position.y, player.position.z);

            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            proj.GetComponent<BossProjectile>().Launch(spawnPos, targetPos, projectileArcHeight, projectileTravelTime);

            yield return new WaitForSeconds(projectileInterval);
        }
    }

    private IEnumerator PlayMusicDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (music != null)
        {
            music.PlayCalmMusicFade();
        }
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
    }
}
