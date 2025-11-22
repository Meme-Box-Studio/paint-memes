// VisualizerController.cs
using UnityEngine;
using UnityEngine.UI;

public class VisualizerController : MonoBehaviour
{
    public GameObject barPrefab;
    public int barCount = 64;
    public float maxBarHeight = 100f;
    public float sensitivity = 50f;

    private GameObject[] bars;
    private RectTransform containerRT;

    void Start()
    {
        containerRT = GetComponent<RectTransform>();
        CreateBars();
    }

    void CreateBars()
    {
        bars = new GameObject[barCount];

        for (int i = 0; i < barCount; i++)
        {
            bars[i] = Instantiate(barPrefab, transform);
            RectTransform barRT = bars[i].GetComponent<RectTransform>();

            // Настройка позиции и размера
            float barWidth = containerRT.rect.width / barCount;
            barRT.sizeDelta = new Vector2(barWidth - 2, 10); // -2 для промежутков
            barRT.anchoredPosition = new Vector2(
                i * barWidth + barWidth / 2,
                0
            );

            // Начальная высота
            barRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 10);
        }
    }

    void Update()
    {
        UpdateVisualizer();
    }

    void UpdateVisualizer()
    {
        float[] spectrum = new float[barCount];
        AudioListener.GetSpectrumData(spectrum, 0, UnityEngine.FFTWindow.Rectangular);

        for (int i = 0; i < barCount; i++)
        {
            if (bars[i] != null)
            {
                RectTransform barRT = bars[i].GetComponent<RectTransform>();
                float targetHeight = Mathf.Clamp(spectrum[i] * sensitivity * maxBarHeight, 10, maxBarHeight);

                // Плавное изменение высоты
                float currentHeight = barRT.rect.height;
                float newHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * 10f);

                barRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);

                // Изменение цвета в зависимости от высоты
                Image barImage = bars[i].GetComponent<Image>();
                float colorValue = newHeight / maxBarHeight;
                barImage.color = Color.Lerp(Color.blue, Color.red, colorValue);
            }
        }
    }
}