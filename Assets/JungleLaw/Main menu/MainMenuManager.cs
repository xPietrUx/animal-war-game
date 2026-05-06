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

    void Start()
    {
        // Sprawdzamy, czy przyszliœmy tu z przycisku REPLAY
        if (SettingsMenu.shouldAutoStartGame)
        {
            // Resetujemy zmienn¹, ¿eby przy kolejnym w³¹czeniu gry nie zapêtli³o auto-startu
            SettingsMenu.shouldAutoStartGame = false;

            // Odpalamy bezpoœrednio mapê (u¿ywaj¹c Twojej funkcji)
            ShowGameMap();

            // Jeœli Twoja gra wymaga specyficznego resetu danych, 
            // upewnij siê, ¿e ShowGameMap() lub inna funkcja to robi.
        }
        else
        {
            // Standardowy start - pokazujemy menu i animacjê ³adowania
            if (topBar != null) topBar.SetActive(false);
            if (commandPanel != null) commandPanel.SetActive(false);
            background.SetActive(false);
            mainMenuContent.SetActive(false);
            HideAllPanels();

            GetComponent<SimpleLoading>().StartStartupLoading();
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