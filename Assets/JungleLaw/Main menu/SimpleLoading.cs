using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimpleLoading : MonoBehaviour
{
    [Header("Co mam wyœwietlaæ?")]
    public GameObject loadingPanel;
    public Image animatedImage;
    public Sprite[] frames;

    [Header("Ustawienia")]
    public float timePerFrame = 0.3f;

    // Usunêliœmy st¹d void Start(), ¿eby skrypty siê nie "k³óci³y" o to, kto startuje pierwszy!

    // --- DROGA 1: DO MENU (Odpala siê sama po w³¹czeniu gry) ---
    public void StartStartupLoading()
    {
        StartCoroutine(PlayAnimation(false)); // false = nie ³aduj gry, ³aduj menu
    }

    // --- DROGA 2: DO GRY (Odpala siê po klikniêciu przycisku START) ---
    public void StartFakeLoading()
    {
        StartCoroutine(PlayAnimation(true)); // true = za³aduj mapê gry
    }

    // --- G£ÓWNA ANIMACJA ---
    private IEnumerator PlayAnimation(bool loadGame)
    {
        // W³¹czamy czarny ekran
        loadingPanel.SetActive(true);

        // Odtwarzamy 5 klatek
        for (int i = 0; i < frames.Length; i++)
        {
            animatedImage.sprite = frames[i];
            yield return new WaitForSeconds(timePerFrame);
        }

        // DECYZJA: Co pokazujemy na koñcu?
        if (loadGame)
        {
            GetComponent<MainMenuManager>().ShowGameMap(); // W³¹cz planszê
        }
        else
        {
            GetComponent<MainMenuManager>().ShowMainMenu(); // W³¹cz menu
        }

        // Chowamy sam ekran ³adowania
        loadingPanel.SetActive(false);
    }
}