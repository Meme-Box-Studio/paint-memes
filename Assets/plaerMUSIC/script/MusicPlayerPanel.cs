using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicPlayerPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public Image songImage;
    public TMP_Text songTitleText;
    public TMP_Text currentTimeText;
    public TMP_Text totalTimeText;
    public TMP_Text trackCounterText;
    public Slider volumeSlider;
    public Slider progressSlider;
    public Button playButton;
    public Button pauseButton;
    public Button stopButton;
    public Button nextButton;
    public Button prevButton;
    public Button muteButton;
    public Button closeButton;

    [Header("Mute Button Sprites")]
    public Sprite muteOnSprite;
    public Sprite muteOffSprite;

    private MusicPlayer musicPlayer;
    private bool isInitialized = false;

    void Start()
    {
        InitializePanel();
    }

    void OnEnable()
    {
        InitializePanel();
        RefreshUI();
    }

    void Update()
    {
        if (musicPlayer != null && musicPlayer.IsPlaying() && gameObject.activeInHierarchy)
        {
            UpdateTimeDisplay();
            UpdateProgressSlider();
        }
    }

    void InitializePanel()
    {
        if (isInitialized) return;

        GameObject musicManager = GameObject.Find("MusicManager");
        if (musicManager != null)
        {
            musicPlayer = musicManager.GetComponent<MusicPlayer>();
            if (musicPlayer != null)
            {
                // Подписываемся на события
                musicPlayer.OnTrackChanged += OnTrackChanged;
                musicPlayer.OnMuteChanged += OnMuteChanged; // Новое событие
                SetupUIListeners();
                isInitialized = true;
                Debug.Log("MusicPlayerPanel инициализирован");
            }
        }
    }

    void SetupUIListeners()
    {
        if (playButton != null) playButton.onClick.AddListener(() => musicPlayer.Play());
        if (pauseButton != null) pauseButton.onClick.AddListener(() => musicPlayer.Pause());
        if (stopButton != null) stopButton.onClick.AddListener(() => musicPlayer.Stop());
        if (nextButton != null) nextButton.onClick.AddListener(() => musicPlayer.NextTrack());
        if (prevButton != null) prevButton.onClick.AddListener(() => musicPlayer.PreviousTrack());
        if (muteButton != null) muteButton.onClick.AddListener(() => musicPlayer.ToggleMute());
        if (closeButton != null) closeButton.onClick.AddListener(() => musicPlayer.HidePlayer());

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener((value) => musicPlayer.SetVolume(value));
        }

        if (progressSlider != null)
        {
            progressSlider.onValueChanged.AddListener(OnProgressSliderChanged);
        }
    }

    void OnTrackChanged(int trackIndex)
    {
        RefreshUI();
    }

    void OnMuteChanged(bool isMuted)
    {
        UpdateMuteButtonAppearance();
        UpdateVolumeSlider();
    }

    public void RefreshUI()
    {
        if (musicPlayer == null || !gameObject.activeInHierarchy) return;

        UpdateSongTitle();
        UpdateTrackCounter();
        UpdateTimeDisplay();
        UpdateProgressSlider();
        UpdateSongImage();
        UpdateButtonStates();
        UpdateMuteButtonAppearance();
        UpdateVolumeSlider();
    }

    void UpdateSongTitle()
    {
        if (songTitleText != null && musicPlayer != null)
        {
            AudioClip currentClip = musicPlayer.GetCurrentClip();
            if (currentClip != null)
            {
                songTitleText.text = $"Сейчас: {currentClip.name}";
            }
        }
    }

    void UpdateTrackCounter()
    {
        if (trackCounterText != null && musicPlayer != null)
        {
            int currentTrack = musicPlayer.GetCurrentTrackIndex() + 1;
            int totalTracks = musicPlayer.GetPlaylistLength();
            trackCounterText.text = $"{currentTrack}/{totalTracks}";
        }
    }

    void UpdateTimeDisplay()
    {
        if (musicPlayer == null) return;

        if (currentTimeText != null)
        {
            currentTimeText.text = musicPlayer.GetCurrentTimeFormatted();
        }

        if (totalTimeText != null)
        {
            totalTimeText.text = musicPlayer.GetTotalTimeFormatted();
        }
    }

    void UpdateProgressSlider()
    {
        if (progressSlider != null && musicPlayer != null)
        {
            progressSlider.value = musicPlayer.GetCurrentTime();
            progressSlider.maxValue = musicPlayer.GetTotalTime();
        }
    }

    void OnProgressSliderChanged(float value)
    {
        if (musicPlayer != null)
        {
            musicPlayer.SetProgress(value);
        }
    }

    void UpdateSongImage()
    {
        if (songImage != null && musicPlayer != null)
        {
            Sprite trackSprite = musicPlayer.GetCurrentTrackImage();
            if (trackSprite != null)
            {
                songImage.sprite = trackSprite;
                songImage.color = Color.white;
            }
            else
            {
                songImage.color = new Color(0, 0, 0, 0);
            }
        }
    }

    void UpdateButtonStates()
    {
        if (musicPlayer == null) return;

        bool isPlaying = musicPlayer.IsPlaying();

        if (playButton != null)
        {
            playButton.gameObject.SetActive(!isPlaying);
            playButton.interactable = (musicPlayer.GetCurrentClip() != null);
        }

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(isPlaying);
            pauseButton.interactable = (musicPlayer.GetCurrentClip() != null);
        }

        if (stopButton != null) stopButton.interactable = (musicPlayer.GetCurrentClip() != null);
        if (nextButton != null) nextButton.interactable = (musicPlayer.GetPlaylistLength() > 1);
        if (prevButton != null) prevButton.interactable = (musicPlayer.GetPlaylistLength() > 1);
        if (muteButton != null) muteButton.interactable = true;
    }

    void UpdateMuteButtonAppearance()
    {
        if (muteButton != null && musicPlayer != null)
        {
            bool isMuted = musicPlayer.IsMuted();

            // Обновляем спрайт
            if (muteButton.image != null)
            {
                if (isMuted)
                {
                    if (muteOnSprite != null)
                    {
                        muteButton.image.sprite = muteOnSprite;
                    }
                }
                else
                {
                    if (muteOffSprite != null)
                    {
                        muteButton.image.sprite = muteOffSprite;
                    }
                }
            }

            // Обновляем цвет
            var colors = muteButton.colors;
            if (isMuted)
            {
                colors.normalColor = Color.red;
                colors.highlightedColor = new Color(1f, 0.5f, 0.5f);
                colors.pressedColor = new Color(1f, 0.3f, 0.3f);
            }
            else
            {
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
                colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            }
            muteButton.colors = colors;
        }
    }

    void UpdateVolumeSlider()
    {
        if (volumeSlider != null && musicPlayer != null)
        {
            volumeSlider.value = musicPlayer.GetCurrentVolume();
        }
    }

    void OnDestroy()
    {
        if (musicPlayer != null)
        {
            musicPlayer.OnTrackChanged -= OnTrackChanged;
            musicPlayer.OnMuteChanged -= OnMuteChanged;
        }
    }
}