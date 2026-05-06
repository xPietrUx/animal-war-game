using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    [Header("Okna Widoku")]
    public GameObject pauseOverlay;
    public GameObject replayConfirmWindow;
    public GameObject quitConfirmWindow;

    // Statyczna zmienna, która "prze¿yje" prze³adowanie sceny
    public static bool shouldAutoStartGame = false;

    private bool isPaused = false;

    void Start()
    {
        if (pauseOverlay != null) pauseOverlay.SetActive(false);
        if (replayConfirmWindow != null) replayConfirmWindow.SetActive(false);
        if (quitConfirmWindow != null) quitConfirmWindow.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
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
        shouldAutoStartGame = false; // Mówimy grze: "Po resecie idŸ do menu"
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ConfirmQuitNo() => quitConfirmWindow.SetActive(false);
}