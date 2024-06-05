using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

// the HUNTER enemy slowly follows your location in every floor
// if you move to a different floor, the HUNTER will follow
public class EnemyHunterController : MonoBehaviour
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

    // properties of HUNTER enemy
    private bool isHunting = false;
    private float walkSpeed = 3.0f; // can be adjusted
    private float searchRadius = 50.0f; // can be adjusted
    private float timer = 0;
    private float update = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        stepSoundCooldown = defaultStepSoundCooldown;
        player = GameObject.Find("PLAYER");
        agent = transform.GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;
        agent.acceleration = 12;
        agent.angularSpeed = 240;
        StartCoroutine(MoveToRandomPoint());
    }

    // Update is called once per frame
    void Update()
    {
        if (isHunting)
        {
            stepSoundCooldown = defaultStepSoundCooldown * 0.5f;
        }
        else
        {
            stepSoundCooldown = defaultStepSoundCooldown;
        }
        // to significantly improve performance, only update roughly every 1s instead of every frame
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
        // if the player is not on floor, warp to their floor
        if (!EnemyUtility.IsDestOnFloor(player.transform.position, floor, storey_height))
        {
            // get a new destination at a certain floor
            float y = Mathf.FloorToInt(player.transform.position.y / storey_height) * storey_height;
            Vector3 newDest = EnemyUtility.GenerateRandomDestAtY(agent, y, searchRadius);
            // warp to that destination
            floor = Mathf.FloorToInt(newDest.y / storey_height);
            transform.position = newDest;
            agent.Warp(newDest);
            // restart coroutine (fixes some weird issues)
            StopAllCoroutines();
            StartCoroutine(MoveToRandomPoint());
        }
        // else if can reach player, chase them
        else if (EnemyUtility.DoesPathToDestExist(agent, player.transform.position))
        {
            transform.GetComponent<Animator>().SetBool("run", true);
            transform.GetComponent<Animator>().SetBool("walk", false);
            transform.GetComponent<Animator>().SetBool("punch", false);
            isHunting = true;
            // transform.GetComponent<Renderer>().material.color = Color.white;
            agent.SetDestination(player.transform.position);
        }
        // otherwise, continue the random movement coroutine
        else
        {
            transform.GetComponent<Animator>().SetBool("run", false);
            transform.GetComponent<Animator>().SetBool("walk", true);
            transform.GetComponent<Animator>().SetBool("punch", false);
            isHunting = false;
            // transform.GetComponent<Renderer>().material.color = Color.black;
        }
    }

    IEnumerator MoveToRandomPoint()
    {
        while (true)
        {
            if (!isHunting)
            {
                Vector3 destination = EnemyUtility.GenerateRandomDest(agent, searchRadius);
                agent.SetDestination(destination);
                yield return new WaitUntil(() => (isHunting || agent.hasPath && agent.remainingDistance <= 0.5f));
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
                // Debug.Log("Hit");
                attackSource.PlayOneShot(attackClip);
                collision.gameObject.transform.position = new Vector3(0, storey_height, 0);

                // decrement health
                renderer.playerHealth -= 1;
                renderer.speedBoostTimer = 0.0f;
            }
        }
    }
}

