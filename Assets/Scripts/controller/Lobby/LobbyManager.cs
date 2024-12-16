using UnityEngine;
using UnityEngine.UI; // For Button
using TMPro; // For TextMeshProUGUI
using UnityEngine.SceneManagement; // For SceneManager
using System.Collections.Generic; // For List

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [SerializeField] private GameObject hostPanel; // Panel with host/join buttons
    [SerializeField] private GameObject scrollViewPanel; // Panel with the player list
    [SerializeField] private Button startButton; // Start button for the host
    [SerializeField] private GameObject joinPopup; // Popup for entering the code
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button joinSubmitButton;
    [SerializeField] private Button closePopUpButton;
    [SerializeField] private TMP_InputField codeInputField; // Input field for joining

    private string lobbyCode;

    private void Awake()
    {
        hostButton.onClick.AddListener(HostLobby);
        joinButton.onClick.AddListener(OpenJoinPopup);
        startButton.onClick.AddListener(StartGameScene);
        joinSubmitButton.onClick.AddListener(JoinGame);
        closePopUpButton.onClick.AddListener(ClosePopUp);
        joinPopup.SetActive(false);
    }

    private void Start()
    {
        startButton.interactable = false;
    }

    public async void HostLobby()
    {
        Debug.Log("Button clicked, checking RelayController.Instance.");
        if (RelayController.Instance == null)
        {
            Debug.LogError("RelayController instance is not initialized.");
        }
        else
        {

            bool isLobbyCreated = await LobbyController.Instance.CreateLobby();

            if (isLobbyCreated) {

                hostPanel.SetActive(false);
                scrollViewPanel.SetActive(true);
                startButton.interactable = true;
            }
        }
    }

    private async void StartGameScene()
    {
        bool isStarted = await LobbyController.Instance.StartGame();

        if (isStarted)
        {
            LobbyController.Instance.GoToGameScene(false);
        }
    }
    private async void JoinGame()
    {
        string enteredCode = codeInputField.text;

        if (string.IsNullOrEmpty(enteredCode))
        {
            ShowErrorMessage("Code cannot be empty!");
            return;
        }

        bool isJoinedLobbyByCode = await LobbyController.Instance.JoinLobbyByCode(enteredCode);
        if (isJoinedLobbyByCode) {
            codeInputField.text = "";
            joinPopup.SetActive(false);

        }
    }

    private void ShowErrorMessage(string message)
    {
        Debug.Log(message);
    }

    private void ClosePopUp()
    {
        joinPopup.SetActive(false);
    }
    public void OpenJoinPopup()
    {
        joinPopup.SetActive(true);
    }
}
