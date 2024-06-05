using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

// Got this from the Unity forums.
public class PickUp : MonoBehaviour
{
    public TextMeshPro text;
    private bool processed;
    public GameObject thingToFace;
    public GameObject backGroundBox;
    public MazeRenderer renderer;
    public AudioClip pingClip;
    public AudioSource pingSource;

    // Use this for initialization 
    void Start()
    {
        text = gameObject.GetComponent<TextMeshPro>();
        processed = false;
        text.gameObject.transform.localScale = Vector3.Scale(text.gameObject.transform.localScale, new Vector3 (-1, 1, 1));
    }

    // Update is called once per frame 
    void LateUpdate()
    {
        gameObject.transform.LookAt(thingToFace.transform);
        backGroundBox.transform.LookAt(thingToFace.transform);
        text.transform.position = backGroundBox.transform.TransformPoint(backGroundBox.GetComponent<BoxCollider>().center) + (backGroundBox.transform.forward * backGroundBox.GetComponent<BoxCollider>().transform.localScale.z);
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name.Equals(MazeRenderer.PLAYER_NAME))
        {
            if(!processed)  // Prevents double-adding when Unity is finnicky with collisions.
            {
                bool processedNow = false;
                if (text.text.Equals(MazeGenerator.HEALTH_BOOST))
                {
                    renderer.playerHealth += 1;
                    processed = true;
                    processedNow = true;
                }
                else if(text.text.Equals(MazeGenerator.SPEED_BOOST))
                {
                    renderer.playerSpeedModifier += MazeRenderer.SPEED_BOOST_MODIFIER;
                    renderer.speedBoostTimer += MazeRenderer.SPEED_BOOST_TIME;
                    processed = true;
                    processedNow = true;
                }
                else if(text.text.Equals(MazeGenerator.POWER_BOOST))
                {
                    renderer.powerBoostTimer += MazeRenderer.POWER_BOOST_TIME;
                    processed = true;
                    processedNow = true;
                }
                else if(!collider.gameObject.GetComponent<Inventory>().IsHotBarFull())
                {
                    collider.gameObject.GetComponent<Inventory>().add(text.text);
                    processed = true;
                    processedNow = true;
                }

                if(processedNow)
                {
                    pingSource.PlayOneShot(pingClip);
                    Destroy(text);
                    Destroy(gameObject);
                    Destroy(backGroundBox);
                }
            }
        }
    }
}