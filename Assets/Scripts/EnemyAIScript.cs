using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAIScript : MonoBehaviour
{

    public NavMeshAgent agent;

    public Transform player;

    public LayerMask groundMask, playerMask;


    //Spotting
    public Light spotlight;
    public float viewDistance;
    public LayerMask viewMask;
    float viewAngle;

    //Patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    //States
    public float sightRange, attackRange;
    public bool playerInSight, playerInAttackRange;

    Color originalSpotlightColour;

    private void Awake()
    {
        player = GameObject.Find("PlayerModel").transform;
        agent = GetComponent<NavMeshAgent>();
        viewAngle = spotlight.spotAngle;
        originalSpotlightColour = spotlight.color;
    }

    private void Update()
    {
        //check attack and sight range
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerMask);
        playerInSight = Physics.CheckSphere(transform.position, sightRange, playerMask);

        if (!playerInAttackRange && !playerInSight)
        {
            Patrolling();
        }
        else if (!playerInAttackRange && playerInSight)
        {
            //ChasePlayer();
        }
        else if (playerInAttackRange && playerInSight)
        {
            AttackPlayer();
        }

        if (CanSeePlayer())
        {
            spotlight.color = Color.red;
            ChasePlayer();
        }
        else
        {
            spotlight.color = originalSpotlightColour;
        }

        //OnDrawGizmosSelected();
        Debug.Log(walkPoint);
    }

    bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            if (angleBetweenGuardAndPlayer < viewAngle / 2f)
            {
                if (!Physics.Linecast(transform.position, player.position, viewMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void Patrolling()
    {
        if (!walkPointSet)
        {
            SearchWalkPoint();
        }
        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //walkpoint reached 
        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.y + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, groundMask))
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
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            //put attack code here:

            //a
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

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }

}
