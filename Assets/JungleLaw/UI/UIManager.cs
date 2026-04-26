using UnityEngine;
using TMPro; // Ważne dla obsługi TextMeshPro

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public TextMeshProUGUI manaDisplay; // Tekst wyświetlający ilość many

    [Header("Top Bar")]
    public TextMeshProUGUI blueBaseHPText;
    public TextMeshProUGUI redBaseHPText;

    [Header("Command Panel")]
    public TextMeshProUGUI currentPlayerText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI unitCostText;

    [Header("Hover Costs")]
    public TextMeshProUGUI costGoldText; // Przeciągnij tu swój 'CostDisplay'
    public TextMeshProUGUI costManaText; // Przeciągnij tu swój 'CostManaDisplay'

    // Funkcja wywoływana, gdy najeżdżamy na przycisk
    public void UpdateHoverCosts(string goldCost, string manaCost)
    {
        if (costGoldText != null) costGoldText.text = "GOLD: " + goldCost;
        if (costManaText != null) costManaText.text = "MANA: " + manaCost;
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

        // Składamy piękny napis ze znakiem '+' i nawiasami
        if (goldText != null) goldText.text = $"GOLD: {goldAmount} (+{goldIncome})";

        if (manaDisplay != null) manaDisplay.text = $"MANA: {manaAmount} (+{manaIncome})";
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
}