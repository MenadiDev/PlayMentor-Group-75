using UnityEngine;

public class FloatingAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float floatHeight = 20f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float animationDelay = 0f;

    private Vector3 startPosition;
    private float randomOffset;

    void Start()
    {
        startPosition = transform.localPosition;
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        float newY = startPosition.y + Mathf.Sin((Time.time + animationDelay) * floatSpeed + randomOffset) * floatHeight;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
