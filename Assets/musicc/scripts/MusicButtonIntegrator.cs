using UnityEngine;
using UnityEngine.UI;

public class MusicButtonIntegrator : MonoBehaviour
{
    [Header("Integration Settings")]
    public string topPanelName = "TopPanel";
    public string bottomPanelName = "BottomPanel";

    [Header("Button Appearance")]
    public Sprite musicIcon;
    public Sprite musicIconPressed;
    public Vector2 buttonSize = new Vector2(80, 80);
    public Vector2 buttonPosition = new Vector2(-100, 0);

    private GameObject musicButton;
    private MusicPlayerManager musicPlayer;

    void Start()
    {
        CreateMusicButton();
        FindOrCreateMusicPlayer();
    }

    void CreateMusicButton()
    {
        // Ищем TopPanel для размещения кнопки
        Transform topPanel = GameObject.Find(topPanelName)?.transform;
        if (topPanel == null)
        {
            Debug.LogError($"TopPanel '{topPanelName}' not found!");
            return;
        }

        // Создаем кнопку
        musicButton = new GameObject("MusicPlayerButton");
        musicButton.transform.SetParent(topPanel, false);

        // Добавляем компоненты
        Image image = musicButton.AddComponent<Image>();
        image.sprite = musicIcon;
        image.color = Color.white;

        Button button = musicButton.AddComponent<Button>();
        button.transition = Selectable.Transition.SpriteSwap;

        // Настройка sprite states
        SpriteState spriteState = new SpriteState();
        spriteState.pressedSprite = musicIconPressed;
        button.spriteState = spriteState;

        button.onClick.AddListener(OnMusicButtonClick);

        // Настройка RectTransform
        RectTransform rt = musicButton.GetComponent<RectTransform>();
        rt.sizeDelta = buttonSize;
        rt.anchorMin = new Vector2(1, 1); // Верхний правый угол
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = buttonPosition;

        Debug.Log("Music button created successfully!");
    }

    void FindOrCreateMusicPlayer()
    {
        // Ищем существующий MusicPlayerManager
        musicPlayer = FindObjectOfType<MusicPlayerManager>();

        if (musicPlayer == null)
        {
            Debug.Log("MusicPlayerManager not found, creating new one...");
            CreateMusicPlayer();
        }
    }

    void CreateMusicPlayer()
    {
        // Создаем новый объект для плеера
        GameObject playerObj = new GameObject("MusicPlayer");
        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas != null)
        {
            playerObj.transform.SetParent(canvas.transform, false);
        }

        // Добавляем компонент
        musicPlayer = playerObj.AddComponent<MusicPlayerManager>();

        // Теперь нужно настроить все ссылки через код или создать префаб
        Debug.Log("MusicPlayer created. Please set up references in inspector.");
    }

    void OnMusicButtonClick()
    {
        if (musicPlayer != null)
        {
            musicPlayer.TogglePlayer();
        }
        else
        {
            Debug.LogWarning("MusicPlayer not found!");
        }
    }

    // Метод для ручной настройки из инспектора
    public void SetupPlayerReferences(MusicPlayerManager player)
    {
        musicPlayer = player;
        Debug.Log("MusicPlayer references set manually");
    }
}