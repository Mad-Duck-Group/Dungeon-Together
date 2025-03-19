using TriInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace DungeonTogether.Scripts.Character.Module
{
    public class CharacterCameraModule : CharacterModule
    {
        [Title("References")] 
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private int startingPriority = 10;
        
        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            if (!IsOwner) return;
            SetPriority(startingPriority);
            cinemachineCamera.transform.position = transform.position;
        }
        
        public void SetPriority(int newPriority)
        {
            cinemachineCamera.Priority = newPriority;
            
        }
    }
}
