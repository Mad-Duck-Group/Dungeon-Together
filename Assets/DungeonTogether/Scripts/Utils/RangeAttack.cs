using System;
using DungeonTogether.Scripts.Character.Module;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RangeAttack : CharacterModule
{
    [SerializeField] private LayerMask targetLayer;
    
    private Collider2D damageCollider;
    
    private int damage;
    
    public delegate void OnRangeHit(Collider2D collider);
    public event OnRangeHit OnRangeHitEvent;
    
    void Start()
    {
        damageCollider = GetComponent<Collider2D>();
        damageCollider.isTrigger = true;
    }
    
    public void SetActive(bool active)
    {
        if (!damageCollider) damageCollider = GetComponent<Collider2D>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer))
        {
            
            Destroy(gameObject);
            
            OnRangeHitEvent?.Invoke(other);
            
        }
    }
}
