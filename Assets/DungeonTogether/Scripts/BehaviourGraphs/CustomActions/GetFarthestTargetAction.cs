using DungeonTogether.Scripts.Character.Module.AI;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Get FarthestTarget", story: "Get [FarthestTarget] from [AIDetection]", category: "AI/Detection", id: "07ba61730c96e789176d50a0a7e13620")]
public partial class GetFarthestTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> FarthestTarget;
    [SerializeReference] public BlackboardVariable<CharacterAIDetectionModule> AIDetection;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        FarthestTarget.Value = AIDetection.Value.FarthestCollider ? AIDetection.Value.FarthestCollider.transform : null;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

