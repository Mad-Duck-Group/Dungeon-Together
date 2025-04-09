using System;
using TriInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


namespace DungeonTogether.Scripts.Character.Module
{
    public enum NavigationProtocol
    {
        Follow,
        Evade
    }
    /// <summary>
    /// Module responsible for handling character AI navigation.
    /// </summary>
    public class CharacterAINavigation : CharacterModule
    {
        #region Inspector
        [Title("AI Navigation References")]
        [SerializeField, Required] private NavMeshAgent navMeshAgent;
        [SerializeField, Required] private CharacterMovementModule characterMovementModule;
        
        [Title("AI Follow Settings")]
        [SerializeField] private float stopMovingDistance = 1f;
        
        [Title("AI Evade Settings")]
        [SerializeField] private float startEvadingDistance = 5f;
        [SerializeField] private float evadingDistance = 2.5f;
        [SerializeField] private float safeSpotSearchRadius = 5f;
        [SerializeField] private int maxSearchAttempts = 5;

        [Title("AI Navigation Debug")] 
        [SerializeField, ReadOnly] private NavigationProtocol navigationProtocol = NavigationProtocol.Follow;
        [ShowInInspector, ReadOnly] private Transform target;
        [ShowInInspector, ReadOnly, DisplayAsString] private ulong TargetClientId {get => _targetClientId.Value; set => _targetClientId.Value = value;}
        [ShowInInspector, ReadOnly, DisplayAsString] private bool IsFollowing {get => _isFollowing.Value; set => _isFollowing.Value = value;}

        #endregion

        #region Network Variables
        private readonly NetworkVariable<ulong> _targetClientId = new(writePerm: NetworkVariableWritePermission.Owner, value: 99999);
        private readonly NetworkVariable<bool> _isFollowing = new(writePerm: NetworkVariableWritePermission.Owner, value: false);
        #endregion
        
        #region Fields
        
        #endregion

        #region Life Cycles
        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            navMeshAgent.enabled = true;
            navMeshAgent.updatePosition = false;
            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;
            navMeshAgent.speed = characterMovementModule.MovementSpeed;
        }
        
        public override void Shutdown()
        {
            base.Shutdown();
            navMeshAgent.enabled = false;
            SetTarget(null);
            SetFollowingRpc(false);
        }
        #endregion

        #region Events

        protected override void Subscribe()
        {
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        protected override void Unsubscribe()
        {
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (clientId != TargetClientId) return;
            SetFollowingRpc(false);
            SetTarget(null);
            SetTargetClientIdRpc(99999);
        }
        #endregion

        #region Updates
        protected override void Update()
        {
            if (!IsOwner) return;
            if (ModulePermitted)
            {
                HandleInput();
            }
            UpdateModule();
        }
        
        protected override void UpdateModule()
        {
            if (!ModulePermitted)
            {
                characterMovementModule.SetDirection(Vector2.zero, true);
                navMeshAgent.Warp(transform.position);
                return;
            }
            switch (navigationProtocol)
            {
                case NavigationProtocol.Follow:
                    Follow();
                    break;
                case NavigationProtocol.Evade:
                    Evade();
                    break;
            }
        }

        private void Follow()
        {
            if (!target)
            {
                characterMovementModule.SetDirection(Vector2.zero);
                return;
            }
            var distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget < stopMovingDistance)
            {
                characterMovementModule.SetDirection(Vector2.zero);
                navMeshAgent.Warp(transform.position);
                return;
            }
            navMeshAgent.SetDestination(target.position);
            characterMovementModule.SetDirection(target.position - transform.position);
            characterMovementModule.SetPosition(navMeshAgent.nextPosition);
        }
        
        private void Evade()
        {
            if (!target)
            {
                characterMovementModule.SetDirection(Vector2.zero);
                return;
            }
            var distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget > startEvadingDistance)
            {
                characterMovementModule.SetDirection(Vector2.zero);
                return;
            }
            if (!FindSafeSpot(out var safeSpot))
            {
                characterMovementModule.SetDirection(Vector2.zero);
                return;
            }
            navMeshAgent.SetDestination(safeSpot);
            characterMovementModule.SetDirection(safeSpot - transform.position);
            characterMovementModule.SetPosition(navMeshAgent.nextPosition);
        }
        
        private bool FindSafeSpot(out Vector3 position)
        {
            position = Vector3.zero;
            var direction = transform.position - target.position;
            direction.Normalize();
            direction *= evadingDistance;
            for (int i = 0; i < maxSearchAttempts; i++)
            {
                var randomPosition = transform.position + new Vector3(direction.x, direction.y, 0);
                if (!NavMesh.SamplePosition(randomPosition, out var hit, safeSpotSearchRadius,
                        NavMesh.AllAreas)) continue;
                position = hit.position;
                return true;
            }
            return false;
        }
        #endregion

        #region RPCs
        [Rpc(SendTo.Server, RequireOwnership = false, DeferLocal = true)]
        private void SetFollowingRpc(bool active)
        {
            IsFollowing = active;
        }

        //[Rpc(SendTo.Server, RequireOwnership = false, DeferLocal = true)]
        public void SetTarget(Transform target)
        {
            if (!target)
            {
                navMeshAgent.Warp(transform.position);
            }
            this.target = target;
        }
        
        public void SetProtocol(NavigationProtocol protocol)
        {
            navigationProtocol = protocol;
        }
        
        [Rpc(SendTo.Server, RequireOwnership = false, DeferLocal = true)]
        public void SetTargetClientIdRpc(ulong clientId)
        {
            TargetClientId = clientId;
        }
        #endregion
    }
}
