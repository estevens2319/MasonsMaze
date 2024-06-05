using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoofTrapAnimation : MonoBehaviour
{
    float timer;
    Vector3 initialScale;

    // some properties of sin transformation
    float speed = 1.0f;
    float minScale = 0.8f;
    float maxScale = 1.2f;
    // Start is called before the first frame update
    void Start()
    {
        // random offset so each animation is not synced
        timer = Random.Range(0f, 2.0f * Mathf.PI);
        initialScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        float newScale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(timer * speed) + 1.0f) / 2.0f);
        transform.localScale = new Vector3(initialScale.x * newScale, initialScale.y, initialScale.z * newScale);
    }
}
