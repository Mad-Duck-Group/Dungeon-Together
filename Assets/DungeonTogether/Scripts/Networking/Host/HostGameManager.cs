using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager : IDisposable
{
    private Allocation allocation;
    //private NetworkObject playerPrefab;
    
    private string lobbyId;
    public string JoinCode { get; private set; }

    public NetworkServer NetworkServer { get; private set; }
    
    private const int MaxConnections = 4;
    private const string LoungeScene = "Lounge";

    public HostGameManager()
    {
        
    }
    
    public void Dispose()
    {
        Shutdown();
    }
    
    public async void Shutdown()
    {
        HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));

        if (!string.IsNullOrEmpty(lobbyId))
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

            lobbyId = string.Empty;
        }
        NetworkServer.OnClientLeft -= HandleClientLeft;
        
        NetworkServer?.Dispose();
    }
    
    public async Task StartHostAsync()
    {
        Debug.Log($"can load");
        
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch(Exception e)
        {
            Debug.Log(e);
            return;
        }
        try
        {
            JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(JoinCode);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
        transport.SetRelayServerData(relayServerData);

        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: JoinCode
                        )
                }
            };
            string playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Unknown");
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync($"{playerName}'s Lobby", MaxConnections, lobbyOptions);
            lobbyId = lobby.Id;

            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return;
        }

        NetworkServer = new NetworkServer(NetworkManager.Singleton);
        
        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId,
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        
        NetworkManager.Singleton.StartHost();
        
        NetworkServer.OnClientLeft += HandleClientLeft;
        
        Debug.Log(NetworkManager.Singleton == null ? "NetworkManager is null" : "NetworkManager is ready");
        
        NetworkManager.Singleton.SceneManager.LoadScene(LoungeScene, LoadSceneMode.Single);
    }

    private async void HandleClientLeft(string authID)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, authID);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
}