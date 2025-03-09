using System;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageArea : MonoBehaviour
{
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected LayerMask ignoreLayer;

    protected Collider2D damageCollider;
    public delegate void OnHit(Collider2D collider);
    public event OnHit OnHitEvent;

    protected virtual void Start()
    {
        damageCollider = GetComponent<Collider2D>();
        damageCollider.isTrigger = true;
    }

    protected virtual void OnDisable()
    {
        OnHitEvent = null;
    }

    public virtual void SetActive(bool active)
    {
        if (!damageCollider) damageCollider = GetComponent<Collider2D>();
        damageCollider.enabled = active;
    }
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer))
        {
            OnHitEvent?.Invoke(other);
        }
    }
}
