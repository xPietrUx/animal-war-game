using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    [Header("Okna Widoku")]
    public GameObject pauseOverlay;
    public GameObject replayConfirmWindow;
    public GameObject quitConfirmWindow;

    public GameObject loadingPanel;

    public static bool shouldAutoStartGame = false;
    
    // ZMIANA: Dodajemy now¹ zmienn¹
    public static bool returnedFromGame = false;

    public static SettingsMenu instance;

    private bool isPaused = false;

    void Start()
    {
        if (pauseOverlay != null) pauseOverlay.SetActive(false);
        if (replayConfirmWindow != null) replayConfirmWindow.SetActive(false);
        if (quitConfirmWindow != null) quitConfirmWindow.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (loadingPanel != null && loadingPanel != gameObject)
            loadingPanel.SetActive(false);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused) { Time.timeScale = 0f; pauseOverlay.SetActive(true); }
        else { ResumeGame(); }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseOverlay.SetActive(false);
    }

    // --- REPLAY ---
    public void OpenReplayConfirmation() => replayConfirmWindow.SetActive(true);

    public void ConfirmReplayYes()
    {
        shouldAutoStartGame = true; // Mówimy grze: "Po resecie odpal siê od razu"
        Time.timeScale = 1f;
        
        replayConfirmWindow.SetActive(false); // Chowamy okienko

        // Zamiast od razu ³adowaæ scenê, odpalamy animacjê ³adowania
        if (LoadingScreenManager.instance != null)
        {
            LoadingScreenManager.instance.gameObject.SetActive(true);
            // Wywo³ujemy asynchroniczne ³adowanie TA SAMEJ SCENY, na której jesteœmy
            LoadingScreenManager.instance.StartCoroutine(
                // Dodaj publiczn¹ metodê wrapper w LoadingScreenManager, np. LoadScenePublicCoroutine
                LoadingScreenManager.instance.LoadScenePublicCoroutine(SceneManager.GetActiveScene().name)
            );
        }
        else
        {
            // W razie jakby mened¿er nie istnia³ (awaryjnie)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void ConfirmReplayNo() => replayConfirmWindow.SetActive(false);

    // --- QUIT ---
    public void OpenQuitConfirmation() => quitConfirmWindow.SetActive(true);

    public void ConfirmQuitYes()
    {
        shouldAutoStartGame = false;
        
        // ZMIANA: Zaznaczamy, ¿e w³aœnie wyszliœmy z gry
        returnedFromGame = true; 
        
        Time.timeScale = 1f;

        if (LoadingScreenManager.instance != null)
        {
            LoadingScreenManager.instance.gameObject.SetActive(true);
            LoadingScreenManager.instance.LoadMainMenuAsync();
        }
        else
        {
            SceneManager.LoadScene("plansza"); 
        }
    }

    public void ConfirmQuitNo() => quitConfirmWindow.SetActive(false);
}