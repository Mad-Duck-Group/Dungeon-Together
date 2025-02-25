using DungeonTogether.Scripts.Character.Module;
using Unity.Netcode;
using UnityEngine;

public class TestAIMove : NetworkBehaviour
{
    [SerializeField] private CharacterMovementModule characterMovementModule;
    [SerializeField] private float stoppingDistance = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    private Transform target;

    public override void OnNetworkSpawn()
    {
        target = FindFirstObjectByType<PlayerInput>().gameObject.transform;
        if (target == null)
        {
            Debug.LogError("PlayerInput not found");
        }
    }

    private void Update()
    {
        if (!target) return;
        var direction = target.transform.position - transform.position;
        if (Vector2.Distance(transform.position, target.position) > stoppingDistance)
        {
            direction.Normalize();
        }
        else
        {
            direction = Vector2.zero;
        }
        characterMovementModule.SetDirection(direction);
    }
}
