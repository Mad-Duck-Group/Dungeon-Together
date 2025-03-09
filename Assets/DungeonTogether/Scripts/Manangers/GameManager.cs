using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using Unity.Netcode;

namespace DungeonTogether.Scripts.Manangers
{
    public class GameManager : NetworkBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameManager>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            _instance = this;
        }
        
        public NetworkObject GetLocalPlayer()
        {
            return NetworkManager.Singleton.LocalClient.PlayerObject;
        }
    }
}
