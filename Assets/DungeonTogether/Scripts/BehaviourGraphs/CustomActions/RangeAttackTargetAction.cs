using DungeonTogether.Scripts.Character.Module;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Range Attack Target", story: "[RangeAttack] [Target]", category: "AI/Attack", id: "e6a3b1e54179572ccb4cd524cb534831")]
public partial class RangeAttackTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<CharacterBasicRangeAttackModule> RangeAttack;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var direction = Target.Value.position - RangeAttack.Value.transform.position;
        RangeAttack.Value.SetAttackDirection(direction);
        RangeAttack.Value.Attack();
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

