using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Grafiki Zwyciêstwa")]
    [SerializeField] private GameObject team1WinGraphic;
    [SerializeField] private GameObject team2WinGraphic;
    [SerializeField] private GameObject drawGraphic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void ShowGameOver(string winnerName)
    {
        Debug.Log("Aktywacja ekranu wygranej dla: " + winnerName);

        if (team1WinGraphic != null) team1WinGraphic.SetActive(false);
        if (team2WinGraphic != null) team2WinGraphic.SetActive(false);
        if (drawGraphic != null) drawGraphic.SetActive(false);

        if (winnerName == "REMIS" && drawGraphic != null) drawGraphic.SetActive(true);
        else if (winnerName == "Gracz 1" && team1WinGraphic != null) team1WinGraphic.SetActive(true);
        else if (winnerName == "Gracz 2" && team2WinGraphic != null) team2WinGraphic.SetActive(true);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
        }

        Time.timeScale = 0f;
    }

    // --- REPLAY (Z ANIMACJ¥) ---
    public void RestartGame()
    {
        SettingsMenu.shouldAutoStartGame = true;
        Time.timeScale = 1f;

        if (LoadingScreenManager.instance != null)
        {
            LoadingScreenManager.instance.gameObject.SetActive(true);
            // Wywo³ujemy korutynê dla obecnej sceny
            LoadingScreenManager.instance.StartCoroutine(
                LoadingScreenManager.instance.LoadScenePublicCoroutine(SceneManager.GetActiveScene().name)
            );
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // --- QUIT / BACK TO MENU (Z ANIMACJ¥) ---
    public void BackToMenu()
    {
        SettingsMenu.shouldAutoStartGame = false;
        SettingsMenu.returnedFromGame = true;
        Time.timeScale = 1f;

        if (LoadingScreenManager.instance != null)
        {
            // Aktywujemy obiekt mened¿era ³adowania
            LoadingScreenManager.instance.gameObject.SetActive(true);

            // ZMIANA: U¿ywamy StartCoroutine i LoadScenePublicCoroutine zamiast LoadMainMenuAsync,
            // aby wymusiæ odtworzenie animacji wejœcia (identycznie jak w Replay)
            LoadingScreenManager.instance.StartCoroutine(
                LoadingScreenManager.instance.LoadScenePublicCoroutine("plansza")
            );
        }
        else
        {
            SceneManager.LoadScene("plansza");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}