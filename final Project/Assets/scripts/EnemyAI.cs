
using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyAi : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public Transform currentLocation;
    public LayerMask whatIsGround, whatIsPlayer;

    //Enemy Stats
    public float health;
    public float cooldown;
    public float damage;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    public float maxTimeOutOfSight = 6.0f;
    public Transform[] teleportLocations;
    public float distantThreshold = 3.0f;

    private float timeOutOfSight = 0.0f;
    private bool hasTeleported = false;

    private void Start()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (playerInSightRange) {
            if (playerInAttackRange) {
                AttackPlayer(player.GetComponent<Player>());
            } else {
                timeOutOfSight = 0.0f;
                ChasePlayer();
            } 
        } 
        else {
            Patroling();
            timeOutOfSight += Time.deltaTime;

            if (timeOutOfSight >= maxTimeOutOfSight)
            {
                Teleport();
                timeOutOfSight = 0.0f; 
            }
        }
    }

    private void Patroling()
    {
        //Debug.Log("Patrolling");
        if (hasTeleported == true) {
            walkPointSet = false;
            hasTeleported = false;
        }

        if (!walkPointSet) 
        {
            GetRandomNavMeshLocation(walkPointRange);
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
    public Vector3 GetRandomNavMeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            walkPoint = hit.position;
        }
        walkPointSet = true;
        return walkPoint;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
        //Debug.Log("Chasing player");
    }

    private void AttackPlayer(Player player)
    {
        player.TakeDamage(damage);

        // Teleport somewhere else after attacking
        Teleport();

    }

    public void TakeDamage(float incomingDamage)
    {
        health -= incomingDamage;

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Destroy enemy
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
                currentLocation = randomLocation;
                checkDistance = false;
            }
        }
        hasTeleported = true;
    }
}
