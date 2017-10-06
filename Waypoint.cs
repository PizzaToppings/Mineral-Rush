using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour {

    public bool walkable;                   // set to true if this waypoint is walkable
    public float shortestDistance;          // 1000 by default, so it will never be too small to start
    public GameObject previousWaypoint;     // the previous waypoint that was used when distance is being measured. these can eventually be used to create the path that will be walked  

    public TextMesh distanceText;           // textmesh that will show the distance from the player. can be toggled on/off. Usefull if you want to see the calculations

	// Use this for initialization
	void Start ()
    {
        distanceText = GetComponent<TextMesh>();
	}

    // sets the distance from the player. Makes sure this  is not repeated if the a bigger distance will be given
    // additionally, sets the previous waypoint (for pathfinding), and will call the function to set the next distance for the adjacent waypoints.
    public void SetDistance(float distance, GameObject PWp)
    {
        // horizontal or vertical is 1 distance. diagonal is 1.5 distance
        if (distance < shortestDistance)
        {
            previousWaypoint = PWp; 
            shortestDistance = distance;
            if (GameManager.showCalculations)
            {
                distanceText.text = shortestDistance.ToString();
            }

            // select and set the next waypoints
            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    if  (x == 0 || z == 0)
                    {
                        StartCoroutine(GameManager.SetWaypointDistance(distance + 1, (int)transform.position.x + x, (int)transform.position.z + z, gameObject, 0.01f));
                    }
                    else
                    {
                        StartCoroutine(GameManager.SetWaypointDistance(distance + 1.5f, (int)transform.position.x + x, (int)transform.position.z + z, gameObject, 0.015f));
                    }
                }
            }
        }
    }
}
