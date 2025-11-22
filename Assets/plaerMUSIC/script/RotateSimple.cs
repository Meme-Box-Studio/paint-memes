using UnityEngine;

public class RotateSimple : MonoBehaviour
{
    public float rotationSpeed = 50f;
    public float pulseSpeed = 2f;
    public float pulseSize = 0.2f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Вращение
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Пульсация
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseSize;
        transform.localScale = originalScale * (1 + pulse);
    }
}