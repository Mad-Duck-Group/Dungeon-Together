using System;
using DungeonTogether.Scripts.Character.Module;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RangeAttack : DamageArea
{
    private float speed;
    private Vector3 direction;

    private void Update()
    {
        transform.position += direction * (speed * Time.deltaTime);
    }
    
    public override void SetActive(bool active)
    {
        if (!damageCollider) damageCollider = GetComponent<Collider2D>();
    }
    
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        Destroy(gameObject);
    }
    
    public void SetDirection(Vector3 dir, float projectileSpeed)
    {
        direction = dir.normalized;
        speed = projectileSpeed;
        Debug.Log($"Direction : {direction}, Speed : {speed}");
    }
}
