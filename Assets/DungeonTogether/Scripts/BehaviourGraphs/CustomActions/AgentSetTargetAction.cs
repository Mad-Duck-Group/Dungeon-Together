using DungeonTogether.Scripts.Character;
using System;
using DungeonTogether.Scripts.Character.Module;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Agent Set Target", story: "Set target for [Agent] to [Target]", category: "AI/Navigation", id: "15b9ef2ebe8e37bb2d170861df82d2eb")]
public partial class AgentSetTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<CharacterAINavigation> Agent;
    protected override Status OnStart()
    {
        Agent.Value.SetTarget(Target.Value);
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

