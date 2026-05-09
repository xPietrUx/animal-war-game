using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    [Header("Okna Widoku")]
    public GameObject pauseOverlay;
    public GameObject replayConfirmWindow;
    public GameObject quitConfirmWindow;

    // Dodano pole loadingPanel, aby naprawiæ b³¹d CS0103
    public GameObject loadingPanel;

    // Statyczna zmienna, która "prze¿yje" prze³adowanie sceny
    public static bool shouldAutoStartGame = false;

    // Dodano statyczne pole instance
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ConfirmReplayNo() => replayConfirmWindow.SetActive(false);

    // --- QUIT ---
    public void OpenQuitConfirmation() => quitConfirmWindow.SetActive(true);

    public void ConfirmQuitYes()
    {
        shouldAutoStartGame = false;
        Time.timeScale = 1f;

        if (LoadingScreenManager.instance != null)
        {
            LoadingScreenManager.instance.gameObject.SetActive(true);
            LoadingScreenManager.instance.LoadMainMenuAsync();
        }
        else
        {
            // Zmiana z "MainMenu" na nazwê Twojej w³aœciwej sceny:
            SceneManager.LoadScene("plansza"); 
        }
    }

    public void ConfirmQuitNo() => quitConfirmWindow.SetActive(false);
}