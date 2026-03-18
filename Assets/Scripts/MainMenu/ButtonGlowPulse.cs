using UnityEngine;
using UnityEngine.UI;

public class ButtonGlowPulse : MonoBehaviour
{
    public Shadow shadow;
    public float speed = 1f;
    private Color baseColor;

    void Start()
    {
        if (shadow == null) shadow = GetComponent<Shadow>();
        baseColor = shadow.effectColor;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed * Mathf.PI) + 1f) / 2f;
        shadow.effectColor = new Color(
            baseColor.r, baseColor.g, baseColor.b,
            Mathf.Lerp(0.3f, 0.7f, t));
    }
}
