using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpAnimation : MonoBehaviour
{
    float timer;
    Vector3 initialPosition;

    // some properties of sin transformation
    float speed = 2.0f;
    float amplitude = 0.2f;
    float translation = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        // random offset so each animation is not synced
        timer = Random.Range(0f, 2.0f * Mathf.PI);
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        float newY = initialPosition.y + Mathf.Sin(timer * speed) * amplitude + translation;

        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);
    }
}
