using UnityEngine.Video;  // ← ДОБАВЬТЕ ЭТУ СТРОКУ
using UnityEngine;
using System.Collections;

public class VideoDiagnostic : MonoBehaviour
{
    IEnumerator Start()
    {
        Debug.Log("=== VIDEO DIAGNOSTIC ===");

        string videoFileName = "videos/background.mp4";
        string streamingPath = Application.streamingAssetsPath;

        Debug.Log($"StreamingAssets Path: {streamingPath}");
        Debug.Log($"Platform: {Application.platform}");

        // Проверяем полный путь к файлу
        string fullPath = System.IO.Path.Combine(streamingPath, videoFileName);
        Debug.Log($"Full Path: {fullPath}");
        Debug.Log($"File Exists: {System.IO.File.Exists(fullPath)}");

        // Проверяем содержимое папки StreamingAssets
        if (System.IO.Directory.Exists(streamingPath))
        {
            Debug.Log("Contents of StreamingAssets:");
            string[] allFiles = System.IO.Directory.GetFiles(streamingPath, "*", System.IO.SearchOption.AllDirectories);
            foreach (string file in allFiles)
            {
                Debug.Log($"  - {file}");
            }
        }
        else
        {
            Debug.LogError("StreamingAssets folder doesn't exist!");
        }

        yield return new WaitForSeconds(1f);

        // Тестируем VideoPlayer
        VideoPlayer vp = FindObjectOfType<VideoPlayer>();
        if (vp != null)
        {
            Debug.Log($"VideoPlayer URL: {vp.url}");
            Debug.Log($"Is Prepared: {vp.isPrepared}");
            Debug.Log($"Is Playing: {vp.isPlaying}");
        }

        Debug.Log("=== END DIAGNOSTIC ===");
    }
}