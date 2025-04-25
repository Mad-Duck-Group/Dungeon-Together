using DungeonTogether.Scripts.Character.Module.Ultimate;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RangerUltimateTarget", story: "[RangerUltimate] [Target]", category: "AI/Attack", id: "9951c04496b93da2b250a456f979cfb3")]
public partial class RangerUltimateTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<CharacterRangerUltimateModule> RangerUltimate;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    protected override Status OnStart()
    {
        RangerUltimate.Value.CastUltimate(Target.Value.position);
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

