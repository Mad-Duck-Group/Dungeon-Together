using DungeonTogether.Scripts.Utils;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageArea : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    
    private Collider2D damageCollider;
    public delegate void OnHit(Collider2D collider);
    public event OnHit OnHitEvent;
    private void Start()
    {
        damageCollider = GetComponent<Collider2D>();
        damageCollider.isTrigger = true;
    }
    public void SetActive(bool active)
    {
        if (!damageCollider) damageCollider = GetComponent<Collider2D>();
        damageCollider.enabled = active;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer))
        {
            OnHitEvent?.Invoke(other);
        }
    }
}
