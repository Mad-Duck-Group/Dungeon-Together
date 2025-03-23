using DungeonTogether.Scripts.Character.Module.Skill;
using UnityEngine;

namespace DungeonTogether.Scripts.Character.Module.Skill
{
    public class CharacterClericSkillModule : CharacterAreaSkillModule
    {
        protected override void OnHit(Collider2D collider)
        {
            if (!collider.TryGetComponent(out CharacterHub characterHub)) return;
            var healthModule = characterHub.FindModuleOfType<CharacterHealthModule>();
            if (healthModule && CurrentPattern != null) 
                healthModule.ChangeHealth(+CurrentPattern.Value.value);
            Debug.Log("Cleric skill hit");
        }
    }
}

