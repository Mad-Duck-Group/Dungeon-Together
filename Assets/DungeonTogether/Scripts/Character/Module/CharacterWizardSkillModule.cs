using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace DungeonTogether.Scripts.Character.Module
{
    [Serializable]
    public struct WizardSkillArea
    {
        [Group("Area"), Required] public DamageArea damageArea;
        [Group("Damage"), Min(0)] public float damage;
        [Group("Timing"), Min(0)] public float delay;
        [Group("Timing"), Min(0)] public float duration;
        [Group("Timing"), Min(0)] public float cooldown;
        [Group("Range"), Min(0)] public float radius;
        [Group("Cost"), Min(0)] public float mana;
    }
    public class CharacterWizardSkillModule : CharacterModule
    {
        [Title("Settings")]
        [SerializeField] private Transform wizardSkillOrigin;
        [TableList(Draggable = true,
            HideAddButton = false,
            HideRemoveButton = false,
            AlwaysExpanded = false)]
        [SerializeField] private List<WizardSkillArea> wizardSkillAreas;
        
        private Coroutine skillCoroutine;
        
        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            foreach (var skillArea in wizardSkillAreas)
            {
                skillArea.damageArea.SetActive(false);
            }
        }

        public override void Shutdown()
        {
            base.Shutdown();
            foreach (var skillArea in wizardSkillAreas)
            {
                skillArea.damageArea.SetActive(false);
            }
        }
        protected override void HandleInput()
        {
            if (characterHub.CharacterType is not CharacterType.Player) return;
            base.HandleInput();
            
            if (PlayerInput.SkillButton.isDown)
            {
                //CastSkill Method
            }
        }
    }
}

