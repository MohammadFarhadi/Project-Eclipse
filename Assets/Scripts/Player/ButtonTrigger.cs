using UnityEngine;
using System.Collections;

public class ButtonTrigger : MonoBehaviour
{
    public Transform buttonObject;             // دکمه که پایین میره (فقط ظاهر)
    public Transform targetObject;             // آبجکت متحرک مثل پل یا در
    public Transform targetMoveDestination;    // مقصد targetObject
    public Vector3 buttonPressedOffset = new Vector3(0, -0.1f, 0);
    public float moveDuration = 1f;

    private Vector3 originalButtonPos;
    private Vector3 originalTargetPos;

    private Coroutine moveCoroutine;
    private Coroutine returnCoroutine;
    private bool isPressed = false;

    void Start()
    {
        originalButtonPos = buttonObject.position;
        originalTargetPos = targetObject.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (isPressed) return; // نذار دوباره فعال بشه

        isPressed = true;

        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveObjects(
            buttonObject.position,
            originalButtonPos + buttonPressedOffset,
            targetObject.position,
            targetMoveDestination.position
        ));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (!isPressed) return;

        isPressed = false;

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        returnCoroutine = StartCoroutine(MoveObjects(
            buttonObject.position,
            originalButtonPos,
            targetObject.position,
            originalTargetPos
        ));
    }

    private IEnumerator MoveObjects(Vector3 btnStart, Vector3 btnEnd, Vector3 tgtStart, Vector3 tgtEnd)
    {
        float timer = 0f;

        while (timer < moveDuration)
        {
            float t = timer / moveDuration;
            buttonObject.position = Vector3.Lerp(btnStart, btnEnd, t);
            targetObject.position = Vector3.Lerp(tgtStart, tgtEnd, t);
            timer += Time.deltaTime;
            yield return null;
        }

        buttonObject.position = btnEnd;
        targetObject.position = tgtEnd;
    }
}
