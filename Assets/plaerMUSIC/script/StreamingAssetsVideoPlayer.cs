using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class VideoPlayerController : MonoBehaviour
{
    [Header("Video Settings")]
    public string videoFileName = "background.mp4";
    public bool playOnStart = true;
    public bool loop = true;

    [Header("References")]
    public RawImage videoDisplay;

    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;
    private bool isPrepared = false;

    void Awake()
    {
        SetupVideoPlayer();
    }

    void SetupVideoPlayer()
    {
        // Создаем VideoPlayer
        videoPlayer = gameObject.AddComponent<VideoPlayer>();

        // Создаем RenderTexture
        renderTexture = new RenderTexture(1920, 1080, 24);
        renderTexture.name = "VideoRT";

        // Настраиваем отображение
        if (videoDisplay != null)
        {
            videoDisplay.texture = renderTexture;
        }

        // Настраиваем VideoPlayer
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = loop;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.skipOnDrop = true;

        // Подписываемся на события
        videoPlayer.prepareCompleted += OnVideoReady;
        videoPlayer.errorReceived += OnVideoError;

        Debug.Log("VideoPlayer настроен");
    }

    void Start()
    {
        if (playOnStart)
        {
            StartCoroutine(StartVideoDelayed());
        }
    }

    IEnumerator StartVideoDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        PlayVideo();
    }

    public void PlayVideo()
    {
        if (videoPlayer != null)
        {
            StartCoroutine(PlayVideoAsync());
        }
    }

    IEnumerator PlayVideoAsync()
    {
        if (!isPrepared)
        {
            yield return StartCoroutine(LoadAndPlayVideo());
        }
        else
        {
            videoPlayer.Play();
            Debug.Log("Video playback started");
        }
    }

    private IEnumerator LoadAndPlayVideo()
    {
        // Для Яндекс.Игр используем специальный путь
        string videoPath = GetVideoPathForPlatform();
        Debug.Log($"Loading video from: {videoPath}");

        videoPlayer.url = videoPath;
        videoPlayer.Prepare();

        yield return new WaitUntil(() => isPrepared || videoPlayer.isPrepared);

        if (isPrepared)
        {
            videoPlayer.Play();
            Debug.Log("Video playback started successfully");
        }
        else
        {
            Debug.LogError("Video failed to prepare");
        }
    }

    private string GetVideoPathForPlatform()
    {
        string videoPath;

#if UNITY_WEBGL && !UNITY_EDITOR
        // Для Яндекс.Игр и WebGL
        videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
        
        // Проверяем наличие папки Videos
        if (!FileExistsInWebGL(videoPath))
        {
            videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Videos", videoFileName);
        }
#else
        // Для редактора и других платформ
        videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);

        if (!System.IO.File.Exists(videoPath))
        {
            videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Videos", videoFileName);
        }
#endif

        return videoPath;
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    private bool FileExistsInWebGL(string path)
    {
        // В WebGL мы не можем проверить существование файла обычным способом
        // Пытаемся загрузить файл и проверяем ошибки
        try
        {
            // Создаем запрос для проверки существования файла
            var request = UnityEngine.Networking.UnityWebRequest.Get(path);
            request.SendWebRequest();
            
            // Ждем немного для получения ответа
            var startTime = Time.time;
            while (!request.isDone && Time.time - startTime < 1f)
            {
                // Ждем завершения запроса
            }
            
            bool exists = !request.isNetworkError && !request.isHttpError;
            request.Dispose();
            return exists;
        }
        catch
        {
            return false;
        }
    }
#endif

    public void StopVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            isPrepared = false;
        }
    }

    void OnVideoReady(VideoPlayer source)
    {
        isPrepared = true;
        Debug.Log($"Video prepared: {source.url}");

        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(true);
        }
    }

    void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"Video error: {message}");
        Debug.LogError($"Tried to play: {source.url}");

        // Дополнительная диагностика для WebGL
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.LogError("WebGL video loading error - check:");
        Debug.LogError("1. Video file is in StreamingAssets folder");
        Debug.LogError("2. Video format is supported (mp4, webm)");
        Debug.LogError("3. File name and extension are correct");
        Debug.LogError($"StreamingAssets path: {Application.streamingAssetsPath}");
#endif
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoReady;
            videoPlayer.errorReceived -= OnVideoError;
        }

        if (renderTexture != null)
        {
            renderTexture.Release();
        }
    }

    public bool IsPlaying
    {
        get { return videoPlayer != null && videoPlayer.isPlaying; }
    }

    public bool IsPrepared
    {
        get { return isPrepared; }
    }
}