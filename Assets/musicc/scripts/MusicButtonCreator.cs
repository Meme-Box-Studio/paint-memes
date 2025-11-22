// MusicButtonCreator.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicButtonCreator : MonoBehaviour
{
    [Header("Settings")]
    public string parentPanelName = "TopPanel";
    public Vector2 buttonPosition = new Vector2(-100, 0);

    private GameObject musicButton;
    private MusicPlayerManager musicPlayer;

    void Start()
    {
        CreateMusicButton();
        FindMusicPlayer();
    }

    void CreateMusicButton()
    {
        // Находим панель для размещения кнопки
        Transform parentPanel = GameObject.Find(parentPanelName)?.transform;
        if (parentPanel == null)
        {
            Debug.LogError($"Panel '{parentPanelName}' not found!");
            return;
        }

        // Создаем кнопку
        musicButton = new GameObject("MusicToggleButton");
        musicButton.transform.SetParent(parentPanel, false);

        // Добавляем компоненты
        Image image = musicButton.AddComponent<Image>();
        image.color = Color.white;

        Button button = musicButton.AddComponent<Button>();
        button.onClick.AddListener(ToggleMusicPlayer);

        // Настраиваем RectTransform
        RectTransform rt = musicButton.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 80);
        rt.anchorMin = new Vector2(1, 1); // Верхний правый угол
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = buttonPosition;

        // Добавляем текст или иконку
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(musicButton.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "🎵";
        text.fontSize = 36;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Debug.Log("Music button created successfully!");
    }

    void FindMusicPlayer()
    {
        musicPlayer = FindObjectOfType<MusicPlayerManager>();
        if (musicPlayer == null)
        {
            Debug.LogError("MusicPlayerManager not found! Make sure the player prefab is in the scene.");
        }
    }

    void ToggleMusicPlayer()
    {
        if (musicPlayer != null)
        {
            musicPlayer.TogglePlayer();
        }
        else
        {
            Debug.LogWarning("Music player not found!");
        }
    }
}