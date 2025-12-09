using UnityEngine;
using UnityEngine.AI;

public class DeerChase : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] float detectionDistance = 20f;
    [SerializeField] float attackDistance = 3f;
    
    [Header("Movement Settings")]
    [SerializeField] float chaseSpeed = 6f; // Deer might be faster than bear
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float runDistance = 8f; // Distance threshold to switch from walk to run
    
    [Header("Attack Settings")]
    [SerializeField] float attackDamage = 8f; // Slightly less damage than bear
    [SerializeField] float attackCooldown = 2f;
    [SerializeField] float attackRange = 2.5f;
    
    [Header("References")]
    [SerializeField] Animator deerAnimator;
    [SerializeField] HUDManager hudManager;
    
    private GameObject playerCar;
    private NavMeshAgent navAgent;
    private Rigidbody rb;
    private CharacterController characterController;
    private bool isChasing = false;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;
    
    // Deer animation parameters
    // Vert: 0 = idle, 1 = walk/run
    // State: 0 = walk, 1 = run (only when Vert = 1)
    private string stateParameter = "State"; // 0 = walk, 1 = run
    private string vertParameter = "Vert"; // 0 = idle, 1 = walk/run
    
    void Start()
    {
        // Get or add components
        navAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
        
        // If no NavMeshAgent, we'll use simple movement
        if (navAgent != null)
        {
            navAgent.speed = chaseSpeed;
            navAgent.angularSpeed = rotationSpeed * 50f;
        }
        
        // Get animator if not assigned
        if (deerAnimator == null)
        {
            deerAnimator = GetComponent<Animator>();
        }
        
        // Find player car by tag
        playerCar = GameObject.FindGameObjectWithTag("PlayerCar");
        
        // Find HUDManager if not assigned
        if (hudManager == null)
        {
            hudManager = FindObjectOfType<HUDManager>();
        }
        
        // Set initial animation state (idle)
        if (deerAnimator != null)
        {
            deerAnimator.SetFloat(vertParameter, 0f); // Idle (Vert = 0)
            deerAnimator.SetFloat(stateParameter, 0f);
        }
    }
    
    void Update()
    {
        if (playerCar == null)
        {
            // Try to find player again
            playerCar = GameObject.FindGameObjectWithTag("PlayerCar");
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerCar.transform.position);
        
        // Check if player is within detection distance
        if (distanceToPlayer <= detectionDistance && !isChasing)
        {
            StartChasing();
        }
        
        // If chasing, move towards player
        if (isChasing && !isAttacking)
        {
            // Check if close enough to attack
            if (distanceToPlayer <= attackDistance)
            {
                TryAttack();
            }
            else
            {
                MoveTowardsPlayer();
            }
        }
        
        // Update animator
        UpdateAnimator();
    }
    
    void StartChasing()
    {
        isChasing = true;
        // Animation will be set in UpdateAnimator based on distance
    }
    
    void MoveTowardsPlayer()
    {
        Vector3 direction = (playerCar.transform.position - transform.position).normalized;
        direction.y = 0; // Keep movement on horizontal plane
        
        // Use NavMeshAgent if available
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.SetDestination(playerCar.transform.position);
        }
        else if (characterController != null)
        {
            // Use CharacterController if available
            characterController.Move(direction * chaseSpeed * Time.deltaTime);
        }
        else if (rb != null)
        {
            // Use Rigidbody
            if (!rb.isKinematic)
            {
                rb.MovePosition(transform.position + direction * chaseSpeed * Time.deltaTime);
            }
            else
            {
                // If kinematic, use transform directly
                transform.position += direction * chaseSpeed * Time.deltaTime;
            }
        }
        else
        {
            // No rigidbody, use transform
            transform.position += direction * chaseSpeed * Time.deltaTime;
        }
        
        // Rotate towards player
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    void TryAttack()
    {
        // Check cooldown
        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerCar.transform.position);
        
        // Check if still in attack range
        if (distanceToPlayer <= attackRange)
        {
            PerformAttack();
        }
    }
    
    void PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Update animation to run when attacking
        if (deerAnimator != null)
        {
            deerAnimator.SetFloat(vertParameter, 1f); // Walk/Run
            deerAnimator.SetFloat(stateParameter, 1f); // Run
        }
        
        // Deal damage to player
        if (hudManager != null)
        {
            hudManager.TakeDamage(attackDamage);
        }
        
        // Also try to get CarDamage component directly from player
        CarDamage carDamage = playerCar.GetComponent<CarDamage>();
        if (carDamage != null && carDamage.hud != null)
        {
            carDamage.hud.TakeDamage(attackDamage);
        }
        
        // Reset attacking state after a delay (attack duration)
        Invoke("ResetAttackState", 1.0f);
    }
    
    void ResetAttackState()
    {
        isAttacking = false;
        // Animation will be updated in UpdateAnimator
    }
    
    void UpdateAnimator()
    {
        if (deerAnimator == null) return;
        
        if (!isChasing)
        {
            // Idle state
            deerAnimator.SetFloat(vertParameter, 0f); // Idle
            deerAnimator.SetFloat(stateParameter, 0f);
        }
        else
        {
            // Chasing - determine walk or run based on distance
            float distanceToPlayer = playerCar != null ? 
                Vector3.Distance(transform.position, playerCar.transform.position) : float.MaxValue;
            
            deerAnimator.SetFloat(vertParameter, 1f); // Walk/Run (not idle)
            
            if (distanceToPlayer <= runDistance || isAttacking)
            {
                // Close to player or attacking - use run animation
                deerAnimator.SetFloat(stateParameter, 1f); // Run
            }
            else
            {
                // Far from player - use walk animation
                deerAnimator.SetFloat(stateParameter, 0f); // Walk
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}

