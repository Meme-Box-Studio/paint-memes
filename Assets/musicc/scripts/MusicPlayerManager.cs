using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;

public class MusicPlayerManager : MonoBehaviour
{
    [Header("Player Components")]
    public GameObject playerPanel;
    public AudioSource audioSource;
    public VideoPlayer backgroundVideoPlayer;
    public RawImage videoDisplay;
    public Image videoOverlay;

    [Header("Audio Clips")]
    public AudioClip[] musicTracks;

    [Header("UI Elements")]
    public Button playButton;
    public Button pauseButton;
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;
    public Slider progressSlider;
    public Slider volumeSlider;
    public Slider brightnessSlider;
    public TextMeshProUGUI songTitleText;
    public TextMeshProUGUI currentTimeText;
    public TextMeshProUGUI totalTimeText;
    public Image albumArt;

    [Header("Settings")]
    public float videoMaxBrightness = 0.4f;

    private bool isPlayerOpen = false;
    private bool isPlaying = false;
    private int currentTrackIndex = 0;
    private Coroutine progressCoroutine;

    void Start()
    {
        InitializePlayer();
        SetupEventListeners();
    }

    void InitializePlayer()
    {
        // Скрываем плеер при старте
        playerPanel.SetActive(false);

        // Настраиваем видео
        if (backgroundVideoPlayer != null)
        {
            backgroundVideoPlayer.playOnAwake = false;
            backgroundVideoPlayer.isLooping = true;
        }

        // Настраиваем аудио
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }

        UpdateUI();
    }

    void SetupEventListeners()
    {
        // Кнопки управления
        playButton.onClick.AddListener(PlayMusic);
        pauseButton.onClick.AddListener(PauseMusic);
        nextButton.onClick.AddListener(NextTrack);
        prevButton.onClick.AddListener(PreviousTrack);
        closeButton.onClick.AddListener(ClosePlayer);

        // Слайдеры
        progressSlider.onValueChanged.AddListener(OnProgressChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);

        // Начальные значения
        volumeSlider.value = 0.7f;
        brightnessSlider.value = 0.3f;
    }

    public void TogglePlayer()
    {
        isPlayerOpen = !isPlayerOpen;

        if (isPlayerOpen)
        {
            OpenPlayer();
        }
        else
        {
            ClosePlayer();
        }
    }

    void OpenPlayer()
    {
        playerPanel.SetActive(true);
        StartCoroutine(AnimatePanelOpen());

        // Запускаем фоновое видео
        if (backgroundVideoPlayer != null && !backgroundVideoPlayer.isPlaying)
        {
            backgroundVideoPlayer.Play();
        }
    }

    void ClosePlayer()
    {
        StartCoroutine(AnimatePanelClose());
    }

    IEnumerator AnimatePanelOpen()
    {
        // Анимация появления
        CanvasGroup canvasGroup = playerPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = playerPanel.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
        playerPanel.transform.localScale = Vector3.one * 0.8f;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            playerPanel.transform.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, t);

            yield return null;
        }

        canvasGroup.alpha = 1;
        playerPanel.transform.localScale = Vector3.one;
    }

    IEnumerator AnimatePanelClose()
    {
        // Анимация скрытия
        CanvasGroup canvasGroup = playerPanel.GetComponent<CanvasGroup>();

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            canvasGroup.alpha = Mathf.Lerp(1, 0, t);
            playerPanel.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.8f, t);

            yield return null;
        }

        playerPanel.SetActive(false);

        // Останавливаем видео
        if (backgroundVideoPlayer != null && backgroundVideoPlayer.isPlaying)
        {
            backgroundVideoPlayer.Stop();
        }
    }

    void PlayMusic()
    {
        if (musicTracks.Length == 0) return;

        if (audioSource.clip == null)
        {
            audioSource.clip = musicTracks[currentTrackIndex];
        }

        audioSource.Play();
        isPlaying = true;
        UpdatePlayPauseButtons();

        // Запускаем обновление прогресса
        if (progressCoroutine != null) StopCoroutine(progressCoroutine);
        progressCoroutine = StartCoroutine(UpdateProgress());
    }

    void PauseMusic()
    {
        audioSource.Pause();
        isPlaying = false;
        UpdatePlayPauseButtons();
    }

    void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;
        SwitchTrack();
    }

    void PreviousTrack()
    {
        currentTrackIndex = (currentTrackIndex - 1 + musicTracks.Length) % musicTracks.Length;
        SwitchTrack();
    }

    void SwitchTrack()
    {
        audioSource.clip = musicTracks[currentTrackIndex];
        if (isPlaying)
        {
            audioSource.Play();
        }
        UpdateUI();
    }

    void OnProgressChanged(float value)
    {
        if (audioSource.clip != null)
        {
            audioSource.time = value * audioSource.clip.length;
        }
    }

    void OnVolumeChanged(float volume)
    {
        audioSource.volume = volume;
    }

    void OnBrightnessChanged(float brightness)
    {
        if (videoOverlay != null)
        {
            float alpha = 1f - (brightness * videoMaxBrightness);
            videoOverlay.color = new Color(0, 0, 0, alpha);
        }
    }

    IEnumerator UpdateProgress()
    {
        while (isPlaying && audioSource.clip != null)
        {
            if (audioSource.clip.length > 0)
            {
                float progress = audioSource.time / audioSource.clip.length;
                progressSlider.SetValueWithoutNotify(progress);

                currentTimeText.text = FormatTime(audioSource.time);
                totalTimeText.text = FormatTime(audioSource.clip.length);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void UpdatePlayPauseButtons()
    {
        playButton.gameObject.SetActive(!isPlaying);
        pauseButton.gameObject.SetActive(isPlaying);
    }

    void UpdateUI()
    {
        if (musicTracks.Length > 0 && currentTrackIndex < musicTracks.Length)
        {
            songTitleText.text = musicTracks[currentTrackIndex].name;
        }
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    void Update()
    {
        // Горячая клавиша для открытия/закрытия плеера
        if (Input.GetKeyDown(KeyCode.M))
        {
            TogglePlayer();
        }
    }
}