using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;

public class SimpleVideoBackground : MonoBehaviour
{
    [Header("Video Settings")]
    public string videoFileName = "videos/background.mp4";
    public RawImage videoDisplay; // Ссылка на RawImage на вашей панели плеера

    [Header("Camera Settings")]
    public Camera videoCamera; // Камера для рендеринга видео

    void Start()
    {
        SetupVideoPlayer();
        PlayVideo();
    }

    void SetupVideoPlayer()
    {
        VideoPlayer videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
            videoPlayer = gameObject.AddComponent<VideoPlayer>();

        string videoPath = Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.url = videoPath;

        // ИСПРАВЛЕНИЕ: Используем RenderTexture вместо CameraFarPlane
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        // Создаем RenderTexture
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
        videoPlayer.targetTexture = renderTexture;

        // Присваиваем RenderTexture вашему UI элементу
        if (videoDisplay != null)
        {
            videoDisplay.texture = renderTexture;
            // Настраиваем отображение (растягиваем на всю панель)
            RectTransform rectTransform = videoDisplay.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
        else
        {
            Debug.LogError("✗ Video Display RawImage not assigned!");
        }

        videoPlayer.aspectRatio = VideoAspectRatio.Stretch;
        videoPlayer.isLooping = true;
        videoPlayer.playOnAwake = true;

        // Проверка файла
        if (File.Exists(videoPath))
        {
            Debug.Log("✓ Video file found: " + videoPath);
        }
        else
        {
            Debug.LogError("✗ Video file not found: " + videoPath);
        }
    }

    void PlayVideo()
    {
        VideoPlayer videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer != null)
            videoPlayer.Play();
    }

    void OnDestroy()
    {
        VideoPlayer videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer != null && videoPlayer.targetTexture != null)
        {
            videoPlayer.targetTexture.Release();
        }
    }
}