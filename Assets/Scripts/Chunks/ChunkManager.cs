using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public GameObject[] chunks = new GameObject[6]; // index 0 = start, index 5 = end

    void Start()
    {
        GameObject[] result = new GameObject[6];

        // قرار دادن اولین و آخرین چانک
        result[0] = chunks[0];
        result[5] = chunks[5];

        // آرایه‌ای از ایندکس‌های چانک‌های وسط (1 تا 4)
        int[] middleIndices = { 1, 2, 3, 4 };

        // شافل کردن ایندکس‌ها با الگوریتم Fisher-Yates
        for (int i = middleIndices.Length - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            int temp = middleIndices[i];
            middleIndices[i] = middleIndices[rand];
            middleIndices[rand] = temp;
        }

        // اضافه کردن چانک‌های وسط به result
        for (int i = 0; i < 4; i++)
        {
            result[i + 1] = chunks[middleIndices[i]];
        }

        // نمونه‌سازی چانک‌ها در صحنه
        Vector3 spawnPosition = Vector3.zero;
        for (int i = 0; i < result.Length; i++)
        {
            Instantiate(result[i], spawnPosition, Quaternion.identity);
            spawnPosition.x += 10f; // فاصله بین چانک‌ها (بسته به سایز چانک)
        }
    }
}