using UnityEngine;
using System.Collections.Generic;

public class ParallaxController : MonoBehaviour
{
    public List<Transform> backgroundLayers;
    public float baseParallaxFactor = 0.5f;
    public bool onlyHorizontal = true;

    [Header("Players")]
    public Transform player1;
    public Transform player2;

    private Vector3 lastMidPoint;

    void Start()
    {
        lastMidPoint = GetMidPoint();
    }

    void LateUpdate()
    {
        Vector3 currentMidPoint = GetMidPoint();
        Vector3 delta = currentMidPoint - lastMidPoint;

        for (int i = 0; i < backgroundLayers.Count; i++)
        {
            float layerFactor = 1f - (i / (float)backgroundLayers.Count);
            float parallaxSpeed = baseParallaxFactor * layerFactor;

            Vector3 move = delta * parallaxSpeed;
            if (onlyHorizontal) move.y = 0;

            backgroundLayers[i].position += move;
        }

        lastMidPoint = currentMidPoint;
    }

    Vector3 GetMidPoint()
    {
        if (player1 != null && player2 != null)
            return (player1.position + player2.position) / 2f;
        else if (player1 != null)
            return player1.position;
        else if (player2 != null)
            return player2.position;
        else
            return Vector3.zero;
    }
}