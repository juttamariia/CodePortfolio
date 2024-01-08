using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovablePlatform : MonoBehaviour
{
    [Header("Movement Config")]
    [SerializeField] private PlatformWayPointPath wayPointPath;
    [SerializeField] private float moveSpeed;

    private int targetWaypointIndex;
    private Transform previousWaypoint;
    private Transform targetWayPoint;

    private float timeToWaypoint;
    private float elapsedTime;

    private void Start()
    {
        TargetNextWaypoint();
    }

    private void FixedUpdate()
    {
        elapsedTime += Time.deltaTime;

        float elapsedPercentage = elapsedTime / timeToWaypoint;
        transform.position = Vector3.Lerp(previousWaypoint.position, targetWayPoint.position, elapsedPercentage);

        if(elapsedPercentage >= 1)
        {
            TargetNextWaypoint();
        }
    }

    private void TargetNextWaypoint()
    {
        // locate next waypoint
        previousWaypoint = wayPointPath.GetWayPoint(targetWaypointIndex);
        targetWaypointIndex = wayPointPath.GetNextWaypointIndex(targetWaypointIndex);
        targetWayPoint = wayPointPath.GetWayPoint(targetWaypointIndex);

        elapsedTime = 0;

        // calculate time to new waypoint based on distance
        float distanceToWayPoint = Vector3.Distance(previousWaypoint.position, targetWayPoint.position);
        timeToWaypoint = distanceToWayPoint / moveSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        // set player as child when they jump onto the platform
        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.parent.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // detach player child when they jump off the platform
        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.parent.SetParent(null);
        }
    }
}