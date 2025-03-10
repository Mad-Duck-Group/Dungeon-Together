using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "GameObject Exists", story: "Does [GameObject] exists?", category: "General", id: "c065453ea977318604981d1c4feef7d5")]
public partial class GameObjectExistsCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> GameObject;

    public override bool IsTrue()
    {
        return GameObject.Value;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
