using DungeonTogether.Scripts.Character.Module.AI;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.Serialization;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Get ClosetTarget", story: "Get [ClosetTarget] from [AIDetection]", category: "AI/Detection", id: "8b22e1d7710d71106c3acf5dc88d46f9")]
public partial class GetClosetTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> ClosetTarget;
    [SerializeReference] public BlackboardVariable<CharacterAIDetectionModule> AIDetection;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        ClosetTarget.Value = AIDetection.Value.ClosestCollider ? AIDetection.Value.ClosestCollider.transform : null;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

