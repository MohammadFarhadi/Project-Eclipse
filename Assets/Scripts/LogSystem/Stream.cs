using System.Collections.Generic;
using UnityEngine;

public class Stream : MonoBehaviour
{
    public GameObject symbolPrefab;  // Prefab کاراکتر (TextMeshPro)
    public int totalSymbols;
    public float speed;

    private List<Symbol> symbols = new List<Symbol>();
    private float symbolSize = 25f;

    public void GenerateStream(float xPosition, float startY)
    {
        speed = Random.Range(50f, 100f);
        totalSymbols = Random.Range(5, 35);

        float opacity = 1f;
        bool first = Random.Range(0, 5) == 1;

        float y = startY;

        for (int i = 0; i < totalSymbols; i++)
        {
            GameObject symbolGO = Instantiate(symbolPrefab, this.transform);
            symbolGO.transform.localPosition = new Vector3(xPosition, y, 0);
            Symbol symbol = symbolGO.GetComponent<Symbol>();
            symbol.Initialize(speed, first, opacity);
            symbol.SetToRandomSymbol();

            symbols.Add(symbol);

            opacity -= (1f / totalSymbols) / 1.6f;
            y += symbolSize;  // بالا می‌رویم چون مختصات ی در یونیتی به بالا است
            first = false;
        }
    }
}