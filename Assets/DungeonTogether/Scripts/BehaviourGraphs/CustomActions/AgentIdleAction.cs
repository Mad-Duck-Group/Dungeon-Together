using DungeonTogether.Scripts.Character.Module;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Agent Idle", story: "[Agent] stands still", category: "AI/Navigation", id: "1dbe2eb2ee27f57a5b774aae9baeba50")]
public partial class AgentIdleAction : Action
{
    [SerializeReference] public BlackboardVariable<CharacterAINavigation> Agent;

    protected override Status OnStart()
    {
        Agent.Value.SetTarget(null);
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

