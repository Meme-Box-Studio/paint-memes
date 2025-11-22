using UnityEngine;
using UnityEngine.UI;

public class SimpleSpectrumAnimator : MonoBehaviour
{
    [Header("Spectrum References")]
    [SerializeField] private RectTransform _spectrumRect;
    [SerializeField] private Image _spectrumImage;

    [Header("Animation Settings")]
    [SerializeField] private float _rotationSpeed = 30f;
    [SerializeField] private float _pulseSpeed = 2f;
    [SerializeField] private float _maxScale = 1.2f;
    [SerializeField] private float _minScale = 0.8f;
    [SerializeField] private Color[] _spectrumColors;

    private int _currentColorIndex = 0;
    private float _colorTimer = 0f;
    private float _colorChangeInterval = 2f;

    private void Update()
    {
        // Вращение
        _spectrumRect.Rotate(0, 0, _rotationSpeed * Time.deltaTime);

        // Пульсация
        float pulse = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f;
        float scale = Mathf.Lerp(_minScale, _maxScale, pulse);
        _spectrumRect.localScale = Vector3.one * scale;

        // Смена цвета
        if (_spectrumColors.Length > 1)
        {
            _colorTimer += Time.deltaTime;
            if (_colorTimer >= _colorChangeInterval)
            {
                _colorTimer = 0f;
                _currentColorIndex = (_currentColorIndex + 1) % _spectrumColors.Length;
            }

            // Плавная смена цвета
            float t = _colorTimer / _colorChangeInterval;
            Color currentColor = _spectrumImage.color;
            Color targetColor = _spectrumColors[_currentColorIndex];
            _spectrumImage.color = Color.Lerp(currentColor, targetColor, t);
        }
    }

    public void PlayClickAnimation()
    {
        // Простая анимация клика через корутину
        StartCoroutine(ClickAnimation());
    }

    private System.Collections.IEnumerator ClickAnimation()
    {
        Vector3 originalScale = _spectrumRect.localScale;

        // Увеличиваем
        _spectrumRect.localScale = originalScale * 1.3f;
        yield return new WaitForSeconds(0.1f);

        // Возвращаем
        _spectrumRect.localScale = originalScale;
    }
}