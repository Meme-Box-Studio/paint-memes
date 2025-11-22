using UnityEngine;
using System.Collections;

public class AudioVisualizer : MonoBehaviour
{
    public UIManager uiManager;
    public float sensitivity = 50f;
    public float rotationSpeed = 30f;

    private void Update()
    {
        if (uiManager != null && uiManager.audioSource.isPlaying)
        {
            AnimateAlbumArt();
        }
    }

    void AnimateAlbumArt()
    {
        // Плавное вращение обложки альбома
        if (uiManager.albumArt != null)
        {
            uiManager.albumArt.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }
}