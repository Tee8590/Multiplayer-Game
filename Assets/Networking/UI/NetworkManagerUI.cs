using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    private void Awake()
    {
        serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });

        //hostBtn.onClick.AddListener(() =>
        //{
        //    NetworkManager.Singleton.StartHost();
        //});

        //clientBtn.onClick.AddListener(() =>
        //{
        //    NetworkManager.Singleton.StartClient();
        //});

        hostBtn.onClick.AddListener( async () =>
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
            }
        });

        clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }
}
