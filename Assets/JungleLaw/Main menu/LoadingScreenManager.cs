using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("Wizualne elementy UI")]
    public GameObject loadingPanel;
    public Image animatedImage;

    [Header("Klatki Animacji (Od 1 do 5)")]
    public Sprite[] frames;

    [Header("Ustawienia scen")]
    public string mainMenuSceneName = "plansza";
    public string gameSceneName = "GameScene";

    [Header("Czas trwania (Sekundy)")]
    public float minimumLoadTime = 1.5f; // Gwarantujemy, ¿e ekran powisi chocia¿ 1.5 sekundy

    public static LoadingScreenManager instance;

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

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    public void LoadMainMenuAsync()
    {
        // Ignorujemy zapisan¹ w edytorze wartoœæ i wymuszamy poprawn¹ nazwê
        StartCoroutine(LoadSceneCoroutine("plansza"));
    }

    public void LoadGameMapAsync()
    {
        Debug.Log("PUK PUK! Przycisk wywo³a³ LoadingScreenManager!");
        StartCoroutine(LoadSceneCoroutine(gameSceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneToLoad)
    {
        // Podmieniamy obrazek na pierwsz¹ klatkê ZANIM w³¹czymy widocznoœæ panelu
        if (frames != null && frames.Length > 0 && animatedImage != null)
        {
            animatedImage.sprite = frames[0];
        }

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        // Zabezpieczamy przed NullReferenceException, sprawdzaj¹c czy zmienne s¹ przypisane
        if (frames != null && frames.Length > 0 && animatedImage != null)
        {
            float timePerFrame = minimumLoadTime / frames.Length;
            for (int i = 0; i < frames.Length; i++)
            {
                animatedImage.sprite = frames[i];
                // U¿ywamy czasu rzeczywistego (odpornego na pauzê / lagi)
                yield return new WaitForSecondsRealtime(timePerFrame);
            }
        }

        // Dopiero po animacji uruchamiamy asynchroniczne ³adowanie sceny
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        
        while (operation != null && !operation.isDone)
        {
            yield return null;
        }

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }
}