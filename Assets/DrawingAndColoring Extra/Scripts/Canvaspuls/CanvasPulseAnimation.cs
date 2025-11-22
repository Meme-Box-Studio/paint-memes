using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanvasPulseAnimation : MonoBehaviour
{
    [Header("Pulse Animation Settings")]
    public float pulseIntensity = 0.05f;        // Сила импульса (5%)
    public float pulseSpeed = 1.5f;             // Скорость анимации
    public float pulseInterval = 3f;            // Интервал между импульсами
    public AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scale Settings")]
    public Vector3 minScale = Vector3.one * 0.98f;
    public Vector3 maxScale = Vector3.one * 1.05f;
    public bool randomizePulses = true;

    private Canvas canvas;
    private RectTransform canvasRect;
    private Vector3 originalScale;
    private bool isAnimating = true;

    void Start()
    {
        canvas = GetComponent<Canvas>();
        canvasRect = GetComponent<RectTransform>();
        originalScale = canvasRect.localScale;

        StartCoroutine(PulseRoutine());
    }

    IEnumerator PulseRoutine()
    {
        while (isAnimating)
        {
            // Ждем перед следующим импульсом
            if (randomizePulses)
            {
                yield return new WaitForSeconds(pulseInterval + Random.Range(-0.5f, 1f));
            }
            else
            {
                yield return new WaitForSeconds(pulseInterval);
            }

            // Запускаем импульсную анимацию
            yield return StartCoroutine(PulseAnimation());
        }
    }

    IEnumerator PulseAnimation()
    {
        float timer = 0f;
        float duration = 1f / pulseSpeed;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            float curveValue = pulseCurve.Evaluate(progress);

            // Плавное изменение масштаба
            Vector3 targetScale = Vector3.Lerp(minScale, maxScale, curveValue);
            canvasRect.localScale = originalScale + (targetScale - Vector3.one) * pulseIntensity;

            yield return null;
        }

        // Возврат к исходному масштабу
        canvasRect.localScale = originalScale;
    }

    void OnDestroy()
    {
        isAnimating = false;
        StopAllCoroutines();
    }
}