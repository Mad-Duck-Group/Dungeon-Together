using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DungeonTogether.Scripts.Utils;
using UnityEngine;
using Unity.Netcode;

public class NetworkServer : IDisposable
{
    private NetworkManager networkManager;
    //private NetworkObject playerPrefab;
    
    public Action<string> OnClientLeft;

    private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();

    public UserData GetUserDataByClientId(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            if (authIdToUserData.TryGetValue(authId, out UserData data))
            {
                return data;
            }
            return null;
        }

        return null;
    }
    
    public void Dispose()
    {
        if (networkManager == null) { return; }
        
        networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        networkManager.OnServerStarted -= OnNetworkReady;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;

        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }

    public NetworkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager;
        //this.playerPrefab = playerPrefab;
        
        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += OnNetworkReady;
    }
    
    private void OnNetworkReady()
    {
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    /*
    private async Task SpawnPlayerDelayed(ulong clientId)
    {
        await Task.Delay(1000);
        
        NetworkObject playerInstance = GameObject.Instantiate(playerPrefab, CharacterPool.SpawnCharacter(), Quaternion.identity);
        Debug.Log("have spawn");
        playerInstance.SpawnAsPlayerObject(clientId);
    }
    */
    
    private void OnClientDisconnect(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            clientIdToAuth.Remove(clientId);
            authIdToUserData.Remove(authId);
            OnClientLeft?.Invoke(authId);
        }
    }

    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request, 
        NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);
        
        clientIdToAuth.TryAdd(request.ClientNetworkId, userData.userAuthId);
        authIdToUserData.TryAdd(userData.userAuthId, userData);

        //_ = SpawnPlayerDelayed(request.ClientNetworkId);
        
        response.Approved = true;
        response.CreatePlayerObject = false;
        
    }
}
