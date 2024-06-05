using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

// a static class which only contains static helper methods for enemy controllers
public static class EnemyUtility
{
    public static bool IsDestOnFloor(Vector3 destination, int enemyFloor, float storey_height)
    {
        // function to check if the player is on the same floor as the enemy agent
        float minHeight = enemyFloor * storey_height;
        float maxHeight = (enemyFloor + 1) * storey_height;
        return (destination.y >= minHeight && destination.y <= maxHeight);
    }

    public static bool IsDestInChaseRadius(NavMeshAgent agent, Vector3 destination, float radiusSqr)
    {
        // function to check if destination point is within radiusSqr squared distance of agent
        // using radiusSqr for faster computation
        float distanceSqr = Vector3.SqrMagnitude(agent.transform.position - destination);
        return distanceSqr <= radiusSqr;
    }

    public static bool DoesPathToDestExist(NavMeshAgent agent, Vector3 destination)
    {
        // function to check if there exists a complete path from player to agent
        // even on the same floor, there may not be a complete path (due to pitfalls, etc.)
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(destination, path);
        return (path.status == NavMeshPathStatus.PathComplete);
    }

    public static Vector3 GenerateRandomDest(NavMeshAgent agent, float searchRadius)
    {
        // function which generates a random Vector3 destination point given an agent and a searchRadius around that agent
        Vector3 center = agent.transform.position;
        Vector3 destination = new Vector3(-1000, -1000, -1000);
        int count = 0;
        while (!DoesPathToDestExist(agent, destination))
        {
            // using unit circle instead of unit sphere to greatly reduce search space
            Vector2 xAndZ = new Vector2(center.x, center.z);
            xAndZ += Random.insideUnitCircle * searchRadius;
            destination = new Vector3(xAndZ.x, center.y, xAndZ.y);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(destination, out hit, 1.0f, NavMesh.AllAreas))
            {
                destination = hit.position;
            }
            if (++count > 1000) return center; // prevent infinite blocking, though I doubt it can ever happen
        }
        return destination;
    }

    public static Vector3 GenerateRandomDestAtY(NavMeshAgent agent, float y, float searchRadius)
    {
        // function which generates a random Vector3 destination point given an agent, a y coord, and a searchRadius around that agent
        Vector3 center = agent.transform.position;
        center.y = y;
        Vector3 destination = new Vector3(-1000, -1000, -1000);
        NavMeshHit hit;
        int count = 0;
        while (!NavMesh.SamplePosition(destination, out hit, 1.0f, NavMesh.AllAreas))
        {
            Vector2 xAndZ = new Vector2(center.x, center.z);
            xAndZ += Random.insideUnitCircle * searchRadius;
            destination = new Vector3(xAndZ.x, center.y, xAndZ.y);
            if (++count > 1000) return agent.transform.position; // prevent infinite blocking, though I doubt it can ever happen
        }
        return hit.position;
    }

    public static Vector3[] GeneratePatrolPoints(NavMeshAgent agent, float startingMinDistance, float startingSearchRadius)
    {
        // function which generates an array of 3 patrol points, attempting to maximize the total distance between all 3 points
        Vector3[] patrolPoints = new Vector3[3];
        patrolPoints[0] = agent.transform.position;
        patrolPoints[1] = GenerateRandomDest(agent, startingSearchRadius);
        patrolPoints[2] = GenerateRandomDest(agent, startingSearchRadius);
        int count = 0;
        while (!ArePointsFarEnough(patrolPoints, startingMinDistance))
        {
            int index = Random.Range(1, 3);
            if (index == 1) patrolPoints[1] = GenerateRandomDest(agent, startingSearchRadius);
            else if (index == 2) patrolPoints[2] = GenerateRandomDest(agent, startingSearchRadius);
            if (++count > 100)
            {
                count = 0;
                startingMinDistance -= 10;
            }
        }
        return patrolPoints;
    }

    public static bool ArePointsFarEnough(Vector3[] points, float minDistance)
    {
        // function which checks if the distance between all 3 points in the array exceed minDistance
        float dist1 = Vector3.Magnitude(points[0] - points[1]);
        float dist2 = Vector3.Magnitude(points[1] - points[2]);
        float dist3 = Vector3.Magnitude(points[2] - points[0]);
        return dist1 + dist2 + dist3 >= minDistance;
    }
}
