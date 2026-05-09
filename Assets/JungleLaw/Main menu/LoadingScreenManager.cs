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
        
        // Zatrzymujemy korutynê SimpleLoading
        SimpleLoading simpleLoading = GetComponent<SimpleLoading>();
        if (simpleLoading != null)
        {
            simpleLoading.StopAllCoroutines();
            if (simpleLoading.loadingPanel != null)
                simpleLoading.loadingPanel.SetActive(false);
        }
        
        StartCoroutine(LoadSceneCoroutine(gameSceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneToLoad)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        // Czekamy jedn¹ klatkê, ¿eby panel siê wyrenderowa³
        yield return null;

        // Odtwarzamy animacjê OD POCZ¥TKU (i=0)
        if (frames != null && frames.Length > 0 && animatedImage != null)
        {
            float timePerFrame = minimumLoadTime / frames.Length;
            for (int i = 0; i < frames.Length; i++)
            {   
                animatedImage.sprite = frames[i];
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