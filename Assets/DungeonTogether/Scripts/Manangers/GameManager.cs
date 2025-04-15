using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using Unity.Netcode;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonTogether.Scripts.Manangers
{
    public class GameManager : NetworkBehaviour
    {
        // private void OnEnable()
        // {
        //     ClassSelector.OnClassSelectorSpawned += SpawnPlayer;
        // }
        //
        // private void OnDisable()
        // {
        //     ClassSelector.OnClassSelectorSpawned -= SpawnPlayer;
        // }
        //
        // private void SpawnPlayer(ClassSelector classSelector)
        // {
        //     var localClientId = NetworkManager.Singleton.LocalClient.ClientId;
        //     Debug.Log($"Local client ID: {localClientId}");
        //     classSelector.SpawnSelectedClass(localClientId);
        // }
    }
}
