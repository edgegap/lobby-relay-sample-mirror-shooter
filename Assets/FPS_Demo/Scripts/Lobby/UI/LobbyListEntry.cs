using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static CustomEvents;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using static ServiceRequests;

public class LobbyListEntry : MonoBehaviour
{
    private readonly ServiceRequests _lobby = new();

    /// <summary>
    /// When clicking on the lobby
    /// </summary>
    public async void LobbyEntryClick()
    {
        try
        {
            GameObject lobbyName = transform.Find("Name").gameObject;
            GameObject lobbyTab = GameObject.Find("LobbyTab");
            TMP_InputField playerNameField = GameObject.Find("PlayerNameField").GetComponent<TMP_InputField>();

            Player playerSelf = new()
            {
                id = playerNameField.text,
                is_host = false,
                authorization_token = null
            };

            Debug.Log("Joining lobby...");
            await _lobby.RequestPost("join", name, playerSelf);

            e_onCallLoading.Invoke(true, true);
            lobbyTab.GetComponent<Toggle>().isOn = true;

            await Task.Delay(100);

            e_OnJoiningLobby.Invoke(name, lobbyName.GetComponent<TextMeshProUGUI>().text, false);
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.Message.Contains("NotFound"))
            {
                e_OnDisplayError.Invoke("An error occured;\nThis lobby cannot be found.");
                Destroy(gameObject);
            }
            else
            {
                e_OnDisplayError.Invoke($"Unable to join this lobby;\n{httpEx}");
            }
        }
        catch (Exception ex)
        {
            e_OnDisplayError.Invoke($"An error occcured;\n{ex.Message}");
        }
    }
}
