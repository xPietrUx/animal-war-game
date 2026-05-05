using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    [Header("Okna Widoku (Przeci¹gnij z Hierarchy)")]
    public GameObject pauseOverlay;       // Tu przeci¹gnij okno z napisem PAUZA
    public GameObject replayConfirmWindow; // Tu przeci¹gnij okno "Are you sure?" dla Replay
    public GameObject quitConfirmWindow;   // Tu przeci¹gnij okno "Are you sure?" dla Quit

    private bool isPaused = false;

    // --- PAUZA ---
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // Zamra¿a grê
            pauseOverlay.SetActive(true); // Pokazuje czarne t³o i napis PAUZA
        }
        else
        {
            Time.timeScale = 1f; // Wznawia grê
            pauseOverlay.SetActive(false); // Ukrywa pauzê
        }
    }

    // --- REPLAY ---
    public void OpenReplayConfirmation()
    {
        // Ta funkcja tylko POKAZUJE okno z pytaniem
        replayConfirmWindow.SetActive(true);
    }

    public void ConfirmReplay(bool wantToReplay)
    {
        // Ta funkcja jest podpiêta pod przyciski TAK i NIE w okienku Replay
        if (wantToReplay)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            // Jeœli klikniesz NIE, tylko ukrywa okienko
            replayConfirmWindow.SetActive(false);
        }
    }

    // --- QUIT ---
    public void OpenQuitConfirmation()
    {
        // Ta funkcja tylko POKAZUJE okno z pytaniem
        quitConfirmWindow.SetActive(true);
    }

    public void ConfirmQuit(bool wantToQuit)
    {
        // Ta funkcja jest podpiêta pod przyciski TAK i NIE w okienku Quit
        if (wantToQuit)
        {
            Time.timeScale = 1f;
            // UWAGA: Upewnij siê, ¿e wpisujesz tu dok³adn¹ nazwê sceny swojego Menu G³ównego!
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            // Jeœli klikniesz NIE, tylko ukrywa okienko
            quitConfirmWindow.SetActive(false);
        }
    }
}