using TMPro;
using UnityEngine;

public class Symbol : MonoBehaviour
{
    public float speed;
    public bool isFirst;
    public float opacity;
    public float switchInterval;

    private float switchTimer;
    private TextMeshProUGUI textComponent;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        switchInterval = Random.Range(0.1f, 0.6f);
        switchTimer = 0f;
    }

    public void Initialize(float speed, bool isFirst, float opacity)
    {
        this.speed = speed;
        this.isFirst = isFirst;
        this.opacity = opacity;
        UpdateColor();
        SetToRandomSymbol();  // وقتی مقداردهی میشه، یک کاراکتر تصادفی هم تنظیم کن
    }

    void Update()
    {
        transform.localPosition += Vector3.down * speed * Time.deltaTime;

        if (transform.localPosition.y < -Screen.height / 2)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, Screen.height / 2, 0);
        }

        switchTimer += Time.deltaTime;
        if (switchTimer >= switchInterval)
        {
            SetToRandomSymbol();
            switchTimer = 0f;
        }
    }

    // تابعی که شما پرسیدی: برای تغییر کاراکتر به صورت تصادفی
    public void SetToRandomSymbol()
    {
        int charType = Random.Range(0, 6);
        if (charType > 1)
        {
            char c = (char)Random.Range(65, 91);  // A-Z
            textComponent.text = c.ToString();
        }
        else
        {
            textComponent.text = Random.Range(0, 10).ToString();
        }
    }

    void UpdateColor()
    {
        Color c = isFirst ? new Color(0.55f, 1f, 0.66f, opacity) : new Color(0f, 1f, 0.27f, opacity);
        textComponent.color = c;
    }
}