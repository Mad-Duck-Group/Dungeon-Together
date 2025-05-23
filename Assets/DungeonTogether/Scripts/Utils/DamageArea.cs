using System;
using System.Collections.Generic;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageArea : NetworkBehaviour
{
    [SerializeField] protected SpriteRenderer visualizer;
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected bool allowReentry;
    [SerializeField] protected bool detach;
    [SerializeField] protected bool followParent = true;
    [SerializeField] protected bool includeSelf;
    [SerializeField] protected bool DOT;
    [SerializeField, ShowIf(nameof(DOT))] protected float DOTInterval;

    protected Collider2D damageCollider;
    protected Transform parent;
    protected bool detached;
    protected Dictionary<Collider2D, float> lastDamageTime = new();
    
    protected HashSet<Collider2D> hitList = new();
    public delegate void OnHit(Collider2D collider);
    public event OnHit OnHitEvent;

    public void Initialize()
    {
        if (!TryGetComponent(out damageCollider))
        {
            Debug.LogError("No Collider2D found on DamageArea");
            return;
        }
        damageCollider.isTrigger = true;
        parent = transform.parent;
    }

    [Rpc(SendTo.Everyone)]
    private void InitializeRpc()
    {
        
    }

    protected virtual void OnDisable()
    {
        OnHitEvent = null;
    }

    public virtual void SetActive(bool active)
    {
        SetActiveRpc(active);
    }

    [Rpc(SendTo.Everyone)]
    private void SetActiveRpc(bool active)
    {
        if (visualizer) visualizer.enabled = active;
        switch (active)
        {
            case true when detach || includeSelf:
                Detach();
                break;
            case false when detached:
                Reattach();
                break;
        }
        if (!active) { hitList.Clear(); }
        damageCollider.enabled = active;
    }
    
    public void SetPosition(Vector3 position)
    {
        SetPositionRpc(position);
    }
    
    [Rpc(SendTo.Everyone)]
    private void SetPositionRpc(Vector3 position)
    {
        transform.position = position;
    }
    
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!allowReentry && hitList.Contains(other)) { return; }
        if (!LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer)) return;
        hitList.Add(other);
        lastDamageTime.TryAdd(other, 0f);
        OnHitEvent?.Invoke(other);
        Debug.Log("Hit: " + other.name);
    }

    protected virtual void FixedUpdate()
    {
        if (!DOT) { return; }
        if (lastDamageTime.Count == 0) { return; }
        List<Collider2D> toModify = new();
        float currentTime = Time.time;
        foreach (var pair in lastDamageTime)
        {
            var other = pair.Key;
            if (!allowReentry && hitList.Contains(other)) { return; }
            if (!LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer)) continue;
            if (currentTime >= lastDamageTime[other] + DOTInterval)
            {
                Debug.Log("DOT applied to: " + other.name);
                toModify.Add(other);
                hitList.Add(other);
                OnHitEvent?.Invoke(other);
            }
        }
        toModify.ForEach(other =>
        {
            lastDamageTime[other] = currentTime;
        });
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (lastDamageTime.ContainsKey(other))
        {
            lastDamageTime.Remove(other);
        }
    }

    private void Update()
    {
        if (detached && followParent)
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
