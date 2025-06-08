using UnityEngine;

public class FloatingMotion : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float floatHeight = 0.15f;

    
    private Vector3 startPos;

    private SpriteRenderer spriteRenderer;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.3f;

    void Start()
    {
        startPos = transform.localPosition;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Float
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = startPos + new Vector3(0, newY, 0);

        // Pulse
        float brightness = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        spriteRenderer.color = new Color(brightness, brightness, brightness, 1f);
    }

}