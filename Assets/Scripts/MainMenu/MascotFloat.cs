using UnityEngine;

public class MascotFloat : MonoBehaviour
{
    public float floatHeight = 22f;
    public float floatSpeed = 1.05f;
    public float rotateAngle = 1f;
    private Vector3 startPos;

    void Start() => startPos = transform.localPosition;

    void Update()
    {
        float t = Mathf.Sin(Time.time * floatSpeed);
        transform.localPosition = startPos + Vector3.up * (t * floatHeight);
        transform.localRotation = Quaternion.Euler(0, 0, t * rotateAngle);
    }
}