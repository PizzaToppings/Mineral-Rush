using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager {
        
    // mapdata                              //these are also explained in 'Map'. Values are set in the menu
    public static int size;                 
    public static float scale;              
    public static float height;             
    public static float heightScale;        
    public static int mineralAmount;        // amount of minerals that will be spawned
    public static bool showCalculations;    // decided if the waypoints will show the distance and selected path. 

    public static GameObject[,] waypoints;                                  // the array of all the waypoints
    public static List<GameObject> targetPos = new List<GameObject>();      // the minerals. Their position will be used
    public static GameObject currentTarget;                                 // the mineral that has been found
    public static bool destinationFound;                                    // turns true if a mineral is found
    public static bool routeFound;                                          // turns true if the fastest path has been found
    public static List<GameObject> RoutePoints = new List<GameObject>();    // the points of the route that will eventually be walked

    public static GameObject character;

    /*
        Sets the distance between the character and the current waypoint. can only be done if the path is walkable
        Also sets the previouswaypoint for the next waypoint. this is used to finally create a path
        Will stop if a destination has been found. Will always find the one with the shortest path
        An IEnumerator has been used to give a 'searching' vibe to the player. Otherwise it would find it instantly
    */
    public static IEnumerator SetWaypointDistance(float distance, int xPos, int zPos, GameObject previousWaypoint, float waitTime) 
    {
        yield return new WaitForSeconds(waitTime);
        if (xPos != -1 && zPos != -1 && xPos != size && zPos != size) 
        {
            if (waypoints[xPos, zPos].GetComponent<Waypoint>().walkable && !destinationFound)
            {
                waypoints[xPos, zPos].GetComponent<Waypoint>().SetDistance(distance, previousWaypoint);

                for (int i = 0; i < targetPos.Count; i++)
                {
                    if (xPos == (int)targetPos[i].transform.position.x && zPos == (int)targetPos[i].transform.position.z)
                    {
                        currentTarget = targetPos[i];
                        targetPos.Remove(targetPos[i]);
                        destinationFound = true;
                        GetRoute(xPos, zPos);
                    }
                }
            }
        }
    }

    /* 
        if a destination has been found, a route will be created. This is done by linking the waypoints from the target to the character. 
        Waypoints are linked by the 'previouswaypoint' gameobjects. they will all be added to the routepoints list
    */
    public static void GetRoute(int xPos, int zPos)
    {
        RoutePoints.Add(waypoints[xPos, zPos]);

        int nextXpos = (int)waypoints[xPos, zPos].GetComponent<Waypoint>().previousWaypoint.transform.position.x;
        int nextZpos = (int)waypoints[xPos, zPos].GetComponent<Waypoint>().previousWaypoint.transform.position.z;

        if (waypoints[xPos, zPos].GetComponent<Waypoint>().shortestDistance > 0)
        {
            GetRoute(nextXpos, nextZpos);
        }
        else
        {
            ShowRoute();
            routeFound = true;
            character.GetComponent<Character>().SetNextPos();
        }
    }

    // reverses the routepoints list, because it needs to go from player to the destination
    // will also  show the path if calculations need to be shown
    public static void ShowRoute() 
    {
        RoutePoints.Reverse();

        if (showCalculations)
        {
            foreach (GameObject RP in RoutePoints)
            {
                RP.GetComponent<Waypoint>().distanceText.color = Color.white;
            }
        }
    }

    // if the destination is reached, everything will be cleared, so it can be repeated for the next destination
    public static void clearRoute()
    {
        destinationFound = false;
        routeFound = false;
        foreach (GameObject WP in waypoints)
        {
            if (showCalculations)
            {
                WP.GetComponent<Waypoint>().distanceText.text = "";
                WP.GetComponent<Waypoint>().distanceText.color = Color.black;
            }
            WP.GetComponent<Waypoint>().shortestDistance = 1000;
        }
        RoutePoints.Clear();
    }
}
