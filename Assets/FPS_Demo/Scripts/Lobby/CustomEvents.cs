using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class CustomEvents
{
    /// <summary>
    /// When joining a lobby;
    /// 
    /// T0 is lobby's id;
    /// T1 is lobby's name;
    /// T2 is if player is host
    /// </summary>
    public static UnityEvent<string, string, bool> e_OnJoiningLobby = new();

    /// <summary>
    /// When changing the displayed view;
    /// 
    /// T0 is View's GameObject;
    /// T1 is if view is active;
    /// </summary>
    public static UnityEvent<GameObject, bool> e_OnViewChanged = new();

    /// <summary>
    /// When changing the display state of the Loading view;
    /// 
    /// T0 is if Loading view is to be activated or deactivated;
    /// T1 is if pause the timeScale;
    /// </summary>
    public static UnityEvent<bool, bool> e_onCallLoading = new();

    /// <summary>
    /// When displaying a prompt;
    /// 
    /// T0 is message to be displayed in prompt;
    /// T1 is what to do on Confirm;
    /// </summary>
    public static UnityEvent<string, UnityAction> e_onShowPrompt = new();

    /// <summary>
    /// When hiding a prompt;
    /// </summary>
    public static UnityEvent e_onHidePrompt = new();

    /// <summary>
    /// When trying to exit a lobby;
    /// 
    /// T0 is what to do if truly leaving;
    /// </summary>
    public static UnityEvent<UnityAction> e_onExitingLobby = new();

    /// <summary>
    /// When exiting a lobby;
    /// 
    /// T0 is which tab to activate;
    /// </summary>
    public static UnityEvent<Toggle> e_onExitLobby = new();

    /// <summary>
    /// When disconnecting to quit the game;
    /// </summary>
    public static UnityEvent e_onQuitGame = new();

    /// <summary>
    /// When displaying an error message;
    /// 
    /// T0 is the message;
    /// </summary>
    public static UnityEvent<string> e_OnDisplayError = new();

    /// <summary>
    /// When the player gets disconnected
    /// </summary>
    public static UnityEvent e_OnDisconnect = new();
}
