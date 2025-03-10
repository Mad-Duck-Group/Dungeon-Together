using System;
using System.Collections.Generic;
using TriInspector;
using Unity.Behavior;
using Unity.Behavior.GraphFramework;
using UnityEngine;
using UnityEngine.Serialization;

namespace DungeonTogether.Scripts.Character.Module.AI
{
    public class CharacterAIDetectionModule : CharacterModule
    {
        [Title("Detection References")]
        [SerializeField] private Transform detectionOrigin;
        // [SerializeField] private BehaviorGraphAgent behaviorGraphAgent;
        // [SerializeField] private string closetTargetVariableName;

        [Title("Detection Settings")]
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private LayerMask detectionLayer;

        [Title("Debug")]
        [ShowInInspector, ReadOnly] public Collider2D ClosestCollider => detectedColliders.Count > 0 ? detectedColliders[0] : null;
        [SerializeField, ReadOnly] private List<Collider2D> detectedColliders = new();

        protected override void UpdateModule()
        {
            if (!ModulePermitted) return;
            base.UpdateModule();
            Detection();
            // var closestTransform = ClosestCollider ? ClosestCollider.transform : null;
            // if (behaviorGraphAgent) behaviorGraphAgent.SetVariableValue(closetTargetVariableName, closestTransform);
        }

        private void Detection()
        {
            detectedColliders.Clear();
            if (!detectionOrigin) detectionOrigin = transform;
            var filter = new ContactFilter2D
            {
                layerMask = detectionLayer,
                useLayerMask = true
            };
            List<Collider2D> results = new List<Collider2D>();
            int count = Physics2D.OverlapCircle(detectionOrigin.position, detectionRadius, filter, results);
            if (count == 0) return;
            // sort the results by distance
            results.Sort((a, b) =>
            {
                var distanceA = Vector2.Distance(detectionOrigin.position, a.transform.position);
                var distanceB = Vector2.Distance(detectionOrigin.position, b.transform.position);
                return distanceA.CompareTo(distanceB);
            });
            detectedColliders.AddRange(results);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = detectionOrigin ? detectionOrigin.position : transform.position;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin, detectionRadius);
        }
    }
}
