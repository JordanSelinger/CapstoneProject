using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking.Match;
using UnityEngine.Networking;

public class MatchmakingLobbyMain : MonoBehaviour
{
    public InputField roomName;
    public Button createButton;
    public Button findButton;
    public Button backButton;
    public RectTransform listPanel;
    public RectTransform matchListWarning;
    public GameObject serverInfoPrefab;

    public void OnEnable()
    {
        listPanel.gameObject.SetActive(false);
        matchListWarning.gameObject.SetActive(false);

        createButton.onClick.RemoveAllListeners();
        createButton.onClick.AddListener(OnClickCreate);

        findButton.onClick.RemoveAllListeners();
        findButton.onClick.AddListener(OnClickFind);

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(OnClickBack);
    }

    public void OnClickCreate()
    {
        var maxPlayers = (uint)LobbyManager.instance.maxPlayers;
        LobbyManager.instance.DisplayInfoNotification("Creating...");

        LobbyManager.instance.isMatchMaking = true;
        LobbyManager.instance.StartMatchMaker();
        NetworkManager.singleton.maxDelay = 0.2f;
		LobbyManager.instance.matchMaker.SetProgramAppID((UnityEngine.Networking.Types.AppID)808401);
        LobbyManager.instance.matchMaker.CreateMatch(roomName.text, maxPlayers, true, "", LobbyManager.instance.OnMatchCreate);
    }

    public void OnClickFind()
    {
        LobbyManager.instance.DisplayInfoNotification("Finding matches...");
        LobbyManager.instance.StartMatchMaker();
		LobbyManager.instance.matchMaker.SetProgramAppID((UnityEngine.Networking.Types.AppID)808401);
        LobbyManager.instance.matchMaker.ListMatches(0, 99, "", OnMatchList);
    }

    private void OnMatchList(ListMatchResponse response)
    {
        LobbyManager.instance.HideInfoPanel();
        ClearMatchList();

        if (response.matches.Count == 0)
        {
            matchListWarning.gameObject.SetActive(true);
            listPanel.gameObject.SetActive(false);
        } 
        else
        {
            matchListWarning.gameObject.SetActive(false);
            listPanel.gameObject.SetActive(true);
        }

        foreach (MatchDesc match in response.matches)
        {
            GameObject go = Instantiate(serverInfoPrefab) as GameObject;
            //go.GetComponent<LobbyServerInfo>().PopulateMatchInfo(match);
            go.transform.SetParent(listPanel, false);
        }
    }

    private void ClearMatchList()
    {
        foreach (Transform child in listPanel)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnClickBack()
    {
        Destroy(LobbyManager.instance.gameObject);
        SceneManager.LoadScene("MainMenu");
    }
}
