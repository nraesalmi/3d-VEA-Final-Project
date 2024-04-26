
using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyAiTutorial : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public Transform currentLocation;
    public LayerMask whatIsGround, whatIsPlayer;
    public float health;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    public float maxTimeOutOfSight = 6.0f;
    public Transform[] teleportLocations;
    public float distantThreshold = 3.0f;

    private float timeOutOfSight = 0.0f;
    private bool canSeePlayer = false;
    private bool hasTeleported = false;

    private void Update()
    {
        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (playerInSightRange) {
            if (playerInAttackRange) {
                AttackPlayer();
            } else {
                canSeePlayer = true;
                timeOutOfSight = 0.0f;
                ChasePlayer();
            } 
        } 
        else {
            canSeePlayer = false;
            timeOutOfSight += Time.deltaTime;

            if (timeOutOfSight >= maxTimeOutOfSight)
            {
                Teleport();
                timeOutOfSight = 0.0f; 
            }
        }

        Patroling();
    }

    private void Patroling()
    {
        if (hasTeleported == true) {
            walkPointSet = false;
            hasTeleported = false;
        }

        if (!walkPointSet) 
        {
            SearchWalkPoint();
        } 
        else
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }
    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;
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

        if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }
    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

    private void Teleport() {
        bool checkDistance = true;
        while (checkDistance) {
            int index = Random.Range(0, teleportLocations.Length);
            Transform randomLocation = teleportLocations[index];
            Vector3 distanceToPlayerVector = randomLocation.position - player.position;
            float distanceToPlayer = distanceToPlayerVector.magnitude;

            if (distanceToPlayer > distantThreshold && currentLocation.position != randomLocation.position) {
                agent.Warp(randomLocation.position);
                currentLocation.position = randomLocation.position;
                checkDistance = false;
            }
        }
        hasTeleported = true;
    }
}
