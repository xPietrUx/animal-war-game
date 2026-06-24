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
        // 1. Zabezpieczamy "migniêcie" starej klatki przypisuj¹c pierwsz¹ ZANIM w³¹czymy ekran
        if (frames != null && frames.Length > 0 && animatedImage != null)
        {
            animatedImage.sprite = frames[0];
        }

        // 2. Natychmiastowe w³¹czenie panelu ³adowania z przygotowan¹ 1 klatk¹
        loadingPanel.SetActive(true);
        
        // 3. Odtwarzamy klatki, u¿ywaj¹c czasu niezale¿nego od lagów (Realtime)
        for (int i = 0; i < frames.Length; i++)
        {
            animatedImage.sprite = frames[i];
            yield return new WaitForSecondsRealtime(timePerFrame);
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

        // Chowamy sam ekran ³adowania dopiero, gdy warstwa pod spodem u³o¿y siê poprawnie
        loadingPanel.SetActive(false);
    }
}