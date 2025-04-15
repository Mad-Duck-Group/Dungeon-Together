using System;
using System.Collections.Generic;
using System.Linq;
using DungeonTogether.Scripts.Character;
using DungeonTogether.Scripts.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
    protected record StayData
    {
        public float time;
        public bool triggered;
    }
    
    [SerializeField] protected float stayDuration = 1f;
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected bool requireAllClients;
    [SerializeField] private UnityEvent<ulong> onEnterEvent;
    [SerializeField] protected UnityEvent<ulong> onStayEvent;
    [SerializeField] protected UnityEvent onStayAllClientsEvent;
    [SerializeField] protected UnityEvent<ulong> onExitEvent;

    protected readonly Dictionary<CharacterHub, StayData> characterStayData = new();
    
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer)) return;
        var character = other.GetComponent<CharacterHub>();
        if (character == null) return;
        if (characterStayData.ContainsKey(character)) return;
        characterStayData.Add(character, new StayData());
        characterStayData[character].time = 0f;
        if (!character.NetworkObject.IsOwner) return;
        var clientId = character.OwnerClientId;
        onEnterEvent?.Invoke(clientId);
    }

    protected virtual void FixedUpdate()
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
                break;
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (!LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer)) return;
        var character = other.GetComponent<CharacterHub>();
        if (character == null) return;
        if (!characterStayData.ContainsKey(character)) return;
        characterStayData.Remove(character);
        if (!character.NetworkObject.IsOwner) return;
        var clientId = character.OwnerClientId;
        onExitEvent?.Invoke(clientId);
    }
}
