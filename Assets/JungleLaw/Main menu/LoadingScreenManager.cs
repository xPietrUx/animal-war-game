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
    public string mainMenuSceneName = "MainMenu";
    public string gameSceneName = "GameScene";

    [Header("Czas trwania (Sekundy)")]
    public float minimumLoadTime = 1.5f; // Gwarantujemy, ¿e ekran powisi chocia¿ 1.5 sekundy

    public static LoadingScreenManager instance;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        loadingPanel.SetActive(false);
    }

    public void LoadMainMenuAsync()
    {
        StartCoroutine(LoadSceneCoroutine(mainMenuSceneName));
    }

    public void LoadGameMapAsync()
    {
        Debug.Log("PUK PUK! Przycisk wywo³a³ LoadingScreenManager!");
        StartCoroutine(LoadSceneCoroutine(gameSceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneToLoad)
    {
        loadingPanel.SetActive(true);

        if (frames.Length > 0)
        {
            animatedImage.sprite = frames[0];
            Debug.Log("ANIMACJA START: Klatka 1"); // Log startowy
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        float elapsedTime = 0f;

        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;

            // 1. Prawdziwy postêp wczytywania plików przez Unity (0.0 do 1.0)
            float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // 2. Nasz sztuczny stoper (0.0 do 1.0 przez okreœlony czas, np. 1.5 sek)
            float timeProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);

            // Wybieramy MNIEJSZ¥ wartoœæ. 
            // Dziêki temu nawet jak gra wgra siê w 0.1s, animacja grzecznie poczeka na stoper.
            float currentProgress = Mathf.Min(loadProgress, timeProgress);

            if (frames.Length > 0)
            {
                int currentFrame = Mathf.FloorToInt(currentProgress * (frames.Length - 1));

                // Aktualizujemy grafikê i LOGUJEMY tylko w momencie zmiany klatki
                if (animatedImage.sprite != frames[currentFrame])
                {
                    animatedImage.sprite = frames[currentFrame];
                    Debug.Log("ZMIANA ANIMACJI: Wyœwietlam klatkê " + (currentFrame + 1));
                }
            }

            // Pozwalamy wejœæ do gry dopiero, gdy i pliki siê wgra³y, i stoper dobi³ do koñca
            if (loadProgress >= 1f && timeProgress >= 1f)
            {
                if (frames.Length > 0 && animatedImage.sprite != frames[frames.Length - 1])
                {
                    animatedImage.sprite = frames[frames.Length - 1];
                    Debug.Log("ANIMACJA KONIEC: Ostatnia klatka");
                }

                yield return new WaitForSeconds(0.4f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        loadingPanel.SetActive(false);
    }
}