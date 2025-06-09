using UnityEngine;
using System.Collections.Generic;

public class ChunkManager : MonoBehaviour
{
    public GameObject[] chunks = new GameObject[6]; // 0=start, 5=end
    public List<GameObject> instantiatedChunks = new List<GameObject>();

    void Start()
    {
        GameObject[] result = new GameObject[6];
        result[0] = chunks[0]; // شروع
        result[5] = chunks[5]; // پایان

        int[] middleIndices = { 1, 2, 3, 4 };

        // Fisher-Yates shuffle
        for (int i = middleIndices.Length - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            int temp = middleIndices[i];
            middleIndices[i] = middleIndices[rand];
            middleIndices[rand] = temp;
        }

        for (int i = 0; i < 4; i++)
        {
            result[i + 1] = chunks[middleIndices[i]];
        }

        Vector3 spawnPosition = Vector3.zero;
        for (int i = 0; i < result.Length; i++)
        {
            GameObject instantiatedChunk = Instantiate(result[i], spawnPosition, Quaternion.identity);
            instantiatedChunks.Add(instantiatedChunk);
            spawnPosition.x += 300f;
        }
    }
}