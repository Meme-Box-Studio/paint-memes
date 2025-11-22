using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class MusicPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip[] playlist;

    [Header("Audio Source from MusicSystem")]
    [SerializeField] private AudioSource audioSource;

    [Header("Image Settings")]
    public Image songImage;
    public Sprite[] songImages;
    public string imageFolderPath = "MusicImages/";

    [Header("UI Elements")]
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

    [Header("Mute Button Sprites")]
    public Sprite muteOnSprite;
    public Sprite muteOffSprite;

    [Header("Player UI")]
    public GameObject playerPanel;
    public Button closeButton;

    private int currentTrackIndex = 0;
    private bool isPlaying = false;
    private bool isDraggingProgress = false;
    private bool isMuted = false;
    private float previousVolume = 0.5f; // Начальная громкость 50%
    private float savedPlaybackTime = 0f;
    private bool isPaused = false;

    public event Action<int> OnTrackChanged;
    public event Action<bool> OnMuteChanged;

    void Awake()
    {
        gameObject.SetActive(true);
    }

    void Start()
    {
        GameObject musicSystem = GameObject.Find("MusicSystem");
        if (musicSystem != null)
        {
            audioSource = musicSystem.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.volume = previousVolume; // Устанавливаем начальную громкость
            }
            else
            {
                audioSource = musicSystem.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.volume = previousVolume;
            }
        }
        else
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = previousVolume;
        }

        if (songImages == null || songImages.Length == 0)
        {
            LoadSongImages();
        }

        SetupUIListeners();
        LoadTrack(currentTrackIndex);
        UpdateTimeDisplay();

        // Инициализируем громкость
        if (volumeSlider != null)
        {
            volumeSlider.value = previousVolume;
        }

        if (IsPlayerVisible())
        {
            RefreshUI();
        }

        if (playlist.Length > 0)
        {
            Invoke(nameof(AutoPlay), 0.1f);
        }
    }

    void Update()
    {
        if (isPlaying && !isDraggingProgress && IsPlayerVisible())
        {
            UpdateTimeDisplay();
            UpdateProgressSlider();
        }

        if (isPlaying && audioSource != null && audioSource.clip != null)
        {
            CheckTrackEndUniversal();
        }
    }

    void CheckTrackEndUniversal()
    {
        if (audioSource.time >= audioSource.clip.length - 0.1f)
        {
            Debug.Log("Трек завершился");
            OnTrackEnded();
        }
    }

    public void RefreshUI()
    {
        if (!IsPlayerVisible()) return;

        UpdateSongTitle();
        UpdateTrackCounter();
        UpdateTimeDisplay();
        UpdateProgressSlider();
        UpdateSongImage();
        UpdateButtonStates(isPlaying);
        UpdateMuteButtonAppearance();
        UpdateVolumeSlider();
    }

    public void ShowPlayer()
    {
        if (playerPanel != null)
        {
            playerPanel.SetActive(true);
            RefreshUI();
        }
    }

    public void HidePlayer()
    {
        if (playerPanel != null)
        {
            playerPanel.SetActive(false);
        }
    }

    public void TogglePlayer()
    {
        if (playerPanel != null)
        {
            bool newState = !playerPanel.activeSelf;
            playerPanel.SetActive(newState);
            if (newState) RefreshUI();
        }
    }

    bool IsPlayerVisible()
    {
        return playerPanel != null && playerPanel.activeSelf;
    }

    void AutoPlay()
    {
        if (!isPlaying && playlist.Length > 0)
        {
            Play();
        }
    }

    void OnTrackEnded()
    {
        NextTrack();
    }

    void LoadSongImages()
    {
        if (!string.IsNullOrEmpty(imageFolderPath))
        {
            Sprite[] loadedSprites = Resources.LoadAll<Sprite>(imageFolderPath);
            if (loadedSprites != null && loadedSprites.Length > 0)
            {
                songImages = loadedSprites;
            }
        }
    }

    public Sprite GetCurrentTrackImage()
    {
        if (audioSource == null || audioSource.clip == null)
            return null;

        string trackName = audioSource.clip.name;

        if (songImages != null)
        {
            foreach (var sprite in songImages)
            {
                if (sprite != null && sprite.name == trackName)
                {
                    return sprite;
                }
            }
        }

        Sprite loadedSprite = Resources.Load<Sprite>(imageFolderPath + trackName);
        if (loadedSprite != null) return loadedSprite;

        if (songImages != null && songImages.Length > 0) return songImages[0];

        return null;
    }

    void UpdateSongImage()
    {
        if (songImage != null)
        {
            Sprite trackSprite = GetCurrentTrackImage();
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

    void SetupUIListeners()
    {
        if (playButton != null) playButton.onClick.AddListener(Play);
        if (pauseButton != null) pauseButton.onClick.AddListener(Pause);
        if (stopButton != null) stopButton.onClick.AddListener(Stop);
        if (nextButton != null) nextButton.onClick.AddListener(NextTrack);
        if (prevButton != null) prevButton.onClick.AddListener(PreviousTrack);
        if (volumeSlider != null) volumeSlider.onValueChanged.AddListener(SetVolume);

        if (muteButton != null)
        {
            muteButton.onClick.AddListener(ToggleMute);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePlayer);
        }
        else if (playerPanel != null)
        {
            closeButton = playerPanel.transform.Find("CloseButton")?.GetComponent<Button>();
            if (closeButton != null) closeButton.onClick.AddListener(HidePlayer);
        }

        if (progressSlider != null)
        {
            progressSlider.onValueChanged.AddListener(OnProgressSliderChanged);

            var eventTrigger = progressSlider.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            var beginDrag = new UnityEngine.EventSystems.EventTrigger.Entry();
            beginDrag.eventID = UnityEngine.EventSystems.EventTriggerType.BeginDrag;
            beginDrag.callback.AddListener((data) => { isDraggingProgress = true; });
            eventTrigger.triggers.Add(beginDrag);

            var endDrag = new UnityEngine.EventSystems.EventTrigger.Entry();
            endDrag.eventID = UnityEngine.EventSystems.EventTriggerType.EndDrag;
            endDrag.callback.AddListener((data) => { isDraggingProgress = false; });
            eventTrigger.triggers.Add(endDrag);
        }
    }

    public void Play()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            if (isPaused && savedPlaybackTime > 0)
            {
                audioSource.time = savedPlaybackTime;
            }

            audioSource.Play();
            isPlaying = true;
            isPaused = false;
            UpdateButtonStates(true);
        }
        else if (playlist.Length > 0)
        {
            LoadTrack(currentTrackIndex);
            audioSource.Play();
            isPlaying = true;
            isPaused = false;
            UpdateButtonStates(true);
        }
    }

    public void Pause()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            savedPlaybackTime = audioSource.time;
            audioSource.Stop();
            isPlaying = false;
            isPaused = true;
            UpdateButtonStates(false);
        }
    }

    public void Stop()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            isPlaying = false;
            isPaused = false;
            savedPlaybackTime = 0f;
            audioSource.time = 0f;
            UpdateButtonStates(false);
            UpdateTimeDisplay();
            UpdateProgressSlider();
        }
    }

    public void NextTrack()
    {
        if (playlist.Length == 0) return;

        bool wasPlaying = isPlaying && !isPaused;
        isPaused = false;
        savedPlaybackTime = 0f;

        if (wasPlaying && audioSource != null)
        {
            audioSource.Stop();
        }

        currentTrackIndex = (currentTrackIndex + 1) % playlist.Length;
        LoadTrack(currentTrackIndex);

        if (wasPlaying)
        {
            Invoke(nameof(PlayDelayed), 0.1f);
        }
    }

    void PlayDelayed()
    {
        Play();
    }

    public void PreviousTrack()
    {
        if (playlist.Length == 0) return;

        bool wasPlaying = isPlaying && !isPaused;
        isPaused = false;
        savedPlaybackTime = 0f;

        if (wasPlaying && audioSource != null)
        {
            audioSource.Stop();
        }

        currentTrackIndex--;
        if (currentTrackIndex < 0) currentTrackIndex = playlist.Length - 1;
        LoadTrack(currentTrackIndex);

        if (wasPlaying)
        {
            Invoke(nameof(PlayDelayed), 0.1f);
        }
    }

    public void ToggleMute()
    {
        if (audioSource != null)
        {
            isMuted = !isMuted;

            if (isMuted)
            {
                // Выключаем звук - устанавливаем 0
                audioSource.volume = 0f;
                Debug.Log($"MUTE ВКЛЮЧЕН. Громкость: 0");
            }
            else
            {
                // Включаем звук - ВСЕГДА устанавливаем 0.5
                audioSource.volume = 0.5f;
                previousVolume = 0.5f;
                Debug.Log($"MUTE ВЫКЛЮЧЕН. Громкость: 0.5");
            }

            // Обновляем слайдер громкости БЕЗ вызова события
            if (volumeSlider != null)
            {
                volumeSlider.SetValueWithoutNotify(isMuted ? 0f : 0.5f);
            }

            // Обновляем внешний вид
            UpdateMuteButtonAppearance();

            // Уведомляем об изменении состояния mute
            OnMuteChanged?.Invoke(isMuted);
        }
    }

    void UpdateMuteButtonAppearance()
    {
        if (muteButton != null && muteButton.image != null)
        {
            if (isMuted)
            {
                // Режим mute включен - красный цвет и иконка выключенного звука
                if (muteOnSprite != null)
                {
                    muteButton.image.sprite = muteOnSprite;
                }
                var colors = muteButton.colors;
                colors.normalColor = Color.red;
                muteButton.colors = colors;
            }
            else
            {
                // Режим mute выключен - белый цвет и иконка включенного звука
                if (muteOffSprite != null)
                {
                    muteButton.image.sprite = muteOffSprite;
                }
                var colors = muteButton.colors;
                colors.normalColor = Color.white;
                muteButton.colors = colors;
            }
        }
    }

    void UpdateVolumeSlider()
    {
        if (volumeSlider != null && audioSource != null)
        {
            // Обновляем слайдер БЕЗ вызова события onValueChanged
            volumeSlider.SetValueWithoutNotify(isMuted ? 0f : audioSource.volume);
        }
    }

    void LoadTrack(int index)
    {
        if (audioSource != null && index >= 0 && index < playlist.Length)
        {
            audioSource.clip = playlist[index];
            audioSource.time = 0f;
            isPaused = false;
            savedPlaybackTime = 0f;

            if (IsPlayerVisible()) RefreshUI();

            OnTrackChanged?.Invoke(index);

            if (progressSlider != null)
            {
                progressSlider.minValue = 0;
                progressSlider.maxValue = audioSource.clip.length;
                progressSlider.value = 0;
            }
        }
    }

    void UpdateSongTitle()
    {
        if (songTitleText != null && audioSource != null && audioSource.clip != null)
        {
            songTitleText.text = $"Сейчас: {audioSource.clip.name}";
        }
    }

    void UpdateTrackCounter()
    {
        if (trackCounterText != null)
        {
            trackCounterText.text = $"{currentTrackIndex + 1}/{playlist.Length}";
        }
    }

    void UpdateTimeDisplay()
    {
        if (currentTimeText != null && audioSource != null)
        {
            currentTimeText.text = FormatTime(audioSource.time);
        }

        if (totalTimeText != null && audioSource != null && audioSource.clip != null)
        {
            totalTimeText.text = FormatTime(audioSource.clip.length);
        }
    }

    void UpdateProgressSlider()
    {
        if (progressSlider != null && audioSource != null && audioSource.clip != null && !isDraggingProgress)
        {
            progressSlider.value = audioSource.time;
        }
    }

    void OnProgressSliderChanged(float value)
    {
        if (isDraggingProgress && audioSource != null && audioSource.clip != null)
        {
            audioSource.time = value;
            UpdateTimeDisplay();
        }
    }

    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null && !isMuted)
        {
            // Если не в режиме mute, обновляем громкость
            audioSource.volume = volume;
            previousVolume = volume;
            Debug.Log($"Громкость установлена: {volume}");
        }
        // В режиме mute ничего не делаем - громкость остается 0
    }

    void UpdateButtonStates(bool playing)
    {
        if (!IsPlayerVisible()) return;

        if (playButton != null)
        {
            playButton.gameObject.SetActive(!playing);
            playButton.interactable = (audioSource != null && audioSource.clip != null);
        }

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(playing);
            pauseButton.interactable = (audioSource != null && audioSource.clip != null);
        }

        if (stopButton != null) stopButton.interactable = (audioSource != null && audioSource.clip != null);
        if (nextButton != null) nextButton.interactable = (playlist.Length > 1);
        if (prevButton != null) prevButton.interactable = (playlist.Length > 1);
        if (muteButton != null) muteButton.interactable = (audioSource != null);
    }

    // Методы для MusicPlayerPanel
    public int GetPlaylistLength() => playlist.Length;
    public float GetCurrentTime() => audioSource != null ? audioSource.time : 0f;
    public float GetTotalTime() => audioSource != null && audioSource.clip != null ? audioSource.clip.length : 0f;
    public string GetCurrentTimeFormatted() => FormatTime(GetCurrentTime());
    public string GetTotalTimeFormatted() => FormatTime(GetTotalTime());
    public float GetCurrentVolume() => audioSource != null ? audioSource.volume : 1f;
    public bool IsMuted() => isMuted;

    public void SetProgress(float progress)
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.time = Mathf.Clamp(progress, 0f, audioSource.clip.length);
            UpdateTimeDisplay();
        }
    }

    public int GetCurrentTrackIndex() => currentTrackIndex;
    public bool IsPlaying() => isPlaying;
    public AudioClip GetCurrentClip() => audioSource != null ? audioSource.clip : null;

    [ContextMenu("Debug Mute State")]
    public void DebugMuteState()
    {
        Debug.Log($"Muted: {isMuted}, Volume: {audioSource?.volume}, Previous: {previousVolume}");
    }
}