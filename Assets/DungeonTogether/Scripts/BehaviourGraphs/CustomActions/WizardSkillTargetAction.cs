using DungeonTogether.Scripts.Character.Module.Skill;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WizardSkillTarget", story: "[WizardSkill] [Target]", category: "AI/Attack", id: "78b7b03260cbd7d8787e6e106397fd25")]
public partial class WizardSkillTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<CharacterWizardSkillModule> WizardSkill;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    protected override Status OnStart()
    {
        //var direction = Target.Value.position - WizardSkill.Value.transform.position;
        WizardSkill.Value.CastSkill();
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

