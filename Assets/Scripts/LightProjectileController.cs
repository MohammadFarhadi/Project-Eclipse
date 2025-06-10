// LightProjectileController.cs
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

[RequireComponent(typeof(Light2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(ParticleSystem))] // ParticleSystem هم روی همین Prefab باشه
public class LightProjectileController : MonoBehaviour
{
    [Header("Timings")]
    [Tooltip("مجموع عمر پرتابه (ثانیه)")]
    public float lifeTime        = 5f;
    [Tooltip("مقدار ثانیه‌ای که در انتها نور سریع کم می‌شود")]
    public float fadeDuration    = 2f;

    [Header("Light Settings")]
    public float initialIntensity      = 1f;
    public float innerRadius           = 0.2f;
    public float outerRadius           = 5f;

    [Header("Particle Settings")]
    [Tooltip("Emission rate of the particle system")]
    public float emissionRate          = 30f;

    [Header("Collision Settings")]
    [Tooltip("زمین یا هر چیزی که می‌خوای برخورد کنه")]
    public LayerMask groundLayer;

    Light2D         projLight;
    SpriteRenderer  spriteRend;
    ParticleSystem  ps;
    ParticleSystem.MainModule         psMain;
    ParticleSystem.ColorOverLifetimeModule psColorOL;
    CircleCollider2D col;
    bool            hasCollided;

    void Awake()
    {
        // Light2D
        projLight = GetComponent<Light2D>();
        projLight.lightType        = Light2D.LightType.Point;
        projLight.shadowIntensity  = 0f;
        projLight.falloffIntensity = 1f;

        // SpriteRenderer
        spriteRend = GetComponent<SpriteRenderer>();

        // Collider (برای برخورد با زمین)
        col = GetComponent<CircleCollider2D>();
        col.isTrigger = false;

        // ParticleSystem
        ps = GetComponent<ParticleSystem>();
        psMain = ps.main;
        var emission = ps.emission;
        emission.rateOverTime = emissionRate;

        psColorOL = ps.colorOverLifetime;
        psColorOL.enabled = false;
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void OnEnable()
    {
        hasCollided = false;
        // pick random color
        Color rand = Random.ColorHSV(0f,1f,0.7f,1f,0.8f,1f);

        // sprite tint
        spriteRend.color = rand;

        // setup light
        projLight.color                   = rand;
        projLight.intensity               = initialIntensity;
        projLight.pointLightInnerRadius  = innerRadius;
        projLight.pointLightOuterRadius  = outerRadius;

        // schedule life+fade
        StartCoroutine(Lifecycle());
    }

    private IEnumerator Lifecycle()
    {
        // 1) صبر کن تا نزدیک انتها
        yield return new WaitForSeconds(lifeTime - fadeDuration);
        // 2) شروع فید نور
        StartCoroutine(FadeLight());
        // 3) بعد از اتمام عمر، غیرفعال شو
        yield return new WaitForSeconds(fadeDuration);
        gameObject.SetActive(false);
    }

    private IEnumerator FadeLight()
    {
        float startInt = initialIntensity;
        float t = 0f;
        while (t < fadeDuration)
        {
            projLight.intensity = Mathf.Lerp(startInt, 0f, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        projLight.intensity = 0f;
    }

    void OnCollisionEnter2D(Collision2D colInfo)
    {
        if (hasCollided) return;
        if (((1 << colInfo.gameObject.layer) & groundLayer) != 0)
        {
            hasCollided = true;

            // play particles on impact
            psColorOL.enabled = true;
            Gradient g = new Gradient();
            Color c = spriteRend.color;
            g.SetKeys(
                new [] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                new [] { new GradientAlphaKey(1f, 0f),   new GradientAlphaKey(0f, 1f) }
            );
            psColorOL.color = new ParticleSystem.MinMaxGradient(g);

            ps.Clear();
            ps.Play();
        }
    }
}
