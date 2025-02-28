using UnityEngine;

namespace DungeonTogether.Scripts.Utils
{
    public class RotateToMouse : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 lookDir = mousePos - transform.position;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
