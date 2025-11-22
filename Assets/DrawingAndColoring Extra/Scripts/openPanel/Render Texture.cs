using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class AutoVideoSetup : MonoBehaviour
{
    [Header("Video Settings")]
    public string videoFileName = "background.mp4";
    public int textureWidth = 1280;
    public int textureHeight = 720;

    private VideoPlayer videoPlayer;
    private RawImage rawImage;
    private RenderTexture renderTexture;

    void Start()
    {
        SetupVideoComponents();
    }

    void SetupVideoComponents()
    {
        // Получаем или добавляем необходимые компоненты
        GetOrAddComponents();

        // Создаем Render Texture
        CreateRenderTexture();

        // Настраиваем связь между компонентами
        SetupConnections();

        // Настраиваем VideoPlayer
        ConfigureVideoPlayer();

        Debug.Log("✅ Video setup completed automatically!");
    }

    void GetOrAddComponents()
    {
        // RawImage для отображения видео
        rawImage = GetComponent<RawImage>();
        if (rawImage == null)
        {
            rawImage = gameObject.AddComponent<RawImage>();
            Debug.Log("📺 RawImage component added");
        }

        // VideoPlayer для воспроизведения
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            Debug.Log("🎬 VideoPlayer component added");
        }
    }

    void CreateRenderTexture()
    {
        // Проверяем, нет ли уже связанного RenderTexture
        if (rawImage.texture != null && rawImage.texture is RenderTexture)
        {
            renderTexture = (RenderTexture)rawImage.texture;
            Debug.Log("🔄 Using existing RenderTexture");
            return;
        }

        // СОЗДАЕМ НОВЫЙ RENDER TEXTURE
        renderTexture = new RenderTexture(textureWidth, textureHeight, 24, RenderTextureFormat.ARGB32)
        {
            name = "AutoCreatedVideoRT",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            antiAliasing = 1,
            anisoLevel = 0,
            useMipMap = false
        };

        // ВАЖНО: Активируем RenderTexture в памяти
        renderTexture.Create();

        Debug.Log($"📹 RenderTexture created: {textureWidth}x{textureHeight}");
    }

    void SetupConnections()
    {
        // Связываем RawImage с RenderTexture
        rawImage.texture = renderTexture;
        rawImage.color = Color.white;

        // Связываем VideoPlayer с RenderTexture
        videoPlayer.targetTexture = renderTexture;

        Debug.Log("🔗 Components connected successfully");
    }

    void ConfigureVideoPlayer()
    {
        // Основные настройки VideoPlayer
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;
        videoPlayer.isLooping = true;

        // Настройки вывода
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.source = VideoSource.Url;

        // Устанавливаем путь к видео файлу
        SetVideoPath();

        Debug.Log("🎛️ VideoPlayer configured");
    }

    void SetVideoPath()
    {
        string videoPath;

#if UNITY_WEBGL && !UNITY_EDITOR
        // Для WebGL/Яндекс Игр
        videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
#else
        // Для редактора и других платформ
        videoPath = "file://" + System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
#endif

        videoPlayer.url = videoPath;
        Debug.Log($"📁 Video path set: {videoPath}");
    }

    // 📋 Публичные методы для управления видео
    public void PlayVideo()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying)
        {
            videoPlayer.Play();
            Debug.Log("▶️ Video playback started");
        }
    }

    public void StopVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            Debug.Log("⏹️ Video playback stopped");
        }
    }

    public void SetVideoAlpha(float alpha)
    {
        if (rawImage != null)
        {
            Color color = rawImage.color;
            color.a = alpha;
            rawImage.color = color;
        }
    }

    // 🔧 Метод для ручного вызова настройки из инспектора
    [ContextMenu("Setup Video Now")]
    void SetupVideoNow()
    {
        SetupVideoComponents();
    }

    [ContextMenu("Debug Video Info")]
    void DebugVideoInfo()
    {
        Debug.Log($"=== VIDEO DEBUG INFO ===");
        Debug.Log($"Video Player: {videoPlayer != null}");
        Debug.Log($"Raw Image: {rawImage != null}");
        Debug.Log($"Render Texture: {renderTexture != null}");
        Debug.Log($"Video URL: {videoPlayer?.url}");
        Debug.Log($"Is Playing: {videoPlayer?.isPlaying}");
        Debug.Log($"RenderTexture Size: {renderTexture?.width}x{renderTexture?.height}");
    }

    // 🧹 Очистка при уничтожении объекта
    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
            Debug.Log("🧹 RenderTexture cleaned up");
        }
    }
}