using System;
using UnityEngine;

public class SpinnerMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveDistanceX = 5f;
    public float moveDistanceY = 1f;

    private Vector2[] directions = new Vector2[] {
        Vector2.left,
        Vector2.down,
        Vector2.right,
        Vector2.up
    };

    private int currentDirectionIndex = 0;
    private float movedDistance = 0f;

    void Update()
    {
        // حرکت در مسیر
        Vector3 direction = directions[currentDirectionIndex];
        float moveDistance = (currentDirectionIndex == 0 || currentDirectionIndex == 2) ? moveDistanceX : moveDistanceY;
        float step = moveSpeed * Time.deltaTime;
        transform.Translate(direction * step, Space.World);
        movedDistance += step;

        // تغییر جهت بعد از رسیدن به مسافت مورد نظر
        if (movedDistance >= moveDistance)
        {
            movedDistance = 0f;
            currentDirectionIndex = (currentDirectionIndex + 1) % directions.Length;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("player hited");
            
        }
    }
}