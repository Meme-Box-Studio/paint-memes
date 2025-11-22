using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Player References")]
    public GameObject playerPanel;
    public AudioSource audioSource;
    public AudioClip[] playlist;

    [Header("UI Elements")]
    public Button musicToggleButton;
    public Slider progressSlider;
    public Slider volumeSlider;
    public TextMeshProUGUI currentTimeText;
    public TextMeshProUGUI totalTimeText;
    public TextMeshProUGUI songTitleText;
    public Image albumArt;

    [Header("Animation")]
    public Animator playerAnimator;
    public AnimationClip slideInAnimation;
    public AnimationClip slideOutAnimation;

    [Header("Control Buttons")]
    public Button playButton;
    public Button pauseButton;
    public Button nextButton;
    public Button prevButton;
    public Button repeatButton;
    public Button shuffleButton;

    [Header("Visual Effects")]
    public ParticleSystem musicParticles;
    public GameObject visualizerBarPrefab;
    public Transform visualizerContainer;

    private bool isPlayerOpen = false;
    private int currentTrackIndex = 0;
    private bool isPlaying = false;
    private bool isRepeat = false;
    private bool isShuffle = false;

    private GameObject[] visualizerBars;
    private const int VISUALIZER_BARS_COUNT = 64;

    void Start()
    {
        InitializePlayer();
        CreateVisualizer();
    }

    void InitializePlayer()
    {
        // Настройка кнопок
        musicToggleButton.onClick.AddListener(TogglePlayer);
        playButton.onClick.AddListener(PlayMusic);
        pauseButton.onClick.AddListener(PauseMusic);
        nextButton.onClick.AddListener(NextTrack);
        prevButton.onClick.AddListener(PreviousTrack);
        repeatButton.onClick.AddListener(ToggleRepeat);
        shuffleButton.onClick.AddListener(ToggleShuffle);

        // Настройка слайдеров
        progressSlider.onValueChanged.AddListener(SeekAudio);
        volumeSlider.onValueChanged.AddListener(SetVolume);

        // Инициализация
        playerPanel.SetActive(false);
        UpdateUI();
    }

    void CreateVisualizer()
    {
        visualizerBars = new GameObject[VISUALIZER_BARS_COUNT];
        for (int i = 0; i < VISUALIZER_BARS_COUNT; i++)
        {
            visualizerBars[i] = Instantiate(visualizerBarPrefab, visualizerContainer);
            visualizerBars[i].transform.localScale = new Vector3(1f, 0.1f, 1f);
        }
    }

    public void TogglePlayer()
    {
        isPlayerOpen = !isPlayerOpen;

        if (isPlayerOpen)
        {
            playerPanel.SetActive(true);
            playerAnimator.Play("PlayerSlideIn");
            StartCoroutine(AnimatePanelOpen());
        }
        else
        {
            playerAnimator.Play("PlayerSlideOut");
            StartCoroutine(AnimatePanelClose());
        }
    }

    IEnumerator AnimatePanelOpen()
    {
        // Анимация появления
        if (musicParticles != null)
            musicParticles.Play();

        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator AnimatePanelClose()
    {
        yield return new WaitForSeconds(0.3f);
        playerPanel.SetActive(false);
    }

    public void PlayMusic()
    {
        if (playlist.Length == 0) return;

        if (!audioSource.isPlaying)
        {
            if (audioSource.clip == null)
                audioSource.clip = playlist[currentTrackIndex];

            audioSource.Play();
            isPlaying = true;
            UpdatePlayPauseButtons();
            StartCoroutine(UpdateProgress());
        }
    }

    public void PauseMusic()
    {
        audioSource.Pause();
        isPlaying = false;
        UpdatePlayPauseButtons();
    }

    public void NextTrack()
    {
        if (isShuffle)
        {
            currentTrackIndex = Random.Range(0, playlist.Length);
        }
        else
        {
            currentTrackIndex = (currentTrackIndex + 1) % playlist.Length;
        }

        PlayNewTrack();
        StartCoroutine(AnimateTrackChange());
    }

    public void PreviousTrack()
    {
        currentTrackIndex = (currentTrackIndex - 1 + playlist.Length) % playlist.Length;
        PlayNewTrack();
        StartCoroutine(AnimateTrackChange());
    }

    void PlayNewTrack()
    {
        audioSource.clip = playlist[currentTrackIndex];
        if (isPlaying)
            audioSource.Play();

        UpdateUI();
    }

    IEnumerator AnimateTrackChange()
    {
        // Анимация смены трека
        albumArt.transform.localScale = Vector3.one * 0.8f;
        yield return new WaitForSeconds(0.1f);
        albumArt.transform.localScale = Vector3.one;
    }

    void ToggleRepeat()
    {
        isRepeat = !isRepeat;
        repeatButton.GetComponent<Image>().color = isRepeat ? Color.green : Color.white;
    }

    void ToggleShuffle()
    {
        isShuffle = !isShuffle;
        shuffleButton.GetComponent<Image>().color = isShuffle ? Color.green : Color.white;
    }

    void SeekAudio(float value)
    {
        if (audioSource.clip != null)
        {
            audioSource.time = value * audioSource.clip.length;
        }
    }

    void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    void UpdatePlayPauseButtons()
    {
        playButton.gameObject.SetActive(!isPlaying);
        pauseButton.gameObject.SetActive(isPlaying);
    }

    void UpdateUI()
    {
        if (playlist.Length > 0 && currentTrackIndex < playlist.Length)
        {
            songTitleText.text = playlist[currentTrackIndex].name;
        }
    }

    IEnumerator UpdateProgress()
    {
        while (isPlaying)
        {
            if (audioSource.clip != null && audioSource.clip.length > 0)
            {
                float progress = audioSource.time / audioSource.clip.length;
                progressSlider.value = progress;

                // Обновление времени
                currentTimeText.text = FormatTime(audioSource.time);
                totalTimeText.text = FormatTime(audioSource.clip.length);

                // Визуализатор
                UpdateVisualizer();
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void UpdateVisualizer()
    {
        float[] spectrum = new float[64];
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        for (int i = 0; i < VISUALIZER_BARS_COUNT; i++)
        {
            float scale = Mathf.Clamp(spectrum[i] * 50f, 0.1f, 2f);
            visualizerBars[i].transform.localScale =
                Vector3.Lerp(visualizerBars[i].transform.localScale,
                            new Vector3(1f, scale, 1f), Time.deltaTime * 10f);
        }
    }

    string FormatTime(float time)
    {
        int minutes = (int)time / 60;
        int seconds = (int)time % 60;
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void Update()
    {
        // Проверка окончания трека
        if (isPlaying && !audioSource.isPlaying)
        {
            if (isRepeat)
            {
                audioSource.Play();
            }
            else
            {
                NextTrack();
            }
        }

        // Горячие клавиши
        if (Input.GetKeyDown(KeyCode.M))
        {
            TogglePlayer();
        }
    }
}