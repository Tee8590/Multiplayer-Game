using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayController : MonoBehaviour
{
    public static RelayController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    //private async void Start()
    //{
    //    await UnityServices.InitializeAsync();

    //    AuthenticationService.Instance.SignedIn += () =>
    //    {
    //        Debug.Log("PlayerID" + AuthenticationService.Instance.PlayerId);
    //    };

    //    await AuthenticationService.Instance.SignInAnonymouslyAsync();
    //}

    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            //NetworkManager.Singleton.StartHost();

            Debug.Log("Relay IP: " + allocation.RelayServer.IpV4 + " Port: " + allocation.RelayServer.Port + " joinCode" + joinCode);

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log("RelayServiceException : " + e);
            return null;
        }
    }

    public async Task<bool> JoinRelay (string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log(" join Relay IP: " + joinAllocation.RelayServer.IpV4 + " join Port: " + joinAllocation.RelayServer.Port + "joinCode" + joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            //NetworkManager.Singleton.StartClient();
            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.Log("RelayServiceException : " + e);
            return false;
        }
    }
}
