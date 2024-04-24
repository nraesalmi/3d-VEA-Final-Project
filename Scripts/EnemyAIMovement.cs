using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
using Random = UnityEngine.Random;

public class EnemyAIMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;

    //Patrolling
    private Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;
    public Transform[] walkPoints;
    public float pauseTime; 
    private float timer = 0f;
    private bool isPaused = false;

    //Attacking
    public float attackRange;
    public Animator animator; 
    public float timeBetweenAttacks;
    private bool alreadyAttacked;
    public float endGameDelay;

    // Reference to sight range
    public float radius;
    [Range(0, 360)]
    public float angle;
    public bool canSeePlayer = false;
    public LayerMask obstructionMask;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        Transform transform1 = GameObject.Find("PlayerObj").transform;
        player = transform1;
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(FOV);
    }

    private void Update()
    {
        // Check if the player is within sight of the enemy camera
        if (FOVCheck())
        {
            ChasePlayer();
            if (PlayerInAttackRange())
                AttackPlayer();
        }
        else
        {
            Patroling();
        }
    }

    private IEnumerator FOV
    {
        get
        {
            WaitForSeconds wait = new(0.2f);

            while (true)
            {
                yield return wait;
                FOVCheck();
            }
        }
    }

    private bool FOVCheck()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, whatIsPlayer);
        Collider[] rangeChecks = colliders;

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                { 
                    canSeePlayer = true;
                }   
            else
                canSeePlayer = false;
            }
            else
                canSeePlayer = false;
        }
        else if (canSeePlayer)
            canSeePlayer = false;
        
        return canSeePlayer;
    }
    //

    private bool PlayerInAttackRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= attackRange;
    }

    private void Patroling()
    {
        if (!walkPointSet)
            SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
        {
            if (!isPaused)
            {
                timer = 0f;
                isPaused = true;
            }

            timer += Time.deltaTime;

            if (timer >= pauseTime)
            {
                isPaused = false;
                walkPointSet = false;
            }
        }
    }

    private void SearchWalkPoint()
    {
        if (walkPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, walkPoints.Length);
            Transform selectedLocation = walkPoints[randomIndex];

            Vector3 randomOffset = Random.insideUnitSphere * walkPointRange;
            randomOffset.y = 0f; 
            walkPoint = selectedLocation.position + randomOffset;

            if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
                walkPointSet = true;
        }
        else
        {
            Debug.LogWarning("No walkponts locations available.");
        }
    }


    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            animator.SetTrigger("Attack");


            float animationDelay = 1.5f; 


            Invoke(nameof(EndGame), animationDelay + endGameDelay);

            alreadyAttacked = true;


            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void EndGame()
    {

        Debug.Log("Game Over!");
        // Application.Quit(); // Use this to quit the application (not recommended in most cases)
        // Or you can set a game over flag and handle game over logic elsewhere
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
}
