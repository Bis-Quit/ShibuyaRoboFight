using UnityEngine;
using System.Collections;
using TMPro;

public class AuthUIManager : MonoBehaviour
{
    [Header("MAIN PANELS")]
    public GameObject settingsPanel;
    public GameObject registerPanel;
    public GameObject successPanel;
    public GameObject loginPanel;

    [Header("SETTINGS: GROUPS & TEXTS")]
    public GameObject accountGuestUI;
    public GameObject accountLoggedInUI;
    public TextMeshProUGUI profileNameText;
    public TextMeshProUGUI profileEmailText;

    [Header("REGISTER POPUP")]
    public TMP_InputField regUsernameInput;
    public TMP_InputField regEmailInput;
    public TMP_InputField regPasswordInput;
    public TMP_InputField regConfirmPasswordInput;
    
    [Header("SUCCESS POPUP")]
    public TextMeshProUGUI successNameText;

    [Header("LOGIN POPUP")]
    public TMP_InputField loginIdInput;
    public TMP_InputField loginPasswordInput;
    public TextMeshProUGUI loginErrorText;

    private Coroutine hideErrorCoroutine;

    private void Start()
    {
        if (loginErrorText != null) loginErrorText.gameObject.SetActive(false);
        RefreshSettingsUI();
    }

    public void RefreshSettingsUI()
    {
        int isLoggedIn = PlayerPrefs.GetInt("IsLoggedIn", 0);

        if (isLoggedIn == 1 && SaveSystem.HasSaveFile())
        {
            accountGuestUI.SetActive(false);
            accountLoggedInUI.SetActive(true);

            PlayerData data = SaveSystem.LoadProfile();
            profileNameText.text = data.playerName.ToUpper();
            profileEmailText.text = data.email;
        }
        else
        {
            accountGuestUI.SetActive(true);
            accountLoggedInUI.SetActive(false);
        }
    }

    public void OpenRegisterPopup() { registerPanel.SetActive(true); }
    public void CloseRegisterPopup() { registerPanel.SetActive(false); }

    public void OpenLoginPopup() 
    { 
        loginErrorText.gameObject.SetActive(false);
        loginPanel.SetActive(true); 
    }
    public void CloseLoginPopup() { loginPanel.SetActive(false); }

    public void CloseSettingsPanel() { settingsPanel.SetActive(false); }

    public void SubmitRegister()
    {
        string user = regUsernameInput.text;
        string email = regEmailInput.text;
        string pass = regPasswordInput.text;
        string confPass = regConfirmPasswordInput.text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass)) return;
        if (pass != confPass) return; 

        PlayerData newData = new PlayerData();
        newData.playerName = user;
        newData.email = email;
        newData.password = pass;
        SaveSystem.SaveProfile(newData);

        PlayerPrefs.SetInt("IsLoggedIn", 1);
        PlayerPrefs.Save();

        registerPanel.SetActive(false);
        successPanel.SetActive(true);
        successNameText.text = user.ToUpper();
    }

    public void ContinueFromSuccess()
    {
        successPanel.SetActive(false);
        RefreshSettingsUI();
    }

    public void SubmitLogin()
    {
        if (!SaveSystem.HasSaveFile())
        {
            ShowError("NO ACCOUNT FOUND! PLEASE REGISTER.");
            return;
        }

        PlayerData data = SaveSystem.LoadProfile();
        string inputId = loginIdInput.text;
        string inputPass = loginPasswordInput.text;

        if ((inputId == data.email || inputId == data.playerName) && inputPass == data.password)
        {
            PlayerPrefs.SetInt("IsLoggedIn", 1);
            PlayerPrefs.Save();
            
            loginPanel.SetActive(false);
            RefreshSettingsUI();
        }
        else
        {
            ShowError("INCORRECT EMAIL/USERNAME OR PASSWORD. PLEASE TRY AGAIN.");
        }
    }

    private void ShowError(string errorMessage)
    {
        loginErrorText.text = errorMessage;
        loginErrorText.gameObject.SetActive(true);

        if (hideErrorCoroutine != null)
        {
            StopCoroutine(hideErrorCoroutine);
        }
        hideErrorCoroutine = StartCoroutine(HideErrorRoutine());
    }

    private IEnumerator HideErrorRoutine()
    {
        yield return new WaitForSeconds(3f);

        if (loginErrorText != null)
        {
            loginErrorText.gameObject.SetActive(false);
        }
    }

    public void LogoutAccount()
    {
        PlayerPrefs.SetInt("IsLoggedIn", 0);
        PlayerPrefs.Save();
        RefreshSettingsUI();
    }

    public void DeleteAccount()
    {
        SaveSystem.DeleteProfile();
        PlayerPrefs.SetInt("IsLoggedIn", 0);
        PlayerPrefs.Save();
        RefreshSettingsUI();
    }
}