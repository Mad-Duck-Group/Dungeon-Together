using System;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageArea : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer visualizer;
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected bool includeSelf;
    [SerializeField] protected bool DOT;

    protected Collider2D damageCollider;
    protected Transform parent;
    protected bool detached;
    public delegate void OnHit(Collider2D collider);
    public event OnHit OnHitEvent;

    public void Initialize()
    {
        damageCollider = GetComponent<Collider2D>();
        damageCollider.isTrigger = true;
        parent = transform.parent;
    }

    protected virtual void OnDisable()
    {
        OnHitEvent = null;
    }

    public virtual void SetActive(bool active)
    {
        if (visualizer) visualizer.enabled = active;
        switch (active)
        {
            case true when includeSelf:
                Detach();
                break;
            case false when includeSelf:
                Reattach();
                break;
        }
        damageCollider.enabled = active;
    }
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (DOT) { return; }
        if (LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer))
        {
            OnHitEvent?.Invoke(other);
        }
    }
    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        if (!DOT) { return; }
        if (LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer))
        {
            OnHitEvent?.Invoke(other);
        }
    }

    private void Update()
    {
        if (detached)
        {
            transform.position = parent.position;
        }
    }

    protected virtual void Detach()
    {
        transform.SetParent(null);
        detached = true;
    }
    
    protected virtual void Reattach()
    {
        transform.SetParent(parent);
        detached = false;
    }
}
