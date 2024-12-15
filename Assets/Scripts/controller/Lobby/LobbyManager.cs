using UnityEngine;
using UnityEngine.UI; // For Button
using TMPro; // For TextMeshProUGUI
using UnityEngine.SceneManagement; // For SceneManager
using System.Collections.Generic; // For List

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI joinCodeText;
    [SerializeField] private GameObject hostPanel; // Panel with host/join buttons
    [SerializeField] private GameObject scrollViewPanel; // Panel with the player list
    [SerializeField] private Button startButton; // Start button for the host
    [SerializeField] private GameObject joinPopup; // Popup for entering the code
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button joinSubmitButton;
    [SerializeField] private Button closePopUpButton;
    [SerializeField] private TMP_InputField codeInputField; // Input field for joining
    [SerializeField] private Transform playerListContainer; // Container for player names
    [SerializeField] private TextMeshProUGUI playerNamePrefab; // Prefab for player names

    private string lobbyCode;
    private List<string> players = new ();

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
            string relayCode = await RelayController.Instance.CreateRelay();
            Debug.Log(relayCode);

            lobbyCode = relayCode;
            joinCodeText.text = "Code: " + lobbyCode;

            AddPlayerToList("Host");

            hostPanel.SetActive(false);
            scrollViewPanel.SetActive(true);
            startButton.interactable = true;
        }
    }

    public void OpenJoinPopup()
    {
        joinPopup.SetActive(true);
    }

    private void AddPlayerToList(string playerName)
    {
        players.Add(playerName);

        TextMeshProUGUI playerNameText = Instantiate(playerNamePrefab, playerListContainer);
        playerNameText.text = playerName;
    }

    private void StartGameScene()
    {
        SceneManager.LoadScene("MultiPlayer");
    }

    private async void JoinGame()
    {
        string enteredCode = codeInputField.text;

        if (string.IsNullOrEmpty(enteredCode))
        {
            ShowErrorMessage("Code cannot be empty!");
            return;
        }

        await RelayController.Instance.JoinRelay(enteredCode);

        codeInputField.text = "";
        joinPopup.SetActive(false);

        AddPlayerToList("Player " + (players.Count + 1));
    }

    private void ShowErrorMessage(string message)
    {
        Debug.Log(message);
    }

    private void ClosePopUp()
    {
        joinPopup.SetActive(false);
    }
}
