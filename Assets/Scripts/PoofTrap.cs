using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoofTrap : MonoBehaviour
{
    public int floor;  // Index
    public float storey_height;
    public Bounds bounds;
    public string[][,][] maze;
    public AudioClip warpClip;
    public AudioSource warpSource;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name.Equals(MazeRenderer.PLAYER_NAME))
        {
            int teleportFloor = floor;
            while((teleportFloor == floor) && (maze.Length > 1))
            {
                teleportFloor = Random.Range(0, maze.Length);
                Debug.Log("Random: " + teleportFloor);
            }
            Debug.Log("Teleport floor: " + teleportFloor);
            Debug.Log("Maze length: " + maze.Length);
            string[,][] floorGrid = maze[teleportFloor];
            float width = (float)floorGrid.GetLength(0);
            float length = (float)floorGrid.GetLength(1);
            (int, int)[] corners = new (int, int)[4];
            corners[0] = (0, ((int)length - 1) / 2);
            corners[1] = ((int)width - 1, ((int)length - 1) / 2);
            corners[2] = (((int)width - 1) / 2, 0);
            corners[3] = (((int)width - 1) / 2, (int)length - 1);
            (int, int) corner = corners[Random.Range(0, corners.Length)];
            collision.gameObject.transform.position = new Vector3(bounds.min[0] + (corner.Item1 * bounds.size[0] / width) + 0.5f,
                                                                  bounds.min[1] + (teleportFloor * storey_height + (storey_height / 2)),
                                                                  bounds.min[2] + (corner.Item2 * bounds.size[2] / length) + 0.5f);
            warpSource.PlayOneShot(warpClip, 0.5f);
        }
    }
}
