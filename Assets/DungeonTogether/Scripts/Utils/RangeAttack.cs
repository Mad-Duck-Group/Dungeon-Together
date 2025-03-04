using System;
using DungeonTogether.Scripts.Character.Module;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RangeAttack : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    
    private Collider2D damageCollider;
    
    private int damage;

    private float speed;

    private Vector3 direction;
    public delegate void OnRangeHit(Collider2D collider);
    public event OnRangeHit OnRangeHitEvent;
    
    void Start()
    {
        damageCollider = GetComponent<Collider2D>();
        damageCollider.isTrigger = true;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
    
    public void SetActive(bool active)
    {
        if (!damageCollider) damageCollider = GetComponent<Collider2D>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer))
        {
            OnRangeHitEvent?.Invoke(other);
        }
    }

    public void SetDirection(Vector3 dir, float projectileSpeed)
    {
        direction = dir.normalized;
        speed = projectileSpeed;
        Debug.Log($"Direction : {direction}, Speed : {speed}");
    }
}
