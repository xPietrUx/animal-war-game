using UnityEngine;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public enum TurnState { Player1, Player2 }
    public TurnState currentTurn;

    [Header("Player 1 Stats")]
    public int p1_HP = 100;
    public int p1_Gold = 50;

    [Header("Player 2 Stats")]
    public int p2_HP = 100;
    public int p2_Gold = 50;

    [Header("Economy")]
    public int baseGoldPerTurn = 30; // Podstawowe złoto, które dostaje się zawsze

    private void Awake() => Instance = this;

    void Start()
    {
        currentTurn = TurnState.Player1;
        StartTurn();

    }

    public void EndTurn()
    {
        // Zmiana gracza
        currentTurn = (currentTurn == TurnState.Player1) ? TurnState.Player2 : TurnState.Player1;

        Debug.Log("Teraz tura: " + currentTurn);
        StartTurn();
    }



    private void StartTurn()
    {
        // 1. ZRESETUJ JEDNOSTKI I ZAKTUALIZUJ ICH KOLORY
        Animal[] allAnimals = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None);
        foreach (Animal a in allAnimals)
        {
            a.ResetTurn();
            a.UpdateAllegianceColor(); // NOWOŚĆ: Każdej jednostce każemy przemyśleć swój kolor!
        }

        // 2. ZBIERZ ZŁOTO Z PRZEJĘTYCH PUNKTÓW
        int income = baseGoldPerTurn; // Zaczynamy od podstawowej kwoty (np. 30)
        CapturePoint[] allPoints = Object.FindObjectsByType<CapturePoint>(FindObjectsSortMode.None);

        // Sprawdzamy każdy punkt na mapie
        foreach (CapturePoint point in allPoints)
        {
            // Jeśli punkt należy do gracza, którego jest teraz tura - dodaj złoto!
            if (point.ownerTeam == 1 && currentTurn == TurnState.Player1) income += point.goldPerTurn;
            if (point.ownerTeam == 2 && currentTurn == TurnState.Player2) income += point.goldPerTurn;
        }

        // 3. DODAJ ZŁOTO I ZAKTUALIZUJ UI
        if (currentTurn == TurnState.Player1)
        {
            p1_Gold += income;
            UIManager.Instance.UpdateTurnInfo("Player1", p1_Gold);
        }
        else
        {
            p2_Gold += income;
            UIManager.Instance.UpdateTurnInfo("Player2", p2_Gold);
        }

        UIManager.Instance.UpdateBaseHP(p1_HP, p2_HP);
        Debug.Log($"UI zaktualizowane dla: {currentTurn}. Dochód w tej turze: {income}");

        CalculateBaseDamage();
        UIManager.Instance.UpdateBaseHP(p1_HP, p2_HP);
    }

    private void CalculateBaseDamage()
    {
        // Znajdź wszystkie punkty na mapie
        CapturePoint[] allPoints = Object.FindObjectsByType<CapturePoint>(FindObjectsSortMode.None);

        foreach (CapturePoint point in allPoints)
        {
            // Jeśli jest tura Gracza 1 i on posiada ten punkt
            if (currentTurn == TurnState.Player1 && point.ownerTeam == 1)
            {
                p2_HP -= point.damagePerTurn; // Gracz 1 bije bazę Gracza 2
            }
            // Jeśli jest tura Gracza 2 i on posiada ten punkt
            else if (currentTurn == TurnState.Player2 && point.ownerTeam == 2)
            {
                p1_HP -= point.damagePerTurn; // Gracz 2 bije bazę Gracza 1
            }
        }
    }
}