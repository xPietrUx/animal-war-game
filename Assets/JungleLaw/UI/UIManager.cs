using UnityEngine; // WAŻNE: Upewnij się, że masz to na samej górze pliku!
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public TextMeshProUGUI manaDisplay; // Tekst wyświetlający ilość many

    [Header("Audio System (SFX)")]
    public AudioSource sfxSource; // Głośnik do strzałów/kliknięć

    // Taśmy z nagraniami
    public AudioClip coinSound;
    public AudioClip clickSound;
    public AudioClip hurtSound;
    public AudioClip deadSound;
    public AudioClip jumpSound;

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    [Header("Top Bar")]
    public TextMeshProUGUI blueBaseHPText;
    public TextMeshProUGUI redBaseHPText;

    [Header("Command Panel")]
    public TextMeshProUGUI currentPlayerText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI unitCostText;
    public Image currentPlayerImage; // <--- Miejsce na komponent obrazka na ekranie
    public Sprite p1TurnGraphic;     // <--- Plik graficzny dla Gracza 1
    public Sprite p2TurnGraphic;     // <--- Plik graficzny dla Gracza  2

    [Header("Hover Costs")]
    public TextMeshProUGUI costGoldText; // Przeciągnij tu swój 'CostDisplay'
    public TextMeshProUGUI costManaText; // Przeciągnij tu swój 'CostManaDisplay'

    [Header("Komunikaty Błędów")]
    public GameObject warningMessageGraphic;

    // DODANO: Zmienna do zapisywania i bezpiecznego resetowania odliczania
    private Coroutine warningRoutine;

    // Funkcja wywoływana, gdy najeżdżamy na przycisk
    public void UpdateHoverCosts(string goldCost, string manaCost)
    {
        if (costGoldText != null) costGoldText.text = goldCost;
        if (costManaText != null) costManaText.text = manaCost;
    }

    private void Awake()
    {
        // Singleton - dzięki temu TurnManager może napisać "UIManager.Instance"
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Dodaliśmy dwie nowe zmienne: goldIncome i manaIncome
    public void UpdateTurnInfo(string playerName, int goldAmount, int goldIncome, int manaAmount, int manaIncome)
    {
        if (currentPlayerText != null) currentPlayerText.text = playerName;
        if (currentPlayerImage != null)
        {
            if (playerName == "PLAYER 1") currentPlayerImage.sprite = p1TurnGraphic;
            else if (playerName == "PLAYER 2") currentPlayerImage.sprite = p2TurnGraphic;
        }

        // Składamy piękny napis ze znakiem '+' i nawiasami
        if (goldText != null) goldText.text = $"{goldAmount} (+{goldIncome})";

        if (manaDisplay != null) manaDisplay.text = $"{manaAmount} (+{manaIncome})";
    }

    // Funkcja do aktualizacji tekstu kosztu
    public void UpdateCostDisplay(string costText)
    {
        if (unitCostText != null)
        {
            unitCostText.text = "COST: " + costText;
        }
    }

    // Funkcja do aktualizacji HP baz
    public void UpdateBaseHP(int blueHP, int redHP)
    {
        if (blueBaseHPText != null) blueBaseHPText.text = blueHP.ToString();
        if (redBaseHPText != null) redBaseHPText.text = redHP.ToString();
    }

    // ZMIENIONO: Bezpieczniejszy sposób kontroli znikającej grafiki
    public void ShowWarningMessage()
    {
        if (warningMessageGraphic != null)
        {
            // Jeśli odliczanie już trwa, przerywamy je żeby wyzerować czas
            if (warningRoutine != null)
            {
                StopCoroutine(warningRoutine);
            }

            // Odpalamy licznik na nowo i zapisujemy go do pamięci
            warningRoutine = StartCoroutine(HideWarningRoutine());
        }
    }

    // Coroutine, która gasi grafikę po X sekundach
    private IEnumerator HideWarningRoutine()
    {
        warningMessageGraphic.SetActive(true);

        // ZMIENIONO: Używamy "Realtime", aby czas mijał nawet jeśli gra laguje
        yield return new WaitForSecondsRealtime(2f);

        warningMessageGraphic.SetActive(false);
    }
}