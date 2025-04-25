using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace DungeonTogether.Scripts.Utils
{
    /// <summary>
    /// Rotates the object to face the mouse cursor.
    /// </summary>
    public class RotateToMouse : NetworkBehaviour
    {
        [SerializeField] private bool active = true;
        void Update()
        {
            if (!IsOwner) return;
            if (!active) return;
            
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 lookDir = mousePos - transform.position;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        public void SetActive(bool isActive)
        {
            active = isActive;
        }
    }
}
