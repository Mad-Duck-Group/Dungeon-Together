using System;
using DungeonTogether.Scripts.Character.Module;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ProjectileDamageArea : DamageArea
{
    private float speed;
    private Vector3 direction;
    private LayerMask passThroughLayer;

    private void Update()
    {
        transform.position += direction * (speed * Time.deltaTime); 
    }

    public override void SetActive(bool active)
    {
        if (!damageCollider) damageCollider = GetComponent<Collider2D>();
    }
    
    public void SetPassThroughLayer(LayerMask layer)
    {
        passThroughLayer = layer;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if (LayerMaskUtils.IsInLayerMask(other.gameObject.layer, passThroughLayer)) return;
        Destroy(gameObject);
    }
    
    public void SetDirection(Vector3 dir, float projectileSpeed)
    {
        direction = dir.normalized;
        speed = projectileSpeed;
        Debug.Log($"Direction : {direction}, Speed : {speed}");
    }
}
