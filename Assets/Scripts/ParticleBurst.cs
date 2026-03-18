using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Random = UnityEngine.Random;
using Image = UnityEngine.UI.Image;

/// <summary>
/// Attach to an empty GameObject inside the Canvas.
/// Call Burst() on correct answers.
/// Spawns sprite particles that float upward and fade out.
/// </summary>
public class ParticleBurst : MonoBehaviour
{
    [Header("Particle Settings")]
    [SerializeField] private int particleCount = 6;
    [SerializeField] private float spreadX = 200f;
    [SerializeField] private float riseHeight = 180f;
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private float spawnRadius = 80f;

    [Header("Sprites — drag your star/sparkle PNGs here")]
    [SerializeField] private Sprite[] particleSprites; // drag 2-3 star sprites in Inspector

    [Header("Colors")]
    [SerializeField]
    private Color[] particleColors = new Color[]
    {
        new Color(0.99f, 0.83f, 0.30f, 1f), // gold   #FCD34D
        new Color(0.67f, 0.36f, 0.98f, 1f), // purple #A855F7
        new Color(0.20f, 0.83f, 0.60f, 1f), // green  #34D399
        new Color(1.00f, 1.00f, 1.00f, 1f), // white
    };

    // ─────────────────────────────────────────────
    // Call this when player answers correctly
    // ─────────────────────────────────────────────
    public void Burst()
    {
        for (int i = 0; i < particleCount; i++)
        {
            StartCoroutine(SpawnParticle(i * 0.08f));
        }
    }

    IEnumerator SpawnParticle(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Create Image GameObject
        GameObject obj = new GameObject("Particle");
        obj.transform.SetParent(transform, false);

        Image img = obj.AddComponent<Image>();

        // Use a sprite if provided, otherwise use Unity's default white circle
        if (particleSprites != null && particleSprites.Length > 0)
            img.sprite = particleSprites[Random.Range(0, particleSprites.Length)];
        else
            img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        // Random color from palette
        Color col = particleColors[Random.Range(0, particleColors.Length)];
        img.color = col;

        // Add canvas group for fading
        CanvasGroup cg = obj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        // Random size
        float size = Random.Range(18f, 36f);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(size, size);

        // Random start position around center
        rt.anchoredPosition = new Vector2(
            Random.Range(-spawnRadius, spawnRadius),
            Random.Range(-spawnRadius * 0.3f, spawnRadius * 0.3f)
        );

        // Random horizontal drift
        float driftX = Random.Range(-spreadX * 0.5f, spreadX * 0.5f);

        // Animate
        float t = 0f;
        Vector2 startPos = rt.anchoredPosition;
        float startRot = Random.Range(-30f, 30f);
        float endRot = startRot + Random.Range(-60f, 60f);

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            // Position — float upward with drift
            rt.anchoredPosition = new Vector2(
                startPos.x + driftX * progress,
                startPos.y + riseHeight * progress
            );

            // Rotation — spin slightly
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(startRot, endRot, progress));

            // Scale — pop up then shrink
            float scale = progress < 0.2f
                ? Mathf.Lerp(0.2f, 1.3f, progress / 0.2f)
                : Mathf.Lerp(1.3f, 0.6f, (progress - 0.2f) / 0.8f);
            rt.localScale = new Vector3(scale, scale, 1f);

            // Fade out in last 40%
            cg.alpha = progress > 0.6f
                ? Mathf.Lerp(1f, 0f, (progress - 0.6f) / 0.4f)
                : 1f;

            yield return null;
        }

        Destroy(obj);
    }
}