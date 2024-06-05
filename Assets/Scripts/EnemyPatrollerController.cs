using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

// the PATROLLER enemy travels between three different patrol points at a constant speed
public class EnemyPatrollerController : MonoBehaviour
{
    // used for determining if player is on current floor
    public int floor;
    public float storey_height;
    public AudioClip stepClip;
    public AudioSource stepSource;
    public float stepSoundCooldown = (2.0f / 3.0f);
    private float currentStepSoundCooldown = 0.0f;
    public AudioClip attackClip;
    public AudioSource attackSource;
    public AudioClip destructionClip;
    public AudioSource destructionSource;
    public float audioDistance;

    NavMeshAgent agent;
    GameObject player;
    public MazeRenderer renderer;

    // properties of PATROLLER enemy
    private Vector3[] patrolPoints = new Vector3[3];
    private int nextPatrolPoint = 0;
    private float walkSpeed = 4.0f; // can be adjusted
    private float minimumPatrolDistance = 100.0f; // can be adjusted
    private float searchRadius = 150.0f; // can be adjusted
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("PLAYER");
        agent = transform.GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;
        agent.acceleration = 12;
        agent.angularSpeed = 240;

        // set three patrol points, one is the starting position and the other two are randomly assigned
        // NOTE: this function call causes some lag at instantiation, can decrease minimumPatrolDistance and searchRadius if needed
        patrolPoints = EnemyUtility.GeneratePatrolPoints(agent, minimumPatrolDistance, searchRadius);
        // foreach (Vector3 p in patrolPoints) Debug.Log(p);
        StartCoroutine(MoveToPatrolPoints());
    }

    IEnumerator MoveToPatrolPoints()
    {
        // Renderer renderer = transform.GetComponent<Renderer>();
        while (true)
        {
            // cycle through patrol points
            nextPatrolPoint = (nextPatrolPoint + 1) % patrolPoints.Length;
            Vector3 destination = patrolPoints[nextPatrolPoint];
            transform.GetComponent<Animator>().SetBool("run", false);
            transform.GetComponent<Animator>().SetBool("walk", true);
            transform.GetComponent<Animator>().SetBool("punch", false);
            // renderer.material.color = colors[nextPatrolPoint];
            agent.SetDestination(destination);
            // Debug.Log("patroller set new dest");
            yield return new WaitUntil(() => agent.hasPath && agent.remainingDistance <= 0.5f);
        }
    }

    void Update()
    {
        currentStepSoundCooldown -= Time.deltaTime;
        if(currentStepSoundCooldown <= 0.0f)
        {
            if (EnemyUtility.IsDestOnFloor(player.transform.position, floor, storey_height) &&
                (Vector3.Distance(player.transform.position, transform.position) < audioDistance))
            {
                stepSource.PlayOneShot(stepClip);
            }
            currentStepSoundCooldown = stepSoundCooldown * (1 + Random.Range(-0.15f, 0.15f));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Equals(MazeRenderer.PLAYER_NAME))
        {
            if (renderer.powerBoostTimer > 0)
            {
                Destroy(gameObject);
                destructionSource.PlayOneShot(destructionClip);
            }
            else
            {
                transform.GetComponent<Animator>().SetBool("run", false);
                transform.GetComponent<Animator>().SetBool("walk", false);
                transform.GetComponent<Animator>().SetBool("punch", true);
                attackSource.PlayOneShot(attackClip);
                collision.gameObject.transform.position = new Vector3(0, storey_height, 0);

                // decrement health
                renderer.playerHealth -= 1;
                renderer.speedBoostTimer = 0.0f;
            }
        }
    }
}