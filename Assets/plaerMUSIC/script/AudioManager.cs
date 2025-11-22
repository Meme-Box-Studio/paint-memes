using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public Sprite soundOn, soundOff;
    private Image buttonImg;
    private bool audioOn = true;

    void Start()
    {
        buttonImg = GetComponent<Image>();
        UpdateIcon();
    }

    public void ToggleSound()
    {
        audioOn = !audioOn;
        AudioListener.volume = audioOn ? 1f : 0f;
        UpdateIcon();
    }

    void UpdateIcon()
    {
        if (buttonImg != null && soundOn != null && soundOff != null)
            buttonImg.sprite = audioOn ? soundOn : soundOff;
    }
}