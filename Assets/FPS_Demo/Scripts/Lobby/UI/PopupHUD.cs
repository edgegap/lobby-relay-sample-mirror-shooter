using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using static CustomEvents;

public class PopupHUD : MonoBehaviour
{
    public GameObject viewContainer;
    public GameObject viewPrompt;
    public GameObject viewLoading;

    // Start is called before the first frame update
    void Start()
    {
        e_onCallLoading.AddListener(DisplayLoading);
        e_onShowPrompt.AddListener(ShowPrompt);
        e_onHidePrompt.AddListener(HidePrompt);
    }

    private void DisplayView(bool isActive, string viewName, bool stopTimeScale)
    {
        viewContainer.SetActive(isActive);

        switch(viewName)
        {
            case "loading": viewLoading.SetActive(isActive); break;
            case "prompt": viewPrompt.SetActive(isActive); break;
        }

        if (stopTimeScale)
        {
            Time.timeScale = isActive ? 0 : 1;
        }
    }

    public void DisplayLoading(bool isActive, bool stopTimeScale)
    {
        DisplayView(isActive, "loading", stopTimeScale);
    }

    public void ShowPrompt(string message, UnityAction confirmAction)
    {
        DisplayView(true, "prompt", true);

        GameObject prompt = viewPrompt.transform.Find("Prompt").gameObject;
        Button btnConfirm = prompt.transform.Find("ConfirmBtn").GetComponent<Button>();
        Button btnCancel = prompt.transform.Find("CancelBtn").GetComponent<Button>();

        prompt.transform.Find("Message").GetComponent<TextMeshProUGUI>().text = message;

        btnConfirm.onClick.AddListener(confirmAction);
        btnCancel.onClick.AddListener(e_onHidePrompt.Invoke);
    }

    public void HidePrompt()
    {
        GameObject prompt = viewPrompt.transform.Find("Prompt").gameObject;
        Button btnConfirm = prompt.transform.Find("ConfirmBtn").GetComponent<Button>();
        Button btnCancel = prompt.transform.Find("CancelBtn").GetComponent<Button>();

        btnConfirm.onClick.RemoveAllListeners();
        btnCancel.onClick.RemoveAllListeners();

        DisplayView(false, "prompt", true);
    }
}
