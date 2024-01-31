using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static CustomEvents;
using UnityEngine.Events;
using System.Net.Http;
using System;
using static ServiceRequests;
using System.Threading.Tasks;

public class LobbyHUD : MonoBehaviour
{
    private readonly ServiceRequests _lobby = new();
    private readonly float waitingTimeSec = 5;
    
    private Lobby activeLobby = null;
    private Player playerSelf = null;
    private bool isWaiting = true;
    private bool isReady = false;
    private bool firstLoad = true;

    public Toggle joinTab;
    public Button btnExitLobby;
    public Button btnStartLobby;
    public GameObject playersDisplay;
    public GameObject playerEntryBase;
    public TextMeshProUGUI lobbyIdDisplay;
    public TextMeshProUGUI lobbyNameDisplay;
    public TMP_InputField playerNameField;


    // Start is called before the first frame update
    void Start()
    {
        e_OnJoiningLobby.AddListener(OnJoiningLobby);
        e_onExitingLobby.AddListener(OnExitingLobby);
        e_onExitLobby.AddListener(OnExitLobby);
        e_onQuitGame.AddListener(() => OnExitLobby(joinTab));
        e_OnDisconnect.AddListener(() => Quit(joinTab));

        btnExitLobby.onClick.AddListener(() => OnExitingLobby(() => OnExitLobby(joinTab)));
        btnStartLobby.onClick.AddListener(StartLobby);
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            if (activeLobby is not null && activeLobby.is_started)
            {
                e_onCallLoading.Invoke(true, false);

                if (playerSelf?.authorization_token is not null && !isReady)
                {
                    isReady = true;
                    LaunchGame();
                }
            }

            if (!isWaiting && !isReady) 
            {
                RefreshLobby(firstLoad);
                StartCoroutine(Waiting());
            }
        }
    }

    /// <summary>
    /// Waits a certain amount of time before returning to wherever the code was being executed
    /// </summary>
    /// <returns>completed task</returns>
    IEnumerator Waiting()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitingTimeSec);
        isWaiting = false;
    }

    /// <summary>
    /// Fetches the lobby's information and displays it
    /// </summary>
    public async void RefreshLobby(bool displayLoading)
    {     
        try
        {
            if (displayLoading)
            {
                e_onCallLoading.Invoke(true, true);
            }

            Debug.Log("Fetching lobby info...");
            activeLobby = await _lobby.RequestGet(lobbyIdDisplay.text);

            EmptyPlayerDisplay();

            foreach (Player player in activeLobby.players)
            {
                GameObject entry = Instantiate(playerEntryBase, playersDisplay.transform);
                entry.name = $"{player.id}";
                entry.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = player.id;

                if (!player.is_host)
                {
                    entry.transform.Find("IsHost").gameObject.SetActive(false);
                }
                else
                {
                    entry.transform.SetAsFirstSibling();
                }

                if (player.id == playerNameField.text)
                {
                    playerSelf = player;
                }
            }

            if (firstLoad)
            {
                firstLoad = false;
            }
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.Message.Contains("NotFound"))
            {
                Quit(joinTab);

                await Task.Delay(100);

                e_OnDisplayError.Invoke("This lobby no longer exists;\nYou have been disconnected.");
            }
            else
            {
                e_OnDisplayError.Invoke($"Request failed;\n{httpEx.Message}");
            }
        }
        catch (Exception ex)
        {
            e_OnDisplayError.Invoke($"An error occcured;\n{ex.Message}");
            OnExitLobby(joinTab);
        }
        finally
        {
            if (displayLoading)
            {
                e_onCallLoading.Invoke(false, true);
            }
        }
    }

    /// <summary>
    /// When the player (as host or not) joins a new lobby
    /// </summary>
    /// <param name="lobbyID">The lobby's id</param>
    /// <param name="lobbyName">The lobby's name</param>
    /// <param name="isHost">If the player joining is the host</param>
    public void OnJoiningLobby(string lobbyID, string lobbyName, bool isHost)
    {
        Debug.Log("onJoining");
        EmptyPlayerDisplay();
        lobbyIdDisplay.text = lobbyID;
        lobbyNameDisplay.text = lobbyName;

        Button btnStart = transform.Find("StartBtn").GetComponent<Button>();

        if (isHost)
        {
            btnStart.interactable = true;
            btnStart.transform.Find("Txt").GetComponent<TextMeshProUGUI>().text = "Start";
        }
        else
        {
            btnStart.interactable = false;
            btnStart.transform.Find("Txt").GetComponent<TextMeshProUGUI>().text = "Waiting for host...";
        }

        isWaiting = false;
    }

    /// <summary>
    /// When the player attempts to exit a lobby
    /// </summary>
    /// <param name="confirmAction">What to do on Confirm</param>
    public void OnExitingLobby(UnityAction confirmAction)
    {
        e_onShowPrompt.Invoke(
            $"Do you really want to exit this lobby?{(playerSelf.is_host ? "If you leave, this lobby will be deleted and everyone else will be disconnected." : "")}",
            confirmAction);
    }

    /// <summary>
    /// When the player exits the lobby
    /// </summary>
    /// <param name="tab">Which tab to activate</param>
    public async void OnExitLobby(Toggle tab)
    {
        try
        {
            if (gameObject.activeSelf)
            {
                if (playerSelf.is_host)
                {
                    Debug.Log("Deleting lobby...");
                    await _lobby.RequestDelete(activeLobby.lobby_id);
                }
                else
                {
                    Debug.Log("Quitting lobby...");
                    await _lobby.RequestPost("leave", activeLobby.lobby_id, playerSelf);
                }
            }           
        }
        catch (HttpRequestException httpEx)
        {
            e_OnDisplayError.Invoke($"Request failed;\n{httpEx.Message}");
        }
        catch (Exception ex)
        {
            e_OnDisplayError.Invoke($"An error occcured;\n{ex.Message}");
        }
        finally
        {
            CleanupView();
            Quit(tab);
        }
    }

    /// <summary>
    /// Exits the player from the lobby depending on if they are the host or not
    /// </summary>
    public async void Quit(Toggle tab)
    {
        e_onHidePrompt.Invoke();

        await Task.Delay(100);

        tab.isOn = true;
    }

    /// <summary>
    /// Starts the lobby
    /// </summary>
    public async void StartLobby()
    {
        try
        {
            if (playerSelf.is_host)
            {
                e_onCallLoading.Invoke(true, false);
                await _lobby.RequestPost(activeLobby.lobby_id);
            }
            else
            {
                e_OnDisplayError.Invoke($"You may not start a lobby you are not the host of.");
            }
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.Message.Contains("NotFound"))
            {
                Quit(joinTab);

                await Task.Delay(100);

                e_OnDisplayError.Invoke("This lobby no longer exists;\nYou have been disconnected.");
            }
            else
            {
                e_OnDisplayError.Invoke($"Request failed;\n{httpEx.Message}");
            }
        }
        catch (Exception ex)
        {
            e_OnDisplayError.Invoke($"An error occcured;\n{ex.Message}");
        }
    }

    /// <summary>
    /// Launches the game
    /// </summary>
    public void LaunchGame()
    {       
        _lobby.StartMatch(activeLobby, playerSelf);
        e_onCallLoading.Invoke(false, false);
    }

    /// <summary>
    /// Resets the LobbyView
    /// </summary>
    public void CleanupView()
    {
        Debug.Log("cleaning up");
        lobbyIdDisplay.text = "---";
        lobbyNameDisplay.text = "---";
        activeLobby = null;
        playerSelf = null;
        isWaiting = true;
        isReady = false;
        firstLoad = true;
        EmptyPlayerDisplay();
    }

    /// <summary>
    /// Empties the displayed list of players in the lobby
    /// </summary>
    public void EmptyPlayerDisplay()
    {
        foreach (Transform child in playersDisplay.transform)
        {
            Destroy(child.gameObject);
        }
    }

}
