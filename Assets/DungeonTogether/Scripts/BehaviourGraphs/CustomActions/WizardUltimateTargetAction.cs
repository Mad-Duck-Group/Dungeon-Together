using DungeonTogether.Scripts.Character.Module.Ultimate;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WizardUltimateTarget", story: "[WizardUltimate] [Target]", category: "AI/Attack", id: "b1188f43e38087dffc945a50097adbce")]
public partial class WizardUltimateTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<CharacterWizardUltimateModule> WizardUltimate;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    protected override Status OnStart()
    {
        var direction = Target.Value.position - WizardUltimate.Value.transform.position;
        WizardUltimate.Value.SetUltimateDirection(direction);
        WizardUltimate.Value.CastUltimate();
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

