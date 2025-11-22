using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SimpleRunningLights : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Light Settings")]
    [SerializeField] private Color lightColor = new Color(0.2f, 1f, 0.3f, 1f);
    [SerializeField] private int lightCount = 8;
    [SerializeField] private float lightSize = 10f;
    [SerializeField] private float rotationSpeed = 180f;

    private RectTransform[] lights;
    private bool isHovered = false;
    private Coroutine rotationCoroutine;

    void Start()
    {
        CreateLights();
        // Сразу скрываем все точки при старте
        ShowLights(false);
    }

    void CreateLights()
    {
        lights = new RectTransform[lightCount];

        for (int i = 0; i < lightCount; i++)
        {
            GameObject lightObj = new GameObject($"Light_{i}");
            RectTransform lightTransform = lightObj.AddComponent<RectTransform>();
            lightTransform.SetParent(transform);
            lightTransform.localScale = Vector3.one;
            lightTransform.sizeDelta = new Vector2(lightSize, lightSize);

            Image lightImage = lightObj.AddComponent<Image>();
            lightImage.color = lightColor;

            // Создаем простой спрайт круга
            lightImage.sprite = CreateCircleSprite();

            // Сразу делаем неактивными
            lightObj.SetActive(false);

            lights[i] = lightTransform;
        }

        PositionLights();
    }

    Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size / 2, size / 2);
        float radius = size / 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = 1f - Mathf.Clamp01(dist / radius);
                colors[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }

        texture.SetPixels(colors);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void PositionLights()
    {
        if (lights == null) return;

        RectTransform rect = GetComponent<RectTransform>();
        float width = rect.rect.width;
        float height = rect.rect.height;

        for (int i = 0; i < lights.Length; i++)
        {
            float angle = i * (360f / lights.Length);
            PositionLight(lights[i], angle, width, height);
        }
    }

    void PositionLight(RectTransform light, float angle, float width, float height)
    {
        float rad = angle * Mathf.Deg2Rad;
        float x = Mathf.Cos(rad) * (width / 2 + lightSize / 2);
        float y = Mathf.Sin(rad) * (height / 2 + lightSize / 2);

        light.localPosition = new Vector3(x, y, 0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        ShowLights(true);
        rotationCoroutine = StartCoroutine(RotateLights());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        ShowLights(false);

        if (rotationCoroutine != null)
            StopCoroutine(rotationCoroutine);
    }

    void ShowLights(bool show)
    {
        if (lights == null) return;

        foreach (RectTransform light in lights)
        {
            if (light != null && light.gameObject != null)
                light.gameObject.SetActive(show);
        }
    }

    IEnumerator RotateLights()
    {
        float currentAngle = 0f;

        while (isHovered)
        {
            currentAngle += Time.deltaTime * rotationSpeed;
            if (currentAngle >= 360f) currentAngle = 0f;

            RectTransform rect = GetComponent<RectTransform>();
            float width = rect.rect.width;
            float height = rect.rect.height;

            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    float angle = currentAngle + i * (360f / lights.Length);
                    PositionLight(lights[i], angle, width, height);
                }
            }

            yield return null;
        }
    }

    // Добавляем метод для принудительного скрытия (на всякий случай)
    [ContextMenu("Скрыть все точки")]
    public void HideAllLights()
    {
        ShowLights(false);
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }
        isHovered = false;
    }

    [ContextMenu("Показать все точки")]
    public void ShowAllLights()
    {
        ShowLights(true);
    }

    void OnDestroy()
    {
        // Очистка при уничтожении объекта
        if (rotationCoroutine != null)
            StopCoroutine(rotationCoroutine);
    }
}