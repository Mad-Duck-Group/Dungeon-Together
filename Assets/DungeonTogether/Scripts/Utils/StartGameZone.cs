using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class StartGameZone : TriggerZone
{
    protected override void FixedUpdate()
    {
        if (characterStayData.Count == 0) return;
        foreach (var character in characterStayData.Keys)
        {
            if (characterStayData[character].triggered) continue;
            if (characterStayData[character].time < stayDuration)
            {
                characterStayData[character].time += Time.fixedDeltaTime;
                continue;
            }
            if (character.NetworkObject.IsOwner)
            {
                var clientId = character.OwnerClientId;
                onStayEvent?.Invoke(clientId);
            }
            characterStayData[character].triggered = true;
           
        }
        switch (requireAllClients)
        {
            case true when characterStayData.Count == NetworkManager.Singleton.ConnectedClients.Count &&
                           characterStayData.Values.All(data => data.triggered):
                onStayAllClientsEvent?.Invoke();
                LoadSceneManager.Instance.LoadScene(SceneType.Game);
                break;
        }
    }
}
