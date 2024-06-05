using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

// the CHASER enemy normally walks slowly through the maze, randomly
// if you get too close, they will chase you with very fast speed
public class EnemyChaserController : MonoBehaviour
{
    // used for determining if player is on current floor
    public int floor;
    public float storey_height;
    public AudioClip stepClip;
    public AudioSource stepSource;
    public float defaultStepSoundCooldown = (2.0f / 3.0f);
    private float stepSoundCooldown;
    private float currentStepSoundCooldown = 0.0f;
    public AudioClip attackClip;
    public AudioSource attackSource;
    public AudioClip destructionClip;
    public AudioSource destructionSource;
    public float audioDistance;

    GameObject player;
    NavMeshAgent agent;
    public MazeRenderer renderer;

    // properties of CHASER enemy
    private bool isChasing = false;
    private float chaseRadiusSqr = 225.0f; // can be adjusted, using 15^2 for faster computation
    private float walkSpeed = 2.5f; // can be adjusted
    private float runSpeed = 7.0f; // can be adjusted
    private float searchRadius = 50.0f; // can be adjusted
    private float timer = 0;
    private float update = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        stepSoundCooldown = defaultStepSoundCooldown;
        player = GameObject.Find("PLAYER");
        agent = transform.GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;
        agent.acceleration = 16;
        agent.angularSpeed = 240;
        StartCoroutine(MoveToRandomPoint());
    }

    // Update is called once per frame
    void Update()
    {
        if (isChasing)
        {
            stepSoundCooldown = defaultStepSoundCooldown * 0.5f;
        }
        else
        {
            stepSoundCooldown = defaultStepSoundCooldown;
        }
        // to significantly improve performance, only update roughly every 0.5s instead of every frame
        timer += Time.deltaTime;
        if (timer > update)
        {
            SlowUpdate();
            timer = 0;
        }
        currentStepSoundCooldown -= Time.deltaTime;
        if (currentStepSoundCooldown <= 0.0f)
        {
            if (EnemyUtility.IsDestOnFloor(player.transform.position, floor, storey_height) &&
                (Vector3.Distance(player.transform.position, transform.position) < audioDistance))
            {
                stepSource.PlayOneShot(stepClip);
            }
            currentStepSoundCooldown = stepSoundCooldown * (1 + Random.Range(-0.15f, 0.15f));
        }
    }

    void SlowUpdate()
    {
        // if can reach player, chase them
        // checking IsDestOnFloor() -> IsDestInSearchDistance() -> DoesPathToDestExist() greatly saves computation due to short-circuiting
        if (EnemyUtility.IsDestOnFloor(player.transform.position, floor, storey_height) &&
            EnemyUtility.IsDestInChaseRadius(agent, player.transform.position, chaseRadiusSqr) &&
            EnemyUtility.DoesPathToDestExist(agent, player.transform.position))
        {
            isChasing = true;
            agent.speed = runSpeed;
            // transform.GetComponent<Renderer>().material.color = Color.white;
            transform.GetComponent<Animator>().SetBool("run", true);
            transform.GetComponent<Animator>().SetBool("punch", false);
            transform.GetComponent<Animator>().SetBool("walk", false);
            // Debug.Log("chasing " + floor);
            agent.SetDestination(player.transform.position);
        }
        // otherwise, continue the random movement coroutine
        else
        {
            isChasing = false;
            transform.GetComponent<Animator>().SetBool("punch", false);
            transform.GetComponent<Animator>().SetBool("run", false);
            transform.GetComponent<Animator>().SetBool("walk", true);
            agent.speed = walkSpeed;
            // transform.GetComponent<Renderer>().material.color = Color.black;
        }
    }

    IEnumerator MoveToRandomPoint()
    {
        while (true)
        {
            if (!isChasing)
            {
                Vector3 destination = EnemyUtility.GenerateRandomDest(agent, searchRadius);
                agent.SetDestination(destination);
                // Debug.Log("chaser set new dest");
                yield return new WaitUntil(() => (isChasing || agent.hasPath && agent.remainingDistance <= 0.5f));
            }
            yield return null;
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

