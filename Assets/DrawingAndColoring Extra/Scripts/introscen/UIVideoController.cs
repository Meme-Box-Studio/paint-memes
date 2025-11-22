using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class AnimatedGradientVideoController : MonoBehaviour
{
    public string videoFileName = "intro.mp4";
    public string mainGameSceneName = "Album";
    public Slider progressSlider;

    [Header("Gradient Colors")]
    public Color gradientStart = new Color(1f, 0f, 1f);    // #FF00FF
    public Color gradientMid = new Color(1f, 0.2f, 1f);    // #FF33FF
    public Color gradientEnd = new Color(1f, 0.6f, 1f);    // #FF99FF
    public Color backgroundColor = new Color(0.176f, 0f, 0.212f); // #2D0036

    [Header("Animation Settings")]
    public float pulseSpeed = 3f;
    public float pulseIntensity = 0.3f;
    public float glowIntensity = 0.5f;
    public bool enableAnimations = true;
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 0.5f;

    private VideoPlayer videoPlayer;
    private RawImage rawImage;
    private bool isVideoPrepared = false;
    private Image fillImage;
    private Image backgroundImage;
    private CanvasGroup sliderCanvasGroup;
    private float animationTimer;
    private float fadeTimer;
    private bool isFadingIn = false;
    private bool isFadingOut = false;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        videoPlayer = GetComponent<VideoPlayer>();

        SetupSliderComponents();
        SetupVideoPlayer();

        // Начальная анимация появления
        if (sliderCanvasGroup != null)
        {
            sliderCanvasGroup.alpha = 0f;
            isFadingIn = true;
            fadeTimer = 0f;
        }
    }

    void Update()
    {
        if (isVideoPrepared && videoPlayer.isPlaying)
        {
            UpdateProgressSlider();
            UpdateGradientWithAnimations();
        }

        if (isVideoPrepared && Input.anyKeyDown)
        {
            SkipVideo();
        }

        // Обработка анимаций прозрачности
        HandleFadeAnimations();

        animationTimer += Time.deltaTime;
    }

    private void HandleFadeAnimations()
    {
        if (isFadingIn && sliderCanvasGroup != null)
        {
            fadeTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(fadeTimer / fadeInDuration);
            sliderCanvasGroup.alpha = progress;

            if (progress >= 1f)
            {
                isFadingIn = false;
            }
        }

        if (isFadingOut && sliderCanvasGroup != null)
        {
            fadeTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(fadeTimer / fadeOutDuration);
            sliderCanvasGroup.alpha = 1f - progress;

            if (progress >= 1f)
            {
                isFadingOut = false;
                OnFadeOutComplete();
            }
        }
    }

    private void SetupSliderComponents()
    {
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = 1;
            progressSlider.value = 0;

            // Получаем компонент Image из Fill Area
            if (progressSlider.fillRect != null)
            {
                fillImage = progressSlider.fillRect.GetComponent<Image>();
                // Устанавливаем белый цвет чтобы градиент был ярким
                if (fillImage != null)
                    fillImage.color = Color.white;
            }

            // Получаем background для анимаций
            Transform background = progressSlider.transform.Find("Background");
            if (background != null)
            {
                backgroundImage = background.GetComponent<Image>();
                backgroundImage.color = backgroundColor;
            }

            // Добавляем CanvasGroup для анимаций прозрачности
            sliderCanvasGroup = progressSlider.GetComponent<CanvasGroup>();
            if (sliderCanvasGroup == null)
                sliderCanvasGroup = progressSlider.gameObject.AddComponent<CanvasGroup>();

            // Добавляем свечение через Shadow компонент
            AddGlowEffect();
        }
    }

    private void AddGlowEffect()
    {
        // Добавляем свечение к fill области
        if (fillImage != null)
        {
            Shadow shadow = fillImage.GetComponent<Shadow>();
            if (shadow == null)
                shadow = fillImage.gameObject.AddComponent<Shadow>();

            shadow.effectColor = new Color(1f, 0.4f, 1f, 0.3f);
            shadow.effectDistance = new Vector2(0, 0);
        }

        // Добавляем свечение к background
        if (backgroundImage != null)
        {
            Shadow bgShadow = backgroundImage.GetComponent<Shadow>();
            if (bgShadow == null)
                bgShadow = backgroundImage.gameObject.AddComponent<Shadow>();

            bgShadow.effectColor = new Color(0.5f, 0f, 0.5f, 0.2f);
            bgShadow.effectDistance = new Vector2(0, 0);
        }
    }

    private void UpdateGradientWithAnimations()
    {
        if (fillImage != null)
        {
            float progress = progressSlider.normalizedValue;

            // Трехцветный градиент для большей выразительности
            Color currentColor;
            if (progress < 0.5f)
            {
                currentColor = Color.Lerp(gradientStart, gradientMid, progress * 2f);
            }
            else
            {
                currentColor = Color.Lerp(gradientMid, gradientEnd, (progress - 0.5f) * 2f);
            }

            // Усиливаем цвет для лучшей видимости
            currentColor = EnhanceColor(currentColor, 1.3f, 1.4f);

            // Пульсация
            if (enableAnimations)
            {
                float pulse = Mathf.Sin(animationTimer * pulseSpeed) * pulseIntensity;
                currentColor += new Color(pulse, pulse, pulse, 0);

                // Мерцание свечения
                float glow = (Mathf.Sin(animationTimer * pulseSpeed * 1.5f) + 1f) * 0.5f * glowIntensity;
                UpdateGlowEffect(glow);
            }

            fillImage.color = currentColor;

            // Анимация масштаба при заполнении
            AnimateScale(progress);
        }
    }

    private Color EnhanceColor(Color color, float saturation = 1.5f, float brightness = 1.2f)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        s = Mathf.Clamp(s * saturation, 0f, 1f);
        v = Mathf.Clamp(v * brightness, 0f, 1f);
        return Color.HSVToRGB(h, s, v);
    }

    private void UpdateGlowEffect(float intensity)
    {
        if (fillImage != null)
        {
            Shadow shadow = fillImage.GetComponent<Shadow>();
            if (shadow != null)
            {
                Color glowColor = new Color(1f, 0.6f, 1f, intensity * 0.4f);
                shadow.effectColor = glowColor;

                // Плавное изменение размера свечения
                float glowSize = 2f + intensity * 3f;
                shadow.effectDistance = new Vector2(glowSize, glowSize);
            }
        }
    }

    private void AnimateScale(float progress)
    {
        if (enableAnimations && progressSlider.fillRect != null)
        {
            // Легкая пульсация масштаба
            float scalePulse = 1f + Mathf.Sin(animationTimer * 4f) * 0.05f;

            // Увеличение при достижении определенного прогресса
            float progressBoost = 1f + progress * 0.1f;

            Vector3 targetScale = Vector3.one * scalePulse * progressBoost;
            progressSlider.fillRect.localScale = Vector3.Lerp(
                progressSlider.fillRect.localScale,
                targetScale,
                Time.deltaTime * 8f
            );
        }
    }

    private void SetupVideoPlayer()
    {
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
        videoPlayer.targetTexture = renderTexture;
        rawImage.texture = renderTexture;

        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.url = videoPath;

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("UI Video prepared!");
        isVideoPrepared = true;

        if (progressSlider != null)
        {
            progressSlider.maxValue = (float)videoPlayer.length;

            // Анимация появления слайдера
            if (sliderCanvasGroup != null)
            {
                sliderCanvasGroup.alpha = 0f;
                isFadingIn = true;
                fadeTimer = 0f;
            }
        }

        videoPlayer.Play();
        rawImage.color = Color.white;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // Анимация исчезновения перед переходом
        if (sliderCanvasGroup != null && !isFadingOut)
        {
            isFadingOut = true;
            fadeTimer = 0f;
        }
        else
        {
            SceneManager.LoadScene(mainGameSceneName);
        }
    }

    private void OnFadeOutComplete()
    {
        SceneManager.LoadScene(mainGameSceneName);
    }

    private void UpdateProgressSlider()
    {
        if (progressSlider != null && videoPlayer.frameCount > 0)
        {
            progressSlider.value = (float)videoPlayer.time;
        }
    }

    public void SkipVideo()
    {
        if (!isVideoPrepared) return;

        // Анимация быстрого исчезновения при пропуске
        if (sliderCanvasGroup != null && !isFadingOut)
        {
            isFadingOut = true;
            fadeTimer = 0f;
            fadeOutDuration = 0.3f; // Быстрое исчезновение
        }
        else
        {
            videoPlayer.Stop();
            SceneManager.LoadScene(mainGameSceneName);
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}