using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static CustomEvents;
using static ServiceRequests;
using System.Threading.Tasks;
using System.Net.Http;
using System;

public class CreateHUD : MonoBehaviour
{
    private readonly ServiceRequests _lobby = new();

    private int minCapacityValue = 2;

    public Button btnCreate;
    public Button btnPlus;
    public Button btnMinus;
    public Toggle lobbyTab;
    public TMP_InputField lobbyNameField;
    public TMP_InputField capacityField;
    public TMP_InputField playerNameField;

    // Start is called before the first frame update
    void Start()
    {
        e_OnViewChanged.AddListener((GameObject view, bool is_active) => {
            if (view.name == "CreateView" && !is_active)
            {
                CleanupView();
            }
        });

        btnCreate.onClick.AddListener(() => CreateLobby(lobbyNameField.text, capacityField.text));
        btnPlus.onClick.AddListener(ButtonIncreaseCapacty);
        btnMinus.onClick.AddListener(ButtonDecreaseCapacty);
        capacityField.onValueChanged.AddListener(MinValueCheck);
        lobbyNameField.onValueChanged.AddListener(CheckNameEmpty);
        playerNameField.onValueChanged.AddListener(CheckNameEmpty);
    }

    /// <summary>
    /// Checks if the LobbyName field is empty
    /// </summary>
    /// <param name="value">The contents of the InputField</param>
    public void CheckNameEmpty(string value)
    {
        if (gameObject.activeSelf)
        {
            if (value.Length >= 1)
            {
                btnCreate.interactable = true;
            }
            else
            {
                btnCreate.interactable = false;
            }
        }       
    }

    /// <summary>
    /// When clicking on the button that increases the lobby capacity
    /// </summary>
    public void ButtonIncreaseCapacty()
    {
        bool parsed = int.TryParse(capacityField.text, out int value);

        if (parsed)
        {
            ++value;
            capacityField.text = value.ToString();

            if (value > minCapacityValue)
            {
                btnMinus.interactable = true;
            }
        }
    }

    /// <summary>
    /// When clicking on the button that decreases the lobby capacity
    /// </summary>
    public void ButtonDecreaseCapacty()
    {
        bool parsed = int.TryParse(capacityField.text, out int value);

        if (parsed)
        {
            --value;
            capacityField.text = value.ToString();

            if (value <= minCapacityValue)
            {
                btnMinus.interactable = false;
            }
        }
    }

    /// <summary>
    /// Checks if the capacity is above the minimum allowed
    /// </summary>
    /// <param name="value">The current capacity entered</param>
    public void MinValueCheck(string value)
    {
        bool parsed = int.TryParse(value, out int valueAsInt);

        if (parsed)
        {
            int checkedValue = Mathf.Max(minCapacityValue, valueAsInt);
            capacityField.text = checkedValue.ToString();

            if (checkedValue <= minCapacityValue)
            {
                btnMinus.interactable = false;
            }
        }
    }

    /// <summary>
    /// Creates a new lobby
    /// </summary>
    /// <param name="name">The lobby's name</param>
    /// <param name="capacity">The lobby's player capacity</param>
    public async void CreateLobby(string name, string capacity)
    {
        try
        {
            bool parsed = int.TryParse(capacity, out int value);
            e_onCallLoading.Invoke(true, true);

            Player playerSelf = new()
            {
                id = playerNameField.text,
                is_host = true,
                authorization_token = null
            };

            Debug.Log("Creating lobby...");
            Lobby newLobby = await _lobby.RequestPost(value, name, playerSelf);

            lobbyTab.isOn = true;

            await Task.Delay(100);

            e_OnJoiningLobby.Invoke(newLobby.lobby_id, newLobby.name, true);
        }
        catch(HttpRequestException httpEx)
        {
            e_OnDisplayError.Invoke($"Request failed;\n{httpEx.Message}");
        }
        catch(Exception ex)
        {
            e_OnDisplayError.Invoke($"An error occcured;\n{ex.Message}");
        }
    }

    /// <summary>
    /// Resets the InputFields
    /// </summary>
    public void CleanupView()
    {
        lobbyNameField.text = "";
        capacityField.text = minCapacityValue.ToString();
        btnMinus.interactable = false;
    }
}
