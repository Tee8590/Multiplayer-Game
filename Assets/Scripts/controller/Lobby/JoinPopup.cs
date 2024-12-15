using UnityEngine;
using TMPro;

public class JoinLobby : MonoBehaviour
{
    public GameObject hostPanel; // Panel with host/join buttons
    public GameObject scrollViewPanel; // Panel with the player list
    public GameObject joinPopup; // Popup for entering the code
    public TMP_InputField codeInputField; // Input field for the code
    public string lobbyCode; // Code from the host (shared manually)

    public void OpenJoinPopup()
    {
        joinPopup.SetActive(true);
    }

    public void SubmitCode()
    {
        if (codeInputField.text == lobbyCode)
        {
            // Code matches, show the player list
            joinPopup.SetActive(false);
            hostPanel.SetActive(false);
            scrollViewPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Invalid code. Try again!");
        }
    }
}
