using UnityEngine;
using System.Collections;

public class TrampolineSpawnButton : MonoBehaviour
{
    [Header("Button Movement")]
    public Transform buttonObject;
    public Vector3 buttonPressedOffset = new Vector3(0, -0.1f, 0);
    public float moveDuration = 0.3f;

    [Header("Trampoline Settings")]
    private bool hasExitedOnce = false;
    public GameObject trampolinePrefab;
    public Transform spawnLocation;
    public float riseOffsetBelow = 1f;
    public float riseDuration = 0.3f;
    
    public float hightOffsetBelow = 0.5f;

    private Vector3 originalButtonPos;
    private Coroutine buttonMoveCoroutine;
    private GameObject spawnedTrampoline;
    private bool isPressed = false;

    void Start()
    {
        originalButtonPos = buttonObject.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isPressed) return;
        isPressed = true;

        if (buttonMoveCoroutine != null)
            StopCoroutine(buttonMoveCoroutine);

        buttonMoveCoroutine = StartCoroutine(MoveButton(originalButtonPos, originalButtonPos + buttonPressedOffset));

        // اگه قبلاً از دکمه خارج شده بودیم، ترامپولین قبلی رو حذف کن
        if (hasExitedOnce && spawnedTrampoline != null)
        {
            StartCoroutine(DestroyTrampolineWithAnimation(spawnedTrampoline));
            hasExitedOnce = false;
        }

        Vector3 spawnPosBelow = spawnLocation.position - new Vector3(0, riseOffsetBelow, 0);
        Vector3 finalPos = spawnLocation.position + new Vector3(0,hightOffsetBelow , 0); 

        spawnedTrampoline = Instantiate(trampolinePrefab, spawnPosBelow, Quaternion.identity);
        StartCoroutine(RiseTrampoline(spawnedTrampoline.transform, spawnPosBelow, finalPos));
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !isPressed) return;
        isPressed = false;

        if (buttonMoveCoroutine != null)
            StopCoroutine(buttonMoveCoroutine);

        buttonMoveCoroutine = StartCoroutine(MoveButton(buttonObject.position, originalButtonPos));

        hasExitedOnce = true;
    }


    private IEnumerator MoveButton(Vector3 from, Vector3 to)
    {
        float timer = 0f;

        while (timer < moveDuration)
        {
            float t = timer / moveDuration;
            buttonObject.position = Vector3.Lerp(from, to, t);
            timer += Time.deltaTime;
            yield return null;
        }

        buttonObject.position = to;
    }

    private IEnumerator RiseTrampoline(Transform trampoline, Vector3 from, Vector3 to)
    {
        float timer = 0f;

        while (timer < riseDuration)
        {
            float t = timer / riseDuration;
            trampoline.position = Vector3.Lerp(from, to, t);
            timer += Time.deltaTime;
            yield return null;
        }

        trampoline.position = to;
    }
    private IEnumerator DestroyTrampolineWithAnimation(GameObject trampoline)
    {
        Transform t = trampoline.transform;
        SpriteRenderer sr = trampoline.GetComponent<SpriteRenderer>();

        float duration = 1f;
        float timer = 0f;

        Vector3 startScale = t.localScale;
        Vector3 endScale = Vector3.zero;

        Color startColor = sr != null ? sr.color : Color.white;
        Color endColor = startColor;
        endColor.a = 0f;

        while (timer < duration)
        {
            float tVal = timer / duration;

            // Shrink
            t.localScale = Vector3.Lerp(startScale, endScale, tVal);

            // Fade out
            if (sr != null)
                sr.color = Color.Lerp(startColor, endColor, tVal);

            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure it's gone
        Destroy(trampoline);
    }


}
