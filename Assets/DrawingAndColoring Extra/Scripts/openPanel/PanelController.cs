using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class PanelController : MonoBehaviour
{
    [Header("Main References")]
    public RectTransform panel;
    public Button toggleButton;
    public Button closeButton;

    [Header("Music Player Reference")]
    public MusicPlayer musicPlayer;

    [Header("Album Art Display on Button")]
    public Image buttonAlbumArt;

    [Header("Video Background")]
    public bool useVideoBackground = true;
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;

    [Header("Animation Settings")]
    public float openDuration = 0.3f;
    public float closeDuration = 0.2f;
    public AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve closeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;

    private bool isOpen = false;
    private bool isAnimating = false;
    private CanvasGroup panelCanvasGroup;
    private Vector3 originalScale;
    private AudioSource audioSource;

    void Start()
    {
        InitializeComponents();
        SetupInitialState();
        SetupAlbumArtOnButton();

        if (useVideoBackground)
        {
            SetupVideoBackground();
        }
    }

    void InitializeComponents()
    {
        // CanvasGroup для плавного появления
        panelCanvasGroup = panel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
            panelCanvasGroup = panel.gameObject.AddComponent<CanvasGroup>();

        // AudioSource для звуков
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Сохраняем оригинальный scale
        originalScale = panel.localScale;

        // Настраиваем кнопки
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(TogglePanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    void SetupAlbumArtOnButton()
    {
        if (musicPlayer != null && buttonAlbumArt != null)
        {
            musicPlayer.OnTrackChanged += OnMusicTrackChanged;
            UpdateButtonAlbumArt();
        }
        else
        {
            Debug.LogWarning("MusicPlayer или ButtonAlbumArt не назначены в PanelController");
        }
    }

    private void OnMusicTrackChanged(int trackIndex)
    {
        UpdateButtonAlbumArt();
    }

    public void UpdateButtonAlbumArt()
    {
        if (buttonAlbumArt != null && musicPlayer != null)
        {
            Sprite currentAlbumArt = musicPlayer.GetCurrentTrackImage();
            if (currentAlbumArt != null)
            {
                buttonAlbumArt.sprite = currentAlbumArt;
                buttonAlbumArt.color = Color.white;
                buttonAlbumArt.preserveAspect = true;
                Debug.Log($"Обновлена обложка на кнопке: {currentAlbumArt.name}");
            }
            else
            {
                buttonAlbumArt.color = new Color(0, 0, 0, 0);
                Debug.LogWarning("Не удалось загрузить обложку для текущего трека");
            }
        }
    }

    void SetupVideoBackground()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponentInChildren<VideoPlayer>();
        }

        if (videoPlayer == null && videoDisplay != null)
        {
            GameObject videoObject = new GameObject("VideoPlayer");
            videoObject.transform.SetParent(videoDisplay.transform, false);
            videoPlayer = videoObject.AddComponent<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.isLooping = true;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

            if (videoDisplay != null && videoDisplay.texture == null)
            {
                RenderTexture renderTexture = new RenderTexture(1280, 720, 24);
                videoDisplay.texture = renderTexture;
                videoPlayer.targetTexture = renderTexture;
            }

            string videoPath = GetVideoPath();
            videoPlayer.url = videoPath;

            Debug.Log($"Video path set to: {videoPath}");

            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.errorReceived += OnVideoError;
        }
    }

    string GetVideoPath()
    {
        string videoFileName = "background.mp4";
#if UNITY_WEBGL && !UNITY_EDITOR
        return System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
#else
        return "file://" + System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
#endif
    }

    void OnVideoPrepared(VideoPlayer source)
    {
        Debug.Log("✓ Video prepared successfully!");
    }

    void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"✗ Video error: {message}");
    }

    void SetupInitialState()
    {
        panel.localScale = Vector3.zero;
        panelCanvasGroup.alpha = 0f;
        panel.gameObject.SetActive(false);

        if (useVideoBackground && videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(false);
        }
    }

    public void TogglePanel()
    {
        if (!isAnimating)
        {
            if (isOpen)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel();
            }
        }
    }

    public void OpenPanel()
    {
        if (!isAnimating && !isOpen)
        {
            StartCoroutine(OpenPanelRoutine());
        }
    }

    public void ClosePanel()
    {
        if (!isAnimating && isOpen)
        {
            StartCoroutine(ClosePanelRoutine());
        }
    }

    IEnumerator OpenPanelRoutine()
    {
        isAnimating = true;

        panel.gameObject.SetActive(true);

        if (useVideoBackground && videoPlayer != null && videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(true);

            if (!videoPlayer.isPrepared)
            {
                videoPlayer.Prepare();
                yield return new WaitUntil(() => videoPlayer.isPrepared);
            }

            if (!videoPlayer.isPlaying)
            {
                videoPlayer.Play();
            }
        }

        PlaySound(openSound);

        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        float startAlpha = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / openDuration;
            float curveProgress = openCurve.Evaluate(progress);

            panel.localScale = Vector3.Lerp(startScale, originalScale, curveProgress);
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, curveProgress);

            yield return null;
        }

        panel.localScale = originalScale;
        panelCanvasGroup.alpha = 1f;

        isOpen = true;
        isAnimating = false;
    }

    IEnumerator ClosePanelRoutine()
    {
        isAnimating = true;

        PlaySound(closeSound);

        float elapsed = 0f;
        Vector3 startScale = panel.localScale;
        float startAlpha = panelCanvasGroup.alpha;

        while (elapsed < closeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / closeDuration;
            float curveProgress = closeCurve.Evaluate(progress);

            panel.localScale = Vector3.Lerp(startScale, Vector3.zero, curveProgress);
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, curveProgress);

            yield return null;
        }

        panel.localScale = Vector3.zero;
        panelCanvasGroup.alpha = 0f;
        panel.gameObject.SetActive(false);

        if (useVideoBackground && videoPlayer != null && videoDisplay != null)
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
            videoDisplay.gameObject.SetActive(false);
        }

        isOpen = false;
        isAnimating = false;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    [ContextMenu("Обновить обложку на кнопке")]
    public void RefreshButtonAlbumArt()
    {
        UpdateButtonAlbumArt();
    }

    [ContextMenu("Debug Video Info")]
    public void DebugVideoInfo()
    {
        Debug.Log("=== VIDEO DEBUG INFO ===");
        Debug.Log($"Use Video Background: {useVideoBackground}");
        Debug.Log($"Video Player: {videoPlayer != null}");
        Debug.Log($"Video Display: {videoDisplay != null}");

        if (videoPlayer != null)
        {
            Debug.Log($"Video URL: {videoPlayer.url}");
            Debug.Log($"Is Prepared: {videoPlayer.isPrepared}");
            Debug.Log($"Is Playing: {videoPlayer.isPlaying}");
            Debug.Log($"Target Texture: {videoPlayer.targetTexture != null}");
        }
    }

    void OnDestroy()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(TogglePanel);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
        }

        if (musicPlayer != null)
        {
            musicPlayer.OnTrackChanged -= OnMusicTrackChanged;
        }

        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.errorReceived -= OnVideoError;
        }
    }
}