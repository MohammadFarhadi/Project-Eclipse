using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MatrixRainEffect : MonoBehaviour
{
    [Header("Settings")]
    public RectTransform rainContainer;
    public GameObject charPrefab;
    public int columns = 30;
    public int charactersPerColumn = 20;
    public float columnSpacing = 20f;
    public float fallSpeed = 50f;
    public float loopHeight = 300f;

    private List<List<TextMeshProUGUI>> matrixColumns = new();

    void Start()
    {
        GenerateColumns();
    }

    void Update()
    {
        AnimateRain();
    }

    void GenerateColumns()
    {
        for (int i = 0; i < columns; i++)
        {
            var column = new List<TextMeshProUGUI>();
            for (int j = 0; j < charactersPerColumn; j++)
            {
                GameObject ch = Instantiate(charPrefab, rainContainer);
                var rect = ch.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(i * columnSpacing, Random.Range(0, loopHeight));
                var text = ch.GetComponent<TextMeshProUGUI>();
                text.text = GetRandomChar();
                text.fontSize = Random.Range(14, 20);
                column.Add(text);
            }
            matrixColumns.Add(column);
        }
    }

    void AnimateRain()
    {
        foreach (var column in matrixColumns)
        {
            foreach (var text in column)
            {
                var rect = text.GetComponent<RectTransform>();
                rect.anchoredPosition += Vector2.down * fallSpeed * Time.deltaTime;

                // loop to top
                if (rect.anchoredPosition.y < -loopHeight)
                {
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, loopHeight);
                    text.text = GetRandomChar(); // refresh char occasionally
                }
            }
        }
    }

    string GetRandomChar()
    {
        const string charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%&";
        return charset[Random.Range(0, charset.Length)].ToString();
    }
}
