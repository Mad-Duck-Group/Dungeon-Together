using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DungeonTogether.Scripts.Manangers;
using TriInspector;
using Unity.Netcode;
using UnityEngine;

namespace DungeonTogether.Scripts.Character.Module.Ultimate
{
    public class CharacterRangerUltimateModule : CharacterAreaUltimateModule
    {
        protected override void OnHit(Collider2D collider)
        {
            if (!collider.TryGetComponent(out CharacterHub characterHub)) return;
            var healthModule = characterHub.FindModuleOfType<CharacterHealthModule>();
            if (CurrentPattern == null) return;
            if (healthModule) 
                healthModule.ChangeHealth(-CurrentPattern.Value.value);
        }
        
        public void CastUltimate(Vector3 position)
        {
            if (!ModulePermitted) return;
            if (!ultimateReady) return;
            if (ultimateCoroutine != null) return;
            ultimateCoroutine = StartCoroutine(CastUltimateCoroutine(position));
        }
    }
}

