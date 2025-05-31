using UnityEngine;

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

        Vector3 center = (player1.transform.position + player2.transform.position) / 2f;
        mergeCamera.transform.position = new Vector3(center.x, center.y + cameraOffsetY, center.z + cameraOffsetZ);
        mergeCamera.transform.LookAt(center);
    }

    void EnableSplitCameras()
    {
        camera1.gameObject.SetActive(true);
        camera2.gameObject.SetActive(true);
        mergeCamera.gameObject.SetActive(false);

        // موقعیت دوربین‌ها نسبت به پلیرها
        Vector3 pos1 = player1.transform.position;
        Vector3 pos2 = player2.transform.position;

        camera1.transform.position = new Vector3(pos1.x, pos1.y + cameraOffsetY, pos1.z + cameraOffsetZ);
        camera1.transform.LookAt(pos1);

        camera2.transform.position = new Vector3(pos2.x, pos2.y + cameraOffsetY, pos2.z + cameraOffsetZ);
        camera2.transform.LookAt(pos2);

        // مشخص کردن کدام پلیر سمت راست‌تر است
        if (pos1.x > pos2.x)
        {
            camera1.rect = new Rect(0.5f, 0f, 0.5f, 1f); // پلیر1 سمت راست
            camera2.rect = new Rect(0f, 0f, 0.5f, 1f);   // پلیر2 سمت چپ
        }
        else
        {
            camera1.rect = new Rect(0f, 0f, 0.5f, 1f);   // پلیر1 سمت چپ
            camera2.rect = new Rect(0.5f, 0f, 0.5f, 1f); // پلیر2 سمت راست
        }
    }
}
