using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static ServiceRequests;
using static CustomEvents;
using System.Threading.Tasks;
using System.Net.Http;
using System;

public class ListHUD : MonoBehaviour
{
    private readonly ServiceRequests _lobby = new();

    public Button btnRefresh;
    public GameObject noEntries;
    public GameObject lobbyEntryBase;
    public GameObject listDisplay;
    public GameObject playerNameValidScreen;
    public TMP_InputField playerNameField;

    // Start is called before the first frame update
    void Start()
    {
        e_OnViewChanged.AddListener((GameObject view, bool is_active) => {
            if (view.name == "ListView")
            {
                if (is_active)
                {
                    EmptyListDisplay();
                    RefreshLobbies();
                }
            }
        });

        btnRefresh.onClick.AddListener(RefreshLobbies);
    }

    private void Update()
    {
        if (listDisplay.transform.childCount == 1)
        {
            noEntries.SetActive(true);
        }
        else if (playerNameField.text.Length == 0)
        {
            playerNameValidScreen.SetActive(true);
        }
        else
        {
            playerNameValidScreen.SetActive(false);
        }
    }

    /// <summary>
    /// Fetches a list of all lobbies and displays it
    /// </summary>
    public async void RefreshLobbies()
    {
        try
        {
            e_OnDisplayError.Invoke("");
            btnRefresh.interactable = false;
            e_onCallLoading.Invoke(true, true);

            Debug.Log("Fetching lobbies...");
            Lobbies lobbies = await _lobby.RequestGet();

            EmptyListDisplay();

            if (lobbies.count > 0)
            {
                noEntries.SetActive(false);

                foreach (Lobby lobby in lobbies.data)
                {
                    GameObject entry = Instantiate(lobbyEntryBase, listDisplay.transform);
                    entry.name = $"{lobby.lobby_id}";
                    entry.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = lobby.name;
                    entry.transform.Find("Capacity").GetComponent<TextMeshProUGUI>().text = $"{lobby.player_count} / {lobby.capacity}";

                    if (lobby.player_count == lobby.capacity || !lobby.is_joinable)
                    {
                        entry.GetComponent<Button>().interactable = false;
                    }
                }
            }
            else
            {
                noEntries.SetActive(true);
            }
        }
        catch (HttpRequestException httpEx)
        {
            e_OnDisplayError.Invoke($"Request failed;\n{httpEx.Message}");
            EmptyListDisplay();
        }
        catch (Exception ex)
        {
            e_OnDisplayError.Invoke($"An error occcured;\n{ex.Message}");
            EmptyListDisplay();
        }
        finally
        {
            btnRefresh.interactable = true;
            e_onCallLoading.Invoke(false, true);
        }
    }

    /// <summary>
    /// Empties the displayed list of lobbies
    /// </summary>
    private void EmptyListDisplay()
    {
        foreach (Transform child in listDisplay.transform)
        {
            if (child.name != "NoEntries")
            {
                Destroy(child.gameObject);
            }
        }

        noEntries.SetActive(true);
    }
}
