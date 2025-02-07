using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    public static NetworkManagerUI Instance { get; private set; }

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

        hostBtn.onClick.AddListener( () =>
        {
            
        });

        clientBtn.onClick.AddListener(() =>
        {

        });
    }
}
