using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using static CustomEvents;
using System.Threading.Tasks;

public class NavigationHUD : MonoBehaviour
{
    private GameObject activeView;

    public GameObject viewList;
    public GameObject viewLobby;
    public GameObject viewCreate;
    public TMP_InputField playerNameField;
    public TextMeshProUGUI errorField;
    public Button btnExit;
    public Button btnOverJoin;
    public Button btnOverCreate;
    public Toggle tabJoin;
    public Toggle tabCreate;
    public Toggle tabLobby;
    public Color cTab;
    public Color cSelectedTab;

    // Start is called before the first frame update
    void Start()
    {
        e_OnDisplayError.AddListener(DisplayError);

        btnExit.onClick.AddListener(() => e_onShowPrompt.Invoke("Do you really want to quit the game? You will be disconnected from any lobby you are in.", CloseApp));
        btnOverJoin.onClick.AddListener(() => OverlayButtonClick(tabJoin, "List"));
        btnOverCreate.onClick.AddListener(() => OverlayButtonClick(tabCreate, "Create"));

        tabJoin.onValueChanged.AddListener((isChecked) => ChangeTab(tabJoin.gameObject, viewList, isChecked));
        tabCreate.onValueChanged.AddListener((isChecked) => ChangeTab(tabCreate.gameObject, viewCreate, isChecked));
        tabLobby.onValueChanged.AddListener((isChecked) => ChangeTab(tabLobby.gameObject, viewLobby, isChecked));

        ChangeTab(tabJoin.gameObject, viewList, true);
    }

    /// <summary>
    /// When clicking on a tab to change the view
    /// </summary>
    /// <param name="linkedToggle">The toggle that changes the view</param>
    /// <param name="viewName">The view's name</param>
    public void OverlayButtonClick(Toggle linkedToggle, string viewName)
    {
        if (!activeView.name.Contains(viewName)) 
        {
            if (activeView.name.Contains("Lobby"))
            {
                e_onExitingLobby.Invoke(() => {
                    e_onExitLobby.Invoke(linkedToggle);
                });
            }
            else
            {
                linkedToggle.isOn = true;
            }
        }
    }

    /// <summary>
    /// Changes a tab's state
    /// </summary>
    /// <param name="tab">Tab whose state is being changed</param>
    /// <param name="view">The view associated with the tab</param>
    /// <param name="isActive">If the tab is active</param>
    public void ChangeTab(GameObject tab, GameObject view, bool isActive)
    {
        DisplayError("");

        if (isActive)
        {
            activeView = view;
        }

        e_OnViewChanged.Invoke(view, isActive);
        view.SetActive(isActive);

        if (tab.name != "LobbyTab")
        {
            GameObject bg = tab.transform.Find("Background").gameObject;

            if (isActive)
            {
                bg.GetComponent<Image>().color = cSelectedTab;
            }
            else
            {
                bg.GetComponent<Image>().color = cTab;
            }

            playerNameField.interactable = true;
        }
        else
        {
            playerNameField.interactable = false;
        }
    }

    /// <summary>
    /// Displays and Logs a message in the Error Message space
    /// </summary>
    /// <param name="message">The message to display</param>
    public void DisplayError(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            Debug.Log(message);
        }
        
        errorField.text = message;
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    public void CloseApp()
    {
        e_onQuitGame.Invoke();

#if UNITY_STANDALONE
        Application.Quit();
#endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
