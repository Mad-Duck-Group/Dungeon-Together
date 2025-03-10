using DungeonTogether.Scripts.Character.Module;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using NavigationProtocol = DungeonTogether.Scripts.Character.Module.NavigationProtocol;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Navigation Protocol", story: "Set protocol for [Agent] to [NavigationProtocol]", category: "AI/Navigation", id: "382e593304e8119fcf0ffec326cfea33")]
public partial class SetNavigationProtocolAction : Action
{
    [SerializeReference] public BlackboardVariable<global::NavigationProtocol> NavigationProtocol;
    [SerializeReference] public BlackboardVariable<CharacterAINavigation> Agent;
    protected override Status OnStart()
    {
        int protocol = (int)NavigationProtocol.Value;
        Agent.Value.SetProtocol((DungeonTogether.Scripts.Character.Module.NavigationProtocol)protocol);
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

