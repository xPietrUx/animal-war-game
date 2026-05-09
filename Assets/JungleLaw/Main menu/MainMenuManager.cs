using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("G£ÓWNE ELEMENTY")]
    public GameObject background;
    public GameObject mainMenuContent;

    [Header("UI GRY (DO UKRYCIA)")]
    public GameObject topBar;
    public GameObject commandPanel;

    [Header("PANELE PODSTRON")]
    public GameObject settingsPanel;
    public GameObject aboutPanel;
    public GameObject infoPanel;
    public GameObject quitPanel;

    void Awake()
    {
        // Gwarantujemy, ¿e interfejs w grze jest absolutnie wy³¹czony zanim "mignie" nam w pierwszej klatce.
        if (topBar != null) topBar.SetActive(false);
        if (commandPanel != null) commandPanel.SetActive(false);
        
        // Nie wy³¹czamy backgroundu tu jeszcze, jeœli ma stanowiæ t³o, samo ShowMainMenu to obs³u¿y
        background.SetActive(false);
        mainMenuContent.SetActive(false);
        HideAllPanels();
    }

    void Start()
    {
        if (SettingsMenu.shouldAutoStartGame)
        {
            SettingsMenu.shouldAutoStartGame = false;
            ShowGameMap();
            // WY£Aczamy ekrany ³adowania
            GetComponent<SimpleLoading>().loadingPanel.SetActive(false);
        }
        else
        {
            // W³¹czamy Loading na samym pocz¹tku klatki Start
            SimpleLoading loader = GetComponent<SimpleLoading>();
            loader.loadingPanel.SetActive(true);
            loader.StartStartupLoading();
        }
    }

    // --- TEJ FUNKCJI BRAKOWA£O! ---
    public void ShowMainMenu()
    {
        // Pokazuje g³ówne t³o i przyciski menu
        background.SetActive(true);
        mainMenuContent.SetActive(true);
    }
    // ------------------------------

    private void HideAllPanels()
    {
        settingsPanel.SetActive(false);
        aboutPanel.SetActive(false);
        infoPanel.SetActive(false);
        quitPanel.SetActive(false);
    }

    public void StartGame()
    {
        LoadingScreenManager.instance.LoadGameMapAsync();
    }

    public void OpenAbout()
    {
        HideAllPanels();
        mainMenuContent.SetActive(true);
        aboutPanel.SetActive(true);
    }

    public void OpenInfo()
    {
        HideAllPanels();
        mainMenuContent.SetActive(true);
        infoPanel.SetActive(true);
    }

    public void OpenQuitConfirmation()
    {
        HideAllPanels();
        quitPanel.SetActive(true);
    }

    public void BackToMenu()
    {
        HideAllPanels();
        mainMenuContent.SetActive(true);
    }

    public void OpenGitHub()
    {
        Application.OpenURL("https://github.com/xPietrUx/animal-war-game");
    }

    public void ConfirmQuit()
    {
        Debug.Log("Gra zosta³a wy³¹czona!");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ShowGameMap()
    {
        background.SetActive(false);
        mainMenuContent.SetActive(false);
        HideAllPanels();

        if (topBar != null) topBar.SetActive(true);
        if (commandPanel != null) commandPanel.SetActive(true);
    }
}