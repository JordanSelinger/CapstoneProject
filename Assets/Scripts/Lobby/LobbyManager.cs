﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkLobbyManager
{
    public class PlayerInfo
    {
        public string name;
        public int slot;
        public bool isAlive;
        public int score;
    }

    public static LobbyManager instance;

    public RectTransform scoreScreenGui;
    public RectTransform lobbyGui;
    public RectTransform menuGui;
    public RectTransform countdownGui;
    public Text countdownText;
    public float countdownTime = 5.0f;
    public float scoreScreenTime = 5.0f;

    private static List<PlayerInfo> _connectedPlayerInfo = new List<PlayerInfo>();         //TODO: limit number of players
    private RectTransform _currentPanel;
    private Vector3[] _playerSpawnVectors = new Vector3[4]
    {
        new Vector3(1.0f, 11.0f, 0.0f),
        new Vector3(13.0f, 1.0f, 0.0f),
        new Vector3(13.0f, 11.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f)
    };

    private bool _sceneLoaded = false;

    public GameObject bombUpgrade;
    public GameObject laserUpgrade;
    public GameObject kickUpgrade;
    public GameObject lineUpgrade;
    public GameObject radioactiveUpgrade;

    public GameObject floor;
    public GameObject destructible;
    public GameObject indestructible;
    public GameObject[] playerAnimations;
    private BoardCreator _boardCreator;

    void Start()
    {
        instance = this;
        _currentPanel = menuGui;
    }


    // **************GAME************** 
    public void PlayerDead(PlayerControllerComponent player)
    {
        SpawnUpgradeInRandomLocation(UpgradeType.Bomb, player.maxNumBombs - 1);
        SpawnUpgradeInRandomLocation(UpgradeType.Laser, player.bombParams.radius - 2);
        SpawnUpgradeInRandomLocation(UpgradeType.Kick, player.bombKick);
        SpawnUpgradeInRandomLocation(UpgradeType.Line, player.bombLine);

        _connectedPlayerInfo[player.slot].isAlive = false;

        Invoke("CheckIfGameOver", 2);

        NetworkServer.Destroy(player.gameObject);
    }

    private void CheckIfGameOver()
    {
        if (_connectedPlayerInfo.Where(x => x.isAlive).Count() == 1)
        {
            _connectedPlayerInfo.Where(x => x.isAlive).First().score++;
            _connectedPlayerInfo.Where(x => x.isAlive).First().isAlive = false;
        }

        if (_connectedPlayerInfo.Where(x => x.isAlive).Count() == 0)
        {
            StartCoroutine(GameOver());
        }
    }

    private IEnumerator GameOver()
    {
        float remainingTime = scoreScreenTime;

        foreach (LobbyPlayer player in lobbySlots)
        {
            if (player == null)
                continue;
            var info = _connectedPlayerInfo.Where(x => x != null && x.slot == player.slot).FirstOrDefault();
			if(info != null)
            	player.RpcAddPlayerToScoreList(info.name, info.score);
        }

        while (remainingTime >= -1)
        {
            yield return null;
            remainingTime -= Time.deltaTime;
        }

        foreach (LobbyPlayer player in lobbySlots)
        {
            if (player == null)
                continue;

            player.RpcClearScoreList();
        }

		_sceneLoaded = false;
        LobbyManager.instance.SendReturnToLobby();
    }

    private void SpawnBoard()
    {
        _boardCreator = new BoardCreator();
        _boardCreator.InitializeDestructible();

        //Initialize spawn for all connected players
        lobbySlots.Where(p => p != null).ToList()
            .ForEach(p => _boardCreator.InitializeSpawn(_playerSpawnVectors[(int)p.slot]));

        //Initialize all upgrades
        _boardCreator.InitializeUpgrades();
        //Get the generated tiles in the board
        var board = _boardCreator.GetBoard();

        //Spawn all objects in the board
        foreach (var tile in board.tiles)
        {
            if (tile.isIndestructible)
            {
                NetworkServer.Spawn(Instantiate(indestructible, new Vector3(tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);
                continue;
            }

            NetworkServer.Spawn(Instantiate(floor, new Vector3(tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);

            if (tile.isDestructible)
                NetworkServer.Spawn(Instantiate(destructible, new Vector3(tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);

            if (tile.isUpgrade)
                switch (tile.upgradeType)
                {
                    case (UpgradeType.Bomb):
                        NetworkServer.Spawn(Instantiate(bombUpgrade, new Vector3(tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);
                        break;
                    case (UpgradeType.Kick):
                        NetworkServer.Spawn(Instantiate(kickUpgrade, new Vector3(tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);
                        break;
                    case (UpgradeType.Laser):
                        NetworkServer.Spawn(Instantiate(laserUpgrade, new Vector3(tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);
                        break;
                    case (UpgradeType.Line):
                        NetworkServer.Spawn(Instantiate(lineUpgrade, new Vector3(tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);
                        break;
                    case (UpgradeType.Radioactive):
                        NetworkServer.Spawn(Instantiate(radioactiveUpgrade, new Vector3(tile.x, tile.y, 0.0f), Quaternion.identity) as GameObject);
                        break;
                    default: // Do nothing
                        break;
                }
        }
    }

    private void SpawnUpgradeInRandomLocation(UpgradeType upgradeType, int num = 0)
    {
        for (int i = 0; i < num; i++)
        {
            Vector2 location = new Vector2();

            do
            {
                location.x = UnityEngine.Random.Range(1, 13);   // Spawnable locations on the board
                location.y = UnityEngine.Random.Range(1, 11);
            } while (Physics2D.RaycastAll(location, new Vector2(1.0f, 1.0f), 0.2f).Length != 0);

            switch (upgradeType)
            {
                case (UpgradeType.Bomb):
                    NetworkServer.Spawn(Instantiate(bombUpgrade, location, Quaternion.identity) as GameObject);
                    break;
                case (UpgradeType.Kick):
                    NetworkServer.Spawn(Instantiate(kickUpgrade, location, Quaternion.identity) as GameObject);
                    break;
                case (UpgradeType.Laser):
                    NetworkServer.Spawn(Instantiate(laserUpgrade, location, Quaternion.identity) as GameObject);
                    break;
                case (UpgradeType.Line):
                    NetworkServer.Spawn(Instantiate(lineUpgrade, location, Quaternion.identity) as GameObject);
                    break;
                default: // Do nothing
                    break;
            }
        }
    }

    // **************SERVER**************
    public override void OnLobbyStartServer()
    {
        base.OnLobbyStartServer();
        _connectedPlayerInfo.Clear();
    }

    public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection networkConnection, short playerControllerId)
    {
        // Figure out what slot the player is in based on the network connection and playerControllerId
        var i = lobbySlots.Where(x => x != null && x.connectionToClient.connectionId == networkConnection.connectionId && x.playerControllerId == playerControllerId).First().slot;
        
		GameObject newPlayer = (GameObject)Instantiate(playerPrefab, Vector2.zero, Quaternion.identity);
        newPlayer.transform.position = _playerSpawnVectors[i];
        newPlayer.GetComponent<PlayerControllerComponent>().slot = (int)i;

		NetworkServer.Spawn(newPlayer);
        return newPlayer;
    }

    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject gameObject1, GameObject gameObject2)
    {
        if (!_sceneLoaded)
            SpawnBoard();
        _sceneLoaded = true;
        return true;
    }

    public override void OnLobbyServerPlayersReady()
    {
        foreach (LobbyPlayer player in lobbySlots)
        {
            if (player != null)
            {
                if (!player.readyToBegin)
                {
                    return;
                }
            }
        }

        StartCoroutine(CountDownCoroutine());
    }

    public IEnumerator CountDownCoroutine()
    {
        float remainingTime = countdownTime;
        int floorTime = Mathf.FloorToInt(remainingTime);

        while (remainingTime >= -1)
        {
            yield return null;

            remainingTime -= Time.deltaTime;
            int newFloorTime = Mathf.FloorToInt(remainingTime);

            if (newFloorTime != floorTime)
            {
                floorTime = newFloorTime;

                foreach (LobbyPlayer player in lobbySlots)
                {
                    if (player != null)
                    {
                        player.RpcUpdateCountdown(floorTime);
                    }
                }
            }
        }

        ServerChangeScene(playScene);
    }

    public void KickPlayer(NetworkConnection conn)
    {
        conn.Disconnect();
    }

    // **************CLIENT**************
	public override void OnLobbyClientExit(){
		ChangePanel(menuGui);
	}

    public override void OnLobbyClientEnter()
    {
        ChangePanel(lobbyGui);
    }
		
    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        base.OnClientError(conn, errorCode);
        Debug.LogError("client error");
    }

    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        base.OnLobbyClientSceneChanged(conn);
		if (SceneManager.GetActiveScene ().name == "Game")
			_currentPanel.gameObject.SetActive (false);
    }

    // **************GUI**************

    public void ChangePanel(RectTransform newPanel)
    {
        if (_currentPanel != null)
        {
            _currentPanel.gameObject.SetActive(false);
        }

        if (newPanel != null)
        {
            newPanel.gameObject.SetActive(true);
        }

        _currentPanel = newPanel;
    }

    // **************PLAYER LIST**************
    public void AddPlayer(LobbyPlayer player)
    {
        // This is called whenever a player enters the lobby, including coming back from the previous game
        // 		Do not add players when they have already connected previouslys
        if (_connectedPlayerInfo.Where(x => x != null && x.slot == player.slot).Count() == 0)
            _connectedPlayerInfo.Add(new PlayerInfo() { slot = player.slot, isAlive = true, score = 0, name = "Anonymous" });   // TODO need to update to allow for login names to work
    }

    public void RemovePlayer(LobbyPlayer player)
    {
        _connectedPlayerInfo.Remove(_connectedPlayerInfo.Where(x => x.slot == player.slot).FirstOrDefault());
    }
}
