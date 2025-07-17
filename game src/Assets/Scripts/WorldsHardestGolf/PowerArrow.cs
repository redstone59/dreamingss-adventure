using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerArrow : MonoBehaviour
{
    public SpriteRenderer arrow;
    public float distance = 10f;
    public Color weakColour;
    public Color medianColour;
    public Color strongColour;

    // Start is called before the first frame update
    void Start()
    {
        arrow = GetComponent<SpriteRenderer>();
        arrow.color = new Color(1, 1, 1, 0);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetPosition(Vector3 worldPosition, Vector3 directionVector, float maxForce, bool flipped)
    {
        Vector3 arrowPosition = worldPosition;
        float force = Mathf.Min(directionVector.magnitude, maxForce);

        if (force == 0) return;

        float halfForce = maxForce / 2f;

        if (force <= halfForce)
            arrow.color = Color.Lerp(weakColour, medianColour, force / halfForce);
        else
            arrow.color = Color.Lerp(medianColour, strongColour, force / halfForce - 1);

        float angle = Mathf.Atan(directionVector.y / directionVector.x);

        Debug.Log(flipped);

        if (flipped)
        {
            arrowPosition.x -= distance * Mathf.Cos(angle);
            arrowPosition.y -= distance * Mathf.Sin(angle);
        }
        else
        {
            arrowPosition.x += distance * Mathf.Cos(angle);
            arrowPosition.y += distance * Mathf.Sin(angle);
        }

        angle *= Mathf.Rad2Deg;
        if (flipped) angle += 180;
        arrow.transform.rotation = Quaternion.Euler(0, 0, angle);

        arrow.transform.position = arrowPosition;
    }

    public void Hide()
    {
        arrow.color = new Color(1, 1, 1, 0);
    }
}
