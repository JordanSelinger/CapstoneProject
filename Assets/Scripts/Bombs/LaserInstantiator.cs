﻿using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class LaserInstantiator : NetworkBehaviour
{
    public uint bombNetId;
    public GameObject laserCross;
    public GameObject laserUp;
    public GameObject laserDown;
    public GameObject laserLeft;
    public GameObject laserRight;
    public GameObject laserHor;
    public GameObject laserVert;

    public void InstantiateLaser(uint netId)
    {
        bombNetId = netId;
        var temp = gameObject.GetComponent<BombController>().paramaters;
        var paramaters = new BombParams
        {
            radius = temp.radius,
            delayTime = temp.delayTime,
            warningTime = temp.warningTime,
            explodingDuration = temp.explodingDuration
        };
        var location = new Vector2(AxisRounder.Round(gameObject.transform.position.x), AxisRounder.Round(gameObject.transform.position.y));

        GameObject laser = Instantiate(laserCross, location, Quaternion.identity) as GameObject;

        //TODO refactor this too
        laser.GetComponent<LaserController>().creationTime = Time.time;
        laser.GetComponent<LaserController>().paramaters = paramaters;
        laser.GetComponent<LaserController>().bombNetId = bombNetId;

		InstantiateInDirection(location, Vector2.up, paramaters);
        InstantiateInDirection(location, Vector2.down, paramaters);
		InstantiateInDirection(location, Vector2.left, paramaters);
		InstantiateInDirection(location, Vector2.right, paramaters);
    }

	[Command]
	private void CmdDestroyThis(GameObject thing){
		NetworkServer.Destroy (thing);
	}

    private void InstantiateInDirection(Vector2 location, Vector2 direction, BombParams paramaters)
    {
        var emptySpace = Physics2D.RaycastAll(location, direction)
            .Where(h => h.distance != 0 && h.transform.tag != "Laser" && h.transform.tag != "Bomb" && h.transform.tag != "Player")
            .First();

        int numLasers = emptySpace.distance < paramaters.radius ? (int)emptySpace.distance : paramaters.radius;

        if ((emptySpace.transform.tag == "Destructible" || emptySpace.transform.tag == "Upgrade") && emptySpace.distance <= paramaters.radius)
			if(isServer)
				CmdDestroyThis(emptySpace.transform.gameObject);

        for (int i = 1; i <= numLasers; i++)
        {
            GameObject laser;
            if (i == paramaters.radius)
                laser = Instantiate(GetLaser(direction), new Vector3(location.x + direction.x * i, location.y + direction.y * i, 0.0f), Quaternion.identity) as GameObject;
            else
                laser = Instantiate(GetMiddleLaser(direction), new Vector3(location.x + direction.x * i, location.y + direction.y * i, 0.0f), Quaternion.identity) as GameObject;

            //TODO refactor all of this to make it cleaner/faster
            laser.GetComponent<LaserController>().creationTime = Time.time;
            laser.GetComponent<LaserController>().paramaters = paramaters;
            laser.GetComponent<LaserController>().bombNetId = bombNetId;
        }
    }

    private GameObject GetMiddleLaser(Vector2 direction)
    {
		if (direction == Vector2.up || direction == Vector2.down)
            return laserVert;
        else
            return laserHor;
    }

    private GameObject GetLaser(Vector2 direction)
    {
		if (direction == Vector2.up)
            return laserUp;
		else if (direction == Vector2.down)
            return laserDown;
		else if (direction == Vector2.left)
            return laserLeft;
        else
            return laserRight;
    }
}
