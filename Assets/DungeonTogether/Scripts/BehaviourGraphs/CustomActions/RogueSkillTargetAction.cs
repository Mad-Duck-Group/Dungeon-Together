using DungeonTogether.Scripts.Character.Module.Skill;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Rogue Skill Target", story: "[RogueSkill] [Target]", category: "AI/Attack", id: "a9697174d21085cc2b5f747ad19f0c30")]
public partial class RogueSkillTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<CharacterRogueMeleeSkill> RogueSkill;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    protected override Status OnStart()
    {
        var direction = Target.Value.position - RogueSkill.Value.transform.position;
        RogueSkill.Value.SetSkillDirection(direction);
        RogueSkill.Value.CastSkill();
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

