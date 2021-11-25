using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAIScript : MonoBehaviour
{

    public NavMeshAgent agent;
    public Transform waypointsHolder;
    public Transform player;

    public LayerMask groundMask, playerMask, obstacleMask;
    public GameObject healthBar;
    public HealthBar healthBarScript;

    public bool randomPatrolling = false;
    public bool isFlyingType = false;
    public BoxCollider boxCollider;


    //Chasiing
    public float agentSpeed = 12;

    //Spotting
    public Light spotlight;
    public float viewDistance;
    public LayerMask viewMask;
    [SerializeField] [Range(0f,1f)] float lerpSpeed;
    float viewAngle;

    //Patrolling
    public Vector3 walkPoint;
    bool walkPointSet = false;
    public float walkPointRange;
    private int waypointIndex;
    private float dist;
    public float walkPointMinRange;
    public float turnSpeed = 90;
    public float waitTime = .3f;




    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    //States
    public float sightRange, attackRange;
    public bool playerInSight, playerInAttackRange;

    Color originalSpotlightColour;

    private void Start()
    {
        player = GameObject.Find("PlayerModel").transform;
        agent = GetComponent<NavMeshAgent>();
        viewAngle = spotlight.spotAngle;
        originalSpotlightColour = spotlight.color;
        waypointIndex = 0;

        if (!isFlyingType)
        {
            if (randomPatrolling)
            {
                Vector3[] waypoints = new Vector3[waypointsHolder.childCount];
                for (int i = 0; i < waypoints.Length; i++)
                {
                    waypoints[i] = waypointsHolder.GetChild(i).position;
                    //waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
                }

                StartCoroutine(FollowPath(waypoints));
            }
        }
    }

    private void Update()
    {
        //check attack and sight range
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerMask);
    
        if (!playerInAttackRange && !CanSeePlayer())
        {
            if (!isFlyingType)
            {
                //Patrolling();
                //PatrolWaypoints();
                //StopAllCoroutines();
                StopCoroutine(WaitDetection());
                //lerpSpeed = 0f;
                //agent.speed = 4f;
                spotlight.color = originalSpotlightColour;
            }
            else if (isFlyingType)
            {
                StopAllCoroutines();
                Patrolling();
                StopCoroutine(WaitDetection());
                spotlight.color = originalSpotlightColour;
            }
        }
        else if (!playerInAttackRange && CanSeePlayer())
        {
            //Debug.Log("seeingplayer");
            StartCoroutine(WaitDetection());
        }
        else if (playerInAttackRange && CanSeePlayer())
        {
            //spotlight.color = Color.Lerp(originalSpotlightColour,Color.red, 1);
            AttackPlayer();
        }
        else
        {
            spotlight.color = originalSpotlightColour;
        }

        //Debug.Log(walkPoint);
    }

    bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            if (!isFlyingType)
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
            if (isFlyingType)
            {
                Vector3 dirToPlayer = (player.position - transform.position).normalized;
                float angleBetweenGuardAndPlayer = Vector3.Angle(-transform.up, dirToPlayer);
                if (angleBetweenGuardAndPlayer < viewAngle / 2f)
                {
                    if (!Physics.Linecast(transform.position, player.position, viewMask))
                    {
                        return true;
                    }
                }
            }

        }
        return false;
    }

    IEnumerator FollowPath(Vector3[] waypoints)
    {
        transform.position = waypoints[0];

        int targetWaypointIndex = 1;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        transform.LookAt(targetWaypoint);

        while (true)
        {
            //transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, agentSpeed * Time.deltaTime);
            //agent.SetDestination(targetWaypoint);
            agent.destination = targetWaypoint;
            Vector3 oldnewPositionDiffrence = targetWaypoint - agent.transform.position;
            //Debug.Log(oldnewPositionDiffrence.magnitude);
            if (oldnewPositionDiffrence.magnitude <0.2f)
            {
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(TurnToFace(targetWaypoint));
            }
            yield return null;
        }
    }
    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }

    private void Patrolling()
    {
        if (randomPatrolling)
        {
            if (!walkPointSet)
            {
                SearchWalkPoint();
            }
            if (walkPointSet)
            {

                if (!Physics.CheckSphere(walkPoint, 1f, obstacleMask))
                {
                    agent.SetDestination(walkPoint);
                }
                else
                {
                    walkPointSet = false;
                }
            }

            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            //walkpoint reached 
            if (distanceToWalkPoint.magnitude < 1f)
            {
                walkPointSet = false;
            }
        }
    }



    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(walkPointMinRange, walkPointRange);
        float randomX = Random.Range(walkPointMinRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);


        if (walkPoint.x > boxCollider.bounds.min.x && walkPoint.x < boxCollider.bounds.max.x &&
            walkPoint.z > boxCollider.bounds.min.z && walkPoint.z < boxCollider.bounds.max.z)
        {


            Debug.Log("WalkPoint is inside");
            if (Physics.Raycast(walkPoint, -transform.up, 2f, groundMask))
            {
                walkPointSet = true;

            }
        }
    }

    private void ChasePlayer()
    {
        if (!isFlyingType)
        {
            transform.LookAt(player);
        }
        agent.speed = agentSpeed;
        agent.SetDestination(player.position);

        if (playerInAttackRange)
        {
            healthBarScript.TakeDamage(50);
        }
        
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        transform.LookAt(player);
        if (!alreadyAttacked)
        {
            //put attack code here:
/////////
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private IEnumerator WaitDetection()
    {
        lerpSpeed += 0.001f;
        spotlight.color = Color.Lerp(originalSpotlightColour, Color.red, lerpSpeed);
        yield return new WaitForSeconds(1f);
        ChasePlayer();

    }

    static bool IsInsideBounds(Vector3 worldPos, BoxCollider bc)
    {
        Vector3 localPos = bc.transform.InverseTransformPoint(worldPos);
        Vector3 delta = localPos - bc.center + bc.size * 0.5f;
        if (Vector3.Max(Vector3.zero, delta) == Vector3.Min(delta, bc.size))
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    public void DealDamage(int damage)
    {
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
        Gizmos.DrawRay(transform.position, player.position * viewDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, walkPointRange);
    }

}
