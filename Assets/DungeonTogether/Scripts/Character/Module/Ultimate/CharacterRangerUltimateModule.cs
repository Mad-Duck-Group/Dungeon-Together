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

        protected override IEnumerator CastUltimateCoroutine()
        {
            if (CurrentPattern == null) yield break;
            if (!ConsumeEnergy(CurrentPattern.Value.energy))
            {
                ultimateCoroutine = null;
                yield break;
            }

            currentComboTime = 0;
            ultimateUsed = true;
            characterHub.ChangeActionState(CharacterActionState.Ultimate);
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;
            var area = CurrentPattern.Value.areaUltimate;
            area.transform.position = mousePosition;
            area.transform.parent = null;
            yield return new WaitForSeconds(CurrentPattern.Value.delay);
            area.SetActive(true);
            yield return new WaitForSeconds(CurrentPattern.Value.duration);
            area.SetActive(false);

            characterHub.ChangeActionState(CharacterActionState.None);
            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % areaUltimatePattern.Count;
            currentCooldown = 0;
            ultimateReady = false;
            ultimateUsed = false;
            ultimateCoroutine = null;
        }
    }
}

