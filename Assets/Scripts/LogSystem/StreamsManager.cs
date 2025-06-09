using UnityEngine;

public class MatrixController : MonoBehaviour
{
    public GameObject streamPrefab;  // آبجکت خالی با اسکریپت Stream
    public GameObject symbolPrefab;  // Prefab TextMeshPro برای Symbol
    public int screenWidth = 1980;
    public int screenHeight = 1080;
    public float symbolSize = 25f;

    void Start()
    {
        int numberOfStreams = Mathf.CeilToInt(screenWidth / symbolSize);
        float x = -screenWidth / 2;

        for (int i = 0; i < numberOfStreams; i++)
        {
            GameObject streamGO = Instantiate(streamPrefab, transform);
            Stream stream = streamGO.GetComponent<Stream>();
            stream.symbolPrefab = symbolPrefab;
            float startY = Random.Range(-screenHeight, 0);
            stream.GenerateStream(x, startY);

            x += symbolSize;
        }
    }
}