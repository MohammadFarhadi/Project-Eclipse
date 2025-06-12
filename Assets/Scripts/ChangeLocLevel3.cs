using System;
using UnityEngine;

public class ChangeLocLevel3 : MonoBehaviour
{
    private GameObject[] players;
    [SerializeField] private GameObject Player1, Player2, Camera;
    public Transform player1_Pos, player2_Pos, CameraPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Camera = GameObject.FindGameObjectWithTag("MainCamera");
        players = GameObject.FindGameObjectsWithTag("Player");
        Player1 = players[0];
        Player2 = players[1];
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Player1.transform.position = player1_Pos.position;
            Player2.transform.position = player2_Pos.position;
            Camera.transform.position = CameraPos.position;
            Camera.GetComponent<Camera>().orthographicSize = 5.9f;
        }
    }
}
