using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    public GameObject player1;
    public GameObject player2;

    public Camera camera1;
    public Camera camera2;
    public Camera mergeCamera;

    public float mergeDistance = 5f;
    public float cameraOffsetY = 10f;
    public float cameraOffsetZ = -5f;
    
    [Header("Smooth Settings")]
    public float smoothSpeed = 5f; // سرعت حرکت/چرخش دوربین
    public float rectSmoothSpeed = 2f; // سرعت نرم شدن تغییر rect

    public Image splitLine; // خط جداکننده (UI Image وسط صفحه)

    private Rect targetRect1;
    private Rect targetRect2;

    void Update()
    {
        if (player1 == null || player2 == null)
        {
            player1 = GameObject.Find("Melle1Player(Clone)") ?? GameObject.Find("Melle2Player(Clone)");
            player2 = GameObject.Find("Ranged1Player(Clone)") ?? GameObject.Find("RangedPlayer(Clone)");
        }
        if (camera1 == null || camera2 == null)
        {
            camera1 = player1.GetComponentInChildren<Camera>();
            camera2 = player2.GetComponentInChildren<Camera>();
        }

        float distance = Vector3.Distance(player1.transform.position, player2.transform.position);

        if (distance < mergeDistance)
        {
            EnableMergeCamera();
        }
        else
        {
            EnableSplitCameras();
        }

        // نرم کردن تغییرات rect با سرعت جدا
        camera1.rect = SmoothRect(camera1.rect, targetRect1, rectSmoothSpeed);
        camera2.rect = SmoothRect(camera2.rect, targetRect2, rectSmoothSpeed);
    }

    void EnableMergeCamera()
    {
        targetRect1 = new Rect(0f, 0f, 1f, 1f);
        targetRect2 = new Rect(0f, 0f, 1f, 1f);

        camera1.gameObject.SetActive(false);
        camera2.gameObject.SetActive(false);
        mergeCamera.gameObject.SetActive(true);

        if (splitLine != null) splitLine.gameObject.SetActive(false);

        Vector3 center = (player1.transform.position + player2.transform.position) / 2f;
        Vector3 targetPos = new Vector3(center.x, center.y + cameraOffsetY, center.z + cameraOffsetZ);
        mergeCamera.transform.position = Vector3.Lerp(mergeCamera.transform.position, targetPos, Time.deltaTime * smoothSpeed);
        mergeCamera.transform.LookAt(center);
    }

    void EnableSplitCameras()
    {
        camera1.gameObject.SetActive(true);
        camera2.gameObject.SetActive(true);
        mergeCamera.gameObject.SetActive(false);

        if (splitLine != null) splitLine.gameObject.SetActive(true);

        Vector3 pos1 = player1.transform.position;
        Vector3 pos2 = player2.transform.position;

        Vector3 targetPos1 = new Vector3(pos1.x, pos1.y + cameraOffsetY, pos1.z + cameraOffsetZ);
        Vector3 targetPos2 = new Vector3(pos2.x, pos2.y + cameraOffsetY, pos2.z + cameraOffsetZ);

        camera1.transform.position = Vector3.Lerp(camera1.transform.position, targetPos1, Time.deltaTime * smoothSpeed);
        camera2.transform.position = Vector3.Lerp(camera2.transform.position, targetPos2, Time.deltaTime * smoothSpeed);

        camera1.transform.LookAt(pos1);
        camera2.transform.LookAt(pos2);

        if (pos1.x > pos2.x)
        {
            targetRect1 = new Rect(0.5f, 0f, 0.5f, 1f);
            targetRect2 = new Rect(0f, 0f, 0.5f, 1f);
        }
        else
        {
            targetRect1 = new Rect(0f, 0f, 0.5f, 1f);
            targetRect2 = new Rect(0.5f, 0f, 0.5f, 1f);
        }
    }

    Rect SmoothRect(Rect current, Rect target, float speed)
    {
        return new Rect(
            Mathf.Lerp(current.x, target.x, Time.deltaTime * speed),
            Mathf.Lerp(current.y, target.y, Time.deltaTime * speed),
            Mathf.Lerp(current.width, target.width, Time.deltaTime * speed),
            Mathf.Lerp(current.height, target.height, Time.deltaTime * speed)
        );
    }
}
