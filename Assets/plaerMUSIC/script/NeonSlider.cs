using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class RunningDotsSlider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Running Dots Settings")]
    [SerializeField] private Color dotsColor = new Color(0.2f, 1f, 0.3f, 1f);
    [SerializeField] private int dotsCount = 5;
    [SerializeField] private float dotsSpeed = 2f;
    [SerializeField] private float dotsSize = 8f;

    private Slider slider;
    private Image fillImage;
    private RectTransform[] dots;
    private bool isHovered = false;
    private Coroutine dotsCoroutine;

    void Start()
    {
        slider = GetComponent<Slider>();
        fillImage = transform.Find("Fill Area/Fill")?.GetComponent<Image>();

        CreateDots();
        HideDots();
    }

    void CreateDots()
    {
        dots = new RectTransform[dotsCount];

        for (int i = 0; i < dotsCount; i++)
        {
            GameObject dotObj = new GameObject($"Dot_{i}");
            RectTransform dotTransform = dotObj.AddComponent<RectTransform>();
            dotTransform.SetParent(fillImage?.transform ?? transform);
            dotTransform.localScale = Vector3.one;
            dotTransform.sizeDelta = new Vector2(dotsSize, dotsSize);
            dotTransform.pivot = new Vector2(0, 0.5f);

            Image dotImage = dotObj.AddComponent<Image>();
            dotImage.color = dotsColor;
            dotImage.sprite = CreateCircleSprite();

            dots[i] = dotTransform;
        }
    }

    Sprite CreateCircleSprite()
    {
        int size = 16;
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size / 2, size / 2);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = 1f - Mathf.Clamp01(dist / (size / 2));
                colors[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }

        texture.SetPixels(colors);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        ShowDots();
        dotsCoroutine = StartCoroutine(AnimateDots());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        HideDots();

        if (dotsCoroutine != null)
            StopCoroutine(dotsCoroutine);
    }

    void ShowDots()
    {
        foreach (RectTransform dot in dots)
        {
            if (dot != null)
                dot.gameObject.SetActive(true);
        }
    }

    void HideDots()
    {
        foreach (RectTransform dot in dots)
        {
            if (dot != null)
                dot.gameObject.SetActive(false);
        }
    }

    IEnumerator AnimateDots()
    {
        float[] dotOffsets = new float[dotsCount];
        for (int i = 0; i < dotsCount; i++)
        {
            dotOffsets[i] = Random.Range(0f, 1f);
        }

        while (isHovered)
        {
            if (fillImage != null && slider != null)
            {
                Rect fillRect = fillImage.rectTransform.rect;
                float fillWidth = fillRect.width * slider.value;

                for (int i = 0; i < dotsCount; i++)
                {
                    if (dots[i] != null)
                    {
                        // Анимация движения точек
                        dotOffsets[i] += Time.deltaTime * dotsSpeed;
                        if (dotOffsets[i] > 1f) dotOffsets[i] = 0f;

                        float xPos = dotOffsets[i] * fillWidth;
                        dots[i].localPosition = new Vector3(xPos, 0, 0);

                        // Пульсация альфа-канала
                        float alpha = 0.5f + Mathf.Sin(Time.time * dotsSpeed + i) * 0.5f;
                        Image dotImage = dots[i].GetComponent<Image>();
                        if (dotImage != null)
                        {
                            Color color = dotImage.color;
                            color.a = alpha;
                            dotImage.color = color;
                        }
                    }
                }
            }

            yield return null;
        }
    }
}