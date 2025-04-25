using DungeonTogether.Scripts.Character.Module.Skill;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RangerSkillTarget", story: "[RangerSkill] [Target]", category: "AI/Attack", id: "943a6dd1c666177354847b78503162fc")]
public partial class RangerSkillTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<CharacterRangerSkillModule> RangerSkill;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    protected override Status OnStart()
    {
        var direction = Target.Value.position - RangerSkill.Value.transform.position;
        RangerSkill.Value.SetSkillDirection(direction);
        RangerSkill.Value.CastSkill();
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

