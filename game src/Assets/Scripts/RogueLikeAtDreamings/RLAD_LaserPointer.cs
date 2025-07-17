using UnityEngine;

namespace RogueLikeAtDreamings
{
    public class LaserPointer : MonoBehaviour
    {
        void Update()
        {
            float resolutionScale = Screen.height / 1080f;
            Vector3 newPosition = Input.mousePosition;
            newPosition.x = Mathf.Clamp(newPosition.x, -200 * resolutionScale, Screen.width + 200 * resolutionScale);
            newPosition.y = Mathf.Clamp(newPosition.y, -250 * resolutionScale, Screen.height + 250 * resolutionScale);
            transform.position = newPosition;
        }
    }
}