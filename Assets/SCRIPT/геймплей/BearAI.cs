using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BearAI : MonoBehaviour
{
    [Header("Движение")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float attackRange = 2f;
    public float sightRange = 15f;

    [Header("Блуждание (патруль)")]
    public float wanderRadius = 20f;        // Радиус случайного блуждания
    public float wanderDistance = 15f;      // Расстояние, которое медведь проходит перед сменой направления
    public float minWanderTime = 3f;        // Минимальное время блуждания
    public float maxWanderTime = 8f;        // Максимальное время блуждания

    [Header("Атака")]
    public float attackCooldown = 1.5f;
    public int attackDamage = 20;
    public AudioClip attackSound;

    [Header("Звуки шагов")]
    public AudioClip[] footstepClips;
    public float stepIntervalWalk = 0.6f;
    public float stepIntervalRun = 0.4f;

    private NavMeshAgent agent;
    private AudioSource footstepAudio;
    private AudioSource attackAudioSource;
    private Transform player;
    private Vector3 wanderTarget;
    private float stepTimer, currentStepInterval, lastAttackTime;
    private bool playerInSight, isDead;
    private float wanderTimer;
    private float currentWanderTime;

    void Start()
    {
        Debug.Log($"BearAI: Старт на {gameObject.name}");

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("BearAI: Игрок НЕ НАЙДЕН!");
            return;
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = gameObject.AddComponent<NavMeshAgent>();

        // Настройка агента
        agent.baseOffset = 0f;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 1.5f;
        agent.autoBraking = true;

        footstepAudio = GetComponent<AudioSource>();
        if (footstepAudio == null)
            footstepAudio = gameObject.AddComponent<AudioSource>();
        footstepAudio.spatialBlend = 1f;

        attackAudioSource = gameObject.AddComponent<AudioSource>();
        attackAudioSource.spatialBlend = 1f;

        lastAttackTime = -attackCooldown;

        // Проверка, что медведь на NavMesh
        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"BearAI: Медведь {gameObject.name} НЕ НА NAVMESH! Переместите его на NavMesh.");
        }

        // Выбираем первую цель для блуждания
        SetNewWanderTarget();
        wanderTimer = 0f;
        currentWanderTime = Random.Range(minWanderTime, maxWanderTime);

        CreateAttackZone();
        CheckSightTrigger();
    }

    void CreateAttackZone()
    {
        Transform existing = transform.Find("AttackZone");
        if (existing != null) return;

        GameObject attackZone = new GameObject("AttackZone");
        attackZone.transform.parent = transform;
        attackZone.transform.localPosition = Vector3.zero;
        
        SphereCollider collider = attackZone.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = attackRange;
        
        AttackTrigger trigger = attackZone.AddComponent<AttackTrigger>();
        trigger.Init(this);
    }

    void CheckSightTrigger()
    {
        SphereCollider[] triggers = GetComponents<SphereCollider>();
        bool found = false;
        foreach (SphereCollider sc in triggers)
        {
            if (sc.isTrigger && sc.radius >= sightRange * 0.8f)
            {
                found = true;
                Debug.Log($"BearAI: ✅ Есть триггер обзора с радиусом {sc.radius}");
            }
        }
        if (!found)
        {
            Debug.LogWarning("BearAI: ⚠️ НЕТ триггера обзора! Добавьте SphereCollider (Is Trigger = true, radius = sightRange)");
        }
    }

    public void OnAttackZoneEnter(PlayerMovement pm)
    {
        if (isDead) return;
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            pm.TakeDamage(attackDamage);
            if (attackSound) attackAudioSource.PlayOneShot(attackSound);
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // Режим преследования
        if (playerInSight && dist <= sightRange)
        {
            agent.speed = chaseSpeed;
            currentStepInterval = stepIntervalRun;
            agent.SetDestination(player.position);
            
            if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                OnAttackZoneEnter(player.GetComponent<PlayerMovement>());
            }
        }
        else
        {
            // Режим блуждания по карте
            agent.speed = patrolSpeed;
            currentStepInterval = stepIntervalWalk;

            // Проверяем, нужно ли сменить направление
            wanderTimer += Time.deltaTime;
            bool reachedTarget = !agent.pathPending && agent.remainingDistance < 0.5f;
            bool timeToChange = wanderTimer >= currentWanderTime;

            if (reachedTarget || timeToChange)
            {
                SetNewWanderTarget();
                wanderTimer = 0f;
                currentWanderTime = Random.Range(minWanderTime, maxWanderTime);
            }

            agent.SetDestination(wanderTarget);
        }

        HandleFootsteps();
    }

    void SetNewWanderTarget()
    {
        // Пытаемся найти случайную точку на NavMesh в радиусе wanderRadius
        for (int i = 0; i < 30; i++)
        {
            // Выбираем случайное направление
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                // Проверяем, что точка на земле (не в воздухе)
                if (IsPointOnGround(hit.position))
                {
                    wanderTarget = hit.position;
                    Debug.Log($"BearAI: Новая цель блуждания: {wanderTarget}");
                    return;
                }
            }
        }
        
        // Если не нашли точку — остаёмся на месте
        wanderTarget = transform.position;
        Debug.LogWarning("BearAI: Не удалось найти точку для блуждания");
    }

    bool IsPointOnGround(Vector3 point)
    {
        RaycastHit hit;
        if (Physics.Raycast(point + Vector3.up * 3f, Vector3.down, out hit, 5f))
        {
            return hit.collider.gameObject.layer == 0 || hit.collider.CompareTag("Ground");
        }
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInSight = true;
            Debug.Log($"BearAI: Игрок ВОШЁЛ в зону видимости!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInSight = false;
            Debug.Log($"BearAI: Игрок ВЫШЕЛ из зоны видимости");
        }
    }

    void HandleFootsteps()
    {
        if (isDead) return;
        bool moving = agent.velocity.magnitude > 0.5f;
        if (moving && !footstepAudio.isPlaying)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= currentStepInterval)
            {
                if (footstepClips.Length > 0)
                    footstepAudio.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)], 0.7f);
                stepTimer = 0;
            }
        }
        else if (!moving)
            stepTimer = currentStepInterval;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        agent.isStopped = true;
        footstepAudio.Stop();
        attackAudioSource.Stop();
        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        // Визуализация радиуса блуждания
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
        
        // Визуализация текущей цели
        if (wanderTarget != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(wanderTarget, 0.5f);
        }
    }
}

// Компонент для зоны атаки
public class AttackTrigger : MonoBehaviour
{
    private BearAI bear;
    public void Init(BearAI b) { bear = b; }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null && bear != null)
            {
                bear.OnAttackZoneEnter(pm);
            }
        }
    }
}