using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DungeonTogether.Scripts.Manangers;
using Redcode.Extensions;
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
        
        public override void CastUltimate()
        {
            if (!ModulePermitted) return;
            if (!ultimateReady) return;
            if (ultimateCoroutine != null) return;
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition).WithZ(0f);
            ultimateCoroutine = StartCoroutine(CastUltimateCoroutine(mousePos));
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

