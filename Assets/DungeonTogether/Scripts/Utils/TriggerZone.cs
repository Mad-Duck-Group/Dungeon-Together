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
    private record StayData
    {
        public float time;
        public bool triggered;
    }
    
    [SerializeField] private float stayDuration = 1f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private bool requireAllClients;
    [SerializeField] private UnityEvent onEnterEvent;
    [SerializeField] private UnityEvent onStayEvent;
    [SerializeField] private UnityEvent onStayAllClientsEvent;
    [SerializeField] private UnityEvent onExitEvent;
    
    private readonly Dictionary<CharacterHub, StayData> characterStayData = new();
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer)) return;
        var character = other.GetComponent<CharacterHub>();
        if (character == null) return;
        if (characterStayData.ContainsKey(character)) return;
        characterStayData.Add(character, new StayData());
        characterStayData[character].time = 0f;
        onEnterEvent?.Invoke();
    }

    private void FixedUpdate()
    {
        if (characterStayData.Count == 0) return;
        foreach (var character in characterStayData.Keys)
        {
            if (characterStayData[character].triggered) continue;
            if (characterStayData[character].time < stayDuration)
            {
                characterStayData[character].time += Time.fixedDeltaTime;
            }
            else
            {
                onStayEvent?.Invoke();
                characterStayData[character].triggered = true;
            }
           
        }
        switch (requireAllClients)
        {
            case true when characterStayData.Count == NetworkManager.Singleton.ConnectedClients.Count &&
                          characterStayData.Values.All(data => data.triggered):
                onStayAllClientsEvent?.Invoke();
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer)) return;
        var character = other.GetComponent<CharacterHub>();
        if (character == null) return;
        if (!characterStayData.ContainsKey(character)) return;
        characterStayData.Remove(character);
        onExitEvent?.Invoke();
    }
}
