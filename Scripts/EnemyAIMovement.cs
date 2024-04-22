using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class EnemyAIMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public float health;

    //Patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;
    public float attackRange;

    // Reference to sight range
    public float radius;
    [Range(0, 360)]
    public float angle;
    public bool canSeePlayer;
    public LayerMask obstructionMask;

    private void Start()
    {
        player = GameObject.Find("PlayerObj").transform;
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(FOV());
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

    private IEnumerator FOV()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            FOVCheck();
        }
    }

    private bool FOVCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, whatIsPlayer);

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
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
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
            ///Attack code here
            Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            rb.AddForce(transform.up * 8f, ForceMode.Impulse);
            ///End of attack code

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
            DestroyEnemy();
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}
