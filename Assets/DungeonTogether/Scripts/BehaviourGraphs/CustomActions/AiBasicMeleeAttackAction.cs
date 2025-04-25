using DungeonTogether.Scripts.Character.Module;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AI Basic Melee Attack", story: "[BasicMeleeAttack] [Target]", category: "AI/Attack", id: "3663c5f47ed808fb1ce5988c6177e45a")]
public partial class AiBasicMeleeAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<CharacterBasicAttackModule> BasicMeleeAttack;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    protected override Status OnStart()
    {
        var direction = Target.Value.position - BasicMeleeAttack.Value.transform.position;
        BasicMeleeAttack.Value.SetAttackDirection(direction);
        BasicMeleeAttack.Value.Attack();
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

