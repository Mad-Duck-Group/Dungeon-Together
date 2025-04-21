using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using DungeonTogether.Scripts.Manangers;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class AnalyticManager : PersistentMonoSingleton<AnalyticManager>
{
    #region Data Structures
    private struct EventData
    {
        public string eventName;
        public List<EventParameterData> eventParameters;
    }

    private struct EventParameterData
    {
        public string parameterName;
        public object parameterValue;
    }
    
    [Serializable]
    private struct EventNameData
    {
        public string eventName;
        public string parameterName;
    }

    private enum EventType
    {
        ClassSelected,
        PlayerCount,
        LevelCompletionTime,
    }
    #endregion
    
    [SerializeField, SerializedDictionary("Event Type", "Event Name")]
    private SerializedDictionary<EventType, EventNameData> eventNameDictionary;
    
    void Start()
    {
        Initialize();
    }

    private async void Initialize()
    {
        await UnityServices.InitializeAsync();
        AnalyticsService.Instance.StartDataCollection();
    }
    
    public void OnClassSelected(ClassType classType)
    {
        string className = classType.ToString();
        var eventData = CreateEventData(EventType.ClassSelected, className);
        SendEvent(eventData);
    }

    public void OnPlayerCount(int playerCount)
    {
        var eventData = CreateEventData(EventType.PlayerCount,playerCount);
        SendEvent(eventData);
    }
    
    public void OnLevelCompletionTime(float time)
    {
        var eventData = CreateEventData(EventType.LevelCompletionTime, time);
        SendEvent(eventData);
    }

    private EventData CreateEventData(EventType eventType, object parameterValue)
    {
        string eventName = eventNameDictionary[eventType].eventName;
        string parameterName = eventNameDictionary[eventType].parameterName;
        List<EventParameterData> eventParameters = new List<EventParameterData>
        {
            new()
            {
                parameterName = parameterName,
                parameterValue = parameterValue
            }
        };
        return new EventData
        {
            eventName = eventName,
            eventParameters = eventParameters
        };
    }

    private void SendEvent(EventData eventData)
    {
        CustomEvent customEvent = new CustomEvent(eventData.eventName);
        eventData.eventParameters.ForEach(parameter =>
        {
            customEvent.Add(parameter.parameterName, parameter.parameterValue);
        });
        AnalyticsService.Instance.RecordEvent(customEvent);
        Debug.Log($"Event sent: {eventData.eventName}");
    }
}
