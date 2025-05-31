using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject player1;
    public GameObject player2;

    public Camera camera1; // دوربین پلیر اول
    public Camera camera2; // دوربین پلیر دوم
    public Camera mergeCamera; // دوربین ادغام‌شده

    public float mergeDistance = 5f;

    void Update()
    {
        if (player1 == null || player2 == null) return;

        float distance = Vector3.Distance(player1.transform.position, player2.transform.position);

        if (distance < mergeDistance)
        {
            EnableMergeCamera();
        }
        else
        {
            EnableSplitCameras();
        }
    }

    void EnableMergeCamera()
    {
        camera1.gameObject.SetActive(false);
        camera2.gameObject.SetActive(false);
        mergeCamera.gameObject.SetActive(true);

        // موقعیت دوربین طوری باشه که بین دو بازیکن باشه
        Vector3 center = (player1.transform.position + player2.transform.position) / 2f;
        mergeCamera.transform.position = new Vector3(center.x, center.y + 10f, center.z - 5f);
        mergeCamera.transform.LookAt(center);
    }

    void EnableSplitCameras()
    {
        camera1.gameObject.SetActive(true);
        camera2.gameObject.SetActive(true);
        mergeCamera.gameObject.SetActive(false);
    }
}