using UnityEngine;

public class SparkleAnim : MonoBehaviour
{
    public float delay = 0f;
    public float speed = 1f;
    private CanvasGroup cg;
    private RectTransform rt;
    private Vector3 startScale;

    void Start()
    {
        cg = gameObject.AddComponent<CanvasGroup>();
        rt = GetComponent<RectTransform>();
        startScale = rt.localScale;
    }

    void Update()
    {
        float t = Mathf.Sin((Time.time + delay) * speed * Mathf.PI);
        cg.alpha = Mathf.Lerp(0.3f, 1f, (t + 1f) / 2f);
        float s = Mathf.Lerp(0.8f, 1.2f, (t + 1f) / 2f);
        rt.localScale = startScale * s;
        rt.Rotate(0, 0, 20f * Time.deltaTime * t);
    }
}
