using System;
using DungeonTogether.Scripts.Character.Module.AI;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetRandomTarget", story: "Get [RandomTarget] from [AIDetection]", category: "AI/Detection", id: "a98aa02f6d2474bf29a85e3a770d4dba")]
public partial class GetRandomTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> RandomTarget;
    [SerializeReference] public BlackboardVariable<CharacterAIDetectionModule> AIDetection;

    protected override Status OnStart()
    {
        RandomTarget.Value = AIDetection.Value.RandomCollider ? AIDetection.Value.transform : null;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

