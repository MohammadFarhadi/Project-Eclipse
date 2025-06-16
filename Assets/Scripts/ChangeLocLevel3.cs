using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChangeLocLevel3 : MonoBehaviour
{
    [Header("Camera & Players")]
    [SerializeField] private float cameraTargetSize = 8.5f;
    [SerializeField] private float transitionTime = 1f;

    private GameObject[] players;
    [SerializeField] private GameObject Player1, Player2;
    private Camera mainCamera;

    public Transform player1_Pos, player2_Pos;
    public Transform CameraPos;

    [Header("UI Panels")]
    public RectTransform leftPanel;
    public RectTransform rightPanel;

    private bool hasTriggered = false;

    void Start()
    {
        mainCamera = Camera.main;

        // گرفتن پلیرها
        players = GameObject.FindGameObjectsWithTag("Player");
        Player1 = players[0];
        Player2 = players[1];
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(TransitionSequence());
        }
    }

    private IEnumerator TransitionSequence()
    {
        // 1. بسته شدن پنل‌ها
        yield return StartCoroutine(ClosePanels());

        // 2. جابه‌جایی پلیر و دوربین
        Player1.transform.position = player1_Pos.position;
        Player2.transform.position = player2_Pos.position;
        mainCamera.transform.position = CameraPos.position;
        mainCamera.orthographicSize = cameraTargetSize;

        yield return new WaitForSeconds(0.2f);

        // 3. باز شدن پنل‌ها
        yield return StartCoroutine(OpenPanels());
    }

    private IEnumerator ClosePanels()
    {
        float t = 0f;
        Vector2 leftStart = new Vector2(-leftPanel.rect.width, 0);
        Vector2 rightStart = new Vector2(rightPanel.rect.width, 0);
        Vector2 center = Vector2.zero;

        while (t < 1f)
        {
            t += Time.deltaTime / transitionTime;
            leftPanel.anchoredPosition = Vector2.Lerp(leftStart, center, t);
            rightPanel.anchoredPosition = Vector2.Lerp(rightStart, center, t);
            yield return null;
        }
    }

    private IEnumerator OpenPanels()
    {
        float t = 0f;
        Vector2 leftEnd = new Vector2(-leftPanel.rect.width, 0);
        Vector2 rightEnd = new Vector2(rightPanel.rect.width, 0);
        Vector2 center = Vector2.zero;

        while (t < 1f)
        {
            t += Time.deltaTime / transitionTime;
            leftPanel.anchoredPosition = Vector2.Lerp(center, leftEnd, t);
            rightPanel.anchoredPosition = Vector2.Lerp(center, rightEnd, t);
            yield return null;
        }
    }
}
