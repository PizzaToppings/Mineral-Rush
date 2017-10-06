using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    public Map mapScript;           // the mapscript. use for finding a location to set the player

    private Vector3 currentPos;     // the position the character is at, or where it was last. set to the same position of a waypoint
    private Vector3 nextPos;        // the waypoint the character is walking towards. it is  adjacent to the currentpos
    private float lerpTime;         // used for lerping the player from currentpos to nextpos
    private bool canMove;           // see if the character has found minerals, and allow it to move

    private Camera cam;             // the camera. this is used to set the startposition close to the player

	void Start ()
    {
        SetStartPos();
        cam = Camera.main;
        cam.transform.parent.transform.position = new Vector3(transform.position.x, transform.position.y + 20, transform.position.z - 30);
    }
	
	void Update ()
    {
        if (canMove)
        {
            Move();
        }      
    }

    // sets the random starting position of the player. Also checks if it is on a walkable place
    void SetStartPos()
    {
        int xValue = Random.Range(0, mapScript.width);
        int zValue = Random.Range(0, mapScript.length);

        if (GameManager.waypoints[xValue, zValue].GetComponent<Waypoint>().walkable) // So it will not spawn in water/ on a mountain
        {
            transform.position = GameManager.waypoints[xValue, zValue].transform.position;
            currentPos = GameManager.waypoints[xValue, zValue].transform.position;
            nextPos = currentPos;
            // if the player is placed, start looking for minerals
            StartCoroutine(GameManager.SetWaypointDistance(0, (int)transform.position.x, (int)transform.position.z, gameObject, 0.01f));
        }
        else
        {
            SetStartPos(); // retry
        }
    }

    // sets the next position to move towards. Deletes the location out of the routepoints list if it has been reached
    public void SetNextPos()
    {
        if (GameManager.RoutePoints.Count != 1) // 1 in stead of 0. With 0 it would stop within the mineral
        {
            nextPos = GameManager.RoutePoints[0].transform.position;
            GameManager.RoutePoints.Remove(GameManager.RoutePoints[0]);
            lerpTime = 0;
            canMove = true;
        }
        else
        {
            // if the mineral has been reached. destroys the mineral and removes it from the targetpos List
            Destroy(GameManager.currentTarget, 1f);
            GameManager.targetPos.Remove(GameManager.currentTarget);
            GameManager.clearRoute();
            canMove = false; 
            if (GameManager.targetPos.Count > 0)
            {
                StartCoroutine(GameManager.SetWaypointDistance(0, (int)transform.position.x, (int)transform.position.z, gameObject, 2f));
            }
            else
            {
                // if all minerals have been found, tell the player
                cam.transform.parent.GetComponent<CameraController>().finishText.enabled = true;
            }
        }
    }

    // moves to the next point by lerping. if the next point has been reached, that point will be set as the current point. Will then set a new nextpoint
    void Move()
    {
        lerpTime += Time.deltaTime * 5;
        transform.position = Vector3.Lerp(currentPos, nextPos, lerpTime);

        if (lerpTime >= 1)
        {
            currentPos = nextPos;
            canMove = false;
            SetNextPos();
        }
    }
}
