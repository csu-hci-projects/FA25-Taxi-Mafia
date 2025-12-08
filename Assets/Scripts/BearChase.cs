using UnityEngine;
using UnityEngine.AI;

public class BearChase : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] float detectionDistance = 20f;
    [SerializeField] float attackDistance = 3f;
    
    [Header("Movement Settings")]
    [SerializeField] float chaseSpeed = 5f;
    [SerializeField] float rotationSpeed = 5f;
    
    [Header("Attack Settings")]
    [SerializeField] float attackDamage = 10f;
    [SerializeField] float attackCooldown = 2f;
    [SerializeField] float attackRange = 2.5f;
    
    [Header("References")]
    [SerializeField] Animator bearAnimator;
    [SerializeField] HUDManager hudManager;
    
    private GameObject playerCar;
    private NavMeshAgent navAgent;
    private Rigidbody rb;
    private bool isChasing = false;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;
    private string attackTrigger = "Attack1";
    
    void Start()
    {
        // Get or add components
        navAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        
        // If no NavMeshAgent, we'll use simple movement
        if (navAgent != null)
        {
            navAgent.speed = chaseSpeed;
            navAgent.angularSpeed = rotationSpeed * 50f;
        }
        
        // Get animator if not assigned
        if (bearAnimator == null)
        {
            bearAnimator = GetComponent<Animator>();
        }
        
        // Find player car by tag
        playerCar = GameObject.FindGameObjectWithTag("PlayerCar");
        
        // Find HUDManager if not assigned
        if (hudManager == null)
        {
            hudManager = FindObjectOfType<HUDManager>();
        }
        
        // Set initial animation state
        if (bearAnimator != null)
        {
            bearAnimator.SetBool("Idle", true);
            bearAnimator.SetBool("Run Forward", false);
            bearAnimator.SetBool("Combat Idle", false);
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
        if (bearAnimator != null)
        {
            bearAnimator.SetBool("Idle", false);
            bearAnimator.SetBool("Combat Idle", false);
            bearAnimator.SetBool("Run Forward", true);
        }
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
        else
        {
            // Simple movement with Rigidbody or Transform
            if (rb != null)
            {
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
        
        // Trigger attack animation
        if (bearAnimator != null)
        {
            bearAnimator.SetTrigger(attackTrigger);
            bearAnimator.SetBool("Run Forward", false);
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
        
        // Reset attacking state after a delay (animation duration)
        Invoke("ResetAttackState", 1.5f);
    }
    
    void ResetAttackState()
    {
        isAttacking = false;
        if (bearAnimator != null)
        {
            bearAnimator.SetBool("Run Forward", true);
        }
    }
    
    void UpdateAnimator()
    {
        if (bearAnimator == null) return;
        
        // Keep Run Forward state while chasing
        if (isChasing && !isAttacking)
        {
            bearAnimator.SetBool("Run Forward", true);
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

