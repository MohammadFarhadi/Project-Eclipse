using UnityEngine;

public class ImageFollower : MonoBehaviour
{
    [System.Serializable]
    public class Pair
    {
        public GameObject targetObject;  // گیم‌ابجکت اصلی
        public GameObject imageObject;   // عکس مربوطه (می‌تونه همون Image UI یا هر آبجکت تصویری باشه)
    }

    public Pair[] pairs;

    void Update()
    {
        foreach (var pair in pairs)
        {
            if (pair.targetObject != null && pair.imageObject != null)
            {
                pair.imageObject.SetActive(pair.targetObject.activeSelf);
            }
        }
    }
}