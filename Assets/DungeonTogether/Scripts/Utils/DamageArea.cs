using System;
using System.Collections.Generic;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageArea : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer visualizer;
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected bool allowReentry;
    [SerializeField] protected bool includeSelf;
    [SerializeField] protected bool DOT;
    [SerializeField, ShowIf("DOT")] protected float DOTInterval;

    protected Collider2D damageCollider;
    protected Transform parent;
    protected bool detached;
    protected Dictionary<Collider2D, float> lastDamageTime = new Dictionary<Collider2D, float>();
    
    protected HashSet<Collider2D> hitList = new HashSet<Collider2D>();
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
        if (!active) { hitList.Clear(); }
        damageCollider.enabled = active;
    }
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (DOT) { return; }
        if (!allowReentry && hitList.Contains(other)) { return; }
        if (LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer))
        {
            hitList.Add(other);
            OnHitEvent?.Invoke(other);
        }
    }
    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        if (!DOT) { return; }
        if (!allowReentry && hitList.Contains(other)) { return; }
        if (LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer)) // ตรวจสอบว่าเป็น Player หรือไม่
        {
            float currentTime = Time.time;

            // ถ้าไม่มีค่าใน Dictionary ให้ใส่ค่าเริ่มต้นเป็น 0
            if (!lastDamageTime.ContainsKey(other))
            {
                lastDamageTime[other] = 0f;
            }

            if (currentTime >= lastDamageTime[other] + DOTInterval) // เช็คว่าผ่านไปพอหรือยัง
            {
                Debug.Log("DOT applied to: " + other.name);
                lastDamageTime[other] = currentTime; // อัปเดตเวลาล่าสุดของเป้าหมายนี้
                hitList.Add(other);
                OnHitEvent?.Invoke(other);
            }
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
