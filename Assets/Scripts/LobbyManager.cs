﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class LobbyManager : NetworkLobbyManager
{
	private BoardCreator boardCreator;
	private IList<int> connectedPlayerIds = new List<int>(); //TODO LIMIT ON THE NUMBER OF PLAYERS
    private Vector3[] playerSpawnVectors = new Vector3[4]
	{   new Vector3(1.0f, 11.0f, 0.0f),
		new Vector3(13.0f, 1.0f, 0.0f),
		new Vector3(13.0f, 11.0f, 0.0f),
		new Vector3(1.0f, 1.0f, 0.0f)
	};

    private bool sceneLoaded = false;

	public GameObject bombUpgrade;
	public GameObject laserUpgrade;
	public GameObject kickUpgrade;
	public GameObject lineUpgrade;
	public GameObject radioactiveUpgrade;

	public GameObject floor;
	public GameObject destructible;
	public GameObject indestructible;
	public GameObject[] playerAnimations;

	public override void OnLobbyStartServer ()
	{
		connectedPlayerIds.Clear ();
	}

	public override void OnLobbyServerConnect (NetworkConnection conn)
	{
		if(conn.address != "localServer")
			connectedPlayerIds.Add (conn.connectionId);
	}

	public override void OnLobbyServerDisconnect(NetworkConnection networkConnection){
		connectedPlayerIds.Remove (networkConnection.connectionId);		
	}

	public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection networkConnection, short playerControllerId)
    {
		int i = getSlotIndex (networkConnection.connectionId);

		GameObject newPlayer = Instantiate (playerPrefab.gameObject);
		newPlayer.transform.position = playerSpawnVectors [i];
		newPlayer.GetComponent<PlayerControllerComponent> ().playerIndex = i;

		NetworkServer.Spawn(newPlayer);
		return newPlayer;
    }

    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject gameObject1, GameObject gameObject2) {

        if(!sceneLoaded)
            SpawnBoard ();
        sceneLoaded = true;
		return true;
    }

    public override void OnLobbyServerPlayersReady()
    {
		ServerChangeScene(playScene);
    }

	private void SpawnBoard() {
		boardCreator = new BoardCreator ();
		boardCreator.InitializeDestructible ();

			//Initialize spawn for all connected players
		connectedPlayerIds.Where (x => x != null).ToList()
			.ForEach (x => boardCreator.InitializeSpawn (playerSpawnVectors [getSlotIndex (x)]));

			//Initialize all upgrades
		boardCreator.InitializeUpgrades();
			//Get the generated tiles in the board
		var board = boardCreator.getBoard();

			//Spawn all objects in the board
		foreach (var tile in board.tiles) {
            if (tile.isIndestructible)
            {
                NetworkServer.Spawn(Instantiate(indestructible, new Vector3(tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);
                continue;
            }

            NetworkServer.Spawn(Instantiate(floor, new Vector3(tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);

            if (tile.isDestructible)
				NetworkServer.Spawn (Instantiate (destructible, new Vector3 (tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);

			if (tile.isUpgrade)
				switch (tile.upgradeType) {
					case(UpgradeType.Bomb):
						NetworkServer.Spawn (Instantiate (bombUpgrade, new Vector3 (tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);
						break;
					case(UpgradeType.Kick):
						NetworkServer.Spawn (Instantiate (kickUpgrade, new Vector3 (tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);
						break;
					case(UpgradeType.Laser):
						NetworkServer.Spawn (Instantiate (laserUpgrade, new Vector3 (tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);
						break;
					case(UpgradeType.Line):
						NetworkServer.Spawn (Instantiate (lineUpgrade, new Vector3 (tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);
						break;
					case (UpgradeType.Radioactive):
						NetworkServer.Spawn (Instantiate (radioactiveUpgrade, new Vector3 (tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);
						break;
					default: // Do nothing
						break;
				}
		}
	}

	private int getSlotIndex(int connectionId){
		int i = 0;
		foreach(var playerId in connectedPlayerIds){
			if (playerId == connectionId)
				return i;
			i++;
		}

		Debug.LogError ("No matching playerControllerId in slots");
		throw new ArgumentOutOfRangeException ();
	}
}
