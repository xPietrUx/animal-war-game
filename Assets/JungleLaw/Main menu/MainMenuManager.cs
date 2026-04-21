using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("G£ÓWNE ELEMENTY")]
    public GameObject background;
    public GameObject mainMenuContent;

    [Header("UI GRY (DO UKRYCIA)")]
    public GameObject topBar;          // Górny pasek z HP
    public GameObject commandPanel;    // Dolny pasek ze z³otem i jednostkami

    [Header("PANELE PODSTRON")]
    public GameObject settingsPanel;
    public GameObject aboutPanel;
    public GameObject infoPanel;
    public GameObject quitPanel;

    void Start()
    {
        // 1. Na starcie gry chowamy absolutnie wszystko (paski gry i menu)
        if (topBar != null) topBar.SetActive(false);
        if (commandPanel != null) commandPanel.SetActive(false);
        background.SetActive(false);
        mainMenuContent.SetActive(false);
        HideAllPanels();

        // 2. Odpalamy pocz¹tkow¹ animacjê z drugiego skryptu
        GetComponent<SimpleLoading>().StartStartupLoading();
    }

    public void ShowMainMenu()
    {
        // Ta funkcja w³¹czy g³ówne menu po zakoñczeniu animacji startowej
        background.SetActive(true);
        mainMenuContent.SetActive(true);
    }

    private void HideAllPanels()
    {
        settingsPanel.SetActive(false);
        aboutPanel.SetActive(false);
        infoPanel.SetActive(false);
        quitPanel.SetActive(false);
    }

    public void StartGame()
    {
        // Chowamy menu...
        //background.SetActive(false);
        //mainMenuContent.SetActive(false);
        //HideAllPanels();

        // ...i W£¥CZAMY interfejs gry!
        //if (topBar != null) topBar.SetActive(true);
        //if (commandPanel != null) commandPanel.SetActive(true);
        LoadingScreenManager.instance.LoadGameMapAsync();
    }

    public void OpenAbout()
    {
        HideAllPanels();
        mainMenuContent.SetActive(true);
        aboutPanel.SetActive(true); // W³¹cza okno About
    }

    public void OpenInfo()
    {
        HideAllPanels();
        mainMenuContent.SetActive(true);
        infoPanel.SetActive(true); // W³¹cza okno Instrukcji
    }

    public void OpenQuitConfirmation()
    {
        HideAllPanels();
        //mainMenuContent.SetActive(false);
        quitPanel.SetActive(true); // W³¹cza okno Wyjœcia
    }

    public void BackToMenu()
    {
        HideAllPanels();
        mainMenuContent.SetActive(true);
    }

    public void OpenGitHub()
    {
        // Ta funkcja otwiera podany adres URL w domyœlnej przegl¹darce gracza
        Application.OpenURL("https://github.com/xPietrUx/animal-war-game");
    }

    public void ConfirmQuit()
    {
        Debug.Log("Gra zosta³a wy³¹czona!");
        // Zamyka grê u graczy (po zbudowaniu pliku .exe)
        Application.Quit();

        // Odklikuje ikonkê Play tylko w Twoim Unity Editor (dla testów)
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void ShowGameMap()
    {
        // Chowamy menu...
        background.SetActive(false);
        mainMenuContent.SetActive(false);
        HideAllPanels();

        // ...i W£¥CZAMY interfejs gry!
        if (topBar != null) topBar.SetActive(true);
        if (commandPanel != null) commandPanel.SetActive(true);
    }
}