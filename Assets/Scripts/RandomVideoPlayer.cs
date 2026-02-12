using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class RandomVideoPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;          // ویدیوپلیر یونیتی
    public string[] videoPaths;              // مسیر یا اسم ویدیوها (StreamingAssets)
    public CanvasGroup fadeCanvas;           // برای فید این/اوت
    public float fadeDuration = 1f;          // مدت زمان فید
    public float fadeBeforeEnd = 2f;         // چند ثانیه قبل از پایان ویدیو فید شروع بشه

    private void Start()
    {
        // مطمئن شو alpha اولیه درست است
        if (fadeCanvas != null)
            fadeCanvas.alpha = 0f;  // شفاف باشه اول بازی

        StartCoroutine(PlayRandomVideos());
    }

    private IEnumerator PlayRandomVideos()
    {
        while (true)
        {
            // انتخاب رندوم ویدیو
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath,
                videoPaths[Random.Range(0, videoPaths.Length)]);

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            videoPath = "file:///" + videoPath.Replace("\\", "/");
#endif

            videoPlayer.url = videoPath;
            videoPlayer.Play();

            // صبر کن تا ویدیو آماده شه
            while (!videoPlayer.isPrepared)
                yield return null;

            double videoLength = videoPlayer.length;

            // --- شروع ویدیو، شفاف باشه
            if (fadeCanvas != null)
                fadeCanvas.alpha = 0f;

            // صبر کن تا نزدیک پایان ویدیو
            float waitTime = Mathf.Max(0f, (float)videoLength - fadeBeforeEnd);
            yield return new WaitForSeconds(waitTime);

            // fade-out: از شفاف -> سیاه
            if (fadeCanvas != null)
                yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

            // 🔹 اینجا دیگر videoPlayer.Stop() حذف شد
            // و fade-in ویدیوی بعدی بلافاصله بعد از fade-out اجرا میشه
            if (fadeCanvas != null)
                yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

            yield return null;
        }
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeCanvas == null)
            yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        fadeCanvas.alpha = to;
    }
}
