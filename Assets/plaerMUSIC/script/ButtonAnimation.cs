using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    [SerializeField] private float animationTime = 0.2f;
    [SerializeField] private float pressedScale = 0.85f;
    [SerializeField] private float hoverScale = 1.05f;

    [Header("Color Settings")]
    [SerializeField] private bool useColorAnimation = true;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Header("Sound Settings")]
    [SerializeField] private bool useSound = true;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private float soundVolume = 0.5f;

    [Header("Rotation Animation")]
    [SerializeField] private bool useRotation = false;
    [SerializeField] private float rotationAngle = 5f;
    [SerializeField] private float rotationTime = 0.3f;

    private Vector3 originalScale;
    private Vector3 originalRotation;
    private Image buttonImage;
    private Button button;
    private AudioSource audioSource;
    private Coroutine scaleCoroutine;
    private Coroutine rotationCoroutine;

    void Start()
    {
        // Сохраняем оригинальные значения
        originalScale = transform.localScale;
        originalRotation = transform.eulerAngles;

        // Получаем компоненты
        buttonImage = GetComponent<Image>();
        button = GetComponent<Button>();

        // Создаем AudioSource если нужно
        if (useSound)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;
        }

        // Устанавливаем начальный цвет
        if (useColorAnimation && buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
    }

    // Обработчик нажатия кнопки
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable()) return;

        // Анимация масштаба
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(AnimateScale(originalScale * pressedScale));

        // Анимация цвета
        if (useColorAnimation && buttonImage != null)
        {
            buttonImage.color = pressedColor;
        }

        // Звук нажатия
        if (useSound && clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        // Анимация вращения (если включена)
        if (useRotation)
        {
            if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
            rotationCoroutine = StartCoroutine(AnimateRotation(originalRotation + new Vector3(0, 0, rotationAngle)));
        }
    }

    // Обработчик отпускания кнопки
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInteractable()) return;

        // Возвращаем масштаб
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(AnimateScale(originalScale));

        // Возвращаем цвет
        if (useColorAnimation && buttonImage != null)
        {
            buttonImage.color = normalColor;
        }

        // Возвращаем вращение
        if (useRotation)
        {
            if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
            rotationCoroutine = StartCoroutine(AnimateRotation(originalRotation));
        }
    }

    // Обработчик наведения курсора
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable()) return;

        // Анимация масштаба при наведении
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(AnimateScale(originalScale * hoverScale));

        // Анимация цвета при наведении
        if (useColorAnimation && buttonImage != null)
        {
            buttonImage.color = hoverColor;
        }

        // Звук наведения
        if (useSound && hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    // Обработчик ухода курсора
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsInteractable()) return;

        // Возвращаем масштаб
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(AnimateScale(originalScale));

        // Возвращаем цвет
        if (useColorAnimation && buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
    }

    // Анимация масштаба
    private IEnumerator AnimateScale(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < animationTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    // Анимация вращения
    private IEnumerator AnimateRotation(Vector3 targetRotation)
    {
        Vector3 startRotation = transform.eulerAngles;
        float elapsed = 0f;

        while (elapsed < rotationTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / rotationTime;
            transform.eulerAngles = Vector3.Lerp(startRotation, targetRotation, progress);
            yield return null;
        }

        transform.eulerAngles = targetRotation;
    }

    // Проверка доступности кнопки
    private bool IsInteractable()
    {
        return button == null || button.interactable;
    }

    // Обновление цвета при изменении состояния кнопки
    void Update()
    {
        if (useColorAnimation && buttonImage != null && button != null)
        {
            if (!button.interactable)
            {
                buttonImage.color = disabledColor;
            }
            else if (buttonImage.color == disabledColor)
            {
                buttonImage.color = normalColor;
            }
        }
    }

    // Методы для ручного управления анимацией
    [ContextMenu("Анимировать нажатие")]
    public void AnimatePress()
    {
        OnPointerDown(new PointerEventData(EventSystem.current));
        StartCoroutine(DelayedRelease());
    }

    private IEnumerator DelayedRelease()
    {
        yield return new WaitForSeconds(animationTime);
        OnPointerUp(new PointerEventData(EventSystem.current));
    }

    [ContextMenu("Сбросить анимацию")]
    public void ResetAnimation()
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);

        transform.localScale = originalScale;
        transform.eulerAngles = originalRotation;

        if (useColorAnimation && buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
    }
}