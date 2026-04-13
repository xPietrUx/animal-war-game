using UnityEngine;
using TMPro; // Ważne dla obsługi TextMeshPro

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Top Bar")]
    public TextMeshProUGUI blueBaseHPText;
    public TextMeshProUGUI redBaseHPText;

    [Header("Command Panel")]
    public TextMeshProUGUI currentPlayerText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI unitCostText;

    private void Awake()
    {
        // Singleton - dzięki temu TurnManager może napisać "UIManager.Instance"
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Tę funkcję wywołuje TurnManager przy zmianie tury
    public void UpdateTurnInfo(string playerName, int gold)
    {
        if (currentPlayerText != null)
        {
            string displayName = playerName == "Player1" ? "Player 1" : "Player 2";
            currentPlayerText.text = displayName;
        }
        if (goldText != null) goldText.text = "Gold: " + gold;
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